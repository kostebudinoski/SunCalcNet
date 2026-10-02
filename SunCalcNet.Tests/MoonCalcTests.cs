using System;
using SunCalcNet.Model;
using Xunit;

namespace SunCalcNet.Tests;

public class MoonCalcTests
{
    [Fact]
    public void Get_Moon_Position_Returns_Azimuth_Altitude_Distance_And_ParallacticAngle_For_The_Given_Time_And_Location()
    {
        //Arrange
        var date = Utc(2013, 3, 5);
        var lat = 50.5;
        var lng = 30.5;

        //Act
        var sunPosition = MoonCalc.GetMoonPosition(date, lat, lng);

        //Assert
        Assert.Equal(-0.9661994436443471, sunPosition.Azimuth, 12);
        Assert.Equal(0.007971096659309906, sunPosition.Altitude, 12);
        Assert.Equal(370193.9925193064, sunPosition.Distance, 6);
        Assert.Equal(-0.5923875457617929, sunPosition.ParallacticAngle, 12);
    }

    [Fact]
    public void Get_Moon_Illumination_Returns_Fraction_And_Angle_Of_Moons_Illuminated_Limb_And_Phase()
    {
        //Arrange
        var date = Utc(2013, 3, 5);

        //Act
        var moonIllum = MoonCalc.GetMoonIllumination(date);

        //Assert
        Assert.Equal(0.4911927817602366, moonIllum.Fraction, 12);
        Assert.Equal(0.7528035696247392, moonIllum.Phase, 12);
        Assert.Equal(1.6763844401987489, moonIllum.Angle, 12);
        Assert.False(moonIllum.Waxing); // phase > 0.5 -> waning
    }

    [Fact]
    public void Get_Moon_Illumination_Reports_Waxing_During_The_First_Half_Of_The_Lunation()
    {
        //Arrange
        var date = Utc(2013, 3, 15);

        //Act
        var moonIllum = MoonCalc.GetMoonIllumination(date);

        //Assert
        Assert.True(moonIllum.Waxing);
        Assert.True(moonIllum.Phase < 0.5);
    }

    [Fact]
    public void Get_Moon_Times_Returns_MoonRise_And_Set_Times()
    {
        //Arrange
        var date = Utc(2013, 3, 4);
        var lat = 50.5;
        var lng = 30.5;

        //Act
        var moonPhase = MoonCalc.GetMoonPhase(date, lat, lng);

        //Assert
        Assert.NotNull(moonPhase.Rise);
        Assert.NotNull(moonPhase.Set);
        var rise = moonPhase.Rise.Value.ToString("yyyy-MM-dd HH:mm:ss");
        var set = moonPhase.Set.Value.ToString("yyyy-MM-dd HH:mm:ss");
        Assert.Equal("2013-03-04 23:53:32", rise);
        Assert.Equal("2013-03-04 07:42:17", set);
        Assert.False(moonPhase.AlwaysDown);
        Assert.False(moonPhase.AlwaysUp);
    }

    [Fact]
    public void Get_Moon_Times_Time_Specified_Returns_MoonRise_And_Set_Times()
    {
        //Arrange
        var date = Utc(2020, 5, 13, 10, 16);
        var lat = 48.2026;
        var lng = 16.3684;

        //Act
        var moonPhase = MoonCalc.GetMoonPhase(date, lat, lng);

        //Assert
        Assert.Null(moonPhase.Rise);
        Assert.NotNull(moonPhase.Set);
        var set = moonPhase.Set.Value.ToString("yyyy-MM-dd HH:mm:ss");
        Assert.Equal("2020-05-13 08:34:06", set);
        Assert.False(moonPhase.AlwaysDown);
        Assert.False(moonPhase.AlwaysUp);
    }

