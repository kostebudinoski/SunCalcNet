using System;

namespace SunCalcNet.Model
{
    [Serializable]
    public struct SunPhase : IEquatable<SunPhase>
    {
        /// <summary>
        /// Sun phase name.
        /// </summary>
        public SunPhaseName Name { get; }

        /// <summary>
        /// Time of the day when the sun phase occurs
        /// </summary>
        public DateTime PhaseTime { get; }

        public SunPhase(SunPhaseName name, DateTime phaseTime)
        {
            Name = name;
            PhaseTime = phaseTime;
        }

        public static bool operator ==(SunPhase lhs, SunPhase rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(SunPhase lhs, SunPhase rhs)
        {
            return !(lhs == rhs);
        }

        public bool Equals(SunPhase other)
        {
            return Name.Value == other.Name.Value
                   && PhaseTime == other.PhaseTime;
        }

        public override bool Equals(object obj)
        {
            if (obj is SunPhase phase)
            {
                return Equals(phase);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.Value.GetHashCode() * 397) ^ PhaseTime.GetHashCode();
            }
        }
    }
}