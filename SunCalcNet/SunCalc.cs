using SunCalcNet.Internal;
using SunCalcNet.Model;
using SunCalcNet.Utils;
using System;
using System.Collections.Generic;

namespace SunCalcNet;

public static class SunCalc
{
    /// <summary>
    /// Calculates sun position for a given date and latitude/longitude.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="lat"></param>
    /// <param name="lng"></param>
    /// <returns></returns>
    public static SunPosition GetSunPosition(DateTime date, double lat, double lng)
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
    /// Calculates phases of the sun for a single day and latitude/longitude
    /// and optionally the observer height (in meters) relative to the horizon
    /// </summary>
    /// <param name="date"></param>
    /// <param name="lat"></param>
    /// <param name="lng"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static IEnumerable<SunPhase> GetSunPhases(DateTime date, double lat, double lng, double height = 0)
    {
        var lw = Constants.Rad * -lng;
        var phi = Constants.Rad * lat;

        var dh = SunTime.GetObserverAngle(height);

        var daysSinceJ2000 = date.ToDaysSinceJ2000();

        // Anchor to the input date's UTC solar day regardless of its time-of-day: round to that
        // day's noon, offset to the nearest local solar noon, then let SolarTransit refine.
        var d = Math.Round(Math.Round(daysSinceJ2000) - Constants.J0 - lw / (2 * Math.PI));
        var dt = SunTime.SolarTransit(d + Constants.J0 + lw / (2 * Math.PI), lw);
        var dec = Sun.GetApparentEquatorialCoords(AstroTime.ToDaysTt(dt)).Declination;

        var solarNoon = (dt + Constants.J2000).FromJulian();
        var nadir = (dt + Constants.J2000 - 0.5).FromJulian();

        var sunPhaseCol = new List<SunPhase>(2 + SunPhaseAngle.Count * 2)
        {
            new(SunPhaseName.SolarNoon, solarNoon),
            new(SunPhaseName.Nadir, nadir)
        };

        for (var i = 0; i < SunPhaseAngle.Count; i++)
        {
            var sunPhase = SunPhaseAngle.GetAt(i);
            var h0 = (sunPhase.Angle + dh) * Constants.Rad;

            var jrise = SunTime.GetSetJ(h0, dt, -1, lw, phi, dec);
            var jset = SunTime.GetSetJ(h0, dt, 1, lw, phi, dec);

            // a NaN means the Sun never reaches this altitude on this day — omit that event
            if (!double.IsNaN(jrise))
            {
                sunPhaseCol.Add(new SunPhase(sunPhase.RiseName, (jrise + Constants.J2000).FromJulian()));
            }

            if (!double.IsNaN(jset))
            {
                sunPhaseCol.Add(new SunPhase(sunPhase.SetName, (jset + Constants.J2000).FromJulian()));
            }
        }

        return sunPhaseCol;
    }
}