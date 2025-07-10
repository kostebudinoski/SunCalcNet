using System;

namespace SunCalcNet.Model;

[Serializable]
public sealed class SunPhaseName : IEquatable<SunPhaseName>
{
    private SunPhaseName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static readonly SunPhaseName SolarNoon = new("Solar Noon");
    public static readonly SunPhaseName Nadir = new("Nadir");
    public static readonly SunPhaseName Sunrise = new("Sunrise");
    public static readonly SunPhaseName Sunset = new("Sunset");
    public static readonly SunPhaseName SunriseEnd = new("Sunrise End");
    public static readonly SunPhaseName SunsetStart = new("Sunset Start");
    public static readonly SunPhaseName Dawn = new("Dawn");
    public static readonly SunPhaseName Dusk = new("Dusk");
    public static readonly SunPhaseName NauticalDawn = new("Nautical Dawn");
    public static readonly SunPhaseName NauticalDusk = new("Nautical Dusk");
    public static readonly SunPhaseName NightEnd = new("Night End");
    public static readonly SunPhaseName Night = new("Night");
    public static readonly SunPhaseName GoldenHourEnd = new("Golden Hour End");
    public static readonly SunPhaseName GoldenHour = new("Golden Hour");

    public override bool Equals(object obj)
    {
        return obj is SunPhaseName other && Equals(other);
    }

    public bool Equals(SunPhaseName other)
    {
        return other != null && Value == other.Value;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Value.GetHashCode() * 397;
        }
    }

    public static bool operator ==(SunPhaseName left, SunPhaseName right)
    {
        return left?.Equals(right) ?? right is null;
    }

    public static bool operator !=(SunPhaseName left, SunPhaseName right)
    {
        return !(left == right);
    }
    
    public override string ToString()
    {
        return Value;
    }
}