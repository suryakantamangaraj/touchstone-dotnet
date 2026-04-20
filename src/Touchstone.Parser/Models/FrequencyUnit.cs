namespace Touchstone.Parser.Models;

/// <summary>
/// Specifies the unit of frequency used in a Touchstone file.
/// </summary>
public enum FrequencyUnit
{
    /// <summary>Hertz (1 Hz).</summary>
    Hz,

    /// <summary>Kilohertz (1e3 Hz).</summary>
    KHz,

    /// <summary>Megahertz (1e6 Hz).</summary>
    MHz,

    /// <summary>Gigahertz (1e9 Hz).</summary>
    GHz
}
