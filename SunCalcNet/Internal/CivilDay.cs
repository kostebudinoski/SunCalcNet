using System;

namespace SunCalcNet.Internal;

/// <summary>
/// One observer's local calendar day as UTC instants, plus the UTC offset to report results at.
/// </summary>
internal sealed class CivilDay
{
    private readonly Func<DateTime, TimeSpan> _offsetAt;

    private CivilDay(DateTime startUtc, DateTime endUtc, DateTime anchorUtc, Func<DateTime, TimeSpan> offsetAt)
    {
        StartUtc = startUtc;
        EndUtc = endUtc;
        AnchorUtc = anchorUtc;
        _offsetAt = offsetAt;
    }

    /// <summary>Local midnight starting the day, as a UTC instant.</summary>
    internal DateTime StartUtc { get; }

    /// <summary>Local midnight ending the day, as a UTC instant (23 or 25 hours later on a DST change).</summary>
    internal DateTime EndUtc { get; }

    /// <summary>
    /// The instant whose nearest solar transit is this day's solar noon: local noon for a civil day.
    /// </summary>
    internal DateTime AnchorUtc { get; }

    /// <summary>
    /// The calendar day of <paramref name="date"/>'s own clock, with every result at its offset.
    /// </summary>
    internal static CivilDay FromOffset(DateTimeOffset date)
    {
        var startUtc = new DateTimeOffset(date.DateTime.Date, date.Offset).UtcDateTime;
        var offset = date.Offset;
        return new CivilDay(startUtc, startUtc.AddDays(1), startUtc.AddHours(12), _ => offset);
    }

    /// <summary>
    /// The calendar day <paramref name="date"/> falls on in <paramref name="timeZone"/>, with every result
    /// at the zone's offset at that moment, so days with a daylight saving change come out right.
    /// </summary>
    internal static CivilDay FromTimeZone(DateTimeOffset date, TimeZoneInfo timeZone)
    {
        if (timeZone is null)
        {
            throw new ArgumentNullException(nameof(timeZone));
        }

        var localDate = TimeZoneInfo.ConvertTime(date, timeZone).DateTime.Date;
        return new CivilDay(
            ToUtc(localDate, timeZone),
            ToUtc(localDate.AddDays(1), timeZone),
            ToUtc(localDate.AddHours(12), timeZone),
            utc => timeZone.GetUtcOffset(utc));
    }

    /// <summary>The result's time at the offset this day reports in.</summary>
    internal DateTimeOffset ToResult(DateTime utc)
    {
        var utcTime = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return new DateTimeOffset(utcTime).ToOffset(_offsetAt(utcTime));
    }

    // a local time the clocks skip (midnight in zones that change DST at 00:00) maps to the first valid one after it
    private static DateTime ToUtc(DateTime local, TimeZoneInfo timeZone)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        while (timeZone.IsInvalidTime(unspecified))
        {
            unspecified = unspecified.AddMinutes(15);
        }

        return TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
    }
}
