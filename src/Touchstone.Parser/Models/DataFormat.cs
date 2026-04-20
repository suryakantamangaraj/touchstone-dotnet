namespace Touchstone.Parser.Models;

/// <summary>
/// Specifies the format of network parameter data in a Touchstone file.
/// </summary>
public enum DataFormat
{
    /// <summary>
    /// Decibel-Angle format (DB).
    /// Data pairs are (magnitude in dB, angle in degrees).
    /// </summary>
    DecibelAngle,

    /// <summary>
    /// Magnitude-Angle format (MA).
    /// Data pairs are (linear magnitude, angle in degrees).
    /// </summary>
    MagnitudeAngle,

    /// <summary>
    /// Real-Imaginary format (RI).
    /// Data pairs are (real part, imaginary part).
    /// </summary>
    RealImaginary
}
