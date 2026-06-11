namespace SunCalcNet.Model
{
    internal readonly struct MoonAltitudeResult
    {
        public MoonAltitudeResult(double apparentAltitude, GeocentricCoords moonCoords, double hourAngle)
        {
            ApparentAltitude = apparentAltitude;
            MoonCoords = moonCoords;
            HourAngle = hourAngle;
        }

        public double ApparentAltitude { get; }

        public GeocentricCoords MoonCoords { get; }

        public double HourAngle { get; }
    }
}
