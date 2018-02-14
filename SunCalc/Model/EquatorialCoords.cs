namespace SunCalc.Model
{
    public struct EquatorialCoords
    {
        public double RightAscension { get; }
        
        public double Declination { get; }
        
        public EquatorialCoords(double rightAscension, double declination)
        {
            RightAscension = rightAscension;
            Declination = declination;
        }
    }
}