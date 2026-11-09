using System;

namespace SunCalcNet.Internal;

/// <summary>
/// Argument checks for the public methods. A NaN or infinite input has no sun or moon to compute, and would
/// otherwise flow into the rise/set solver (which it used to hang, suncalc #193) or come back as a quiet NaN.
/// </summary>
internal static class Guard
{
    /// <summary>
    /// Throws unless <paramref name="lat"/> is within ±90° and <paramref name="lng"/> is finite (any finite
    /// longitude names a meridian, so it is not limited to ±180°).
    /// </summary>
    internal static void Location(double lat, double lng)
    {
        if (!(lat >= -90 && lat <= 90))
        {
            throw new ArgumentOutOfRangeException(nameof(lat), lat, "Latitude must be a number from -90 to 90 degrees.");
        }

        Finite(lng, nameof(lng));
    }

    /// <summary>
    /// Throws unless <paramref name="value"/> is a finite number.
    /// </summary>
    internal static void Finite(double value, string paramName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Must be a finite number.");
        }
    }
}
