using SunCalcNet.Model;

namespace SunCalcNet.Demo.Services;

public sealed record Place(string Name, double Lat, double Lng, string TimeZoneId);

public sealed record SunEvent(SunPhaseName Name, DateTime Utc, string Color);

public sealed record SkySample(double Minute, double SunAltitude, double SunAzimuth, double MoonAltitude, double MoonAzimuth);

public sealed record MoonEvents(DateTime? Rise, DateTime? Set, DateTime? Transit, DateTime? LowerTransit, bool AlwaysUp, bool AlwaysDown);

public sealed record SkyPosition(double SunAltitude, double SunAzimuth, double MoonAltitude, double MoonAzimuth, double MoonDistance);

/// <summary>
/// Everything the page shows for one place and one local calendar day. The library works in UTC;
/// this class picks the UTC instants that cover the place's local day and converts results back.
/// </summary>
public sealed class AlmanacDay
{
    private const double Deg = 180 / Math.PI;

    public static readonly IReadOnlyList<Place> Places =
    [
        new("London", 51.5072, -0.1276, "Europe/London"),
        new("Copenhagen", 55.6761, 12.5683, "Europe/Copenhagen"),
        new("Vienna", 48.2082, 16.3738, "Europe/Vienna"),
        new("Skopje", 41.9981, 21.4254, "Europe/Skopje"),
        new("Kyiv", 50.4547, 30.5238, "Europe/Kyiv"),
        new("Reykjavík", 64.1355, -21.8954, "Atlantic/Reykjavik"),
        new("Longyearbyen", 78.2233, 15.6469, "Arctic/Longyearbyen"),
        new("New York", 40.7128, -74.0060, "America/New_York"),
        new("Boston", 42.3601, -71.0589, "America/New_York"),
        new("Tokyo", 35.6762, 139.6503, "Asia/Tokyo"),
        new("Sydney", -33.8688, 151.2093, "Australia/Sydney"),
        new("Cape Town", -33.9249, 18.4241, "Africa/Johannesburg"),
        new("Quito", -0.1807, -78.4678, "America/Guayaquil"),
    ];

    private static readonly Dictionary<SunPhaseName, string> PhaseColors = new()
    {
        [SunPhaseName.Nadir] = "#4b5280",
        [SunPhaseName.NightEnd] = "#4f5fa8",
        [SunPhaseName.NauticalDawn] = "#6574c4",
        [SunPhaseName.Dawn] = "#8c6fc2",
        [SunPhaseName.Sunrise] = "#e06a3b",
        [SunPhaseName.SunriseEnd] = "#e5823f",
        [SunPhaseName.GoldenHourEnd] = "#d69a2d",
        [SunPhaseName.SolarNoon] = "#c99316",
        [SunPhaseName.GoldenHour] = "#d69a2d",
        [SunPhaseName.SunsetStart] = "#e5823f",
        [SunPhaseName.Sunset] = "#e06a3b",
        [SunPhaseName.Dusk] = "#8c6fc2",
        [SunPhaseName.NauticalDusk] = "#6574c4",
        [SunPhaseName.Night] = "#4f5fa8",
    };

    /// <summary>
    /// Plain-language explanation of each built-in phase, with the sun-centre altitude SunPhaseAngle.Default uses.
    /// </summary>
    public static readonly IReadOnlyDictionary<SunPhaseName, string> PhaseDescriptions = new Dictionary<SunPhaseName, string>
    {
        [SunPhaseName.Nadir] = "Solar midnight: the sun is at its lowest point, on the meridian below your feet.",
        [SunPhaseName.NightEnd] = "Astronomical dawn (sun 18° below the horizon). The sky starts to brighten; the faintest stars fade.",
        [SunPhaseName.NauticalDawn] = "Nautical dawn (sun 12° below). The horizon becomes visible at sea while bright stars are still out.",
        [SunPhaseName.Dawn] = "Civil dawn (sun 6° below). Enough light for most outdoor activities without artificial light.",
        [SunPhaseName.Sunrise] = "The top edge of the sun appears on the horizon (centre 0.833° below, allowing for refraction).",
        [SunPhaseName.SunriseEnd] = "The bottom edge of the sun clears the horizon (centre 0.3° below).",
        [SunPhaseName.GoldenHourEnd] = "The sun climbs above 6°. Soft, warm morning light gives way to full daylight.",
        [SunPhaseName.SolarNoon] = "The sun crosses the meridian at its highest point of the day.",
        [SunPhaseName.GoldenHour] = "The sun drops below 6°. Evening golden hour: soft, warm light that photographers love.",
        [SunPhaseName.SunsetStart] = "The bottom edge of the sun touches the horizon (centre 0.3° below).",
        [SunPhaseName.Sunset] = "The sun disappears below the horizon (centre 0.833° below, allowing for refraction).",
        [SunPhaseName.Dusk] = "Civil dusk (sun 6° below). Artificial light is now needed; the brightest planets and stars appear.",
        [SunPhaseName.NauticalDusk] = "Nautical dusk (sun 12° below). The horizon can no longer be seen at sea.",
        [SunPhaseName.Night] = "Astronomical night (sun 18° below). The sky is fully dark; the faintest stars are visible.",
    };

