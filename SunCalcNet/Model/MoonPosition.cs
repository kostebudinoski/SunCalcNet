namespace SunCalcNet.Model
{
    public struct MoonPosition
    {
        /// <summary>
        /// Moon azimuth in radians
        /// </summary>
        public double Azimuth { get; }

        /// <summary>
        /// Moon altitude above the horizon in radians
        /// </summary>
        public double Altitude { get; }

        /// <summary>
        /// Distance to moon in kilometers
        /// </summary>
        public double Distance { get; }

        /// <summary>
        /// Parallactic angle of the moon in radians
        /// </summary>
        public double ParallacticAngle { get; }

        public MoonPosition(double azimuth, double altitude, double distance, double parallacticAngle)
        {
            Azimuth = azimuth;
            Altitude = altitude;
            Distance = distance;
            ParallacticAngle = parallacticAngle;
        }
    }
}