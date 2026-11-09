using System;
using System.Linq;
using SunCalcNet.Model;
using Xunit;

namespace SunCalcNet.Tests;

public class CivilDayTests
{
    [Theory]
    [InlineData(2020, 10, 9, 56, 35, 180)]
    [InlineData(2022, 4, 14, -14.415, 128.525, 480)]
    [InlineData(2026, 9, 30, 42.764767, -71.042023, -240)]
    [InlineData(2026, 8, 19, 40, -179.7, -720)]
    [InlineData(2026, 8, 19, 40, 179.7, 720)]
    [InlineData(2026, 8, 19, 27.7, 85.3, 345)]
    [InlineData(2026, 6, 1, 42.2406, -8.7207, 120)]
    [InlineData(2026, 6, 21, 39.4704, 75.9898, 480)]
    [InlineData(2026, 7, 25, 69.6492, 18.9553, 120)]
    public void Get_Sun_And_Moon_Phases_Return_The_Calendar_Day_Of_The_Date_At_Any_Time_Of_Day(int year, int month, int day, double lat, double lng, int utcOffsetMinutes)
    {
        //Arrange
        var offset = TimeSpan.FromMinutes(utcOffsetMinutes);
        var start = new DateTimeOffset(year, month, day, 0, 0, 0, offset);
        var end = start.AddDays(1);

        foreach (var date in new[] { start, start.AddHours(12), end.AddMinutes(-1) })
        {
            //Act
            var sunPhases = SunCalc.GetSunPhases(date, lat, lng).ToList();
            var moonPhase = MoonCalc.GetMoonPhase(date, lat, lng);

            //Assert
            var solarNoon = sunPhases.First(x => x.Name == SunPhaseName.SolarNoon).PhaseTime;
            Assert.True(solarNoon >= start && solarNoon < end, $"input {date:O}: solar noon {solarNoon:O} outside the day");
            Assert.Equal(offset, solarNoon.Offset);
            Assert.All(sunPhases, x => Assert.True(x.PhaseTime >= start && x.PhaseTime < end, $"input {date:O}: {x.Name.Value} {x.PhaseTime:O} outside the day"));

            foreach (var e in new[] { moonPhase.Rise, moonPhase.Set, moonPhase.Transit, moonPhase.LowerTransit })
            {
                if (e is { } time)
                {
                    Assert.True(time >= start && time < end, $"input {date:O}: moon event {time:O} outside the day");
                    Assert.Equal(offset, time.Offset);
                }
            }
        }
    }

    [Fact]
    public void Get_Moon_Phase_Finds_The_Evening_Moonrise_Of_The_Local_Day()
    {
        //Arrange
        var date = new DateTimeOffset(2026, 9, 30, 0, 0, 0, TimeSpan.FromHours(-4));
        var lat = 42.764767;
        var lng = -71.042023;

        //Act
        var moonPhase = MoonCalc.GetMoonPhase(date, lat, lng);

        //Assert
        Assert.NotNull(moonPhase.Rise);
        Assert.NotNull(moonPhase.Set);
        var offMinutes = (moonPhase.Rise.Value - new DateTimeOffset(2026, 9, 30, 20, 23, 0, TimeSpan.FromHours(-4))).TotalMinutes;
        Assert.True(Math.Abs(offMinutes) < 1, $"moonrise {moonPhase.Rise:O} is {offMinutes:F2} min from 20:23");
    }

