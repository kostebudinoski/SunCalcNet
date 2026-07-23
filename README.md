SunCalc-Net
============

[![build](https://github.com/kostebudinoski/SunCalcNet/actions/workflows/master_build.yml/badge.svg)](https://github.com/kostebudinoski/SunCalcNet/actions/workflows/master_build.yml)

A .NET port of the [SunCalc JS lib](https://github.com/mourner/suncalc) for calculating sun/moon positions and phases.

Getting Started
============

The best way to get started is to:

- Add a Nuget dependency to [SunCalcNet](https://www.nuget.org/packages/SunCalcNet/).
- Use SunCalc and MoonCalc class methods. 

Usage example
==========

Get position of the sun (azimuth and altitude)
```csharp
var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
var lat = 50.5;
var lng = 30.5;

var sunPosition = SunCalc.GetSunPosition(date, lat, lng);

Assert.Equal(-2.4967445445669547, sunPosition.Azimuth, 14);
Assert.Equal(-0.6888030343391054, sunPosition.Altitude, 14);
```
Get position of the moon (azimuth, altitude, distance and parallactic angle)
```csharp
var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
var lat = 50.5;
var lng = 30.5;

var moonPosition = MoonCalc.GetMoonPosition(date, lat, lng);

Assert.Equal(-0.9661994436443471, moonPosition.Azimuth, 12);
Assert.Equal(0.007971096659309906, moonPosition.Altitude, 12);
Assert.Equal(370193.9925193064, moonPosition.Distance, 6);
Assert.Equal(-0.5923875457617929, moonPosition.ParallacticAngle, 12);
```
Get Sun phases
```csharp
var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
var lat = 50.5;
var lng = 30.5;

var sunPhases = SunCalc.GetSunPhases(date, lat, lng).ToList();

foreach (var sunPhase in sunPhases)
{
    ...
}
```
Get Sun phases with custom angles

Define your own phase angles (in degrees relative to the horizon) and pass them to `GetSunPhases`. Use `SunPhaseAngle.Default` to compose your angles with the built-in ones.
```csharp
var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
var lat = 50.5;
var lng = 30.5;

// Only your own phases (plus solar noon and nadir, which are always included)
var customAngles = new[] { new SunPhaseAngle(-4, "blueHourDawn", "blueHourDusk") };

var sunPhases = SunCalc.GetSunPhases(date, lat, lng, customAngles).ToList();

// Or combine with the built-in phases
var angles = SunPhaseAngle.Default.Append(new SunPhaseAngle(-4, "blueHourDawn", "blueHourDusk"));

var allPhases = SunCalc.GetSunPhases(date, lat, lng, angles).ToList();
```
Get Moon Illumination
```csharp
var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);

var moonIllum = MoonCalc.GetMoonIllumination(date);

Assert.Equal(0.4911927817602366, moonIllum.Fraction, 12);
Assert.Equal(0.7528035696247392, moonIllum.Phase, 12);
Assert.Equal(1.6763844401987489, moonIllum.Angle, 12);
Assert.False(moonIllum.Waxing);
```

About Suncalc.js
==========

SunCalc is a tiny BSD-licensed JavaScript library for calculating sun position, sunlight phases (times for sunrise, sunset, dusk, etc.), moon position and lunar phase for the given location and time, created by Vladimir Agafonkin ([@mourner](https://github.com/mourner))
as a part of the [SunCalc.net project](http://suncalc.net).

Most calculations are based on the formulas given in the excellent Astronomy Answers articles
about [position of the sun](http://aa.quae.nl/en/reken/zonpositie.html)
and [the planets](http://aa.quae.nl/en/reken/hemelpositie.html).
You can read about different twilight phases calculated by SunCalc
in the [Twilight article on Wikipedia](http://en.wikipedia.org/wiki/Twilight).

Sun phases
==========

Currently supported sun phases are:

| Phase           | Description                                                              |
| --------------- | ------------------------------------------------------------------------ |
| `Sunrise`       | sunrise (top edge of the sun appears on the horizon)                     |
| `SunriseEnd`    | sunrise ends (bottom edge of the sun touches the horizon)                |
| `GoldenHourEnd` | morning golden hour (soft light, best time for photography) ends         |
| `SolarNoon`     | solar noon (sun is in the highest position)                              |
| `GoldenHour`    | evening golden hour starts                                               |
| `SunsetStart`   | sunset starts (bottom edge of the sun touches the horizon)               |
| `Sunset`        | sunset (sun disappears below the horizon, evening civil twilight starts) |
| `Dusk`          | dusk (evening nautical twilight starts)                                  |
| `NauticalDusk`  | nautical dusk (evening astronomical twilight starts)                     |
| `Night`         | night starts (dark enough for astronomical observations)                 |
| `Nadir`         | nadir (darkest moment of the night, sun is in the lowest position)       |
| `NightEnd`      | night ends (morning astronomical twilight starts)                        |
| `NauticalDawn`  | nautical dawn (morning nautical twilight starts)                         |
| `Dawn`          | dawn (morning nautical twilight ends, morning civil twilight starts)     |
