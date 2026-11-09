using SunCalcNet.Model;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace SunCalcNet.Tests;

public class ArgumentValidationTests
{
    private static readonly DateTimeOffset Date = new(2026, 8, 28, 12, 0, 0, TimeSpan.Zero);
    private static readonly TimeZoneInfo Paris = TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris");

    [Theory]
    [InlineData(double.NaN, 2.35)]
    [InlineData(48.85, double.NaN)]
    [InlineData(double.PositiveInfinity, 2.35)]
    [InlineData(double.NegativeInfinity, 2.35)]
    [InlineData(48.85, double.PositiveInfinity)]
    [InlineData(48.85, double.NegativeInfinity)]
    [InlineData(90.5, 2.35)]
    [InlineData(-91, 2.35)]
    public void Methods_Taking_A_Location_Throw_For_A_Latitude_Or_Longitude_That_Is_Not_A_Place(double lat, double lng)
    {
        //Arrange
        var calls = new (string Name, Action Call)[]
        {
            ("GetSunPosition", () => SunCalc.GetSunPosition(Date, lat, lng)),
            ("GetSunPhases", () => SunCalc.GetSunPhases(Date, lat, lng)),
            ("GetSunPhases angles", () => SunCalc.GetSunPhases(Date, lat, lng, SunPhaseAngle.Default)),
            ("GetSunPhases zone", () => SunCalc.GetSunPhases(Date, lat, lng, Paris)),
            ("GetSunPhases angles zone", () => SunCalc.GetSunPhases(Date, lat, lng, SunPhaseAngle.Default, Paris)),
            ("GetMoonPosition", () => MoonCalc.GetMoonPosition(Date, lat, lng)),
            ("GetMoonPhase", () => MoonCalc.GetMoonPhase(Date, lat, lng)),
            ("GetMoonPhase zone", () => MoonCalc.GetMoonPhase(Date, lat, lng, Paris)),
        };

        foreach (var (name, call) in calls)
        {
            //Act
            var exception = Record.Exception(call);

            //Assert
            var outOfRange = Assert.IsType<ArgumentOutOfRangeException>(exception);
            Assert.True(outOfRange.ParamName is "lat" or "lng", $"{name}: {outOfRange.ParamName}");
        }
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Get_Sun_Phases_Throws_For_A_Height_That_Is_Not_A_Number(double height)
    {
        //Act
        var exception = Record.Exception(() => SunCalc.GetSunPhases(Date, 48.85, 2.35, height));

        //Assert
        Assert.Equal("height", Assert.IsType<ArgumentOutOfRangeException>(exception).ParamName);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Sun_Phase_Angle_Throws_For_An_Angle_That_Is_Not_A_Number(double angle)
    {
        //Act
        var exception = Record.Exception(() => new SunPhaseAngle(angle, "rise", "set"));

        //Assert
        Assert.Equal("angle", Assert.IsType<ArgumentOutOfRangeException>(exception).ParamName);
    }

    [Theory]
    [InlineData(90, 0)]
    [InlineData(-90, 0)]
    [InlineData(48.85, 362.35)]
    [InlineData(48.85, -357.65)]
    public void Methods_Taking_A_Location_Accept_The_Poles_And_Any_Finite_Longitude(double lat, double lng)
    {
        //Act
        var exception = Record.Exception(() =>
        {
            SunCalc.GetSunPosition(Date, lat, lng);
            SunCalc.GetSunPhases(Date, lat, lng, -100).ToList();
            MoonCalc.GetMoonPosition(Date, lat, lng);
            MoonCalc.GetMoonPhase(Date, lat, lng);
        });

        //Assert
        Assert.Null(exception);
    }

    // the public methods no longer let a NaN through, so the solver's own guard is reached by reflection
    [Theory]
    [InlineData(double.NaN, 0.1, 0.9)]
    [InlineData(-0.0145, double.NaN, 0.9)]
    [InlineData(-0.0145, -0.5, double.NaN)]
    public void Rise_Set_Solver_Returns_NaN_Instead_Of_Looping_For_A_NaN_Input(double h0, double sa, double sb)
    {
        //Arrange
        var getSetJ = typeof(SunCalc).Assembly.GetType("SunCalcNet.Internal.SunTime")!
            .GetMethod("GetSetJ", BindingFlags.Static | BindingFlags.NonPublic)!;
        const double dt = 9735.0;

        //Act
        var solve = Task.Run(() => (double)getSetJ.Invoke(null, [h0, dt - 0.5, dt, sa, sb, dt, 0.0, 0.85, 0.17])!);
        var finished = solve.Wait(TimeSpan.FromSeconds(5));

        //Assert
        Assert.True(finished, "solver still running after 5 s");
        Assert.True(double.IsNaN(solve.Result), $"returned {solve.Result}");
    }
}
