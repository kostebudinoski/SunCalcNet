namespace SunCalcNet.Demo.Services;

/// <summary>
/// Clock times for display, rounded to the nearest minute. The library returns exact times with seconds, and
/// "HH:mm" alone would truncate them (07:00:45 shown as 07:00), one minute off what almanacs like USNO publish.
/// </summary>
public static class TimeFormat
{
    public static string Clock(this DateTimeOffset time) => Round(time).ToString("HH:mm");

    public static string Clock(this DateTime time) => Round(time).ToString("HH:mm");

    private static DateTimeOffset Round(DateTimeOffset t) => new(Round(t.DateTime), t.Offset);

    private static DateTime Round(DateTime t) =>
        new DateTime((t.Ticks + TimeSpan.TicksPerMinute / 2) / TimeSpan.TicksPerMinute * TimeSpan.TicksPerMinute, t.Kind);
}
