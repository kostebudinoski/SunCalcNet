using SunCalcNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SunCalcNet.Tests;

public class SunCalcTests
{
    [Fact]
    public void Get_Sun_Position_Returns_Azimuth_And_Altitude_For_The_Given_Time_And_Location()
    {
        //Arrange
        var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
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

        var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
        var lat = 50.5;
        var lng = 30.5;

        //Act
        var sunPhases = SunCalc.GetSunPhases(date, lat, lng).ToList();

        //Assert
        foreach (var testSunPhase in testData)
        {
            var sunPhaseValue = sunPhases.First(x => x.Name == testSunPhase.Name);

            var testDataPhaseTime = testSunPhase.PhaseTime.ToString("yyyy-MM-dd HH:mm:ss");
            var sunPhaseTime = sunPhaseValue.PhaseTime.ToString("yyyy-MM-dd HH:mm:ss");
            Assert.Equal(testDataPhaseTime, sunPhaseTime);
        }
    }

    [Fact]
    public void Get_Sun_Phases_Works_At_North_Pole()
    {
        //Arrange
        var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
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

        var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
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
            var sunPhaseTime = sunPhaseValue.PhaseTime.ToString("yyyy-MM-dd HH:mm:ss");
            Assert.Equal(testDataPhaseTime, sunPhaseTime);
        }
    }

    [Fact]
    public void Get_Sun_Phases_Uses_The_Supplied_Custom_Phase_Angles()
    {
        //Arrange
        var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
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
        var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
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
}
