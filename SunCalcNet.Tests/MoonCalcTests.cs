using System;
using Xunit;

namespace SunCalcNet.Tests
{
    public class MoonCalcTests
    {
        [Fact]
        public void Get_Moon_Position_Returns_Azimuth_Altitude_Distance_And_ParallacticAngle_For_The_Given_Time_And_Location()
        {
            //Arrange
            var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);
            var lat = 50.5;
            var lng = 30.5;

            //Act
            var sunPosition = MoonCalc.GetMoonPosition(date, lat, lng);

            //Assert
            Assert.Equal(-0.9783999522438226, sunPosition.Azimuth, 15);
            Assert.Equal(0.0145514822438922, sunPosition.Altitude, 15);
            Assert.Equal(364121.37256256194, sunPosition.Distance, 15);
            Assert.Equal(-0.59832117604234014, sunPosition.ParallacticAngle, 15);
        }

        [Fact]
        public void Get_Moon_Illumination_Returns_Fraction_And_Angle_Of_Moons_Illuminated_Limb_And_Phase()
        {
            //Arrange
            var date = new DateTime(2013, 3, 5, 0, 0, 0, DateTimeKind.Utc);

            //Act
            var moonIllum = MoonCalc.GetMoonIllumination(date);

            //Assert
            Assert.Equal(0.4848068202456373, moonIllum.Fraction, 15);
            Assert.Equal(0.7548368838538762, moonIllum.Phase, 15);
            Assert.Equal(1.6732942678578346, moonIllum.Angle, 15);
        }

        [Fact]
        public void Get_Moon_Times_Returns_MoonRise_And_Set_Times()
        {
            //Arrange
            var date = new DateTime(2013, 3, 4, 0, 0, 0, DateTimeKind.Utc);
            var lat = 50.5;
            var lng = 30.5;

            //Act
            var moonPhase = MoonCalc.GetMoonPhase(date, lat, lng);

            //Assert
            Assert.NotNull(moonPhase.Rise);
            Assert.NotNull(moonPhase.Set);
            var rise = moonPhase.Rise.Value.ToString("yyyy-MM-dd hh:mm:ss");
            var set = moonPhase.Set.Value.ToString("yyyy-MM-dd hh:mm:ss");
            Assert.Equal("2013-03-04 11:54:29", rise);
            Assert.Equal("2013-03-04 07:47:58", set);
            Assert.False(moonPhase.AlwaysDown);
            Assert.False(moonPhase.AlwaysUp);
        }
        
        [Fact]
        public void Get_Moon_Times_Time_Specified_Returns_MoonRise_And_Set_Times()
        {
            //Arrange
            var date = new DateTime(2020, 5, 13, 10, 16, 0, DateTimeKind.Utc);
            var lat = 48.2026;
            var lng = 16.3684;

            //Act
            var moonPhase = MoonCalc.GetMoonPhase(date, lat, lng);
            
            //Assert
            Assert.Null(moonPhase.Rise);
            Assert.NotNull(moonPhase.Set);
            var set = moonPhase.Set.Value.ToString("yyyy-MM-dd hh:mm:ss");
            Assert.Equal("2020-05-13 08:37:56", set);
            Assert.False(moonPhase.AlwaysDown);
            Assert.False(moonPhase.AlwaysUp);
        }
    }
}