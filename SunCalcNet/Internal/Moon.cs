using SunCalcNet.Model;
using System;

namespace SunCalcNet.Internal;

/// <summary>
/// Moon calculations based on Meeus ch. 47 (truncated ELP-2000/82 series).
/// </summary>
internal static class Moon
{
    // Meeus table 47.A — periodic terms for the Moon's longitude (Σl, ×1e-6 deg) and
    // distance (Σr, ×1e-3 km). Flat rows of 6: D, M, M', F, Σl, Σr.
    private static readonly int[] MoonLon =
    [
        0, 0, 1, 0, 6288774, -20905355,
        2, 0, -1, 0, 1274027, -3699111,
        2, 0, 0, 0, 658314, -2955968,
        0, 0, 2, 0, 213618, -569925,
        0, 1, 0, 0, -185116, 48888,
        0, 0, 0, 2, -114332, -3149,
        2, 0, -2, 0, 58793, 246158,
        2, -1, -1, 0, 57066, -152138,
        2, 0, 1, 0, 53322, -170733,
        2, -1, 0, 0, 45758, -204586,
        0, 1, -1, 0, -40923, -129620,
        1, 0, 0, 0, -34720, 108743,
        0, 1, 1, 0, -30383, 104755,
        2, 0, 0, -2, 15327, 10321,
        0, 0, 1, 2, -12528, 0,
        0, 0, 1, -2, 10980, 79661,
        4, 0, -1, 0, 10675, -34782,
        0, 0, 3, 0, 10034, -23210,
        4, 0, -2, 0, 8548, -21636,
        2, 1, -1, 0, -7888, 24208,
        2, 1, 0, 0, -6766, 30824,
        1, 0, -1, 0, -5163, -8379,
        1, 1, 0, 0, 4987, -16675,
        2, -1, 1, 0, 4036, -12831,
        2, 0, 2, 0, 3994, -10445,
        4, 0, 0, 0, 3861, -11650,
        2, 0, -3, 0, 3665, 14403,
        0, 1, -2, 0, -2689, -7003,
        2, 0, -1, 2, -2602, 0,
        2, -1, -2, 0, 2390, 10056,
        1, 0, 1, 0, -2348, 6322,
        2, -2, 0, 0, 2236, -9884,
        0, 1, 2, 0, -2120, 5751,
        0, 2, 0, 0, -2069, 0,
        2, -2, -1, 0, 2048, -4950,
        2, 0, 1, -2, -1773, 4130,
        2, 0, 0, 2, -1595, 0,
        4, -1, -1, 0, 1215, -3958,
        0, 0, 2, 2, -1110, 0,
        3, 0, -1, 0, -892, 3258,
        2, 1, 1, 0, -810, 2616,
        4, -1, -2, 0, 759, -1897,
        0, 2, -1, 0, -713, -2117,
        2, 2, -1, 0, -700, 2354,
        2, 1, -2, 0, 691, 0,
        2, -1, 0, -2, 596, 0,
        4, 0, 1, 0, 549, -1423,
        0, 0, 4, 0, 537, -1117,
        4, -1, 0, 0, 520, -1571,
        1, 0, -2, 0, -487, -1739,
        2, 1, 0, -2, -399, 0,
        0, 0, 2, -2, -381, -4421,
        1, 1, 1, 0, 351, 0,
        3, 0, -2, 0, -340, 0,
        4, 0, -3, 0, 330, 0,
        2, -1, 2, 0, 327, 0,
        0, 2, 1, 0, -323, 1165,
        1, 1, -1, 0, 299, 0,
        2, 0, 3, 0, 294, 0,
        2, 0, -1, -2, 0, 8752
    ];

    // Meeus table 47.B — periodic terms for the Moon's latitude (Σb, ×1e-6 deg).
    // Flat rows of 5: D, M, M', F, Σb.
    private static readonly int[] MoonLat =
    [
        0, 0, 0, 1, 5128122,
        0, 0, 1, 1, 280602,
        0, 0, 1, -1, 277693,
        2, 0, 0, -1, 173237,
        2, 0, -1, 1, 55413,
        2, 0, -1, -1, 46271,
        2, 0, 0, 1, 32573,
        0, 0, 2, 1, 17198,
        2, 0, 1, -1, 9266,
        0, 0, 2, -1, 8822,
        2, -1, 0, -1, 8216,
        2, 0, -2, -1, 4324,
        2, 0, 1, 1, 4200,
        2, 1, 0, -1, -3359,
        2, -1, -1, 1, 2463,
        2, -1, 0, 1, 2211,
        2, -1, -1, -1, 2065,
        0, 1, -1, -1, -1870,
        4, 0, -1, -1, 1828,
        0, 1, 0, 1, -1794,
        0, 0, 0, 3, -1749,
        0, 1, -1, 1, -1565,
        1, 0, 0, 1, -1491,
        0, 1, 1, 1, -1475,
        0, 1, 1, -1, -1410,
        0, 1, 0, -1, -1344,
        1, 0, 0, -1, -1335,
        0, 0, 3, 1, 1107,
        4, 0, 0, -1, 1021,
        4, 0, -1, 1, 833,
        0, 0, 1, -3, 777,
        4, 0, -2, 1, 671,
        2, 0, 0, -3, 607,
        2, 0, 2, -1, 596,
        2, -1, 1, -1, 491,
        2, 0, -2, 1, -451,
        0, 0, 3, -1, 439,
        2, 0, 2, 1, 422,
        2, 0, -3, -1, 421,
        2, 1, -1, 1, -366,
        2, 1, 0, 1, -351,
        4, 0, 0, 1, 331,
        2, -1, 1, 1, 315,
        2, -2, 0, -1, 302,
        0, 0, 1, 3, -283,
        2, 1, 1, -1, -229,
        1, 1, 0, -1, 223,
        1, 1, 0, 1, 223,
        0, 1, -2, -1, -220,
        2, 1, -1, -1, -220,
        1, 0, 1, 1, -185,
        2, -1, -2, -1, 181,
        0, 1, 2, 1, -177,
        4, 0, -2, -1, 176,
        4, -1, -1, -1, 166,
        1, 0, 1, -1, -164,
        4, 0, 1, -1, 132,
        1, 0, -1, -1, -119,
        4, -1, 0, -1, 115,
        2, -2, 0, 1, 107
    ];

