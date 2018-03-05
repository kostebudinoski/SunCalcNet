using System;

namespace SunCalcNet.Model
{
    public struct GeocentricCoords : IEquatable<GeocentricCoords>
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

        public bool Equals(GeocentricCoords other)
        {
            return RightAscension.Equals(other.RightAscension)
                   && Declination.Equals(other.Declination)
                   && Distance.Equals(other.Distance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is GeocentricCoords coords && Equals(coords);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = RightAscension.GetHashCode();
                hashCode = (hashCode * 397) ^ Declination.GetHashCode();
                return (hashCode * 397) ^ Distance.GetHashCode();
            }
        }
    }
}