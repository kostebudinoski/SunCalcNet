using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace SunCalcNet.Demo.Services;

public enum WeatherKind
{
    Clear,
    PartlyCloudy,
    Cloudy,
    Fog,
    Rain,
    Snow,
    Storm,
}

/// <summary>One day's weather for a place, with hourly cloud cover in the place's local time.</summary>
public sealed record DayWeather(
    string Summary,
    WeatherKind Kind,
    double MinTemperature,
    double MaxTemperature,
    int? RainChance,
    TimeSpan Sunshine,
    IReadOnlyDictionary<DateTime, int> HourlyCloudCover)
{
    /// <summary>Average cloud cover (%) over a local time window, or null when no hour falls in it.</summary>
    public int? CloudCoverDuring(DateTime localStart, DateTime localEnd)
    {
        // hours that overlap the window, weighted equally; a short window still uses the hour it falls in
        var hours = HourlyCloudCover
            .Where(x => x.Key < localEnd && x.Key.AddHours(1) > localStart)
            .Select(x => x.Value)
            .ToList();
        return hours.Count == 0 ? null : (int)Math.Round(hours.Average());
    }
}

/// <summary>
/// Weather from Open-Meteo (open-meteo.com): free, no API key, callable from the browser. Forecasts cover about the
/// last three months and the next 16 days; other dates return null.
/// </summary>
public sealed class WeatherService(HttpClient http)
{
    private readonly Dictionary<string, DayWeather?> cache = [];

    public async Task<DayWeather?> GetAsync(Place place, DateOnly date)
    {
        var key = string.Create(CultureInfo.InvariantCulture, $"{place.Lat:0.###},{place.Lng:0.###},{date:yyyy-MM-dd}");
        if (cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        const string fields = "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,sunshine_duration" +
            "&hourly=cloud_cover";
        var url = string.Create(CultureInfo.InvariantCulture,
            $"https://api.open-meteo.com/v1/forecast?latitude={place.Lat:0.####}&longitude={place.Lng:0.####}&timezone={Uri.EscapeDataString(place.TimeZoneId)}&start_date={date:yyyy-MM-dd}&end_date={date:yyyy-MM-dd}") + fields;

        DayWeather? result = null;
        try
        {
            var response = await http.GetAsync(url);
            if (response.IsSuccessStatusCode && await response.Content.ReadFromJsonAsync<Response>() is { Daily: { } daily, Hourly: { } hourly }
                && daily.WeatherCode is [{ } code, ..])
            {
                var (summary, kind) = Describe(code);
                var clouds = hourly.Time
                    .Zip(hourly.CloudCover, (t, c) => (Time: DateTime.Parse(t, CultureInfo.InvariantCulture), Cover: c))
                    .Where(x => x.Cover.HasValue)
                    .ToDictionary(x => x.Time, x => x.Cover!.Value);
                result = new DayWeather(summary, kind, daily.MinTemperature[0] ?? double.NaN, daily.MaxTemperature[0] ?? double.NaN,
                    daily.RainChance is [{ } rain, ..] ? rain : null, TimeSpan.FromSeconds(daily.Sunshine is [{ } s, ..] ? s : 0), clouds);
            }
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            // offline or blocked: the page simply shows no weather
        }

        cache[key] = result;
        return result;
    }

    // WMO weather interpretation codes, as used by Open-Meteo
    private static (string, WeatherKind) Describe(int code) => code switch
    {
        0 => ("Clear sky", WeatherKind.Clear),
        1 => ("Mainly clear", WeatherKind.Clear),
        2 => ("Partly cloudy", WeatherKind.PartlyCloudy),
        3 => ("Overcast", WeatherKind.Cloudy),
        45 or 48 => ("Fog", WeatherKind.Fog),
        51 or 53 or 55 or 56 or 57 => ("Drizzle", WeatherKind.Rain),
        61 or 63 or 66 => ("Rain", WeatherKind.Rain),
        65 or 67 => ("Heavy rain", WeatherKind.Rain),
        71 or 73 or 75 or 77 => ("Snow", WeatherKind.Snow),
        80 or 81 or 82 => ("Rain showers", WeatherKind.Rain),
        85 or 86 => ("Snow showers", WeatherKind.Snow),
        95 or 96 or 99 => ("Thunderstorms", WeatherKind.Storm),
        _ => ("Mixed weather", WeatherKind.PartlyCloudy),
    };

    private sealed record Response(
        [property: JsonPropertyName("daily")] Daily? Daily,
        [property: JsonPropertyName("hourly")] Hourly? Hourly);

    private sealed record Daily(
        [property: JsonPropertyName("weather_code")] int?[] WeatherCode,
        [property: JsonPropertyName("temperature_2m_max")] double?[] MaxTemperature,
        [property: JsonPropertyName("temperature_2m_min")] double?[] MinTemperature,
        [property: JsonPropertyName("precipitation_probability_max")] int?[]? RainChance,
        [property: JsonPropertyName("sunshine_duration")] double?[]? Sunshine);

    private sealed record Hourly(
        [property: JsonPropertyName("time")] string[] Time,
        [property: JsonPropertyName("cloud_cover")] int?[] CloudCover);
}
