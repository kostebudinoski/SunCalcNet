using SunCalcNet.Model;

namespace SunCalcNet.Demo.Services;

public enum UpcomingKind
{
    Moon,
    Season,
    Light,
    Clock,
    Polar,
}

/// <summary>
/// An interesting event after a given day: <see cref="When"/> is local to the place, <see cref="Day"/> is the
/// calendar date to jump to. Moon events carry the illuminated fraction for drawing.
/// </summary>
public sealed record UpcomingEvent(
    DateTimeOffset When,
    DateOnly Day,
    UpcomingKind Kind,
    string Title,
    string Detail,
    bool ShowTime,
    double MoonFraction = 0,
    bool MoonWaxing = false,
    bool Highlight = false,
    bool SunComingUp = false);

/// <summary>
/// Finds upcoming sky events for a place using only the library's public API: moon phases from
/// <c>GetMoonIllumination</c>, seasons and light extremes from a day-by-day scan of <c>GetSunPhases</c>
/// and <c>GetSunPosition</c>, clock changes from the time zone.
/// </summary>
public static class Upcoming
{
    private const double Deg = 180 / Math.PI;

    // full moons closer than this are commonly called supermoons (mean distance is ~384,400 km)
    private const double SupermoonKm = 361_900;

    public static IReadOnlyList<UpcomingEvent> Find(Place place, TimeZoneInfo zone, DateOnly from, int count = 8)
    {
        var start = LocalNoon(from, zone);
        var events = new List<UpcomingEvent>();
        events.AddRange(MoonPhases(place, zone, start));
        events.AddRange(SunYear(place, zone, from));

        return events
            .Where(e => e.When >= start.AddHours(-12))
            .OrderBy(e => e.When)
            .Take(count)
            .ToList();
    }

    // ---- moon: the next new, first quarter, full and last quarter -------------------------------------------

    private static IEnumerable<UpcomingEvent> MoonPhases(Place place, TimeZoneInfo zone, DateTimeOffset start)
    {
        foreach (var (when, phase) in FindPhases(start))
        {
            var local = TimeZoneInfo.ConvertTime(when, zone);
            var illumination = MoonCalc.GetMoonIllumination(when);
            var distance = MoonCalc.GetMoonPosition(when, place.Lat, place.Lng).Distance;
            var supermoon = phase == MoonPhaseKind.Full && distance < SupermoonKm;

            var (title, detail) = phase switch
            {
                MoonPhaseKind.New => ("New moon", "Darkest nights of the month, best for stargazing"),
                MoonPhaseKind.FirstQuarter => ("First quarter", "Half lit, high in the evening sky"),
                MoonPhaseKind.Full when supermoon => ("Full moon", $"Supermoon: only {distance:N0} km away, a little bigger and brighter"),
                MoonPhaseKind.Full => ("Full moon", $"Rises around sunset, {distance:N0} km away"),
                _ => ("Last quarter", "Half lit, rises around midnight"),
            };

            yield return new UpcomingEvent(local, DateOnly.FromDateTime(local.DateTime), UpcomingKind.Moon, title, detail,
                ShowTime: true, illumination.Fraction, illumination.Waxing, Highlight: supermoon);
        }
    }

    private enum MoonPhaseKind
    {
        New,
        FirstQuarter,
        Full,
        LastQuarter,
    }

    /// <summary>
    /// The first of each principal phase in the month after <paramref name="start"/>: where
    /// <c>GetMoonIllumination().Phase</c> passes 0, 0.25, 0.5 and 0.75. Since SunCalcNet 3.0.1 the phase comes from
    /// the moon–sun difference in ecliptic longitude, so these are the exact USNO instants. 6-hour samples bracket
    /// each crossing, then bisection refines it.
    /// </summary>
    private static IEnumerable<(DateTimeOffset When, MoonPhaseKind Phase)> FindPhases(DateTimeOffset start)
    {
        (double Target, MoonPhaseKind Kind)[] targets =
            [(0, MoonPhaseKind.New), (0.25, MoonPhaseKind.FirstQuarter), (0.5, MoonPhaseKind.Full), (0.75, MoonPhaseKind.LastQuarter)];

        foreach (var (target, kind) in targets)
        {
            // signed distance past the target, wrapped to [-0.5, 0.5): the phase only increases, so a crossing
            // shows as this going from negative to non-negative
            double Past(DateTimeOffset t) => ((MoonCalc.GetMoonIllumination(t).Phase - target) % 1 + 1.5) % 1 - 0.5;

            var a = start;
            var pa = Past(a);
            for (var i = 0; i < 4 * 31; i++)
            {
                var b = a.AddHours(6);
                var pb = Past(b);
                if (pa < 0 && pb >= 0)
                {
                    yield return (Crossing(a, b, Past), kind);
                    break;
                }

                (a, pa) = (b, pb);
            }
        }
    }

