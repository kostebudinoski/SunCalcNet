using System;

namespace SunCalc.Internal
{
    public static class SunTime
    {
        public static double GetJulianCycle(double d, double lw)
        {
            return Math.Round(d - Constants.J0 - lw / (2 * Math.PI));
        }

        public static double GetApproxTransit(double ht, double lw, double n)
        {
            return Constants.J0 + (ht + lw) / (2 * Math.PI) + n;
        }

        public static double GetSolarTransitJ(double ds, double m, double l)
        {
            return Constants.J2000 + ds + 0.0053 * Math.Sin(m) - 0.0069 * Math.Sin(2 * l);
        }

        /// <summary>
        /// returns set time for the given sun altitude
        /// </summary>
        public static double GetSetJ(double h, double lw, double phi, double dec, double n, double m, double l)
        {
            var w = GetHourAngle(h, phi, dec);
            var a = GetApproxTransit(w, lw, n);
            return GetSolarTransitJ(a, m, l);
        }
        
        private static double GetHourAngle(double h, double phi, double d)
        {
            return Math.Acos((Math.Sin(h) - Math.Sin(phi) * Math.Sin(d)) / (Math.Cos(phi) * Math.Cos(d)));
        }
    }
}