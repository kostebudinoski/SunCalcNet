using System;
using SunCalc.Internal;

namespace SunCalc.Utils
{
    internal static class DateTimeUtils
    {
        private const int DayMs = 1000 * 60 * 60 * 24;
        private const double J1899 = 2415018.5;

        public static DateTime FromJulian(this double j)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((j + 0.5 - Constants.J1970) * DayMs);
        }

        public static double ToDays(this DateTime date)
        {
            return ToJulianDate(date) - Constants.J2000;
        }
        
        /// <summary>
        /// OADate is similar to Julian Dates, but uses a different starting point (December 30, 1899 vs. January 1, 4713 BC).
        /// The Julian Date of midnight, December 30, 1899 is 2415018.5.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static double ToJulianDate(this DateTime date)
        {
            return date.ToOADate() + J1899;
        }
    }
}