using SunCalcNet.Internal;
using SunCalcNet.Model;
using SunCalcNet.Utils;
using System;
using System.Collections.Generic;

namespace SunCalcNet
{
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

            var sunCoords = Sun.GetEquatorialCoords(daysSinceJ2000);
            var h = Position.GetSiderealTime(daysSinceJ2000, lw) - sunCoords.RightAscension;

            var azimuth = Position.GetAzimuth(h, phi, sunCoords.Declination);
            var altitude = Position.GetAltitude(h, phi, sunCoords.Declination);

            return new SunPosition(azimuth, altitude);
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

            var n = SunTime.GetJulianCycle(daysSinceJ2000, lw);
            var approxTransitDaysSinceJ2000 = SunTime.GetApproxTransit(0, lw, n);

            var m = Sun.GetMeanAnomaly(approxTransitDaysSinceJ2000);
            var l = Sun.GetEclipticLongitude(m);
            var dec = Position.GetDeclination(l, 0);

            var jnoon = SunTime.GetSolarTransitJ(approxTransitDaysSinceJ2000, m, l);
            var solarNoon = jnoon.FromJulian();
            var nadir = (jnoon - 0.5).FromJulian();

            var sunPhaseCol = new List<SunPhase>(2 + SunPhaseAngle.Count * 2)
            {
                new(SunPhaseName.SolarNoon, solarNoon),
                new(SunPhaseName.Nadir, nadir)
            };

            for (var i = 0; i < SunPhaseAngle.Count; i++)
            {
                var sunPhase = SunPhaseAngle.GetAt(i);
                var h0 = (sunPhase.Angle + dh) * Constants.Rad;
                var jset = SunTime.GetSetJ(h0, lw, phi, dec, n, m, l);

                if (double.IsNaN(jset) || double.IsInfinity(jset))
                {
                    continue;
                }

                var jrise = jnoon - (jset - jnoon);
                sunPhaseCol.Add(new SunPhase(sunPhase.RiseName, jrise.FromJulian()));
                sunPhaseCol.Add(new SunPhase(sunPhase.SetName, jset.FromJulian()));
            }

            return sunPhaseCol;
        }
    }
}