using System;

namespace SunCalcNet.Model
{
    [Serializable]
    public struct MoonPhase : IEquatable<MoonPhase>
    {
        /// <summary>
        /// Moonrise time as Date
        /// </summary>
        public DateTimeOffset? Rise { get; }

        /// <summary>
        /// Moonset time as Date
        /// </summary>
        public DateTimeOffset? Set { get; }

        /// <summary>
        /// True if the moon never rises/sets and is always above the horizon during the day
        /// </summary>
        public bool AlwaysUp { get; }

        /// <summary>
        /// True if the moon is always below the horizon
        /// </summary>
        public bool AlwaysDown { get; }

        public MoonPhase(DateTimeOffset? rise, DateTimeOffset? set)
        {
            Rise = rise;
            Set = set;
            AlwaysUp = false;
            AlwaysDown = false;
        }

        public MoonPhase(bool alwaysUp)
        {
            Rise = null;
            Set = null;
            AlwaysUp = alwaysUp;
            AlwaysDown = !alwaysUp;
        }

        public static bool operator ==(MoonPhase lhs, MoonPhase rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(MoonPhase lhs, MoonPhase rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(MoonPhase other)
        {
            return Rise == other.Rise
                   && Set == other.Set
                   && AlwaysUp == other.AlwaysUp
                   && AlwaysDown == other.AlwaysDown;
        }

        public override bool Equals(object obj)
        {
            if (obj is MoonPhase phase)
            {
                return Equals(phase);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Rise.GetHashCode();
                hashCode = (hashCode * 397) ^ Set.GetHashCode();
                hashCode = (hashCode * 397) ^ AlwaysUp.GetHashCode();
                return (hashCode * 397) ^ AlwaysDown.GetHashCode();
            }
        }
    }
}