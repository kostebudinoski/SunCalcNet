using SunCalcNet.Internal;
using SunCalcNet.Model;
using SunCalcNet.Utils;
using System;

namespace SunCalcNet;

public static class MoonCalc
{
    /// <summary>
    /// Calculates moon position for a given date and latitude/longitude.
    /// </summary>
    /// <param name="date">The date and time to calculate the moon position for.</param>
    /// <param name="lat">The observer latitude in degrees.</param>
    /// <param name="lng">The observer longitude in degrees.</param>
    /// <returns>The moon position for the given date and location.</returns>
    public static MoonPosition GetMoonPosition(DateTime date, double lat, double lng)
    {
        var lw = Constants.Rad * -lng;
        var phi = Constants.Rad * lat;
        var moonPositionCalculation = GetMoonPositionCalculation(date, lw, phi);
        var moonCoords = moonPositionCalculation.MoonCoords;
        var h = moonPositionCalculation.HourAngle;

        // formula 14.1 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
        var pa = Math.Atan2(Math.Sin(h), Math.Tan(phi) * Math.Cos(moonCoords.Declination) - Math.Sin(moonCoords.Declination) * Math.Cos(h));

        var azimuth = Position.GetAzimuth(h, phi, moonCoords.Declination);

        return new MoonPosition(azimuth, moonPositionCalculation.ApparentAltitude, moonCoords.Distance, pa);
    }

    /// <summary>
    /// Calculates illumination parameters of the moon.
    /// Location is not needed because percentage will be the same for both Northern and Southern hemisphere.
    /// Based on http://idlastro.gsfc.nasa.gov/ftp/pro/astro/mphase.pro formulas and
    /// Chapter 48 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
    /// </summary>
    /// <param name="date">The date and time to calculate moon illumination for.</param>
    /// <returns>The moon illumination parameters for the given date.</returns>
    public static MoonIllumination GetMoonIllumination(DateTime date)
    {
        var d = AstroTime.ToDaysTt(date.ToDaysSinceJ2000());
        const int sdist = 149598000; // distance from Earth to Sun in km
        var sunCoords = Sun.GetApparentEquatorialCoords(d);
        var moonCoords = Moon.GetGeocentricCoords(d);

        var phi = Math.Acos(Math.Sin(sunCoords.Declination) * Math.Sin(moonCoords.Declination) +
                            Math.Cos(sunCoords.Declination) * Math.Cos(moonCoords.Declination) *
                            Math.Cos(sunCoords.RightAscension - moonCoords.RightAscension));

        var inc = Math.Atan2(sdist * Math.Sin(phi), moonCoords.Distance - sdist * Math.Cos(phi));

        var angle = Math.Atan2(
            Math.Cos(sunCoords.Declination) * Math.Sin(sunCoords.RightAscension - moonCoords.RightAscension),
            Math.Sin(sunCoords.Declination) * Math.Cos(moonCoords.Declination) -
            Math.Cos(sunCoords.Declination) * Math.Sin(moonCoords.Declination) *
            Math.Cos(sunCoords.RightAscension - moonCoords.RightAscension));

        var fraction = (1 + Math.Cos(inc)) / 2;
        var phase = 0.5 + 0.5 * inc * (angle < 0 ? -1 : 1) / Math.PI;

        return new MoonIllumination(fraction, phase, angle);
    }

