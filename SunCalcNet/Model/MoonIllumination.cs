namespace SunCalcNet.Model
{
    public struct MoonIllumination
    {
        public double Fraction { get; }

        public double Phase { get; }

        public double Angle { get; }

        public MoonIllumination(double fraction, double phase, double angle)
        {
            Fraction = fraction;
            Phase = phase;
            Angle = angle;
        }
    }
}