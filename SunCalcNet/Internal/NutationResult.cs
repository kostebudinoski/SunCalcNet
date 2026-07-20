namespace SunCalcNet.Internal;

/// <summary>
/// Result of a nutation calculation: nutation in longitude and the true obliquity of the ecliptic.
/// </summary>
internal readonly struct NutationResult
{
    /// <summary>
    /// Nutation in longitude (Δψ), in degrees.
    /// </summary>
    public double DeltaPsi { get; }

    /// <summary>
    /// True obliquity of the ecliptic (ε), in radians.
    /// </summary>
    public double Obliquity { get; }

    public NutationResult(double deltaPsi, double obliquity)
    {
        DeltaPsi = deltaPsi;
        Obliquity = obliquity;
    }
}
