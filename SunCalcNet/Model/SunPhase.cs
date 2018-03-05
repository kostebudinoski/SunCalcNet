using System;

namespace SunCalcNet.Model
{
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

        public bool Equals(SunPhase other)
        {
            return Equals(Name, other.Name)
                   && PhaseTime.Equals(other.PhaseTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is SunPhase phase && Equals(phase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name?.GetHashCode() ?? 0) * 397) ^ PhaseTime.GetHashCode();
            }
        }
    }
}