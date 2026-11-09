using SunCalcNet.Internal;
using SunCalcNet.Model;
using SunCalcNet.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SunCalcNet;

public static class SunCalc
{
    /// <summary>
    /// Calculates sun position for a given instant and latitude/longitude.
    /// </summary>
    /// <param name="date">The instant to calculate the sun position for.</param>
    /// <param name="lat">The observer latitude in degrees.</param>
    /// <param name="lng">The observer longitude in degrees.</param>
    /// <returns>The sun's azimuth and apparent altitude, in radians.</returns>
    public static SunPosition GetSunPosition(DateTimeOffset date, double lat, double lng)
    {
        var lw = Constants.Rad * -lng;
        var phi = Constants.Rad * lat;
        var daysSinceJ2000 = date.ToDaysSinceJ2000();

        // position series run on Terrestrial Time; sidereal time stays on UT
        var sunCoords = Sun.GetApparentEquatorialCoords(AstroTime.ToDaysTt(daysSinceJ2000));
        var h = Position.GetSiderealTime(daysSinceJ2000, lw) - sunCoords.RightAscension;

        var azimuth = Position.GetAzimuth(h, phi, sunCoords.Declination);
        var altitude = Position.GetAltitude(h, phi, sunCoords.Declination);

        // apparent (refraction-corrected) altitude, radians
        return new SunPosition(azimuth, altitude + Position.GetAstroRefraction(altitude));
    }

    /// <summary>
    /// Calculates the sun phases of the local calendar day containing <paramref name="date"/>, at its own UTC offset,
    /// using the built-in <see cref="SunPhaseAngle.Default"/> phase angles.
    /// The time of day doesn't matter, and every returned time is at <paramref name="date"/>'s offset.
    /// </summary>
    /// <param name="date">Any time on the day, with the observer's UTC offset.</param>
    /// <param name="lat">The observer latitude in degrees.</param>
    /// <param name="lng">The observer longitude in degrees.</param>
    /// <param name="height">The observer height above the horizon in meters.</param>
    public static IEnumerable<SunPhase> GetSunPhases(DateTimeOffset date, double lat, double lng, double height = 0)
    {
        return GetSunPhases(date, lat, lng, SunPhaseAngle.Default, height);
    }

    /// <summary>
    /// Calculates the sun phases of the local calendar day containing <paramref name="date"/>, at its own UTC offset,
    /// using the supplied phase angles. Solar noon and nadir are always included. Pass <see cref="SunPhaseAngle.Default"/>
    /// for the built-in set, or compose custom angles, e.g. <c>SunPhaseAngle.Default.Append(myAngle)</c>.
    /// </summary>
    /// <param name="date">Any time on the day, with the observer's UTC offset.</param>
    /// <param name="lat">The observer latitude in degrees.</param>
    /// <param name="lng">The observer longitude in degrees.</param>
    /// <param name="phaseAngles">The sun phase angles to calculate rise/set events for.</param>
    /// <param name="height">The observer height above the horizon in meters.</param>
    public static IEnumerable<SunPhase> GetSunPhases(DateTimeOffset date, double lat, double lng, IEnumerable<SunPhaseAngle> phaseAngles, double height = 0)
    {
        return GetSunPhases(CivilDay.FromOffset(date), lat, lng, phaseAngles, height);
    }

    /// <summary>
    /// Calculates the sun phases of the calendar day <paramref name="date"/> falls on in <paramref name="timeZone"/>,
    /// using the built-in <see cref="SunPhaseAngle.Default"/> phase angles. Every returned time carries the zone's
    /// UTC offset at that moment, so days with a daylight saving change are reported correctly.
    /// </summary>
    /// <param name="date">Any instant on the day in <paramref name="timeZone"/>.</param>
    /// <param name="lat">The observer latitude in degrees.</param>
    /// <param name="lng">The observer longitude in degrees.</param>
    /// <param name="timeZone">The observer's time zone.</param>
    /// <param name="height">The observer height above the horizon in meters.</param>
    public static IEnumerable<SunPhase> GetSunPhases(DateTimeOffset date, double lat, double lng, TimeZoneInfo timeZone, double height = 0)
    {
        return GetSunPhases(date, lat, lng, SunPhaseAngle.Default, timeZone, height);
    }

    /// <summary>
    /// Calculates the sun phases of the calendar day <paramref name="date"/> falls on in <paramref name="timeZone"/>,
    /// using the supplied phase angles. Every returned time carries the zone's UTC offset at that moment.
    /// </summary>
    /// <param name="date">Any instant on the day in <paramref name="timeZone"/>.</param>
    /// <param name="lat">The observer latitude in degrees.</param>
    /// <param name="lng">The observer longitude in degrees.</param>
    /// <param name="phaseAngles">The sun phase angles to calculate rise/set events for.</param>
    /// <param name="timeZone">The observer's time zone.</param>
    /// <param name="height">The observer height above the horizon in meters.</param>
    public static IEnumerable<SunPhase> GetSunPhases(DateTimeOffset date, double lat, double lng, IEnumerable<SunPhaseAngle> phaseAngles, TimeZoneInfo timeZone, double height = 0)
    {
        return GetSunPhases(CivilDay.FromTimeZone(date, timeZone), lat, lng, phaseAngles, height);
    }

