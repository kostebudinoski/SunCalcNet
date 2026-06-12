using System;

namespace SunCalcNet.Internal
{
    internal static class SunTime
    {
        internal static double GetJulianCycle(double daysSinceJ2000, double lw)
        {
            return Math.Round(daysSinceJ2000 - Constants.J0 - lw / (2 * Math.PI));
        }

        internal static double GetApproxTransit(double ht, double lw, double n)
        {
            return Constants.J0 + (ht + lw) / (2 * Math.PI) + n;
        }

        internal static double GetSolarTransitJ(double approxTransitDaysSinceJ2000, double m, double l)
        {
            return Constants.J2000 + approxTransitDaysSinceJ2000 + 0.0053 * Math.Sin(m) - 0.0069 * Math.Sin(2 * l);
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
        internal static double GetSetJ(double h, double lw, double phi, double dec, double n, double m, double l)
        {
            var w = GetHourAngle(h, phi, dec);
            var approxTransitDaysSinceJ2000 = GetApproxTransit(w, lw, n);
            return GetSolarTransitJ(approxTransitDaysSinceJ2000, m, l);
        }
        
        internal static double GetObserverAngle(double height)
        {
            return -2.076 * Math.Sqrt(height) / 60;
        }

        private static double GetHourAngle(double h, double phi, double d)
        {
            return Math.Acos((Math.Sin(h) - Math.Sin(phi) * Math.Sin(d)) / (Math.Cos(phi) * Math.Cos(d)));
        }
    }
}