    public static class MoonDescriptions
    {
        public const string Rise = "The top edge of the moon appears on the horizon.";
        public const string Set = "The top edge of the moon disappears below the horizon.";
        public const string Transit = "The moon crosses the meridian at its highest point (\"moon overhead\"). Reported even if the moon is below the horizon then.";
        public const string LowerTransit = "The moon crosses the meridian on the far side, at its lowest point (\"moon underfoot\").";
        public const string Missing = "A lunar day lasts about 24 h 50 min, so each event skips one calendar day about once a month.";
    }

    public Place Place { get; }
    public DateOnly Date { get; }
    public TimeZoneInfo Zone { get; }
    public DateTime StartUtc { get; }
    public DateTime EndUtc { get; }
    public double LengthMinutes => (EndUtc - StartUtc).TotalMinutes;

    public IReadOnlyList<SunEvent> SunEvents { get; }
    public IReadOnlyList<SunPhaseName> MissingSunEvents { get; }
    public IReadOnlyList<SkySample> Samples { get; }
    public MoonEvents Moon { get; }
    public MoonIllumination Illumination { get; }

    /// <summary>Sunrise to sunset, or null on a polar day or night.</summary>
    public TimeSpan? DayLength { get; }

    /// <summary>Change in <see cref="DayLength"/> since the previous day, when both days have one.</summary>
    public TimeSpan? DayLengthChange { get; }

    public DateTime SolarNoonUtc => Find(SunPhaseName.SolarNoon)!.Utc;

    public AlmanacDay(Place place, DateOnly date)
    {
        Place = place;
        Date = date;
        Zone = FindZone(place.TimeZoneId);

        var localMidnight = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        StartUtc = TimeZoneInfo.ConvertTimeToUtc(localMidnight, Zone);
        EndUtc = TimeZoneInfo.ConvertTimeToUtc(localMidnight.AddDays(1), Zone);
        var localNoonUtc = TimeZoneInfo.ConvertTimeToUtc(localMidnight.AddHours(12), Zone);

        // anchoring at local noon makes GetSunPhases resolve the solar day of this calendar day
        var phases = SunCalc.GetSunPhases(localNoonUtc, place.Lat, place.Lng).ToList();
        SunEvents = phases
            .OrderBy(x => x.PhaseTime)
            .Select(x => new SunEvent(x.Name, x.PhaseTime, PhaseColors.GetValueOrDefault(x.Name, "#ffffff")))
            .ToList();
        MissingSunEvents = SunPhaseAngle.Default
            .SelectMany(x => new[] { x.RiseName, x.SetName })
            .Where(name => phases.All(x => x.Name != name))
            .ToList();

        var samples = new List<SkySample>();
        for (var minute = 0.0; minute <= LengthMinutes; minute += 10)
        {
            var p = PositionAt(StartUtc.AddMinutes(minute));
            samples.Add(new SkySample(minute, p.SunAltitude, p.SunAzimuth, p.MoonAltitude, p.MoonAzimuth));
        }

        Samples = samples;
        DayLength = GetDayLength(phases);
        var yesterday = GetDayLength(SunCalc.GetSunPhases(localNoonUtc.AddDays(-1), place.Lat, place.Lng).ToList());
        DayLengthChange = DayLength - yesterday;
        Moon = GetMoonEvents(samples);
        Illumination = MoonCalc.GetMoonIllumination(localNoonUtc);
    }

    public SkyPosition PositionAt(DateTime utc)
    {
        var sun = SunCalc.GetSunPosition(utc, Place.Lat, Place.Lng);
        var moon = MoonCalc.GetMoonPosition(utc, Place.Lat, Place.Lng);
        return new SkyPosition(sun.Altitude * Deg, ToNorthAzimuth(sun.Azimuth), moon.Altitude * Deg, ToNorthAzimuth(moon.Azimuth), moon.Distance);
    }

    public DateTime ToLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(utc, Zone);

    public double MinuteOf(DateTime utc) => (utc - StartUtc).TotalMinutes;

    public bool IsToday(DateTime utc) => utc >= StartUtc && utc < EndUtc;

