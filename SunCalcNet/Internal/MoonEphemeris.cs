using SunCalcNet.Model;

namespace SunCalcNet.Internal;

/// <summary>
/// The Moon's geocentric position at one instant: equatorial coordinates and distance, and the apparent ecliptic
/// longitude, which defines its named phases.
/// </summary>
internal readonly struct MoonEphemeris
{
    /// <summary>
    /// Geocentric equatorial coordinates (right ascension and declination, radians) and distance (km).
    /// </summary>
    public GeocentricCoords Equatorial { get; }

    /// <summary>
    /// Apparent ecliptic longitude (λ), in radians, not normalised.
    /// </summary>
    public double EclipticLongitude { get; }

    public MoonEphemeris(GeocentricCoords equatorial, double eclipticLongitude)
    {
        Equatorial = equatorial;
        EclipticLongitude = eclipticLongitude;
    }
}
