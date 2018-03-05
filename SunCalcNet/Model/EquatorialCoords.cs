using System;

namespace SunCalcNet.Model
{
    public struct EquatorialCoords : IEquatable<EquatorialCoords>
    {
        public double RightAscension { get; }

        public double Declination { get; }

        public EquatorialCoords(double rightAscension, double declination)
        {
            RightAscension = rightAscension;
            Declination = declination;
        }

        public bool Equals(EquatorialCoords other)
        {
            return RightAscension.Equals(other.RightAscension)
                   && Declination.Equals(other.Declination);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is EquatorialCoords coords && Equals(coords);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (RightAscension.GetHashCode() * 397) ^ Declination.GetHashCode();
            }
        }
    }
}