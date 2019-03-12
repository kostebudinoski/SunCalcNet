using SunCalcNet.Model;
using System;

namespace SunCalcNet.Internal
{
    internal static class Sun
    {
        /// <summary>
        /// The position that the planet would have relative to its perihelion if the orbit of the planet were a circle is called the mean anomaly.
        /// </summary>
        /// <param name="days"> Julian Day Number</param>
        /// <returns></returns>
        public static double GetMeanAnomaly(double days)
        {
            return Constants.Rad * (357.5291 + 0.98560028 * days);
        }

        /// <summary>
        /// The difference between the true anomaly and the mean anomaly is called the Equation of Center.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static double GetEclipticLongitude(double m)
        {
            var equationOfCenter = GetEquationOfCenter(m);
            return m + equationOfCenter + Constants.EarthPerihelion + Math.PI;
        }

        /// <summary>
        /// Get Sun coordinates.
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static EquatorialCoords GetEquatorialCoords(double days)
        {
            var meanAnomaly = GetMeanAnomaly(days);
            var eclipticLongitude = GetEclipticLongitude(meanAnomaly);

            var dec = Position.GetDeclination(eclipticLongitude, 0);
            var ra = Position.GetRightAscension(eclipticLongitude, 0);

            return new EquatorialCoords(ra, dec);
        }

        private static double GetEquationOfCenter(double m)
        {
            return Constants.Rad * (1.9148 * Math.Sin(m) + 0.02 * Math.Sin(2 * m) + 0.0003 * Math.Sin(3 * m));
        }
    }
}