namespace SunCalcNet.Internal;

/// <summary>
/// One solar day at an observer: its transit, and the Sun's declination and altitude then.
/// </summary>
internal readonly struct SolarDay
{
    /// <summary>
    /// Solar transit (solar noon), days since J2000 (UT).
    /// </summary>
    public double Transit { get; }

    /// <summary>
    /// The Sun's apparent declination at transit, in radians.
    /// </summary>
    public double Declination { get; }

    /// <summary>
    /// Sine of the Sun's geometric altitude at transit; with the nadirs it brackets the day's rises and sets.
    /// </summary>
    public double SinNoonAltitude { get; }

    public SolarDay(double transit, double declination, double sinNoonAltitude)
    {
        Transit = transit;
        Declination = declination;
        SinNoonAltitude = sinNoonAltitude;
    }
}
