using System;

namespace SunCalcNet.Internal
{
    internal static class Constants
    {
        public const double Rad = Math.PI / 180;

        public const double EarthPerihelion = Rad * 102.9372;

        public const double EarthObliquity = Rad * 23.4397;

        public const double J0 = 0.0009;

        public const double J1970 = 2440588;

        public const double J2000 = 2451545;
    }
}