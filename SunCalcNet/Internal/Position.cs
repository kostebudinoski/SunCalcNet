using System;

namespace SunCalcNet.Internal
{
    internal static class Position
    {
        public static double GetRightAscension(double longitude, double b)
        {
            return Math.Atan2(Math.Sin(longitude) * Math.Cos(Constants.EarthObliquity) - Math.Tan(b) * Math.Sin(Constants.EarthObliquity), Math.Cos(longitude));
        }

        public static double GetDeclination(double longitude, double b)
        {
            return Math.Asin(Math.Sin(b) * Math.Cos(Constants.EarthObliquity) + Math.Cos(b) * Math.Sin(Constants.EarthObliquity) * Math.Sin(longitude));
        }

        public static double GetAzimuth(double h, double phi, double dec)
        {
            return Math.Atan2(Math.Sin(h), Math.Cos(h) * Math.Sin(phi) - Math.Tan(dec) * Math.Cos(phi));
        }

        public static double GetAltitude(double h, double phi, double dec)
        {
            return Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(h));
        }

        public static double GetSiderealTime(double d, double lw)
        {
            return Constants.Rad * (280.16 + 360.9856235 * d) - lw;
        }

        public static double GetAstroRefraction(double h)
        {
            if (h < 0) // the following formula works for positive altitudes only.
            {
                h = 0; // if h = -0.08901179 a div/0 would occur.
            }

            // formula 16.4 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            // 1.02 / tan(h + 10.26 / (h + 5.10)) h in degrees, result in arc minutes -> converted to rad:
            return 0.0002967 / Math.Tan(h + 0.00312536 / (h + 0.08901179));
        }
    }
}