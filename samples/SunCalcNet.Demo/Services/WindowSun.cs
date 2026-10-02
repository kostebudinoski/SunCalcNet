namespace SunCalcNet.Demo.Services;

/// <summary>
/// A window and the building opposite it. Heights are above the ground, in meters; the facing direction is a
/// compass bearing (180 = south). The opposite building is treated as wide: it blocks the sky within
/// <see cref="BlockedHalfAngle"/> degrees either side of straight ahead, up to its roofline.
/// </summary>
public sealed record WindowSetup(
    double FacingAzimuth = 180,
    double WindowHeight = 10,
    double ObstacleHeight = 20,
    double ObstacleDistance = 25,
    double BlockedHalfAngle = 60)
{
    /// <summary>How high above the horizon the opposite roofline appears from the window, in degrees.</summary>
    public double ObstacleAngle =>
        ObstacleDistance <= 0 || ObstacleHeight <= WindowHeight
            ? 0
            : Math.Atan((ObstacleHeight - WindowHeight) / ObstacleDistance) * 180 / Math.PI;
}

public sealed record SunnyPeriod(DateTimeOffset Start, DateTimeOffset End, bool Blocked);

/// <summary>Direct sun on a window over one local day.</summary>
public sealed record WindowSunDay(
    DateOnly Date,
    TimeSpan Sunlit,
    TimeSpan WithoutObstacle,
    IReadOnlyList<SunnyPeriod> Periods,
    DateTimeOffset DayStart,
    DateTimeOffset DayEnd);

/// <summary>
/// Direct sun on a window: the minutes the sun is in front of the wall, above the horizon and clear of the building
/// opposite, sampled every few minutes with <c>GetSunPosition</c>.
/// </summary>
public static class WindowSun
{
    private const double Deg = 180 / Math.PI;
    private const int StepMinutes = 2;

    public static WindowSunDay Compute(Place place, TimeZoneInfo zone, DateOnly date, WindowSetup window)
    {
        var start = Midnight(date, zone);
        var end = Midnight(date.AddDays(1), zone);
        var obstacleAngle = window.ObstacleAngle;

        var periods = new List<SunnyPeriod>();
        var sunlit = TimeSpan.Zero;
        var open = TimeSpan.Zero;
        SunnyPeriod? current = null;

        for (var t = start; t < end; t = t.AddMinutes(StepMinutes))
        {
            var position = SunCalc.GetSunPosition(t, place.Lat, place.Lng);
            var altitude = position.Altitude * Deg;
            var azimuth = position.Azimuth * Deg + 180; // compass bearing
            var offFacing = Math.Abs(Wrap180(azimuth - window.FacingAzimuth));

            // the sun reaches the glass only while above the horizon and in front of the wall
            var inFront = altitude > 0 && offFacing < 90;
            var blocked = inFront && offFacing <= window.BlockedHalfAngle && altitude <= obstacleAngle;
            var step = TimeSpan.FromMinutes(StepMinutes);

            if (inFront)
            {
                open += step;
                if (!blocked)
                {
                    sunlit += step;
                }
            }

            // collect contiguous stretches of "sun in front", marked as sunny or blocked
            var local = TimeZoneInfo.ConvertTime(t, zone);
            if (inFront && current is { } c && c.Blocked == blocked)
            {
                current = c with { End = local.AddMinutes(StepMinutes) };
            }
            else
            {
                if (current is not null)
                {
                    periods.Add(current);
                }

                current = inFront ? new SunnyPeriod(local, local.AddMinutes(StepMinutes), blocked) : null;
            }
        }

        if (current is not null)
        {
            periods.Add(current);
        }

        return new WindowSunDay(date, sunlit, open, periods, TimeZoneInfo.ConvertTime(start, zone), TimeZoneInfo.ConvertTime(end, zone));
    }

    private static double Wrap180(double degrees) => degrees - 360 * Math.Round(degrees / 360);

    private static DateTimeOffset Midnight(DateOnly date, TimeZoneInfo zone)
    {
        var local = date.ToDateTime(TimeOnly.MinValue);
        while (zone.IsInvalidTime(local))
        {
            local = local.AddMinutes(15);
        }

        return new DateTimeOffset(local, zone.GetUtcOffset(local));
    }
}
