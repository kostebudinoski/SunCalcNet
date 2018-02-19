using System;

namespace SunCalcNet.Model
{
    public class SunPhase
    {
        /// <summary>
        /// Sun phase name.
        /// </summary>
        public SunPhaseName Name { get; }

        /// <summary>
        /// Time of the day when the sun phase occurs
        /// </summary>
        public DateTime PhaseTime { get; }

        public SunPhase(SunPhaseName name, DateTime phaseTime)
        {
            Name = name;
            PhaseTime = phaseTime;
        }
    }
}