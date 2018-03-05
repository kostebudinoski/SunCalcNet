using System;

namespace SunCalcNet.Model
{
    public struct MoonIllumination : IEquatable<MoonIllumination>
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

        public bool Equals(MoonIllumination other)
        {
            return Fraction.Equals(other.Fraction)
                   && Phase.Equals(other.Phase)
                   && Angle.Equals(other.Angle);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is MoonIllumination illumination && Equals(illumination);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Fraction.GetHashCode();
                hashCode = (hashCode * 397) ^ Phase.GetHashCode();
                return (hashCode * 397) ^ Angle.GetHashCode();
            }
        }
    }
}