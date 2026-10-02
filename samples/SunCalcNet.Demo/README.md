# Sky Almanac (SunCalcNet demo)

> Vibe coded with AI. This sample was generated with an AI coding assistant and checked by running it,
> not reviewed line by line. It's a showcase of the library, not an example of production-quality Blazor code.

A Blazor WebAssembly app that runs [SunCalcNet](../../README.md) in the browser and shows, for a place and a local calendar day:

- a 24-hour ribbon coloured by the sun's altitude, with sun and moon altitude curves, phase times and a time-of-day slider
- a sky map with the sun's and moon's paths and current positions
- every `GetSunPhases` event in local time, with an explanation behind each info icon
- the moon's phase, illumination, distance, rise/set and transit times

## Run it

```
cd samples/SunCalcNet.Demo
dotnet run
```

Then open the URL it prints.

Links can open a specific place and day, e.g. `?place=Skopje&date=2026-10-02`.

## Notes

- The library returns UTC; the demo converts times to the place's own time zone for display only.
- The weather line uses [Open-Meteo](https://open-meteo.com/) (free, no API key, CC BY 4.0). It's the only outside call:
  the place's coordinates are sent to Open-Meteo, and the line is hidden when a date has no forecast or the call fails.
- The project references `SunCalcNet.csproj` directly, so it always shows the current library code.
- It's not part of `SunCalcNet.slnx`, so CI and the NuGet package are unaffected.
