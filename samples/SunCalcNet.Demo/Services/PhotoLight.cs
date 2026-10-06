using SunCalcNet.Model;

namespace SunCalcNet.Demo.Services;

/// <summary>A stretch of time with a name, e.g. the morning blue hour; times are local to the place.</summary>
public sealed record LightWindow(string Name, DateTimeOffset Start, DateTimeOffset End, double? LightFromAzimuth = null)
{
    public TimeSpan Length => End - Start;
}

/// <summary>One side of the day (morning or evening) for photographers.</summary>
public sealed record LightSession(string Title, LightWindow? BlueHour, LightWindow? GoldenHour);

/// <summary>
/// The day's light the way photographers plan it: blue and golden hours from custom <see cref="SunPhaseAngle"/>s,
/// the direction the light comes from, tonight's dark-sky window and moon opportunities.
/// </summary>
public sealed class PhotoLight
{
    private const double Deg = 180 / Math.PI;

    /// <summary>
    /// Photographers' definitions, wider than the library's built-in golden hour (sunrise to +6°): blue hour while
    /// the sun is 6° to 4° below the horizon, golden hour from 4° below to 6° above it.
    /// </summary>
    public static readonly IReadOnlyList<SunPhaseAngle> Angles =
    [
        new(-6, "blue-am-start", "blue-pm-end"),
        new(-4, "golden-am-start", "golden-pm-end"),
        new(6, "golden-am-end", "golden-pm-start"),
    ];

    public LightSession Morning { get; }
    public LightSession Evening { get; }

    /// <summary>Tonight's dark-sky stretches: sun 18° or more below the horizon and the moon down.</summary>
    public IReadOnlyList<LightWindow> DarkSky { get; }

    /// <summary>Why there's no dark sky tonight, when there isn't.</summary>
    public string? DarkSkyNote { get; }

    /// <summary>A moon worth photographing near sunrise or sunset, if there is one today.</summary>
    public string? MoonOpportunity => moonShot?.Sentence;

    private readonly MoonShot? moonShot;

    // Sentence for the card; Phrase for the one-line summary ("the full moon rises 12 min before sunset")
    private sealed record MoonShot(string Sentence, string Phrase, bool Evening);

    public PhotoLight(AlmanacDay day)
    {
        var place = day.Place;
        var zone = day.Zone;
        var noon = Noon(day.Date, zone);

        var phases = SunCalc.GetSunPhases(noon, place.Lat, place.Lng, Angles, zone).ToList();
        DateTimeOffset? At(string name) => phases.FirstOrDefault(x => x.Name == SunPhaseName.Custom(name)).Name is null
            ? null
            : phases.First(x => x.Name == SunPhaseName.Custom(name)).PhaseTime;

        Morning = new LightSession("Morning",
            BlueHour: Window(place, "Blue hour", At("blue-am-start"), At("golden-am-start")),
            GoldenHour: Window(place, "Golden hour", At("golden-am-start"), At("golden-am-end")));
        Evening = new LightSession("Evening",
            BlueHour: Window(place, "Blue hour", At("golden-pm-end"), At("blue-pm-end")),
            GoldenHour: Window(place, "Golden hour", At("golden-pm-start"), At("golden-pm-end")));

        (DarkSky, DarkSkyNote) = FindDarkSky(day, zone);
        moonShot = FindMoonShot(day);
    }

    /// <summary>"Shadows are 1.6× as long as things are tall", or null while the sun is down.</summary>
    public static string? ShadowSentence(double sunAltitude)
    {
        if (sunAltitude <= 0.5)
        {
            return null;
        }

        var ratio = 1 / Math.Tan(sunAltitude / Deg);
        return ratio switch
        {
            > 20 => "Shadows stretch across the whole scene",
            > 1.15 => $"Shadows are {ratio:0.0}× as long as things are tall",
            > 0.85 => "Shadows are about as long as things are tall",
            _ => $"Shadows are short, {ratio:0.0}× the height of things: harsh, high light",
        };
    }

    private static LightWindow? Window(Place place, string name, DateTimeOffset? start, DateTimeOffset? end)
    {
        if (start is not { } a || end is not { } b || b <= a)
        {
            return null;
        }

        // where the light comes from, halfway through the window
        var middle = a + (b - a) / 2;
        var azimuth = SunCalc.GetSunPosition(middle, place.Lat, place.Lng).Azimuth * Deg + 180;
        return new LightWindow(name, a, b, (azimuth % 360 + 360) % 360);
    }

