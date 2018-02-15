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

            var sunCoords = Sun.GetEquatorialCoords(d);
            var h = Position.GetSiderealTime(d, lw) - sunCoords.RightAscension;

            var azimuth = Position.GetAzimuth(h, phi, sunCoords.Declination);
            var altitude = Position.GetAltitude(h, phi, sunCoords.Declination);

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

            var moonCoords = Moon.GetGeocentricCoords(d);
            var h = Position.GetSiderealTime(d, lw) - moonCoords.RightAscension;
            var hAltitude = Position.GetAltitude(h, phi, moonCoords.Declination);

            // formula 14.1 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            var pa = Math.Atan2(Math.Sin(h), Math.Tan(phi) * Math.Cos(moonCoords.Declination) - Math.Sin(moonCoords.Declination) * Math.Cos(h));

            // altitude correction for refraction
            hAltitude = hAltitude + Position.GetAstroRefraction(hAltitude);

            var azimuth = Position.GetAzimuth(h, phi, moonCoords.Declination);

            return new MoonPosition(azimuth, hAltitude, moonCoords.Distance, pa);
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

        /// <summary>
        /// Calculates illumination parameters of the moon.
        /// Location is not needed because percentage will be the same for both Northern and Southern hemisphere.
        /// Based on http://idlastro.gsfc.nasa.gov/ftp/pro/astro/mphase.pro formulas and
        /// Chapter 48 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static MoonIllumination GetMoonIllumination(DateTime date)
        {
            var d = date.ToDays();
            const int sdist = 149598000; // distance from Earth to Sun in km
            var sunCoords = Sun.GetEquatorialCoords(d);
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
        /// <param name="date"></param>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        public static MoonPhase GetMoonPhase(DateTime date, double lat, double lng)
        {
            const double hc = 0.133 * Constants.Rad;
            var h0 = GetMoonPosition(date, lat, lng).Altitude - hc;
            double? rise = null;
            double? set = null;
            double ye = 0;
            
            // go in 2-hour chunks,
            // each time seeing if a 3-point quadratic curve crosses zero (which means rise or set)
            for (var i = 1; i <= 24; i += 2)
            {
                var h1 = GetMoonPosition(date.HoursLater(i), lat, lng).Altitude - hc;
                var h2 = GetMoonPosition(date.HoursLater(i + 1), lat, lng).Altitude - hc;

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

            var moonPhase = new MoonPhase();
            if (rise.HasValue)
            {
                moonPhase.Rise = date.HoursLater(rise.Value);
            }

            if (set.HasValue)
            {
                moonPhase.Set = date.HoursLater(set.Value);
            }

            if (rise.HasValue || set.HasValue)
            {
                return moonPhase;
            }
            
            moonPhase.AlwaysUp = ye > 0;
            moonPhase.AlwaysDown = !moonPhase.AlwaysUp;
            return moonPhase;
        }
    }
}