    // where a rising function crosses zero in [a, b], by bisection
    private static DateTimeOffset Crossing(DateTimeOffset a, DateTimeOffset b, Func<DateTimeOffset, double> f)
    {
        for (var i = 0; i < 20; i++)
        {
            var mid = a + (b - a) / 2;
            if (f(mid) < 0)
            {
                a = mid;
            }
            else
            {
                b = mid;
            }
        }

        return a + (b - a) / 2;
    }

    // ---- sun: one year scanned day by day ---------------------------------------------------------------------

    private sealed record SunDay(DateOnly Date, DateTimeOffset Noon, double Declination, double NoonAltitude,
        DateTimeOffset? Sunrise, DateTimeOffset? Sunset, TimeSpan NoonOffset);

    private static IEnumerable<UpcomingEvent> SunYear(Place place, TimeZoneInfo zone, DateOnly from)
    {
        var days = new List<SunDay>();
        for (var i = -1; i <= 366; i++)
        {
            var date = from.AddDays(i);
            var phases = SunCalc.GetSunPhases(LocalNoon(date, zone), place.Lat, place.Lng, zone).ToList();
            var noon = phases.First(x => x.Name == SunPhaseName.SolarNoon).PhaseTime;
            var altitude = SunCalc.GetSunPosition(noon, place.Lat, place.Lng).Altitude * Deg;
            var equator = SunCalc.GetSunPosition(noon, 0, place.Lng);

            days.Add(new SunDay(
                date,
                noon,
                Declination(equator.Altitude * Deg, equator.Azimuth * Deg + 180),
                altitude,
                phases.FirstOrDefault(x => x.Name == SunPhaseName.Sunrise).Name is null ? null : phases.First(x => x.Name == SunPhaseName.Sunrise).PhaseTime,
                phases.FirstOrDefault(x => x.Name == SunPhaseName.Sunset).Name is null ? null : phases.First(x => x.Name == SunPhaseName.Sunset).PhaseTime,
                zone.GetUtcOffset(LocalNoon(date, zone))));
        }

        var north = place.Lat >= 0;

        for (var i = 1; i < days.Count - 1; i++)
        {
            var (prev, day, next) = (days[i - 1], days[i], days[i + 1]);

            // solstice: the sun's declination turns around
            var isMax = day.Declination > prev.Declination && day.Declination >= next.Declination;
            var isMin = day.Declination < prev.Declination && day.Declination <= next.Declination;
            if (isMax || isMin)
            {
                var longest = isMax == north;
                var month = isMax ? "June" : "December";

                // vertex of the parabola through the three noon declinations, in days from this noon
                var curvature = prev.Declination - 2 * day.Declination + next.Declination;
                var shift = curvature == 0 ? 0 : 0.5 * (prev.Declination - next.Declination) / curvature;
                yield return Season(day, zone, day.Noon.AddDays(shift), longest ? "Longest day" : "Shortest day",
                    $"{month} solstice · {Daylight(day)}");
            }

            // equinox: the declination changes sign
            if (Math.Sign(prev.Declination) != Math.Sign(day.Declination) && prev.Declination != 0)
            {
                var springing = day.Declination > 0 == north;
                var month = day.Declination > 0 ? "March" : "September";

                // linear interpolation between the two noons to the moment the declination is zero
                var fraction = prev.Declination / (prev.Declination - day.Declination);
                yield return Season(day, zone, prev.Noon + (day.Noon - prev.Noon) * fraction, springing ? "Spring equinox" : "Autumn equinox",
                    $"{month} equinox · day and night nearly equal ({Daylight(day)})");
            }

            // daylight saving switch between this day and the previous one
            if (day.NoonOffset != prev.NoonOffset)
            {
                var forward = day.NoonOffset > prev.NoonOffset;
                var shift = (day.NoonOffset - prev.NoonOffset).Duration();
                yield return Event(day, UpcomingKind.Clock, forward ? "Clocks go forward" : "Clocks go back",
                    $"{(shift.TotalMinutes == 60 ? "1 hour" : $"{shift.TotalMinutes:0} min")}: sunrise and sunset move {(forward ? "later" : "earlier")} on the clock");
            }

            // polar transitions
            var wasPolar = prev.Sunrise is null || prev.Sunset is null;
            var isPolar = day.Sunrise is null || day.Sunset is null;
            if (!wasPolar && isPolar)
            {
                var midnightSun = day.NoonAltitude > 0;
                yield return Event(day, UpcomingKind.Polar, midnightSun ? "Midnight sun begins" : "Polar night begins",
                    midnightSun ? "The sun stops setting" : "The sun stops rising") with { SunComingUp = midnightSun };
            }
            else if (wasPolar && !isPolar)
            {
                var afterMidnightSun = prev.NoonAltitude > 0;
                yield return Event(day, UpcomingKind.Polar, afterMidnightSun ? "Midnight sun ends" : "The sun returns",
                    afterMidnightSun ? "First sunset in weeks" : "First sunrise after the polar night") with { SunComingUp = !afterMidnightSun };
            }
        }

        // the year's earliest sunset and latest sunrise: not on the shortest day, which surprises most people
        var scan = days.Skip(1).Take(366).ToList();
        if (scan.All(x => x.Sunrise is not null && x.Sunset is not null))
        {
            var earliestSunset = scan.MinBy(x => x.Sunset!.Value.TimeOfDay)!;
            var latestSunrise = scan.MaxBy(x => x.Sunrise!.Value.TimeOfDay)!;

            if (!IsEdge(earliestSunset, scan))
            {
                yield return Event(earliestSunset, UpcomingKind.Light, "Earliest sunset of the year",
                    $"{earliestSunset.Sunset!.Value.Clock()}{Why(earliestSunset, days)}", earliestSunset.Sunset!.Value);
            }

            if (!IsEdge(latestSunrise, scan))
            {
                yield return Event(latestSunrise, UpcomingKind.Light, "Latest sunrise of the year",
                    $"{latestSunrise.Sunrise!.Value.Clock()}{Why(latestSunrise, days)}", latestSunrise.Sunrise!.Value);
            }
        }
    }