    [Theory]
    [InlineData(2026, 3, 29, 23)]
    [InlineData(2026, 10, 25, 25)]
    public void Time_Zone_Overloads_Cover_The_Whole_Day_And_Report_Each_Time_At_Its_Own_Offset_On_A_Clock_Change(int year, int month, int day, int expectedHours)
    {
        //Arrange
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
        var lat = 48.2082;
        var lng = 16.3738;
        var date = new DateTimeOffset(year, month, day, 12, 0, 0, zone.GetUtcOffset(new DateTime(year, month, day, 12, 0, 0)));
        var start = Midnight(year, month, day, zone);
        var end = Midnight(year, month, day + 1, zone);
        Assert.Equal(expectedHours, (end - start).TotalHours);

        //Act
        var sunPhases = SunCalc.GetSunPhases(date, lat, lng, zone).ToList();
        var moonPhases = Enumerable.Range(-15, 30).Select(i => MoonCalc.GetMoonPhase(Midnight(year, month, day + i, zone).AddHours(12), lat, lng, zone)).ToList();
        var moonPhase = MoonCalc.GetMoonPhase(date, lat, lng, zone);

        //Assert
        Assert.All(sunPhases, x => Assert.Equal(zone.GetUtcOffset(x.PhaseTime), x.PhaseTime.Offset));
        // a 23 h day can hold no nadir at all (29 March: it is at 23:59 the day before); the solar day's own one is kept
        Assert.All(sunPhases.Where(x => x.Name != SunPhaseName.Nadir), x => Assert.True(x.PhaseTime >= start && x.PhaseTime < end, $"{x.Name.Value} {x.PhaseTime:O} outside the day"));
        var sunrise = sunPhases.First(x => x.Name == SunPhaseName.Sunrise).PhaseTime;
        Assert.Equal(zone.GetUtcOffset(new DateTime(year, month, day, 12, 0, 0)), sunrise.Offset);

        foreach (var e in new[] { moonPhase.Rise, moonPhase.Set, moonPhase.Transit, moonPhase.LowerTransit })
        {
            if (e is { } time)
            {
                Assert.True(time >= start && time < end, $"moon event {time:O} outside the day");
                Assert.Equal(zone.GetUtcOffset(time), time.Offset);
            }
        }

        // a 23 h or 25 h day must neither drop nor double-count a moon transit
        var transits = moonPhases.Where(x => x.Transit.HasValue).Select(x => x.Transit!.Value).ToList();
        for (var i = 1; i < transits.Count; i++)
        {
            var gapHours = (transits[i] - transits[i - 1]).TotalHours;
            Assert.True(gapHours > 24.2 && gapHours < 25.6, $"transit {transits[i]:O} is {gapHours:F2} h after the previous one");
        }
    }
    
    [Theory]
    [InlineData(2026, 6, 1, 42.2406, -8.7207, 120, "Night", "2026-05-31T22:11:55Z")]
    [InlineData(2026, 6, 21, 39.4704, 75.9898, 480, "Night", "2026-06-20T16:27:38Z")]
    [InlineData(2026, 7, 25, 69.6492, 18.9553, 120, "Sunset", null)]
    public void Get_Sun_Phases_Finds_The_Event_Between_Local_Midnight_And_The_Solar_Nadir(int year, int month, int day, double lat, double lng, int utcOffsetMinutes, string name, string expected)
    {
        //Arrange
        var start = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.FromMinutes(utcOffsetMinutes));

