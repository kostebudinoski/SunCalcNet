using System;

namespace SunCalcNet.Model;

[Serializable]
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
    /// Upper meridian transit, when the moon is at its highest point ("moon overhead").
    /// Reported even when the moon is below the horizon; null on the ~1 day a month the day misses it.
    /// </summary>
    public DateTime? Transit { get; }

    /// <summary>
    /// Lower meridian transit, when the moon is at its lowest point ("moon underfoot").
    /// Reported even when the moon is below the horizon; null on the ~1 day a month the day misses it.
    /// </summary>
    public DateTime? LowerTransit { get; }

    /// <summary>
    /// True if the moon never rises/sets and is always above the horizon during the day
    /// </summary>
    public bool AlwaysUp { get; }

    /// <summary>
    /// True if the moon is always below the horizon
    /// </summary>
    public bool AlwaysDown { get; }

    /// <param name="rise">Moonrise time, or null if the moon doesn't rise during the day.</param>
    /// <param name="set">Moonset time, or null if the moon doesn't set during the day.</param>
    /// <param name="maxHeight">Highest moon height above the rise/set horizon sampled over the day;
    /// decides <see cref="AlwaysUp"/> / <see cref="AlwaysDown"/> when there's neither rise nor set.</param>
    public MoonPhase(DateTime? rise, DateTime? set, double maxHeight)
        : this(rise, set, null, null, maxHeight)
    {
    }

    /// <param name="rise">Moonrise time, or null if the moon doesn't rise during the day.</param>
    /// <param name="set">Moonset time, or null if the moon doesn't set during the day.</param>
    /// <param name="transit">Upper meridian transit, or null if it doesn't occur during the day.</param>
    /// <param name="lowerTransit">Lower meridian transit, or null if it doesn't occur during the day.</param>
    /// <param name="maxHeight">Highest moon height above the rise/set horizon sampled over the day;
    /// decides <see cref="AlwaysUp"/> / <see cref="AlwaysDown"/> when there's neither rise nor set.</param>
    public MoonPhase(DateTime? rise, DateTime? set, DateTime? transit, DateTime? lowerTransit, double maxHeight)
    {
        Rise = rise;
        Set = set;
        Transit = transit;
        LowerTransit = lowerTransit;
        AlwaysUp = false;
        AlwaysDown = false;

        if (rise.HasValue || set.HasValue)
        {
            return;
        }

        if (maxHeight > 0)
        {
            AlwaysUp = true;
        }
        else
        {
            AlwaysDown = true;
        }
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
               && Transit == other.Transit
               && LowerTransit == other.LowerTransit
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
            hashCode = (hashCode * 397) ^ Transit.GetHashCode();
            hashCode = (hashCode * 397) ^ LowerTransit.GetHashCode();
            hashCode = (hashCode * 397) ^ AlwaysUp.GetHashCode();
            return (hashCode * 397) ^ AlwaysDown.GetHashCode();
        }
    }
}
