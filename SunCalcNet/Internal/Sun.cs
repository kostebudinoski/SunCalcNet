using SunCalcNet.Model;
using System;

namespace SunCalcNet.Internal;

internal static class Sun
{
    /// <summary>
    /// Sun's apparent equatorial coordinates, Meeus ch. 25 (nutation + aberration, T-dependent obliquity).
    /// </summary>
    /// <param name="daysSinceJ2000Tt">Days since J2000.0 in Terrestrial Time (see <see cref="AstroTime.ToDaysTt"/>).</param>
    /// <returns>Apparent equatorial coordinates of the Sun.</returns>
    internal static EquatorialCoords GetApparentEquatorialCoords(double daysSinceJ2000Tt)
    {
        var t = daysSinceJ2000Tt / 36525; // Julian centuries since J2000

        var l0 = Constants.Rad * (280.46646 + t * (36000.76983 + t * 0.0003032)); // 25.2 geometric mean longitude
        var m = Constants.Rad * (357.52911 + t * (35999.05029 - t * 0.0001537)); // 25.3 mean anomaly
        var sinM = Math.Sin(m);
        var cosM = Math.Cos(m);

        // equation of center
        var c = Constants.Rad * ((1.914602 - t * (0.004817 + t * 0.000014)) * sinM + (0.019993 - 0.000101 * t) * 2 * sinM * cosM + 0.000289 * sinM * (3 - 4 * sinM * sinM));

        var om = Constants.Rad * (125.04 - 1934.136 * t); // longitude of the ascending node
        var l = l0 + c - Constants.Rad * (0.00569 + 0.00478 * Math.Sin(om)); // apparent longitude (nutation + aberration)

        // 22.2 mean obliquity + 25.8 correction for apparent position
        var e = Constants.Rad * (23.439291 - t * (0.0130042 + t * (0.00000016 - t * 0.000000504))) + Constants.Rad * 0.00256 * Math.Cos(om);

        var ra = Position.GetRightAscension(l, 0, e); // 25.6
        var dec = Position.GetDeclination(l, 0, e); // 25.7

        return new EquatorialCoords(ra, dec);
    }
}