    // from this evening's astronomical dusk to tomorrow's astronomical dawn, minus the time the moon is up
    private static (IReadOnlyList<LightWindow>, string?) FindDarkSky(AlmanacDay day, TimeZoneInfo zone)
    {
        var place = day.Place;
        var night = day.Find(SunPhaseName.Night);
        var nextDay = SunCalc.GetSunPhases(Noon(day.Date.AddDays(1), zone), place.Lat, place.Lng, zone).ToList();
        var nightEnd = nextDay.FirstOrDefault(x => x.Name == SunPhaseName.NightEnd);

        if (night is null || nightEnd.Name is null)
        {
            return ([], day.Samples.Max(x => x.SunAltitude) < -18
                ? "Polar night: dark all day, so plan around the moon"
                : "No astronomical darkness tonight: the sun never gets 18° below the horizon");
        }

        var start = new DateTimeOffset(night.Utc).ToOffset(zone.GetUtcOffset(night.Utc));
        var end = nightEnd.PhaseTime;

        var windows = new List<LightWindow>();
        DateTimeOffset? open = null;
        for (var t = start; t <= end; t = t.AddMinutes(5))
        {
            var moonDown = MoonCalc.GetMoonPosition(t, place.Lat, place.Lng).Altitude < 0;
            if (moonDown && open is null)
            {
                open = t;
            }
            else if (!moonDown && open is { } from)
            {
                windows.Add(new LightWindow("Dark sky", Local(from, zone), Local(t, zone)));
                open = null;
            }
        }

        if (open is { } last)
        {
            windows.Add(new LightWindow("Dark sky", Local(last, zone), Local(end, zone)));
        }

        windows.RemoveAll(w => w.Length < TimeSpan.FromMinutes(20));
        if (windows.Count == 0)
        {
            var fraction = day.Illumination.Fraction;
            return ([], $"The moon is up all night ({fraction:P0} lit): {(fraction > 0.5 ? "better for moonlit landscapes than stars" : "faint, so bright stars still show")}");
        }

        return (windows, null);
    }

    // a full-ish moon rising or setting within an hour of sunset or sunrise: a big moon over the horizon, in good light
    private static MoonShot? FindMoonShot(AlmanacDay day)
    {
        var fraction = day.Illumination.Fraction;
        if (fraction < 0.85)
        {
            return null;
        }

        var sunrise = day.Find(SunPhaseName.Sunrise)?.Utc;
        var sunset = day.Find(SunPhaseName.Sunset)?.Utc;
        var lit = $"{fraction:P0} lit";
        var moonName = fraction >= 0.97 ? "full moon" : "nearly full moon";

        MoonShot? Near(DateTime? moon, DateTime? sun, string moonEvent, string sunEvent, bool evening)
        {
            if (moon is not { } m || sun is not { } s || Math.Abs((m - s).TotalMinutes) > 60)
            {
                return null;
            }

            var gap = $"{Math.Abs((m - s).TotalMinutes):0} min {(m < s ? "before" : "after")} {sunEvent}";
            return new MoonShot(
                $"Moon{moonEvent} at {day.ToLocal(m).Clock()}, {gap} ({lit}): a big moon low over the horizon in soft light",
                $"the {moonName} {moonEvent}s {gap}",
                evening);
        }

        return Near(day.Moon.Rise, sunset, "rise", "sunset", evening: true)
            ?? Near(day.Moon.Set, sunrise, "set", "sunrise", evening: false)
            ?? Near(day.Moon.Rise, sunrise, "rise", "sunrise", evening: false)
            ?? Near(day.Moon.Set, sunset, "set", "sunset", evening: true);
    }

    /// <summary>
    /// The day's photography plan in one line: the half of the day with a moon shot when there is one ("Best light
    /// today"), otherwise the next golden hour after <paramref name="at"/> ("Next good light"); for days without a
    /// golden hour, tonight's dark sky.
    /// </summary>
    public (string Label, string Text)? Summary(DateTimeOffset at)
    {
        if (moonShot is { } shot && (shot.Evening ? Evening : Morning).GoldenHour is { } best)
        {
            return ("Best light today", $"{Describe(shot.Evening ? "evening" : "morning", best)}, and {shot.Phrase}");
        }

        foreach (var (name, session) in new[] { ("morning", Morning), ("evening", Evening) })
        {
            if (session.GoldenHour is { } golden && golden.End > at)
            {
                return golden.Start <= at
                    ? ("Good light now", $"golden hour until {golden.End.Clock()}{LightFrom(golden)}")
                    : ("Next good light", Describe(name, golden));
            }
        }

        if (DarkSky.Count > 0)
        {
            var dark = DarkSky[0];
            var label = Morning.GoldenHour is null && Evening.GoldenHour is null ? "Tonight" : "Still to come";
            return (label, $"dark sky {dark.Start.Clock()}–{dark.End.Clock()} for stargazing");
        }

        return DarkSkyNote is null ? null : ("Tonight", char.ToLowerInvariant(DarkSkyNote[0]) + DarkSkyNote[1..]);
    }

    private static string Describe(string partOfDay, LightWindow golden) =>
        $"{partOfDay} golden hour {golden.Start.Clock()}–{golden.End.Clock()}{LightFrom(golden)}";

    private static string LightFrom(LightWindow window) =>
        window.LightFromAzimuth is { } az ? $", light from the {Compass(az)}" : "";

    private static string Compass(double azimuth)
    {
        string[] points = ["N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"];
        return points[(int)Math.Round(azimuth / 22.5) % 16];
    }

    private static DateTimeOffset Local(DateTimeOffset t, TimeZoneInfo zone) => TimeZoneInfo.ConvertTime(t, zone);

    private static DateTimeOffset Noon(DateOnly date, TimeZoneInfo zone)
    {
        var local = date.ToDateTime(new TimeOnly(12, 0));
        return new DateTimeOffset(local, zone.GetUtcOffset(local));
    }
}
