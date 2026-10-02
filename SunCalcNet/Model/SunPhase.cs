using System;

namespace SunCalcNet.Model;

[Serializable]
public struct SunPhase : IEquatable<SunPhase>
{
    /// <summary>
    /// Sun phase name.
    /// </summary>
    public SunPhaseName Name { get; }

    /// <summary>
    /// When the sun phase occurs, at the observer's UTC offset (the requested date's offset, or the time zone's
    /// offset at that moment). Two phases are equal when they occur at the same instant, whatever their offsets.
    /// </summary>
    public DateTimeOffset PhaseTime { get; }

    public SunPhase(SunPhaseName name, DateTimeOffset phaseTime)
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
