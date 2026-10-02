using System.Globalization;
using System.Text;

namespace SunCalcNet.Demo.Services;

/// <summary>
/// Builds a one-event iCalendar (.ics, RFC 5545) file. Timed events are written in UTC so every calendar app shows
/// them in its own zone; all-day events use the place's local date.
/// </summary>
public static class CalendarFile
{
    public static string Timed(string title, string description, string location, DateTimeOffset start, DateTimeOffset end, int remindMinutesBefore = 30)
    {
        var lines = Header(title, description, location);
        lines.Add($"DTSTART:{Utc(start)}");
        lines.Add($"DTEND:{Utc(end > start ? end : start.AddMinutes(30))}");
        lines.Add("BEGIN:VALARM");
        lines.Add("ACTION:DISPLAY");
        lines.Add($"DESCRIPTION:{Escape(title)}");
        lines.Add($"TRIGGER:-PT{remindMinutesBefore}M");
        lines.Add("END:VALARM");
        return Footer(lines);
    }

    public static string AllDay(string title, string description, string location, DateOnly day)
    {
        var lines = Header(title, description, location);
        lines.Add($"DTSTART;VALUE=DATE:{day:yyyyMMdd}");
        lines.Add($"DTEND;VALUE=DATE:{day.AddDays(1):yyyyMMdd}");
        lines.Add("TRANSP:TRANSPARENT"); // doesn't block the day as busy
        return Footer(lines);
    }

    /// <summary>A safe file name, e.g. "full-moon-2026-10-26.ics".</summary>
    public static string FileName(string title, DateOnly day)
    {
        var slug = new string(title.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray());
        while (slug.Contains("--"))
        {
            slug = slug.Replace("--", "-");
        }

        return $"{slug.Trim('-')}-{day:yyyy-MM-dd}.ics";
    }

    private static List<string> Header(string title, string description, string location) =>
    [
        "BEGIN:VCALENDAR",
        "VERSION:2.0",
        "PRODID:-//SunCalcNet//Sky Almanac//EN",
        "CALSCALE:GREGORIAN",
        "BEGIN:VEVENT",
        $"UID:{Guid.NewGuid():N}@suncalcnet-sky-almanac",
        $"DTSTAMP:{Utc(DateTimeOffset.UtcNow)}",
        $"SUMMARY:{Escape(title)}",
        $"DESCRIPTION:{Escape(description)}",
        $"LOCATION:{Escape(location)}",
    ];

    private static string Footer(List<string> lines)
    {
        lines.Add("END:VEVENT");
        lines.Add("END:VCALENDAR");
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            Fold(sb, line);
        }

        return sb.ToString();
    }

    private static string Utc(DateTimeOffset t) => t.UtcDateTime.ToString("yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture);

    private static string Escape(string text) =>
        text.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,").Replace("\r\n", "\\n").Replace("\n", "\\n");

    // lines longer than 75 octets continue on the next line after a single space (RFC 5545 section 3.1)
    private static void Fold(StringBuilder sb, string line)
    {
        var bytes = 0;
        var first = true;
        foreach (var rune in line.EnumerateRunes())
        {
            var size = rune.Utf8SequenceLength;
            if (bytes + size > (first ? 75 : 74))
            {
                sb.Append("\r\n ");
                bytes = 0;
                first = false;
            }

            sb.Append(rune.ToString());
            bytes += size;
        }

        sb.Append("\r\n");
    }
}
