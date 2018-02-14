using System;

namespace SunCalc.Model
{
    public class SunPhase
    {
        public SunPhaseName Name { get; }

        public DateTime PhaseTime { get; }

        public SunPhase(SunPhaseName name, DateTime phaseTime)
        {
            Name = name;
            PhaseTime = phaseTime;
        }
    }
}