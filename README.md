SunCalc-Net
============

A .NET port of the [SunCalc JS lib](https://github.com/mourner/suncalc) for calculating sun/moon positions and phases.

Usage example
==========

Get position of the sun (azimuth and altitude)
```csharp
var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
var lat = 50.5;
var lng = 30.5;

var sunPosition = SunCalc.GetSunPosition(date, lat, lng);

Assert.Equal(-2.5003175907168385, sunPosition.Azimuth, 15);
Assert.Equal(-0.7000406838781611, sunPosition.Altitude, 15);
```
Get position of the moon (azimuth, altitude, distance and parallactic angle)
```csharp
var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
var lat = 50.5;
var lng = 30.5;

var sunPosition = SunCalc.GetMoonPosition(date, lat, lng);

Assert.Equal(-0.9783999522438226, sunPosition.Azimuth, 15);
Assert.Equal(0.0145514822438922, sunPosition.Altitude, 15);
Assert.Equal(364121.37256256194, sunPosition.Distance, 15);
Assert.Equal(-0.59832117604234014, sunPosition.ParallacticAngle, 15);
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
Get Moon Illumination
```csharp
var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);

var moonIllum = SunCalc.GetMoonIllumination(date);

Assert.Equal(0.4848068202456373, moonIllum.Fraction, 15);
Assert.Equal(0.7548368838538762, moonIllum.Phase, 15);
Assert.Equal(1.6732942678578346, moonIllum.Angle, 15);
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
