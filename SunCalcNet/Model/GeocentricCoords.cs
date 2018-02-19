namespace SunCalcNet.Model
{
    public struct GeocentricCoords
    {
        public double RightAscension { get; }

        public double Declination { get; }

        public double Distance { get; }

        public GeocentricCoords(double rightAscension, double declination, double distance)
        {
            RightAscension = rightAscension;
            Declination = declination;
            Distance = distance;
        }
    }
}