    /// <summary>
    /// Geocentric apparent equatorial coordinates of the Moon, Meeus ch. 47.
    /// </summary>
    /// <param name="daysSinceJ2000Tt">Days since J2000.0 in Terrestrial Time (see <see cref="AstroTime.ToDaysTt"/>).</param>
    /// <returns>Geocentric coordinates of the Moon (right ascension, declination, distance in km).</returns>
    internal static GeocentricCoords GetGeocentricCoords(double daysSinceJ2000Tt)
    {
        var t = daysSinceJ2000Tt / 36525;

        // fundamental arguments (degrees), 47.1–47.6
        var lp = 218.3164477 + t * (481267.88123421 + t * (-0.0015786 + t * (1.0 / 538841 - t / 65194000)));
        var d = 297.8501921 + t * (445267.1114034 + t * (-0.0018819 + t * (1.0 / 545868 - t / 113065000)));
        var m = 357.5291092 + t * (35999.0502909 + t * (-0.0001536 + t / 24490000));
        var mp = 134.9633964 + t * (477198.8675055 + t * (0.0087414 + t * (1.0 / 69699 - t / 14712000)));
        var f = 93.2720950 + t * (483202.0175233 + t * (-0.0036539 + t * (-1.0 / 3526000 + t / 863310000)));
        var a1 = 119.75 + 131.849 * t;
        var a2 = 53.09 + 479264.290 * t;
        var a3 = 313.45 + 481266.484 * t;
        var e = 1 - t * (0.002516 + t * 0.0000074); // eccentricity factor for solar-anomaly terms

        var dr = Constants.Rad * d;
        var mr = Constants.Rad * m;
        var mpr = Constants.Rad * mp;
        var fr = Constants.Rad * f;

        double sl = 0, sr = 0, sb = 0;
        for (var i = 0; i < MoonLon.Length; i += 6)
        {
            var mm = MoonLon[i + 1];
            var arg = MoonLon[i] * dr + mm * mr + MoonLon[i + 2] * mpr + MoonLon[i + 3] * fr;
            var factor = mm == 1 || mm == -1 ? e : mm == 2 || mm == -2 ? e * e : 1.0;
            sl += MoonLon[i + 4] * factor * Math.Sin(arg);
            sr += MoonLon[i + 5] * factor * Math.Cos(arg);
        }

        for (var i = 0; i < MoonLat.Length; i += 5)
        {
            var mm = MoonLat[i + 1];
            var arg = MoonLat[i] * dr + mm * mr + MoonLat[i + 2] * mpr + MoonLat[i + 3] * fr;
            var factor = mm == 1 || mm == -1 ? e : mm == 2 || mm == -2 ? e * e : 1.0;
            sb += MoonLat[i + 4] * factor * Math.Sin(arg);
        }

        // additive terms (Venus, Jupiter and flattening of the Earth), 47 p.342
        var a1R = Constants.Rad * a1;
        var lpr = Constants.Rad * lp;
        sl += 3958 * Math.Sin(a1R) + 1962 * Math.Sin(lpr - fr) + 318 * Math.Sin(Constants.Rad * a2);
        sb += -2235 * Math.Sin(lpr) + 382 * Math.Sin(Constants.Rad * a3) + 175 * Math.Sin(a1R - fr) + 175 * Math.Sin(a1R + fr) + 127 * Math.Sin(lpr - mpr) - 115 * Math.Sin(lpr + mpr);

        var nutation = Nutation.LongitudeAndObliquity(t);
        var l = Constants.Rad * (lp + sl / 1e6 + nutation.DeltaPsi); // apparent ecliptic longitude
        var b = Constants.Rad * (sb / 1e6);                          // ecliptic latitude

        var ra = Position.GetRightAscension(l, b, nutation.Obliquity); // 13.3
        var declination = Position.GetDeclination(l, b, nutation.Obliquity); // 13.4
        var dist = 385000.56 + sr / 1000; // distance to the Moon in km

        return new GeocentricCoords(ra, declination, dist);
    }
}