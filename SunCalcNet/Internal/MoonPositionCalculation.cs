using System;
using SunCalcNet.Model;

namespace SunCalcNet.Internal
{
    [Serializable]
    internal readonly struct MoonPositionCalculation : IEquatable<MoonPositionCalculation>
    {
        public MoonPositionCalculation(double apparentAltitude, GeocentricCoords moonCoords, double hourAngle)
        {
            ApparentAltitude = apparentAltitude;
            MoonCoords = moonCoords;
            HourAngle = hourAngle;
        }

        public double ApparentAltitude { get; }

        public GeocentricCoords MoonCoords { get; }

        public double HourAngle { get; }

        public static bool operator ==(MoonPositionCalculation lhs, MoonPositionCalculation rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(MoonPositionCalculation lhs, MoonPositionCalculation rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(MoonPositionCalculation other)
        {
            return ApparentAltitude == other.ApparentAltitude
                   && MoonCoords == other.MoonCoords
                   && HourAngle == other.HourAngle;
        }

        public override bool Equals(object obj)
        {
            if (obj is MoonPositionCalculation calculation)
            {
                return Equals(calculation);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ApparentAltitude.GetHashCode();
                hashCode = (hashCode * 397) ^ MoonCoords.GetHashCode();
                return (hashCode * 397) ^ HourAngle.GetHashCode();
            }
        }
    }
}
