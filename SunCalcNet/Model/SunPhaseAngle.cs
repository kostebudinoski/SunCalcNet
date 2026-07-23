using System;
using System.Collections.Generic;

namespace SunCalcNet.Model;

[Serializable]
public class SunPhaseAngle
{
    private static readonly SunPhaseAngle[] DefaultValues =
    {
        new(-0.833, SunPhaseName.Sunrise, SunPhaseName.Sunset),
        new(-0.3, SunPhaseName.SunriseEnd, SunPhaseName.SunsetStart),
        new(-6, SunPhaseName.Dawn, SunPhaseName.Dusk),
        new(-12, SunPhaseName.NauticalDawn, SunPhaseName.NauticalDusk),
        new(-18, SunPhaseName.NightEnd, SunPhaseName.Night),
        new(6, SunPhaseName.GoldenHourEnd, SunPhaseName.GoldenHour)
    };

    /// <summary>
    /// The six built-in sun phase angle pairs used by <see cref="SunCalc.GetSunPhases(System.DateTime, double, double, double)"/>.
    /// Compose with your own angles, e.g. <c>SunPhaseAngle.Default.Append(myAngle)</c>.
    /// </summary>
    public static IReadOnlyList<SunPhaseAngle> Default { get; } = Array.AsReadOnly(DefaultValues);

    /// <summary>
    /// The angle of the sun's centre relative to the horizon, in degrees
    /// (negative below the horizon, positive above it).
    /// </summary>
    public double Angle { get; }

    /// <summary>
    /// Name of the morning (rising) event when the sun reaches <see cref="Angle"/>.
    /// </summary>
    public SunPhaseName RiseName { get; }

    /// <summary>
    /// Name of the evening (setting) event when the sun reaches <see cref="Angle"/>.
    /// </summary>
    public SunPhaseName SetName { get; }

    /// <summary>
    /// Creates a sun phase angle with the given horizon angle (in degrees) and rise/set names.
    /// </summary>
    public SunPhaseAngle(double angle, SunPhaseName riseName, SunPhaseName setName)
    {
        Angle = angle;
        RiseName = riseName ?? throw new ArgumentNullException(nameof(riseName));
        SetName = setName ?? throw new ArgumentNullException(nameof(setName));
    }

    /// <summary>
    /// Creates a custom sun phase angle with the given horizon angle (in degrees) and rise/set names.
    /// </summary>
    public SunPhaseAngle(double angle, string riseName, string setName)
        : this(angle, SunPhaseName.Custom(riseName), SunPhaseName.Custom(setName))
    {
    }

    /// <summary>
    /// The six built-in sun phase angle pairs. Retained for backwards compatibility; prefer <see cref="Default"/>.
    /// </summary>
    public static IReadOnlyCollection<SunPhaseAngle> List => Default;
}