    /// <summary>
    /// Calculates phases of the moon for a single day and latitude/longitude.
    /// Calculations for moon rise/set times are based on http://www.stargazing.net/kepler/moonrise.html article.
    /// </summary>
    /// <param name="date">The date to calculate moon rise and set times for.</param>
    /// <param name="lat">The observer latitude in degrees.</param>
    /// <param name="lng">The observer longitude in degrees.</param>
    public static MoonPhase GetMoonPhase(DateTime date, double lat, double lng)
    {
        date = date.Add(-date.TimeOfDay);

        var lw = Constants.Rad * -lng;
        var phi = Constants.Rad * lat;
        var h0 = GetMoonHeight(date, lw, phi);
        double? rise = null;
        double? set = null;
        double ye = 0;

        // go in 2-hour chunks,
        // each time seeing if a 3-point quadratic curve crosses zero (which means rise or set)
        for (var i = 1; i <= 24; i += 2)
        {
            var h1 = GetMoonHeight(date.HoursLater(i), lw, phi);
            var h2 = GetMoonHeight(date.HoursLater(i + 1), lw, phi);

            var a = (h0 + h2) / 2 - h1;
            var b = (h2 - h0) / 2;
            var xe = -b / (2 * a);
            ye = (a * xe + b) * xe + h1;
            var d = b * b - 4 * a * h1;
            var roots = 0;
            double x1 = 0;
            double x2 = 0;

            if (d >= 0)
            {
                var dx = Math.Sqrt(d) / (Math.Abs(a) * 2);
                x1 = xe - dx;
                x2 = xe + dx;
                if (Math.Abs(x1) <= 1)
                {
                    roots++;
                }

                if (Math.Abs(x2) <= 1)
                {
                    roots++;
                }

                if (x1 < -1)
                {
                    x1 = x2;
                }
            }

            if (roots == 1)
            {
                if (h0 < 0)
                {
                    rise = i + x1;
                }
                else
                {
                    set = i + x1;
                }
            }
            else if (roots == 2)
            {
                rise = i + (ye < 0 ? x2 : x1);
                set = i + (ye < 0 ? x1 : x2);
            }

            if (rise.HasValue && set.HasValue)
            {
                break;
            }

            h0 = h2;
        }
            
        // Newton-refine each crossing against the real moon-height curve (the quadratic sampler's
        // parabola root can sit ~0.2° off), then convert fractional hours to an absolute time.
        var riseTime = rise.HasValue ? RefineMoonCross(date.HoursLater(rise.Value), lw, phi) : (DateTime?) null;
        var setTime = set.HasValue ? RefineMoonCross(date.HoursLater(set.Value), lw, phi) : (DateTime?) null;

        return new MoonPhase(riseTime, setTime, ye);
    }

    /// <summary>
    /// Height of the Moon's upper limb above the rise/set horizon (radians): topocentric centre
    /// altitude plus the Moon's semidiameter (which tracks distance) plus the residual horizon
    /// refraction the model under-bends (~0.09°). Crossing zero == upper-limb rise/set (USNO convention).
    /// </summary>
    private static double GetMoonHeight(DateTime date, double lw, double phi)
    {
        var calculation = GetMoonPositionCalculation(date.ToDaysSinceJ2000(), lw, phi);
        var semidiameter = 0.2725 * Math.Asin(Constants.EarthRadius / calculation.MoonCoords.Distance);
        return calculation.ApparentAltitude + semidiameter + 0.09 * Constants.Rad;
    }

    /// <summary>
    /// Polishes a crossing time with two central-difference Newton steps (±30 s) against
    /// <see cref="GetMoonHeight"/>.
    /// </summary>
    private static DateTime RefineMoonCross(DateTime time, double lw, double phi)
    {
        for (var i = 0; i < 2; i++)
        {
            var h = GetMoonHeight(time, lw, phi);
            var dh = (GetMoonHeight(time.AddSeconds(30), lw, phi) - GetMoonHeight(time.AddSeconds(-30), lw, phi)) / 60.0;
            if (Math.Abs(dh) < 1e-12)
            {
                break; // grazing the horizon — the Newton correction is ill-conditioned
            }

            time = time.AddSeconds(-h / dh);
        }

        return time;
    }

    private static MoonPositionCalculation GetMoonPositionCalculation(DateTime date, double lw, double phi)
    {
        return GetMoonPositionCalculation(date.ToDaysSinceJ2000(), lw, phi);
    }

    private static MoonPositionCalculation GetMoonPositionCalculation(double daysSinceJ2000, double lw, double phi)
    {
        var moonCoords = Moon.GetGeocentricCoords(AstroTime.ToDaysTt(daysSinceJ2000)); // series run on TT
        var h = Position.GetSiderealTime(daysSinceJ2000, lw) - moonCoords.RightAscension; // sidereal on UT
        var geometricAltitude = Position.GetAltitude(h, phi, moonCoords.Declination);

        // geocentric parallax (Meeus ch.40) lowers the moon along its vertical circle
        var altitude = geometricAltitude - Math.Asin(Constants.EarthRadius / moonCoords.Distance * Math.Cos(geometricAltitude));

        return new MoonPositionCalculation(
            altitude + Position.GetAstroRefraction(altitude),
            moonCoords,
            h);
    }
}