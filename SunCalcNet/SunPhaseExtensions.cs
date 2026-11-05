using SunCalcNet.Model;
using System;
using System.Collections.Generic;

namespace SunCalcNet;

public static class SunPhaseExtensions
{
    /// <summary>
    /// Finds when the named phase occurs among the phases returned by <see cref="SunCalc.GetSunPhases(DateTimeOffset, double, double, double)"/>.
    /// A phase is omitted on days the sun doesn't reach its angle (e.g. no <see cref="SunPhaseName.Night"/> on a summer
    /// night at higher latitudes), so check the result for null rather than assuming every phase is present.
    /// </summary>
    /// <param name="phases">The phases of one day.</param>
    /// <param name="name">The phase to look up, e.g. <see cref="SunPhaseName.Sunrise"/> or <c>SunPhaseName.Custom("blueHourDawn")</c>.</param>
    /// <returns>When the phase occurs; null if it doesn't happen that day.</returns>
    public static DateTimeOffset? FindTime(this IEnumerable<SunPhase> phases, SunPhaseName name)
    {
        if (phases is null)
        {
            throw new ArgumentNullException(nameof(phases));
        }

        foreach (var phase in phases)
        {
            if (phase.Name == name)
            {
                return phase.PhaseTime;
            }
        }

        return null;
    }
}
