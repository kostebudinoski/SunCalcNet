namespace SunCalcNet.Internal;

/// <summary>
/// Terrestrial Time (TT) support. The Meeus position series are defined in Terrestrial Time,
/// but SunCalc's input dates are Universal Time (UT). ΔT = TT − UT lets the position math run on
/// TT-shifted days-since-J2000 while sidereal time stays on UT.
/// </summary>
internal static class AstroTime
{
    /// <summary>
    /// ΔT = TT − UT in seconds (Espenak & Meeus polynomial fits, good ~1900–2150).
    /// The decimal year is derived arithmetically from <paramref name="daysSinceJ2000"/> rather
    /// than from the date, since ΔT only needs ~month accuracy here (it changes &lt; 1 s/yr).
    /// </summary>
    /// <param name="daysSinceJ2000">Days since J2000.0 (UT).</param>
    /// <returns>ΔT in seconds.</returns>
    private static double DeltaT(double daysSinceJ2000)
    {
        var y = 2000 + daysSinceJ2000 / 365.2425;
        double t;
        if (y < 1920)
        {
            t = y - 1900;
            return -2.79 + t * (1.494119 + t * (-0.0598939 + t * (0.0061966 - t * 0.000197)));
        }

        if (y < 1941)
        {
            t = y - 1920;
            return 21.20 + t * (0.84493 + t * (-0.076100 + t * 0.0020936));
        }

        if (y < 1961)
        {
            t = y - 1950;
            return 29.07 + t * (0.407 + t * (-1.0 / 233 + t / 2547));
        }

        if (y < 1986)
        {
            t = y - 1975;
            return 45.45 + t * (1.067 + t * (-1.0 / 260 - t / 718));
        }

        if (y < 2005)
        {
            t = y - 2000;
            return 63.86 + t * (0.3345 + t * (-0.060374 + t * (0.0017275 + t * (0.000651814 + t * 0.00002373599))));
        }

        if (y < 2050)
        {
            t = y - 2000;
            return 62.92 + t * (0.32217 + t * 0.005589);
        }

        t = (y - 1820) / 100;
        return -20 + 32 * t * t - 0.5628 * (2150 - y);
    }

    /// <summary>
    /// Converts UT days-since-J2000 to TT days-since-J2000 by applying ΔT.
    /// </summary>
    /// <param name="daysSinceJ2000">Days since J2000.0 (UT).</param>
    /// <returns>Days since J2000.0 in Terrestrial Time.</returns>
    internal static double ToDaysTt(double daysSinceJ2000)
    {
        return daysSinceJ2000 + DeltaT(daysSinceJ2000) / 86400;
    }
}