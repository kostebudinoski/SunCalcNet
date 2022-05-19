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
        public static SunPosition GetSunPosition(DateTimeOffset date, double lat, double lng)
        {
            return GetSunPosition(date.UtcDateTime, lat, lng);
        }

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
            var d = date.ToDays();

            var sunCoords = Sun.GetEquatorialCoords(d);
            var h = Position.GetSiderealTime(d, lw) - sunCoords.RightAscension;

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
        public static IEnumerable<SunPhase> GetSunPhases(DateTimeOffset date, double lat, double lng, double height = 0)
        {
            var phasesDictionary = GetSunPhasesInternal(date.UtcDateTime.ToDays(), lat, lng, height);
            var sunPhaseWithOffset = new List<SunPhase>();
            foreach (var phase in phasesDictionary)
            {
                sunPhaseWithOffset.Add(new SunPhase(phase.Key, phase.Value.FromJulian(date.Offset)));
            }
            return sunPhaseWithOffset;
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
            var phasesDictionary = GetSunPhasesInternal(date.ToDays(), lat, lng, height);
            var sunPhaseCol = new List<SunPhase>();
            foreach(var phase in phasesDictionary)
            {
                sunPhaseCol.Add(new SunPhase(phase.Key, phase.Value.FromJulian()));
            }
            return sunPhaseCol;
        }

        /// <summary>
        /// Calculates phases of the sun for a single day and latitude/longitude
        /// and optionally the observer height (in meters) relative to the horizon
        /// </summary>
        /// <param name="days"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <param name="height"></param>
        /// <returns>A dictionary with phase name, and julian time</returns>
        private static Dictionary<SunPhaseName, double> GetSunPhasesInternal(double days, double lat, double lng, double height)
        {
            var lw = Constants.Rad * -lng;
            var phi = Constants.Rad * lat;

            var dh = SunTime.GetObserverAngle(height);

            var d = days;

            var n = SunTime.GetJulianCycle(d, lw);
            var ds = SunTime.GetApproxTransit(0, lw, n);

            var m = Sun.GetMeanAnomaly(ds);
            var l = Sun.GetEclipticLongitude(m);
            var dec = Position.GetDeclination(l, 0);

            var jnoon = SunTime.GetSolarTransitJ(ds, m, l);
            var solarNoon = jnoon;
            var nadir = (jnoon - 0.5);

            var phasesDictionary = new Dictionary<SunPhaseName, double>
            {
                [SunPhaseName.SolarNoon] = solarNoon,
                [SunPhaseName.Nadir] = nadir
            };

            foreach (var sunPhase in SunPhaseAngle.List)
            {
                var h0 = (sunPhase.Angle + dh) * Constants.Rad;
                var jset = SunTime.GetSetJ(h0, lw, phi, dec, n, m, l);

                if (double.IsNaN(jset) || double.IsInfinity(jset))
                {
                    continue;
                }

                var jrise = jnoon - (jset - jnoon);
                phasesDictionary.Add(sunPhase.RiseName, jrise);
                phasesDictionary.Add(sunPhase.SetName, jset);
            }

            return phasesDictionary;

        }
    }
}