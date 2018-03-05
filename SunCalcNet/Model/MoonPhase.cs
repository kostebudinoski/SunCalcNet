using System;

namespace SunCalcNet.Model
{
    public struct MoonPhase : IEquatable<MoonPhase>
    {
        /// <summary>
        /// Moonrise time as Date
        /// </summary>
        public DateTime? Rise { get; }

        /// <summary>
        /// Moonset time as Date
        /// </summary>
        public DateTime? Set { get; }

        /// <summary>
        /// True if the moon never rises/sets and is always above the horizon during the day
        /// </summary>
        public bool AlwaysUp { get; }

        /// <summary>
        /// True if the moon is always below the horizon
        /// </summary>
        public bool AlwaysDown { get; }

        public MoonPhase(DateTime? rise, DateTime? set)
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

        public bool Equals(MoonPhase other)
        {
            return Rise.Equals(other.Rise)
                   && Set.Equals(other.Set)
                   && AlwaysUp == other.AlwaysUp
                   && AlwaysDown == other.AlwaysDown;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is MoonPhase phase && Equals(phase);
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