using SunCalcNet.Model;

namespace SunCalcNet.Internal;

/// <summary>
/// The Sun's apparent position at one instant: equatorial coordinates for altitude and azimuth, and the apparent
/// ecliptic longitude, which defines the Moon's named phases.
/// </summary>
internal readonly struct SunEphemeris
{
    /// <summary>
    /// Apparent equatorial coordinates (right ascension and declination, radians).
    /// </summary>
    public EquatorialCoords Equatorial { get; }

    /// <summary>
    /// Apparent ecliptic longitude (λ), in radians, not normalised.
    /// </summary>
    public double EclipticLongitude { get; }

    public SunEphemeris(EquatorialCoords equatorial, double eclipticLongitude)
    {
        Equatorial = equatorial;
        EclipticLongitude = eclipticLongitude;
    }
}