    private static UpcomingEvent Event(SunDay day, UpcomingKind kind, string title, string detail, DateTimeOffset? when = null) =>
        new(when ?? day.Noon, day.Date, kind, title, detail, ShowTime: false);

    // a solstice or equinox dated by its interpolated moment, which can fall on the evening before the scanned noon
    private static UpcomingEvent Season(SunDay day, TimeZoneInfo zone, DateTimeOffset moment, string title, string detail)
    {
        var local = TimeZoneInfo.ConvertTime(moment, zone);
        return new UpcomingEvent(local, DateOnly.FromDateTime(local.DateTime), UpcomingKind.Season, title, detail, ShowTime: true);
    }

    private static bool IsEdge(SunDay day, IReadOnlyList<SunDay> scan) => day == scan[0] || day == scan[^1];

    // ", 13 days before the shortest day", or ", the day before the clocks change" when that's the reason
    private static string Why(SunDay day, IReadOnlyList<SunDay> days)
    {
        var i = days.ToList().IndexOf(day);
        if (i + 1 < days.Count && days[i + 1].NoonOffset != day.NoonOffset)
        {
            return ", the day before the clocks change";
        }

        // the solstice: the shortest day within 40 days, and a real minimum rather than the end of that window
        var window = days
            .Where(x => Math.Abs(x.Date.DayNumber - day.Date.DayNumber) <= 40 && x.Sunrise is not null && x.Sunset is not null)
            .ToList();
        var shortest = window.MinBy(x => x.Sunset!.Value - x.Sunrise!.Value);
        if (shortest is null || shortest == window[0] || shortest == window[^1])
        {
            return "";
        }

        var gap = day.Date.DayNumber - shortest.Date.DayNumber;
        return gap switch
        {
            0 => ", on the shortest day",
            < 0 => $", {-gap} days before the shortest day",
            _ => $", {gap} days after the shortest day",
        };
    }

    private static string Daylight(SunDay day)
    {
        if (day.Sunrise is { } rise && day.Sunset is { } set)
        {
            var length = set - rise;
            return $"{(int)length.TotalHours} h {length.Minutes} min of daylight";
        }

        return day.NoonAltitude > 0 ? "the sun doesn't set" : "the sun doesn't rise";
    }

    /// <summary>
    /// The sun's declination from its noon position seen from the equator, where it is at least 66° high:
    /// refraction is removed with Bennett's formula, then sin(dec) = cos(alt)·cos(az) on the equator.
    /// </summary>
    private static double Declination(double apparentAltitude, double azimuthFromNorth)
    {
        var refraction = 1 / Math.Tan((apparentAltitude + 7.31 / (apparentAltitude + 4.4)) / Deg) / 60;
        var altitude = (apparentAltitude - refraction) / Deg;
        return Math.Asin(Math.Cos(altitude) * Math.Cos(azimuthFromNorth / Deg)) * Deg;
    }

    private static DateTimeOffset LocalNoon(DateOnly date, TimeZoneInfo zone)
    {
        var local = date.ToDateTime(new TimeOnly(12, 0));
        return new DateTimeOffset(local, zone.GetUtcOffset(local));
    }
}
