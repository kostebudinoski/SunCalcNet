using SunCalcNet.Model;
using System;

namespace SunCalcNet.Internal
{
    internal static class Moon
    {
        private const double LongitudeBase = 218.316;
        private const double LongitudeRate = 13.176396;
        private const double MeanAnomalyBase = 134.963;
        private const double MeanAnomalyRate = 13.064993;
        private const double MeanDistanceBase = 93.272;
        private const double MeanDistanceRate = 13.229350;
    
        private const double LongitudePerturbation = 6.289;
        private const double LatitudePerturbation = 5.128;
        private const double BaseDistance = 385001;
        private const double DistancePerturbation = 20905;
        
        /// <summary>
        /// Get Moon geocentric coordinates based on days since J2000.0 epoch
        /// </summary>
        /// <param name="days">Days since J2000.0 (January 1, 2000 12:00 UTC)</param>
        /// <returns>Geocentric coordinates of the Moon</returns>
        internal static GeocentricCoords GetGeocentricCoords(double days)
        {
            var eclipticLongitude = Constants.Rad * (LongitudeBase + LongitudeRate * days);
            var meanAnomaly = Constants.Rad * (MeanAnomalyBase + MeanAnomalyRate * days);
            var meanDistance = Constants.Rad * (MeanDistanceBase + MeanDistanceRate * days);


            var longitude = eclipticLongitude + Constants.Rad * LongitudePerturbation * Math.Sin(meanAnomaly);
            var latitude = Constants.Rad * LatitudePerturbation * Math.Sin(meanDistance);
            var dt = BaseDistance - DistancePerturbation * Math.Cos(meanAnomaly); // distance to the moon in km
            
            var ra = Position.GetRightAscension(longitude, latitude);
            var declination = Position.GetDeclination(longitude, latitude);

            return new GeocentricCoords(ra, declination, dt);
        }
    }
}