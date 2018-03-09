using System;

namespace SunCalcNet.Model
{
    [Serializable]
    public struct MoonPosition : IEquatable<MoonPosition>
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

        public static bool operator ==(MoonPosition lhs, MoonPosition rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(MoonPosition lhs, MoonPosition rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(MoonPosition other)
        {
            return Azimuth == other.Azimuth
                   && Altitude == other.Altitude
                   && Distance == other.Distance
                   && ParallacticAngle == other.ParallacticAngle;
        }

        public override bool Equals(object obj)
        {
            if (obj is MoonPosition position)
            {
                return Equals(position);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Azimuth.GetHashCode();
                hashCode = (hashCode * 397) ^ Altitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Distance.GetHashCode();
                return (hashCode * 397) ^ ParallacticAngle.GetHashCode();
            }
        }
    }
}