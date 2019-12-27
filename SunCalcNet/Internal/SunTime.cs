using System;

namespace SunCalcNet.Internal
{
    internal static class SunTime
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
        /// Returns set time for the given sun altitude
        /// </summary>
        /// <param name="h"></param>
        /// <param name="lw"></param>
        /// <param name="phi"></param>
        /// <param name="dec"></param>
        /// <param name="n"></param>
        /// <param name="m"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        public static double GetSetJ(double h, double lw, double phi, double dec, double n, double m, double l)
        {
            var w = GetHourAngle(h, phi, dec);
            var a = GetApproxTransit(w, lw, n);
            return GetSolarTransitJ(a, m, l);
        }
        
        public static double GetObserverAngle(double height)
        {
            return -2.076 * Math.Sqrt(height) / 60;
        }

        private static double GetHourAngle(double h, double phi, double d)
        {
            return Math.Acos((Math.Sin(h) - Math.Sin(phi) * Math.Sin(d)) / (Math.Cos(phi) * Math.Cos(d)));
        }
    }
}