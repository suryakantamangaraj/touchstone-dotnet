namespace Touchstone.Parser.Models;

/// <summary>
/// Specifies the type of network parameter stored in a Touchstone file.
/// </summary>
public enum ParameterType
{
    /// <summary>Scattering parameters (S-parameters).</summary>
    S,

    /// <summary>Admittance parameters (Y-parameters).</summary>
    Y,

    /// <summary>Impedance parameters (Z-parameters).</summary>
    Z,

    /// <summary>Hybrid parameters (H-parameters).</summary>
    H,

    /// <summary>Inverse hybrid parameters (G-parameters).</summary>
    G
}
