using System;

namespace SunCalcNet.Internal;

internal static class SunTime
{
    /// <summary>
    /// Observer-height horizon dip (radians) for an observer at the given height (meters).
    /// </summary>
    internal static double GetObserverAngle(double height)
    {
        return -2.076 * Math.Sqrt(height) / 60;
    }

    /// <summary>
    /// Wraps an angle to the range (-PI, PI].
    /// </summary>
    private static double WrapPi(double a)
    {
        return a - 2 * Math.PI * Math.Round(a / (2 * Math.PI));
    }

    /// <summary>
    /// Refines a transit time so the Sun's local hour angle is zero (Meeus 15.2).
    /// </summary>
    /// <param name="dt">Approximate transit, days since J2000 (UT).</param>
    /// <param name="lw">Longitude west in radians.</param>
    /// <returns>Refined transit, days since J2000 (UT).</returns>
    internal static double SolarTransit(double dt, double lw)
    {
        for (var i = 0; i < 3; i++)
        {
            var hourAngle = WrapPi(Position.GetSiderealTime(dt, lw) - Sun.GetApparentEquatorialCoords(AstroTime.ToDaysTt(dt)).RightAscension);
            dt -= hourAngle / (2 * Math.PI);
        }

        return dt;
    }

    /// <summary>
    /// Time the Sun reaches altitude <paramref name="h0"/> on the given side of transit,
    /// converging with Meeus' altitude correction (15.2).
    /// </summary>
    /// <param name="h0">Target altitude in radians.</param>
    /// <param name="dt">Transit time, days since J2000 (UT).</param>
    /// <param name="sign">-1 for rise, +1 for set.</param>
    /// <param name="lw">Longitude west in radians.</param>
    /// <param name="phi">Observer latitude in radians.</param>
    /// <param name="dec">Sun declination at transit in radians.</param>
    /// <returns>Event time, days since J2000 (UT); NaN if the Sun stays above/below this altitude all day.</returns>
    internal static double GetSetJ(double h0, double dt, int sign, double lw, double phi, double dec)
    {
        var cosH0 = (Math.Sin(h0) - Math.Sin(phi) * Math.Sin(dec)) / (Math.Cos(phi) * Math.Cos(dec));
        if (cosH0 < -1 || cosH0 > 1)
        {
            return double.NaN; // sun stays above / below this altitude all day
        }

        var d = dt + sign * Math.Acos(cosH0) / (2 * Math.PI);
        for (var i = 0; i < 2; i++)
        {
            var c = Sun.GetApparentEquatorialCoords(AstroTime.ToDaysTt(d));
            var hourAngle = WrapPi(Position.GetSiderealTime(d, lw) - c.RightAscension);
            var h = Position.GetAltitude(hourAngle, phi, c.Declination);
            var sinH = Math.Cos(phi) * Math.Cos(c.Declination) * Math.Sin(hourAngle);
            if (Math.Abs(sinH) < 1e-6)
            {
                break; // grazing the horizon — correction is ill-conditioned
            }

            d += (h - h0) / (2 * Math.PI * sinH);
        }

        return d;
    }
}