    // Removed in 3.0. These stubs turn old DateTime calls into a compile error; without them a DateTime would
    // silently convert to DateTimeOffset and pick a different day than 2.x did.

    [Obsolete(Constants.DateTimeOverloadRemoved, error: true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IEnumerable<SunPhase> GetSunPhases(DateTime date, double lat, double lng, double height = 0)
    {
        throw new NotSupportedException(Constants.DateTimeOverloadRemoved);
    }

    [Obsolete(Constants.DateTimeOverloadRemoved, error: true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IEnumerable<SunPhase> GetSunPhases(DateTime date, double lat, double lng, IEnumerable<SunPhaseAngle> phaseAngles, double height = 0)
    {
        throw new NotSupportedException(Constants.DateTimeOverloadRemoved);
    }

    private static IEnumerable<SunPhase> GetSunPhases(CivilDay day, double lat, double lng, IEnumerable<SunPhaseAngle> phaseAngles, double height)
    {
        if (phaseAngles is null)
        {
            throw new ArgumentNullException(nameof(phaseAngles));
        }

        var lw = Constants.Rad * -lng;
        var phi = Constants.Rad * lat;

        var dh = SunTime.GetObserverAngle(height);

        // the solar transit nearest the anchor (local noon for a civil day) is this day's solar noon;
        // round to it on the observer's meridian, then let SolarTransit refine
        var d = Math.Round(day.AnchorUtc.ToDaysSinceJ2000() - Constants.J0 - lw / (2 * Math.PI));
        var today = SunTime.GetSolarDay(d + Constants.J0 + lw / (2 * Math.PI), lw, phi);
        var dt = today.Transit;
        var dec = today.Declination;
        var sNoon = today.SinNoonAltitude;

        // sine of the Sun's altitude at the nadirs either side of transit (H = π), bracketing with sNoon every
        // rise and set; the nadirs use their own declination, which matters on regime-boundary days
        var sNadir1 = -Math.Cos(phi + Sun.GetApparentEquatorialCoords(AstroTime.ToDaysTt(dt - 0.5)).Declination);
        var sNadir2 = -Math.Cos(phi + Sun.GetApparentEquatorialCoords(AstroTime.ToDaysTt(dt + 0.5)).Declination);

        // the civil day is this solar day [dt - 0.5, dt + 0.5) shifted by up to half a day (and 23 or 25 h long on a
        // clock change), so it can also take in the evening of the previous solar day and/or the morning of the next;
        // an event this solar day puts outside the civil day is looked up there instead
        var start = day.StartUtc.ToDaysSinceJ2000();
        var end = day.EndUtc.ToDaysSinceJ2000();
        SolarDay? prev = start < dt - 0.5 ? SunTime.GetSolarDay(dt - 1, lw, phi) : null;
        SolarDay? next = end > dt + 0.5 ? SunTime.GetSolarDay(dt + 1, lw, phi) : null;

        // solar days run up to ~30 s longer than 24 h and a civil day can be 23 h, so a civil day can miss a nadir;
        // keep this solar day's one then, so solar noon and nadir are always there
        var nadir = next is { } n && n.Transit - 0.5 < end && !(dt - 0.5 >= start) ? n.Transit - 0.5 : dt - 0.5;

        var sunPhaseCol = new List<SunPhase>
        {
            new(SunPhaseName.SolarNoon, day.ToResult((dt + Constants.J2000).FromJulian())),
            new(SunPhaseName.Nadir, day.ToResult((nadir + Constants.J2000).FromJulian()))
        };

        foreach (var sunPhase in phaseAngles)
        {
            var h0 = (sunPhase.Angle + dh) * Constants.Rad;

            var jrise = SunTime.GetSetJ(h0, dt - 0.5, dt, sNadir1, sNoon, dt, lw, phi, dec);
            var jset = SunTime.GetSetJ(h0, dt, dt + 0.5, sNoon, sNadir2, dt, lw, phi, dec);

            // NaN-safe comparisons: a NaN (the Sun never reaches this altitude) is outside the day too
            if (next is { } nd && !(jrise >= start))
            {
                jrise = SunTime.GetSetJ(h0, dt + 0.5, nd.Transit, sNadir2, nd.SinNoonAltitude, nd.Transit, lw, phi, nd.Declination);
            }

            if (prev is { } pd && !(jset < end))
            {
                jset = SunTime.GetSetJ(h0, pd.Transit, dt - 0.5, pd.SinNoonAltitude, sNadir1, pd.Transit, lw, phi, pd.Declination);
            }

            if (jrise >= start && jrise < end)
            {
                sunPhaseCol.Add(new SunPhase(sunPhase.RiseName, day.ToResult((jrise + Constants.J2000).FromJulian())));
            }

            if (jset >= start && jset < end)
            {
                sunPhaseCol.Add(new SunPhase(sunPhase.SetName, day.ToResult((jset + Constants.J2000).FromJulian())));
            }
        }

        return sunPhaseCol;
    }
}
