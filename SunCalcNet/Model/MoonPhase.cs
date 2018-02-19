using System;

namespace SunCalcNet.Model
{
    public class MoonPhase
    {
        /// <summary>
        /// Moonrise time as Date
        /// </summary>
        public DateTime? Rise { get; set; }

        /// <summary>
        /// Moonset time as Date
        /// </summary>
        public DateTime? Set { get; set; }

        /// <summary>
        /// True if the moon never rises/sets and is always above the horizon during the day
        /// </summary>
        public bool AlwaysUp { get; set; }

        /// <summary>
        /// True if the moon is always below the horizon
        /// </summary>
        public bool AlwaysDown { get; set; }
    }
}