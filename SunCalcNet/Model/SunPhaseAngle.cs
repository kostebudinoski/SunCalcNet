using System;
using System.Collections.Generic;

namespace SunCalcNet.Model
{
    [Serializable]
    public class SunPhaseName
    {
        private SunPhaseName(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public static SunPhaseName SolarNoon => new SunPhaseName("Solar Noon");
        public static SunPhaseName Nadir => new SunPhaseName("Nadir");
        public static SunPhaseName Sunrise => new SunPhaseName("Sunrise");
        public static SunPhaseName Sunset => new SunPhaseName("Sunset");
        public static SunPhaseName SunriseEnd => new SunPhaseName("Sunrise End");
        public static SunPhaseName SunsetStart => new SunPhaseName("Sunset Start");
        public static SunPhaseName Dawn => new SunPhaseName("Dawn");
        public static SunPhaseName Dusk => new SunPhaseName("Dusk");
        public static SunPhaseName NauticalDawn => new SunPhaseName("Nautical Dawn");
        public static SunPhaseName NauticalDusk => new SunPhaseName("Nautical Dusk");
        public static SunPhaseName NightEnd => new SunPhaseName("Night End");
        public static SunPhaseName Night => new SunPhaseName("Night");
        public static SunPhaseName GoldenHourEnd => new SunPhaseName("Golden Hour End");
        public static SunPhaseName GoldenHour => new SunPhaseName("Golden Hour");

        public override string ToString()
        {
            return Value;
        }
    }

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
            new SunPhaseAngle(-0.833, SunPhaseName.Sunrise, SunPhaseName.Sunset),
            new SunPhaseAngle(-0.3, SunPhaseName.SunriseEnd, SunPhaseName.SunsetStart),
            new SunPhaseAngle(-6, SunPhaseName.Dawn, SunPhaseName.Dusk),
            new SunPhaseAngle(-12, SunPhaseName.NauticalDawn, SunPhaseName.NauticalDusk),
            new SunPhaseAngle(-18, SunPhaseName.NightEnd, SunPhaseName.Night),
            new SunPhaseAngle(6, SunPhaseName.GoldenHourEnd, SunPhaseName.GoldenHour)
        };
    }
}