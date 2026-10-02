SunCalc-Net
============

[![build](https://github.com/kostebudinoski/SunCalcNet/actions/workflows/master_build.yml/badge.svg)](https://github.com/kostebudinoski/SunCalcNet/actions/workflows/master_build.yml)
[![NuGet](https://img.shields.io/nuget/v/SunCalcNet)](https://www.nuget.org/packages/SunCalcNet/)
[![Live demo](https://img.shields.io/badge/live%20demo-Sky%20Almanac-e0952b)](https://kostebudinoski.dev/SunCalcNet/)

A .NET port of the [SunCalc JS lib](https://github.com/mourner/suncalc) for calculating sun/moon positions and phases.

> 🌅 **[See it in action: Sky Almanac →](https://kostebudinoski.dev/SunCalcNet/)**
>
> When is golden hour tomorrow? Where will the full moon rise? When is the sky dark enough for the Milky Way?
> Pick any place on Earth and find out, with sunrise to sunset, blue and golden hours, moonrise and transits, the
> year's earliest sunset and tonight's dark-sky window, plus a one-click "add to calendar". It's all calculated
> by SunCalcNet itself, running in your browser with no server behind it.

Getting Started
============

The best way to get started is to:

- Add a Nuget dependency to [SunCalcNet](https://www.nuget.org/packages/SunCalcNet/).
- Use SunCalc and MoonCalc class methods. 

Usage example
==========

All methods take a `DateTimeOffset`. Positions and illumination are for that instant. Sun and moon phases are for
the observer's **local calendar day** containing it, and every returned time is at the same UTC offset.

Get position of the sun (azimuth and altitude)
```csharp
var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.Zero);
var lat = 50.5;
var lng = 30.5;

var sunPosition = SunCalc.GetSunPosition(date, lat, lng);

Assert.Equal(-2.4967445445669547, sunPosition.Azimuth, 14);
Assert.Equal(-0.6888030343391054, sunPosition.Altitude, 14);
```
Get position of the moon (azimuth, altitude, distance and parallactic angle)
```csharp
var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.Zero);
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
// any time on 5 March in Kyiv (UTC+2); every returned PhaseTime is at +02:00
var date = new DateTimeOffset(2013, 3, 5, 18, 30, 0, TimeSpan.FromHours(2));
var lat = 50.5;
var lng = 30.5;

var sunPhases = SunCalc.GetSunPhases(date, lat, lng).ToList();

foreach (var sunPhase in sunPhases)
{
    Console.WriteLine($"{sunPhase.Name}: {sunPhase.PhaseTime:HH:mm zzz}"); // Sunrise: 06:33 +02:00 ...
}
```
Get Sun phases with custom angles

Define your own phase angles (in degrees relative to the horizon) and pass them to `GetSunPhases`. Use `SunPhaseAngle.Default` to compose your angles with the built-in ones.
```csharp
var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.Zero);
var lat = 50.5;
var lng = 30.5;

// Only your own phases (plus solar noon and nadir, which are always included)
var customAngles = new[] { new SunPhaseAngle(-4, "blueHourDawn", "blueHourDusk") };

var sunPhases = SunCalc.GetSunPhases(date, lat, lng, customAngles).ToList();

// Or combine with the built-in phases
var angles = SunPhaseAngle.Default.Append(new SunPhaseAngle(-4, "blueHourDawn", "blueHourDusk"));

var allPhases = SunCalc.GetSunPhases(date, lat, lng, angles).ToList();
```
Get Moon rise, set and transit times
```csharp
var date = new DateTimeOffset(2026, 9, 30, 0, 0, 0, TimeSpan.FromHours(-4)); // 30 September in Boston (EDT)

var moonPhase = MoonCalc.GetMoonPhase(date, 42.76, -71.04);

// Rise, Set, Transit (highest point) and LowerTransit (lowest point) are DateTimeOffset? at -04:00,
// null when the event doesn't happen that day; AlwaysUp / AlwaysDown flag days without a rise or set
```
Get Moon Illumination
```csharp
var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.Zero);

var moonIllum = MoonCalc.GetMoonIllumination(date);

Assert.Equal(0.4911927817602366, moonIllum.Fraction, 12);
Assert.Equal(0.7528035696247392, moonIllum.Phase, 12);
Assert.Equal(1.6763844401987489, moonIllum.Angle, 12);
Assert.False(moonIllum.Waxing);
```

Which day, and time zones
==========

`GetSunPhases` and `GetMoonPhase` return the events of the calendar day that `date` falls on at its own offset,
whatever its time of day: 00:01 and 23:59 give the same day. Use the `TimeZoneInfo` overloads when the
observer's zone has daylight saving time. They cover the whole local day, which is 23 or 25 hours long when
the clocks change, and each returned time carries the zone's offset at that moment:

```csharp
var vienna = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
var date = new DateTimeOffset(2026, 10, 25, 12, 0, 0, TimeSpan.FromHours(1)); // the day the clocks go back

var sunPhases = SunCalc.GetSunPhases(date, 48.21, 16.37, vienna);
var moonPhase = MoonCalc.GetMoonPhase(date, 48.21, 16.37, vienna);
```

Upgrading from 2.x
==========

3.0 is a breaking release: sun and moon phases take a `DateTimeOffset` and return `DateTimeOffset` times.
Calls that pass a `DateTime` to `GetSunPhases` or `GetMoonPhase` no longer compile, so every place that needs a
decision is flagged. See [Migrating to 3.0](https://github.com/kostebudinoski/SunCalcNet/wiki/Migrating-to-3.0).

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
