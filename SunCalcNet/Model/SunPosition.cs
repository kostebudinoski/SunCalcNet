using System;

namespace SunCalcNet.Model
{
    [Serializable]
    public struct SunPosition : IEquatable<SunPosition>
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

        public static bool operator ==(SunPosition lhs, SunPosition rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(SunPosition lhs, SunPosition rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(SunPosition other)
        {
            return Azimuth == other.Azimuth
                   && Altitude == other.Altitude;
        }

        public override bool Equals(object obj)
        {
            if (obj is SunPosition position)
            {
                return Equals(position);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Azimuth.GetHashCode() * 397) ^ Altitude.GetHashCode();
            }
        }
    }
}