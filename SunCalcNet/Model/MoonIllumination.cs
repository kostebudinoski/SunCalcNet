using System;

namespace SunCalcNet.Model;

[Serializable]
public struct MoonIllumination : IEquatable<MoonIllumination>
{
    public double Fraction { get; }

    public double Phase { get; }

    public double Angle { get; }

    /// <summary>
    /// True while the Moon is waxing (illuminated fraction growing, new -> full),
    /// false while waning (full -> new).
    /// </summary>
    public bool Waxing { get; }

    public MoonIllumination(double fraction, double phase, double angle, bool waxing)
    {
        Fraction = fraction;
        Phase = phase;
        Angle = angle;
        Waxing = waxing;
    }

    public static bool operator ==(MoonIllumination lhs, MoonIllumination rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(MoonIllumination lhs, MoonIllumination rhs)
    {
        return !(lhs == rhs);
    }

    public bool Equals(MoonIllumination other)
    {
        return Fraction == other.Fraction
               && Phase == other.Phase
               && Angle == other.Angle
               && Waxing == other.Waxing;
    }

    public override bool Equals(object obj)
    {
        if (obj is MoonIllumination illumination)
        {
            return Equals(illumination);
        }

        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Fraction.GetHashCode();
            hashCode = (hashCode * 397) ^ Phase.GetHashCode();
            hashCode = (hashCode * 397) ^ Angle.GetHashCode();
            return (hashCode * 397) ^ Waxing.GetHashCode();
        }
    }
}
