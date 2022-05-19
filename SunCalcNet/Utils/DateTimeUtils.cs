using SunCalcNet.Internal;
using System;

namespace SunCalcNet.Utils
{
    internal static class DateTimeUtils
    {
        

        public static DateTime FromJulian(this double j)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((j + 0.5 - Constants.J1970) * Constants.DayMs);
        }

        public static double ToDays(this DateTime date)
        {
            return ToJulianDate(date) - Constants.J2000;
        }

        public static DateTime HoursLater(this DateTime date, double hours)
        {
            return date.AddHours(hours);
        }

        /// <summary>
        /// OADate is similar to Julian Dates, but uses a different starting point (December 30, 1899 vs. January 1, 4713 BC).
        /// The Julian Date of midnight, December 30, 1899 is 2415018.5.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static double ToJulianDate(this DateTime date)
        {
            return date.ToUniversalTime().ToOADate() + Constants.J1899;
        }
    }

    internal static class DateTimeOffsetUtils
    {
        public static DateTimeOffset FromJulian(this double j, TimeSpan offset)
        {
            return new DateTimeOffset(1970, 1, 1, 0, 0, 0, offset).AddMilliseconds((j + 0.5 - Constants.J1970) * Constants.DayMs);
        }
    }
}