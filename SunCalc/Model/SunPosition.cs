namespace SunCalc.Model
{
    public struct SunPosition
    {
        /// <summary>
        /// Sun azimuth in radians (direction along the horizon, measured from south to west),
        /// e.g. 0 is south and Math.PI * 3/4 is northwest
        /// </summary>
        public double Azimuth { get; }
        
        /// <summary>
        /// Sun altitude above the horizon in radians,
        /// e.g. 0 at the horizon and PI/2 at the zenith (straight over your head)
        /// </summary>
        public double Altitude { get; }
        
        public SunPosition(double azimuth, double altitude)
        {
            Azimuth = azimuth;
            Altitude = altitude;
        }
    }
}