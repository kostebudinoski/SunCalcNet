using System;

namespace SunCalcNet.Internal;

/// <summary>
/// Constants used in astronomical calculations.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Conversion factor from degrees to radians.
    /// </summary>
    internal const double Rad = Math.PI / 180;

    /// <summary>
    /// Earth's equatorial radius in km, used for the Moon's topocentric parallax.
    /// </summary>
    internal const double EarthRadius = 6378.14;

    /// <summary>
    /// Julian date constant for calculations.
    /// </summary>
    internal const double J0 = 0.0009;

    /// <summary>
    /// Julian date for January 1, 1970 (Unix epoch).
    /// </summary>
    internal const double J1970 = 2440588;

    /// <summary>
    /// Julian date for January 1, 2000 (J2000 epoch).
    /// </summary>
    internal const double J2000 = 2451545;
}