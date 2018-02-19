using SunCalcNet.Model;
using System;

namespace SunCalcNet.Internal
{
    internal static class Moon
    {
        /// <summary>
        /// Get Moon coordinates.
        /// </summary>
        /// <param name="days"></param>
        /// <returns></returns>
        public static GeocentricCoords GetGeocentricCoords(double days)
        {
            var eclipticLongitude = Constants.Rad * (218.316 + 13.176396 * days);
            var meanAnomaly = Constants.Rad * (134.963 + 13.064993 * days);
            var meanDistance = Constants.Rad * (93.272 + 13.229350 * days);

            var longitude = eclipticLongitude + Constants.Rad * 6.289 * Math.Sin(meanAnomaly);
            var latitude = Constants.Rad * 5.128 * Math.Sin(meanDistance);
            var dt = 385001 - 20905 * Math.Cos(meanAnomaly); // distance to the moon in km

            var ra = Position.GetRightAscension(longitude, latitude);
            var declination = Position.GetDeclination(longitude, latitude);

            return new GeocentricCoords(ra, declination, dt);
        }
    }
}