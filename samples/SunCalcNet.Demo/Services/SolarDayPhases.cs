using SunCalcNet.Model;

namespace SunCalcNet.Demo.Services;

/// <summary>
/// The sun phases of one solar day: this morning's and this evening's events around the date's solar noon, even
/// when one falls on the other side of midnight. <c>GetSunPhases</c> returns calendar days, so in Spain in June
/// tonight's astronomical night starts after midnight and is in tomorrow's list, while today's list starts with
/// last night's. Twilight lengths, day length and tonight's dark sky need the solar day, so this stitches the
/// previous, current and next calendar days together and keeps what lies within half a day of solar noon.
/// </summary>
public static class SolarDayPhases
{
    public static IReadOnlyList<SunPhase> Get(DateOnly date, Place place, TimeZoneInfo zone) =>
        Get(date, place, zone, SunPhaseAngle.Default);

    public static IReadOnlyList<SunPhase> Get(DateOnly date, Place place, TimeZoneInfo zone, IEnumerable<SunPhaseAngle> angles)
    {
        var angleList = angles.ToList();
        List<SunPhase> On(DateOnly day) => SunCalc.GetSunPhases(LocalNoon(day, zone), place.Lat, place.Lng, angleList, zone).ToList();

        var today = On(date);
        var noon = today.First(x => x.Name == SunPhaseName.SolarNoon).PhaseTime;

        // rises lie between the nadir and noon, sets between noon and the next nadir, each within 12 h of noon;
        // the nadirs themselves sit right on that edge and belong to either day, so they are left out
        return On(date.AddDays(-1)).Concat(today).Concat(On(date.AddDays(1)))
            .Where(x => x.Name != SunPhaseName.Nadir && (x.PhaseTime - noon).Duration() < TimeSpan.FromHours(12))
            .OrderBy(x => x.PhaseTime)
            .ToList();
    }

    private static DateTimeOffset LocalNoon(DateOnly date, TimeZoneInfo zone)
    {
        var local = date.ToDateTime(new TimeOnly(12, 0));
        return new DateTimeOffset(local, zone.GetUtcOffset(local));
    }
}
