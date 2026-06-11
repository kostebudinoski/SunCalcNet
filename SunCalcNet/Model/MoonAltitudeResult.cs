using System;

namespace SunCalcNet.Model
{
    [Serializable]
    internal readonly struct MoonAltitudeResult
    {
        public MoonAltitudeResult(double apparentAltitude, GeocentricCoords moonCoords, double hourAngle)
        {
            ApparentAltitude = apparentAltitude;
            MoonCoords = moonCoords;
            HourAngle = hourAngle;
        }

        public double ApparentAltitude { get; }

        public GeocentricCoords MoonCoords { get; }

        public double HourAngle { get; }
    }
}
