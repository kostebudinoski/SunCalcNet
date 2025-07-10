using System;
using System.Collections.Generic;

namespace SunCalcNet.Model
{
    [Serializable]
    public class SunPhaseAngle
    {
        public double Angle { get; }

        public SunPhaseName RiseName { get; }

        public SunPhaseName SetName { get; }

        private SunPhaseAngle(double angle, SunPhaseName riseName, SunPhaseName setName)
        {
            Angle = angle;
            RiseName = riseName;
            SetName = setName;
        }

        public static IEnumerable<SunPhaseAngle> List => new List<SunPhaseAngle>
        {
            new(-0.833, SunPhaseName.Sunrise, SunPhaseName.Sunset),
            new(-0.3, SunPhaseName.SunriseEnd, SunPhaseName.SunsetStart),
            new(-6, SunPhaseName.Dawn, SunPhaseName.Dusk),
            new(-12, SunPhaseName.NauticalDawn, SunPhaseName.NauticalDusk),
            new(-18, SunPhaseName.NightEnd, SunPhaseName.Night),
            new(6, SunPhaseName.GoldenHourEnd, SunPhaseName.GoldenHour)
        };
    }
}