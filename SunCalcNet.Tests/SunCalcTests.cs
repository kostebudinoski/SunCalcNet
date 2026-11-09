using SunCalcNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SunCalcNet.Tests;

public class SunCalcTests
{
    [Fact]
    public void Get_Sun_Position_Returns_Azimuth_And_Altitude_For_The_Given_Time_And_Location()
    {
        //Arrange
        var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.Zero);
        var lat = 50.5;
        var lng = 30.5;

        //Act
        var sunPosition = SunCalc.GetSunPosition(date, lat, lng);

        //Assert
        Assert.Equal(-2.4967445445669547, sunPosition.Azimuth, 14);
        Assert.Equal(-0.6888030343391054, sunPosition.Altitude, 14);
    }

    [Fact]
    public void Get_Sun_Phases_Returns_Sun_Phases_For_The_Given_Date_And_Location()
    {
        //Arrange
        var testData = new List<SunPhase>
        {
            new(SunPhaseName.SolarNoon, new DateTime(2013, 3, 5, 10, 9, 28, DateTimeKind.Utc)),
            new(SunPhaseName.Nadir, new DateTime(2013, 3, 4, 22, 9, 28, DateTimeKind.Utc)),
            new(SunPhaseName.Sunrise, new DateTime(2013, 3, 5, 4, 33, 31, DateTimeKind.Utc)),
            new(SunPhaseName.Sunset, new DateTime(2013, 3, 5, 15, 46, 19, DateTimeKind.Utc)),
            new(SunPhaseName.SunriseEnd, new DateTime(2013, 3, 5, 4, 36, 54, DateTimeKind.Utc)),
            new(SunPhaseName.SunsetStart, new DateTime(2013, 3, 5, 15, 42, 56, DateTimeKind.Utc)),
            new(SunPhaseName.Dawn, new DateTime(2013, 3, 5, 4, 0, 55, DateTimeKind.Utc)),
            new(SunPhaseName.Dusk, new DateTime(2013, 3, 5, 16, 18, 59, DateTimeKind.Utc)),
            new(SunPhaseName.NauticalDawn, new DateTime(2013, 3, 5, 3, 23, 12, DateTimeKind.Utc)),
            new(SunPhaseName.NauticalDusk, new DateTime(2013, 3, 5, 16, 56, 49, DateTimeKind.Utc)),
            new(SunPhaseName.NightEnd, new DateTime(2013, 3, 5, 2, 45, 2, DateTimeKind.Utc)),
            new(SunPhaseName.Night, new DateTime(2013, 3, 5, 17, 35, 7, DateTimeKind.Utc)),
            new(SunPhaseName.GoldenHourEnd, new DateTime(2013, 3, 5, 5, 17, 32, DateTimeKind.Utc)),
            new(SunPhaseName.GoldenHour, new DateTime(2013, 3, 5, 15, 2, 14, DateTimeKind.Utc)),
        };

        var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.FromHours(2)); // Kyiv, EET
        var lat = 50.5;
        var lng = 30.5;

        //Act
        var sunPhases = SunCalc.GetSunPhases(date, lat, lng).ToList();

        //Assert
        foreach (var testSunPhase in testData)
        {
            var sunPhaseValue = sunPhases.First(x => x.Name == testSunPhase.Name);

            var testDataPhaseTime = testSunPhase.PhaseTime.ToString("yyyy-MM-dd HH:mm:ss");
            var sunPhaseTime = sunPhaseValue.PhaseTime.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            Assert.Equal(testDataPhaseTime, sunPhaseTime);
        }
    }

    [Fact]
    public void Get_Sun_Phases_Works_At_North_Pole()
    {
        //Arrange
        var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.Zero);
        var lat = 90;
        var lng = 135;

        //Act
        var sunPhases = SunCalc.GetSunPhases(date, lat, lng).ToList();

        //Assert
        Assert.Equal(2, sunPhases.Count);
    }

    [Fact]
    public void Get_Sun_Phases_Adjusts_Sun_Phases_When_Additionally_Given_The_Observer_Height()
    {
        //Arrange
        var heightTestData = new List<SunPhase>
        {
            new(SunPhaseName.SolarNoon, new DateTime(2013, 3, 5, 10, 9, 28, DateTimeKind.Utc)),
            new(SunPhaseName.Nadir, new DateTime(2013, 3, 4, 22, 9, 28, DateTimeKind.Utc)),
            new(SunPhaseName.Sunrise, new DateTime(2013, 3, 5, 4, 23, 43, DateTimeKind.Utc)),
            new(SunPhaseName.Sunset, new DateTime(2013, 3, 5, 15, 56, 8, DateTimeKind.Utc))
        };

        var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.FromHours(2)); // Kyiv, EET
        var lat = 50.5;
        var lng = 30.5;
        var height = 2000;

        //Act
        var sunPhases = SunCalc.GetSunPhases(date, lat, lng, height).ToList();

        //Assert
        foreach (var testSunPhase in heightTestData)
        {
            var sunPhaseValue = sunPhases.First(x => x.Name == testSunPhase.Name);

            var testDataPhaseTime = testSunPhase.PhaseTime.ToString("yyyy-MM-dd HH:mm:ss");
            var sunPhaseTime = sunPhaseValue.PhaseTime.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            Assert.Equal(testDataPhaseTime, sunPhaseTime);
        }
    }

    [Fact]
    public void Get_Sun_Phases_Uses_The_Supplied_Custom_Phase_Angles()
    {
        //Arrange
        var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.Zero);
        var lat = 50.5;
        var lng = 30.5;
        var customAngles = new[] { new SunPhaseAngle(-4, "blueHourDawn", "blueHourDusk") };

        //Act
        var sunPhases = SunCalc.GetSunPhases(date, lat, lng, customAngles).ToList();

        //Assert
        Assert.Equal(4, sunPhases.Count);
        Assert.DoesNotContain(sunPhases, x => x.Name == SunPhaseName.Sunrise);
        var dawn = sunPhases.First(x => x.Name == SunPhaseName.Custom("blueHourDawn"));
        var dusk = sunPhases.First(x => x.Name == SunPhaseName.Custom("blueHourDusk"));
        Assert.Equal("2013-03-05 04:13:30", dawn.PhaseTime.ToString("yyyy-MM-dd HH:mm:ss"));
        Assert.Equal("2013-03-05 16:06:23", dusk.PhaseTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [Fact]
    public void Get_Sun_Phases_Can_Combine_Default_And_Custom_Phase_Angles()
    {
        //Arrange
        var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.Zero);
        var lat = 50.5;
        var lng = 30.5;
        var angles = SunPhaseAngle.Default.Append(new SunPhaseAngle(-4, "blueHourDawn", "blueHourDusk"));

        //Act
        var sunPhases = SunCalc.GetSunPhases(date, lat, lng, angles).ToList();

        //Assert
        Assert.Equal(16, sunPhases.Count);
        Assert.Contains(sunPhases, x => x.Name == SunPhaseName.Sunrise);
        Assert.Contains(sunPhases, x => x.Name == SunPhaseName.Custom("blueHourDawn"));
    }

    // on days the Sun barely reaches an altitude, each event lands on its line within its half day or is
    // absent — never thrown hours or days away by an ill-conditioned step; JPL Horizons times (suncalc #191)
    [Theory]
    // each at its local mean time (longitude / 15), so the local day is the solar day these grazing events are judged on
    [InlineData("2026-05-22", 51.5074, -0.1278, "+00:00", "Night", null)]
    [InlineData("2026-05-22", 51.5074, -0.1278, "+00:00", "Night End", "2026-05-22T00:18:06Z")]
    [InlineData("2027-07-26", 52.52, 13.405, "+00:54", "Night End", null)]
    [InlineData("2027-07-26", 52.52, 13.405, "+00:54", "Night", "2027-07-26T22:50:31Z")]
    [InlineData("2026-08-14", 69.6496, 18.956, "+01:16", "Dawn", null)]
    [InlineData("2026-08-14", 69.6496, 18.956, "+01:16", "Dusk", "2026-08-14T22:14:03Z")]
    [InlineData("2026-05-18", 69.6496, 18.956, "+01:16", "Sunrise", "2026-05-17T22:52:02Z")]
    [InlineData("2026-07-25", 69.6496, 18.956, "+01:16", "Sunset", "2026-07-25T22:37:09Z")]
    [InlineData("2026-04-18", 67, 18.956, "+01:16", "Nautical Dusk", null)]
    public void Get_Sun_Phases_Resolves_Grazing_Events_Or_Omits_Them(string day, double lat, double lng, string offset, string name, string expected)
    {
        //Arrange
        var date = DateTimeOffset.Parse($"{day}T12:00:00{offset}");

        //Act
        var phase = SunCalc.GetSunPhases(date, lat, lng).Where(x => x.Name.Value == name).Cast<SunPhase?>().FirstOrDefault();

        //Assert
        if (expected is null)
        {
            Assert.True(phase is null, $"{day} {name}: {phase?.PhaseTime:O}");
        }
        else
        {
            Assert.True(phase.HasValue, $"{day} {name} missing");
            var error = (phase.Value.PhaseTime - DateTimeOffset.Parse(expected)).Duration();
            Assert.True(error < TimeSpan.FromMinutes(1), $"{day} {name}: {phase.Value.PhaseTime:O}");
        }
    }

    [Fact]
    public void Get_Sun_Phases_Keeps_Every_Event_On_Its_Altitude_Within_The_Local_Day()
    {
        //Arrange
        const double lng = 10;
        const double belowHorizonRefraction = 0.484; // GetSunPosition adds a constant refraction below the horizon

        for (var lat = 48.0; lat <= 72; lat += 0.5)
        {
            for (var day = 0; day < 365; day += 3)
            {
                //Act
                var date = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero).AddDays(day);
                var phases = SunCalc.GetSunPhases(date, lat, lng).ToList();
                var start = date.AddHours(-12);

                //Assert
                foreach (var angle in SunPhaseAngle.Default)
                {
                    foreach (var name in new[] { angle.RiseName, angle.SetName })
                    {
                        if (!phases.Any(x => x.Name == name))
                        {
                            continue;
                        }

                        var phase = phases.First(x => x.Name == name);

                        Assert.True(phase.PhaseTime >= start && phase.PhaseTime < start.AddDays(1), $"{lat} {day} {name} outside the day");

                        if (angle.Angle < -1)
                        {
                            var altitude = SunCalc.GetSunPosition(phase.PhaseTime, lat, lng).Altitude / Math.PI * 180 - belowHorizonRefraction;
                            Assert.True(Math.Abs(altitude - angle.Angle) < 0.01, $"{lat} {day} {name} at {altitude}°");
                        }
                    }
                }
            }
        }
    }

    [Fact]
    public void Find_Time_Returns_The_Time_Of_A_Phase_That_Occurs()
    {
        //Arrange
        var date = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.Zero);
        var angles = SunPhaseAngle.Default.Append(new SunPhaseAngle(-4, "blueHourDawn", "blueHourDusk"));
        var sunPhases = SunCalc.GetSunPhases(date, 50.5, 30.5, angles).ToList();

        //Act
        var sunrise = sunPhases.FindTime(SunPhaseName.Sunrise);
        var blueHourDawn = sunPhases.FindTime(SunPhaseName.Custom("blueHourDawn"));

        //Assert
        Assert.Equal("2013-03-05 04:33:31", sunrise?.ToString("yyyy-MM-dd HH:mm:ss"));
        Assert.Equal("2013-03-05 04:13:30", blueHourDawn?.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    [Fact]
    public void Find_Time_Returns_Null_When_The_Phase_Does_Not_Occur()
    {
        //Arrange
        var date = new DateTimeOffset(2026, 5, 22, 12, 0, 0, TimeSpan.Zero);
        var sunPhases = SunCalc.GetSunPhases(date, 51.5074, -0.1278).ToList(); // London: no astronomical night

        //Act
        var night = sunPhases.FindTime(SunPhaseName.Night);

        //Assert
        Assert.Null(night);
        Assert.NotNull(sunPhases.FindTime(SunPhaseName.NightEnd));
    }

    [Fact]
    public void Get_Sun_Phases_Returns_Times_At_The_Offset_Of_The_Given_Date()
    {
        //Arrange
        var utc = new DateTimeOffset(2013, 3, 5, 0, 0, 0, TimeSpan.Zero);
        var kyiv = new DateTimeOffset(2013, 3, 5, 18, 30, 0, TimeSpan.FromHours(2));
        var lat = 50.5;
        var lng = 30.5;

        //Act
        var utcPhases = SunCalc.GetSunPhases(utc, lat, lng).ToList();
        var kyivPhases = SunCalc.GetSunPhases(kyiv, lat, lng).ToList();

        //Assert
        Assert.All(kyivPhases, x => Assert.Equal(TimeSpan.FromHours(2), x.PhaseTime.Offset));
        // the UTC and Kyiv days overlap in all but two hours: the same events, except the nadir at 22:09 UTC
        Assert.Equal(utcPhases.Where(x => x.Name != SunPhaseName.Nadir), kyivPhases.Where(x => x.Name != SunPhaseName.Nadir));
        var sunrise = kyivPhases.First(x => x.Name == SunPhaseName.Sunrise);
        Assert.Equal("2013-03-05 06:33:31 +02:00", sunrise.PhaseTime.ToString("yyyy-MM-dd HH:mm:ss zzz"));
    }

    [Fact]
    public void DateTime_Overloads_Of_Sun_And_Moon_Phases_Are_Compile_Errors()
    {
        //Arrange
        var methods = typeof(SunCalc).GetMethods().Concat(typeof(MoonCalc).GetMethods())
            .Where(m => m.Name is nameof(SunCalc.GetSunPhases) or nameof(MoonCalc.GetMoonPhase))
            .Where(m => m.GetParameters()[0].ParameterType == typeof(DateTime))
            .ToList();

        //Assert
        Assert.Equal(3, methods.Count);
        Assert.All(methods, m =>
        {
            var obsolete = m.GetCustomAttribute<ObsoleteAttribute>();
            Assert.NotNull(obsolete);
            Assert.True(obsolete.IsError, $"{m} must fail to compile, not just warn");
            Assert.Contains("Migrating-to-3.0", obsolete.Message);
        });
    }
}
