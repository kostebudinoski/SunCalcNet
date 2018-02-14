using System;
using System.Collections.Generic;
using SunCalc.Internal;
using SunCalc.Model;
using SunCalc.Utils;

namespace SunCalc
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
            var d = date.ToDays();

            var equatorialCoords = Sun.GetEquatorialCoords(d);
            var h = Position.GetSiderealTime(d, lw) - equatorialCoords.RightAscension;

            var azimuth = Position.GetAzimuth(h, phi, equatorialCoords.Declination);
            var altitude = Position.GetAltitude(h, phi, equatorialCoords.Declination);

            return new SunPosition(azimuth, altitude);
        }

        /// <summary>
        /// Calculates moon position for a given date and latitude/longitude.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns></returns>
        public static MoonPosition GetMoonPosition(DateTime date, double lat, double lng)
        {
            var lw = Constants.Rad * -lng;
            var phi = Constants.Rad * lat;
            var d = date.ToDays();

            var geocentricCoords = Moon.GetGeocentricCoords(d);
            var h = Position.GetSiderealTime(d, lw) - geocentricCoords.RightAscension;
            var hAltitude = Position.GetAltitude(h, phi, geocentricCoords.Declination);

            // formula 14.1 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            var pa = Math.Atan2(Math.Sin(h), Math.Tan(phi) * Math.Cos(geocentricCoords.Declination) - Math.Sin(geocentricCoords.Declination) * Math.Cos(h));

            // altitude correction for refraction
            hAltitude = hAltitude + Position.GetAstroRefraction(hAltitude);

            var azimuth = Position.GetAzimuth(h, phi, geocentricCoords.Declination);

            return new MoonPosition(azimuth, hAltitude, geocentricCoords.Distance, pa);
        }

        /// <summary>
        /// Calculates phases of the sun for a single day and latitude/longitude.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns></returns>
        public static IEnumerable<SunPhase> GetSunPhases(DateTime date, double lat, double lng)
        {
            var lw = Constants.Rad * -lng;
            var phi = Constants.Rad * lat;
            var d = date.ToDays();

            var n = SunTime.GetJulianCycle(d, lw);
            var ds = SunTime.GetApproxTransit(0, lw, n);

            var m = Sun.GetMeanAnomaly(ds);
            var l = Sun.GetEclipticLongitude(m);
            var dec = Position.GetDeclination(l, 0);

            var jnoon = SunTime.GetSolarTransitJ(ds, m, l);
            var solarNoon = jnoon.FromJulian();
            var nadir = (jnoon - 0.5).FromJulian();
            
            var sunPhaseCol = new List<SunPhase>
            {
                new SunPhase(SunPhaseName.SolarNoon, solarNoon), 
                new SunPhase(SunPhaseName.Nadir, nadir)
            };

            foreach (var sunPhase in SunPhaseAngle.GetAll())
            {
                var jset = SunTime.GetSetJ(sunPhase.Angle * Constants.Rad, lw, phi, dec, n, m, l);
                var jrise = jnoon - (jset - jnoon);

                sunPhaseCol.Add(new SunPhase(sunPhase.RiseName, jrise.FromJulian()));
                sunPhaseCol.Add(new SunPhase(sunPhase.SetName, jset.FromJulian()));
            }

            return sunPhaseCol;
        }
    }
}