    public string UtcOffsetLabel(DateTime utc)
    {
        var offset = Zone.GetUtcOffset(utc);
        return offset == TimeSpan.Zero ? "UTC" : $"UTC{(offset < TimeSpan.Zero ? "−" : "+")}{offset.Duration():h\\:mm}";
    }

    /// <summary>
    /// Sun-centre altitude that defines a phase, from <see cref="SunPhaseAngle.Default"/>; null for noon and nadir.
    /// </summary>
    public static double? PhaseAngle(SunPhaseName name) =>
        SunPhaseAngle.Default.FirstOrDefault(x => x.RiseName == name || x.SetName == name)?.Angle;

    /// <summary>
    /// What the sky is doing at a given sun altitude, using the same thresholds as the built-in phases.
    /// </summary>
    public static string SkyState(double altitude) => altitude switch
    {
        >= 6 => "Daylight",
        >= -0.833 => "Golden hour",
        >= -6 => "Civil twilight",
        >= -12 => "Nautical twilight",
        >= -18 => "Astronomical twilight",
        _ => "Night",
    };

    private static TimeSpan? GetDayLength(IReadOnlyList<SunPhase> phases)
    {
        var rise = phases.FirstOrDefault(x => x.Name == SunPhaseName.Sunrise);
        var set = phases.FirstOrDefault(x => x.Name == SunPhaseName.Sunset);
        return rise.Name is not null && set.Name is not null ? set.PhaseTime - rise.PhaseTime : null;
    }

    public SunEvent? Find(SunPhaseName name) => SunEvents.FirstOrDefault(x => x.Name == name);

    /// <summary>
    /// GetMoonPhase scans a UTC day (until utcOffset lands in 2.1.0), so scan the UTC days the local day
    /// overlaps and keep the events that fall inside it.
    /// </summary>
    private MoonEvents GetMoonEvents(IReadOnlyList<SkySample> samples)
    {
        var scans = new List<MoonPhase>();
        for (var day = StartUtc.Date; day < EndUtc; day = day.AddDays(1))
        {
            scans.Add(MoonCalc.GetMoonPhase(DateTime.SpecifyKind(day, DateTimeKind.Utc), Place.Lat, Place.Lng));
        }

        DateTime? First(Func<MoonPhase, DateTime?> pick) => scans
            .Select(pick)
            .Where(x => x.HasValue && IsToday(x.Value))
            .OrderBy(x => x)
            .FirstOrDefault();

        var rise = First(x => x.Rise);
        var set = First(x => x.Set);
        var up = rise is null && set is null && samples.All(x => x.MoonAltitude > 0);
        var down = rise is null && set is null && !up;
        return new MoonEvents(rise, set, First(x => x.Transit), First(x => x.LowerTransit), up, down);
    }

    // the library's azimuth is south-based radians, clockwise via west; the page uses compass degrees
    private static double ToNorthAzimuth(double azimuth) => ((azimuth * Deg + 180) % 360 + 360) % 360;

    public static TimeZoneInfo FindZone(string id)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception)
        {
            return TimeZoneInfo.Local;
        }
    }

    public static string PhaseName(double phase) => phase switch
    {
        < 0.02 or > 0.98 => "New Moon",
        < 0.23 => "Waxing Crescent",
        < 0.27 => "First Quarter",
        < 0.48 => "Waxing Gibbous",
        < 0.52 => "Full Moon",
        < 0.73 => "Waning Gibbous",
        < 0.77 => "Last Quarter",
        _ => "Waning Crescent",
    };

    /// <summary>
    /// Sky colour for a given sun altitude in degrees, from deep night through twilight to day.
    /// </summary>
    public static string SkyColor(double altitude)
    {
        (double Alt, int R, int G, int B)[] stops =
        [
            (-90, 4, 6, 16), (-18, 8, 12, 34), (-12, 20, 28, 78), (-6, 44, 52, 122),
            (-2, 112, 72, 140), (0, 232, 128, 96), (4, 247, 184, 112), (10, 128, 186, 240), (90, 74, 150, 236),
        ];

        if (altitude <= stops[0].Alt)
        {
            return Hex(stops[0].R, stops[0].G, stops[0].B);
        }

        for (var i = 1; i < stops.Length; i++)
        {
            if (altitude <= stops[i].Alt)
            {
                var a = stops[i - 1];
                var b = stops[i];
                var t = (altitude - a.Alt) / (b.Alt - a.Alt);
                return Hex(Lerp(a.R, b.R, t), Lerp(a.G, b.G, t), Lerp(a.B, b.B, t));
            }
        }

        var last = stops[^1];
        return Hex(last.R, last.G, last.B);

        static int Lerp(int a, int b, double t) => (int)Math.Round(a + (b - a) * t);
        static string Hex(int r, int g, int b) => $"#{r:x2}{g:x2}{b:x2}";
    }
}