        foreach (var date in new[] { start, start.AddHours(12), start.AddDays(1).AddMinutes(-1) })
        {
            //Act
            var phase = SunCalc.GetSunPhases(date, lat, lng).Where(x => x.Name.Value == name).Select(x => (DateTimeOffset?)x.PhaseTime).FirstOrDefault();

            //Assert
            if (expected is null)
            {
                Assert.True(phase is null, $"input {date:O}: {name} {phase:O}");
            }
            else
            {
                Assert.True(phase.HasValue, $"input {date:O}: {name} missing");
                var error = (phase.Value - DateTimeOffset.Parse(expected)).Duration();
                Assert.True(error < TimeSpan.FromMinutes(1), $"input {date:O}: {name} {phase:O}");
            }
        }
    }

    // scans the apparent altitude minute by minute through the local day and checks that every rise and set the
    // scan finds is returned, and nothing it does not find; locations sit far from their zone's meridian
    [Theory]
    [InlineData(42.2406, -8.7207, 120)]
    [InlineData(39.4704, 75.9898, 480)]
    [InlineData(69.6492, 18.9553, 120)]
    [InlineData(64.1466, -21.9426, 0)]
    [InlineData(-54.8019, -68.303, -180)]
    [InlineData(40, 179.7, 720)]
    public void Get_Sun_Phases_Returns_Every_Event_Of_The_Local_Day_And_Nothing_Else(double lat, double lng, int utcOffsetMinutes)
    {
        var offset = TimeSpan.FromMinutes(utcOffsetMinutes);

        for (var dayOfYear = 0; dayOfYear < 365; dayOfYear += 7)
        {
            //Arrange
            var start = new DateTimeOffset(2026, 1, 1, 0, 0, 0, offset).AddDays(dayOfYear);
            var altitudes = Enumerable.Range(0, 24 * 60 + 1)
                .Select(m => SunCalc.GetSunPosition(start.AddMinutes(m), lat, lng).Altitude / Math.PI * 180)
                .ToArray();

            //Act
            var phases = SunCalc.GetSunPhases(start.AddHours(12), lat, lng).ToList();

            //Assert
            foreach (var angle in SunPhaseAngle.Default)
            {
                var apparent = angle.Angle + ApparentRefraction(angle.Angle);
                foreach (var (name, rising) in new[] { (angle.RiseName, true), (angle.SetName, false) })
                {
                    var crossings = Enumerable.Range(0, 24 * 60)
                        .Where(m => rising
                            ? altitudes[m] < apparent && altitudes[m + 1] >= apparent
                            : altitudes[m] >= apparent && altitudes[m + 1] < apparent)
                        .Select(m => start.AddMinutes(m))
                        .ToList();
                    var phase = phases.FindTime(name);
                    var label = $"{lat} {lng} {start:yyyy-MM-dd} {name.Value}: returned {phase:O}, scan found {string.Join(", ", crossings.Select(c => c.ToString("HH:mm")))}";

                    if (crossings.Count == 0)
                    {
                        // a grazing event can cross and come back inside one minute; accept it if it sits on its altitude
                        Assert.True(phase is null || OnAltitude(phase.Value, lat, lng, apparent), label);
                    }
                    else
                    {
                        Assert.True(phase.HasValue, label);
                        Assert.Contains(crossings, c => Math.Abs((phase.Value - c).TotalMinutes - 0.5) <= 1);
                    }
                }
            }
        }
    }

    private static double ApparentRefraction(double altitudeDegrees)
    {
        // the refraction GetSunPosition adds (Meeus 16.4, clamped at the horizon), in degrees
        var h = Math.Max(altitudeDegrees, 0) * Math.PI / 180;
        return 0.0002967 / Math.Tan(h + 0.00312536 / (h + 0.08901179)) * 180 / Math.PI;
    }

    private static bool OnAltitude(DateTimeOffset time, double lat, double lng, double apparent)
    {
        return Math.Abs(SunCalc.GetSunPosition(time, lat, lng).Altitude / Math.PI * 180 - apparent) < 0.01;
    }

    [Fact]
    public void Time_Zone_Overload_Picks_The_Day_The_Instant_Falls_On_In_That_Zone()
    {
        //Arrange
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
        var instant = new DateTimeOffset(2026, 10, 1, 20, 0, 0, TimeSpan.Zero); // already 2 October in Tokyo

        //Act
        var sunPhases = SunCalc.GetSunPhases(instant, 35.6762, 139.6503, zone).ToList();

        //Assert
        var solarNoon = sunPhases.First(x => x.Name == SunPhaseName.SolarNoon).PhaseTime;
        Assert.Equal(new DateTime(2026, 10, 2), solarNoon.DateTime.Date);
        Assert.Equal(TimeSpan.FromHours(9), solarNoon.Offset);
    }

    private static DateTimeOffset Midnight(int year, int month, int day, TimeZoneInfo zone)
    {
        var local = new DateTime(year, month, 1).AddDays(day - 1);
        return new DateTimeOffset(local, zone.GetUtcOffset(local));
    }
}
