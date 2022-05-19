using System;
using System.Collections.Generic;

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

        public DateTimeOffset PhaseTimeOffset { get; }

        public SunPhase(SunPhaseName name, DateTime phaseTime)
        {
            Name = name;
            PhaseTime = phaseTime;
            PhaseTimeOffset = phaseTime;
        }

        public SunPhase(SunPhaseName name, DateTimeOffset phaseTimeOffset): this(name, phaseTimeOffset.UtcDateTime)
        {
            PhaseTimeOffset = phaseTimeOffset;
        }

        public override int GetHashCode()
        {
            int hashCode = 799229037;
            hashCode = (hashCode * -1521134295) + EqualityComparer<SunPhaseName>.Default.GetHashCode(Name);
            hashCode = (hashCode * -1521134295) + PhaseTime.GetHashCode();
            hashCode = (hashCode * -1521134295) + PhaseTimeOffset.GetHashCode();
            return hashCode;
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
                   && PhaseTime == other.PhaseTime
                   && PhaseTimeOffset == other.PhaseTimeOffset;
        }

        public override bool Equals(object obj) => obj is SunPhase phase && Equals(phase);
    }
}