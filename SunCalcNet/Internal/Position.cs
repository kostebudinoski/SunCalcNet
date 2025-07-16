using System;

namespace SunCalcNet.Internal
{
    /// <summary>
    /// Provides methods for astronomical position calculations.
    /// </summary>
    internal static class Position
    {
        private const double RefractionCoefficient = 0.0002967;
        private const double RefractionInnerNumerator = 0.00312536;
        private const double RefractionDenominatorOffset = 0.08901179;
        
        private const double SiderealBase = 280.16;
        private const double SiderealCoefficient = 360.9856235;
        
        /// <summary>
        /// Calculates the right ascension for the given celestial coordinates.
        /// </summary>
        /// <param name="longitude">The celestial longitude in radians.</param>
        /// <param name="latitude">The celestial latitude in radians.</param>
        /// <returns>The right ascension in radians.</returns>
        internal static double GetRightAscension(double longitude, double latitude)
        {
            return Math.Atan2(
                Math.Sin(longitude) * Math.Cos(Constants.EarthObliquity) - 
                Math.Tan(latitude) * Math.Sin(Constants.EarthObliquity), 
                Math.Cos(longitude));
        }
        
        /// <summary>
        /// Calculates the declination for the given celestial coordinates.
        /// </summary>
        /// <param name="longitude">The celestial longitude in radians.</param>
        /// <param name="latitude">The celestial latitude in radians.</param>
        /// <returns>The declination in radians.</returns>
        internal static double GetDeclination(double longitude, double latitude)
        {
            return Math.Asin(
                Math.Sin(latitude) * Math.Cos(Constants.EarthObliquity) + 
                Math.Cos(latitude) * Math.Sin(Constants.EarthObliquity) * Math.Sin(longitude));
        }
        
        /// <summary>
        /// Calculates the azimuth angle.
        /// </summary>
        /// <param name="hourAngle">The hour angle in radians.</param>
        /// <param name="latitude">The observer's latitude in radians.</param>
        /// <param name="declination">The declination in radians.</param>
        /// <returns>The azimuth angle in radians.</returns>
        internal static double GetAzimuth(double hourAngle, double latitude, double declination)
        {
            return Math.Atan2(
                Math.Sin(hourAngle),
                Math.Cos(hourAngle) * Math.Sin(latitude) - Math.Tan(declination) * Math.Cos(latitude));
        }
        
        /// <summary>
        /// Calculates the altitude of a celestial body.
        /// </summary>
        /// <param name="hourAngle">The hour angle in radians.</param>
        /// <param name="latitude">The observer's latitude in radians.</param>
        /// <param name="declination">The declination in radians.</param>
        /// <returns>The altitude in radians.</returns>
        internal static double GetAltitude(double hourAngle, double latitude, double declination)
        {
            return Math.Asin(
                Math.Sin(latitude) * Math.Sin(declination) + 
                Math.Cos(latitude) * Math.Cos(declination) * Math.Cos(hourAngle));
        }
        
        /// <summary>
        /// Calculates the local sidereal time.
        /// </summary>
        /// <param name="julianDay">The julian day.</param>
        /// <param name="longitudeWest">The longitude west in radians.</param>
        /// <returns>The local sidereal time in radians.</returns>
        internal static double GetSiderealTime(double julianDay, double longitudeWest)
        {
            return Constants.Rad * (SiderealBase + SiderealCoefficient * julianDay) - longitudeWest;
        }

        /// <summary>
        /// Calculates the atmospheric refraction correction.
        /// </summary>
        /// <param name="altitude">The altitude in radians.</param>
        /// <returns>The refraction correction in radians.</returns>
        internal static double GetAstroRefraction(double altitude)
        {
            // Formula 16.4 of "Astronomical Algorithms" 2nd edition by Jean Meeus
            if (altitude < 0) // The formula works for positive altitudes only
            {
                altitude = 0; // Prevent division by zero when altitude is -0.08901179
            }

            return RefractionCoefficient / Math.Tan(altitude + RefractionInnerNumerator / (altitude + RefractionDenominatorOffset));
        }
    }
}