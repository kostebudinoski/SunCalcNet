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
    /// Refines a transit time so the Sun's local hour angle is zero (Meeus 15.2).
    /// </summary>
    /// <param name="dt">Approximate transit, days since J2000 (UT).</param>
    /// <param name="lw">Longitude west in radians.</param>
    /// <returns>Refined transit, days since J2000 (UT).</returns>
    internal static double SolarTransit(double dt, double lw)
    {
        for (var i = 0; i < 3; i++)
        {
            var hourAngle = Position.WrapPi(Position.GetSiderealTime(dt, lw) - Sun.GetApparentEquatorialCoords(AstroTime.ToDaysTt(dt)).RightAscension);
            dt -= hourAngle / (2 * Math.PI);
        }

        return dt;
    }

    /// <summary>
    /// Time the Sun reaches altitude <paramref name="h0"/> within the half day [<paramref name="a"/>, <paramref name="b"/>]
    /// between transit and nadir. Altitude is monotonic over the half day, so a sign change means exactly one crossing.
    /// Seeds from the hour angle at transit declination, then runs Newton on sin(h) kept inside a shrinking bracket,
    /// bisecting whenever a step leaves it — on near-grazing days the slope is tiny and plain Newton steps land days away.
    /// </summary>
    /// <param name="h0">Target altitude in radians.</param>
    /// <param name="a">Start of the half day, days since J2000 (UT).</param>
    /// <param name="b">End of the half day, days since J2000 (UT).</param>
    /// <param name="sa">Sine of the Sun's altitude at <paramref name="a"/>.</param>
    /// <param name="sb">Sine of the Sun's altitude at <paramref name="b"/>.</param>
    /// <param name="dt">Transit time, days since J2000 (UT).</param>
    /// <param name="lw">Longitude west in radians.</param>
    /// <param name="phi">Observer latitude in radians.</param>
    /// <param name="dec">Sun declination at transit in radians.</param>
    /// <returns>Event time, days since J2000 (UT); NaN if the Sun stays above/below this altitude all half day.</returns>
    internal static double GetSetJ(double h0, double a, double b, double sa, double sb, double dt, double lw, double phi, double dec)
    {
        var s0 = Math.Sin(h0);
        var rising = b <= dt;
        if ((sa - s0) * (sb - s0) > 0)
        {
            return double.NaN; // sun stays above / below this altitude all half day
        }

        if (sa < sb != rising)
        {
            return double.NaN; // only at the poles, where the declination drift outruns the diurnal swing — no rise/set to name
        }

        var cosH0 = (s0 - Math.Sin(phi) * Math.Sin(dec)) / (Math.Cos(phi) * Math.Cos(dec));
        var hourAngle0 = cosH0 <= -1 ? Math.PI : cosH0 >= 1 ? 0 : Math.Acos(cosH0);
        var d = dt + (rising ? -hourAngle0 : hourAngle0) / (2 * Math.PI);

        while (true)
        {
            var c = Sun.GetApparentEquatorialCoords(AstroTime.ToDaysTt(d));
            var hourAngle = Position.GetSiderealTime(d, lw) - c.RightAscension;
            var k = Math.Cos(phi) * Math.Cos(c.Declination);
            var f = Math.Sin(phi) * Math.Sin(c.Declination) + k * Math.Cos(hourAngle) - s0;
            if (f < 0 == rising)
            {
                a = d;
            }
            else
            {
                b = d;
            }

            var step = f / (2 * Math.PI * k * Math.Sin(hourAngle));
            if (d + step > a && d + step < b)
            {
                d += step;
                if (Math.Abs(step) < 1e-5)
                {
                    return d; // ~1 s step, so quadratic convergence leaves ~µs error
                }
            }
            else
            {
                d = (a + b) / 2;
                if (b - a < 1e-8)
                {
                    return d;
                }
            }
        }
    }
}
