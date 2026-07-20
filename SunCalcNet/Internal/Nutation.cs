using System;

namespace SunCalcNet.Internal;

/// <summary>
/// Nutation and obliquity of the ecliptic, from the abridged series of Meeus ch. 22
/// (sub-arcsecond accuracy, ample for SunCalc's needs).
/// </summary>
internal static class Nutation
{
    /// <summary>
    /// Computes the nutation in longitude (Δψ, in degrees) and the true obliquity of the
    /// ecliptic (ε, in radians) for the given time.
    /// </summary>
    /// <param name="t">Julian centuries since J2000.0 (Terrestrial Time).</param>
    /// <returns>The nutation in longitude (degrees) and true obliquity (radians).</returns>
    internal static NutationResult LongitudeAndObliquity(double t)
    {
        var om = Constants.Rad * (125.04452 - 1934.136261 * t); // longitude of the Moon's ascending node
        var ls = Constants.Rad * (280.4665 + 36000.7698 * t);   // mean longitude of the Sun
        var lm = Constants.Rad * (218.3165 + 481267.8813 * t);  // mean longitude of the Moon

        var deltaPsi = (-17.20 * Math.Sin(om) - 1.32 * Math.Sin(2 * ls) - 0.23 * Math.Sin(2 * lm) + 0.21 * Math.Sin(2 * om)) / 3600;
        var deltaEps = (9.20 * Math.Cos(om) + 0.57 * Math.Cos(2 * ls) + 0.10 * Math.Cos(2 * lm) - 0.09 * Math.Cos(2 * om)) / 3600;

        var eps0 = 23.439291 - t * (0.0130042 + t * (0.00000016 - t * 0.000000504)); // 22.2 mean obliquity

        return new NutationResult(deltaPsi, Constants.Rad * (eps0 + deltaEps));
    }
}