    [Theory]
    [InlineData(2022, 1, 14)]
    [InlineData(2022, 1, 15)]
    [InlineData(2022, 1, 16)]
    public void Get_Moon_Times_No_Crossing_Day_Flags_AlwaysUp_When_Moon_Stays_Above_Horizon(int year, int month, int day)
    {
        //Arrange
        var date = Utc(year, month, day);
        var lat = 78;
        var lng = 78;

        var minAltitude = double.MaxValue;
        for (var h = 0.0; h <= 24; h += 0.5)
        {
            minAltitude = Math.Min(minAltitude, MoonCalc.GetMoonPosition(date.AddHours(h), lat, lng).Altitude);
        }

        Assert.True(minAltitude > 0, "moon dips below the horizon, fixture assumption wrong");

        //Act
        var moonPhase = MoonCalc.GetMoonPhase(date, lat, lng);

        //Assert
        Assert.Null(moonPhase.Rise);
        Assert.Null(moonPhase.Set);
        Assert.True(moonPhase.AlwaysUp);
        Assert.False(moonPhase.AlwaysDown);
    }

    [Theory]
    [InlineData(50.45466, 30.5238, 2026, 1, 3, 22, 36)]
    [InlineData(50.45466, 30.5238, 2026, 1, 18, 9, 51)]
    [InlineData(78.22334, 15.64689, 2026, 1, 3, 23, 38)]
    [InlineData(78.22334, 15.64689, 2026, 1, 10, 4, 28)]
    public void Get_Moon_Times_Returns_Transit_Matching_USNO(double lat, double lng, int year, int month, int day, int hour, int minute)
    {
        //Arrange
        var date = Utc(year, month, day);

        //Act
        var moonPhase = MoonCalc.GetMoonPhase(date, lat, lng);

        //Assert
        Assert.NotNull(moonPhase.Transit);
        var offMinutes = (moonPhase.Transit.Value - Utc(year, month, day, hour, minute)).TotalMinutes;
        Assert.True(Math.Abs(offMinutes) <= 1, $"transit {moonPhase.Transit:O} is {offMinutes:F2} min from USNO {hour:00}:{minute:00}");
    }

    [Fact]
    public void Get_Moon_Times_Returns_Lower_Transit_Matching_USNO_When_Moon_Is_Always_Up()
    {
        //Arrange
        var date = Utc(2026, 1, 3);
        var lat = 78.22334;
        var lng = 15.64689;

        //Act
        var moonPhase = MoonCalc.GetMoonPhase(date, lat, lng);

        //Assert
        Assert.True(moonPhase.AlwaysUp);
        Assert.NotNull(moonPhase.LowerTransit);
        var offMinutes = (moonPhase.LowerTransit.Value - Utc(2026, 1, 3, 11, 6)).TotalMinutes;
        Assert.True(Math.Abs(offMinutes) <= 1, $"lower transit {moonPhase.LowerTransit:O} is {offMinutes:F2} min from USNO 11:06");
    }

    [Theory]
    [InlineData(51.5, -0.1, 0)]
    [InlineData(-33.9, 151.2, 600)]
    [InlineData(40, -179.7, -720)]
    [InlineData(78.2, 15.6, 60)]
    public void Get_Moon_Times_Reports_Each_Transit_Exactly_Once_Across_Consecutive_Days(double lat, double lng, int utcOffsetMinutes)
    {
        //Arrange
        var start = new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.FromMinutes(utcOffsetMinutes));
        const int days = 60;

        //Act
        var moonPhases = new MoonPhase[days];
        for (var i = 0; i < days; i++)
        {
            moonPhases[i] = MoonCalc.GetMoonPhase(start.AddDays(i), lat, lng);
        }

        //Assert
        AssertEachTransitOnce(moonPhases, x => x.Transit, "transit");
        AssertEachTransitOnce(moonPhases, x => x.LowerTransit, "lower transit");
    }

    private static void AssertEachTransitOnce(MoonPhase[] moonPhases, Func<MoonPhase, DateTimeOffset?> transit, string name)
    {
        DateTimeOffset? previous = null;
        var skipped = 0;
        foreach (var moonPhase in moonPhases)
        {
            var current = transit(moonPhase);
            if (current is null)
            {
                skipped++;
                continue;
            }

            if (previous.HasValue)
            {
                var gapHours = (current.Value - previous.Value).TotalHours;
                Assert.True(gapHours > 24.2 && gapHours < 25.6, $"{name} {current:O} is {gapHours:F2} h after the previous one");
            }

            previous = current;
        }

        Assert.InRange(skipped, 1, 3);
    }

    private static DateTimeOffset Utc(int year, int month, int day, int hour = 0, int minute = 0) =>
        new(year, month, day, hour, minute, 0, TimeSpan.Zero);
}
