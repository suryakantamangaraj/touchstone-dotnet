namespace Touchstone.Parser.Models;

/// <summary>
/// Represents the option line configuration parsed from a Touchstone file.
/// The option line (starting with '#') defines the frequency unit, parameter type,
/// data format, and reference impedance for the file.
/// </summary>
/// <remarks>
/// Default values per the Touchstone specification:
/// frequency unit = GHz, parameter type = S, format = MA, reference impedance = 50 Ω.
/// </remarks>
public sealed class TouchstoneOptions
{
    /// <summary>
    /// Gets the frequency unit used in the data section.
    /// </summary>
    public FrequencyUnit FrequencyUnit { get; }

    /// <summary>
    /// Gets the type of network parameters stored in the file.
    /// </summary>
    public ParameterType ParameterType { get; }

    /// <summary>
    /// Gets the data format (representation) of the network parameter values.
    /// </summary>
    public DataFormat DataFormat { get; }

    /// <summary>
    /// Gets the reference impedance in ohms.
    /// </summary>
    public double ReferenceImpedance { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TouchstoneOptions"/> class.
    /// </summary>
    /// <param name="frequencyUnit">The frequency unit.</param>
    /// <param name="parameterType">The parameter type.</param>
    /// <param name="dataFormat">The data format.</param>
    /// <param name="referenceImpedance">The reference impedance in ohms.</param>
    public TouchstoneOptions(
        FrequencyUnit frequencyUnit = FrequencyUnit.GHz,
        ParameterType parameterType = ParameterType.S,
        DataFormat dataFormat = DataFormat.MagnitudeAngle,
        double referenceImpedance = 50.0)
    {
        FrequencyUnit = frequencyUnit;
        ParameterType = parameterType;
        DataFormat = dataFormat;
        ReferenceImpedance = referenceImpedance;
    }

    /// <summary>
    /// Gets the default Touchstone options (GHz, S, MA, R 50).
    /// </summary>
    public static TouchstoneOptions Default { get; } = new TouchstoneOptions();

    /// <inheritdoc/>
    public override string ToString() =>
        $"# {FrequencyUnit.ToString().ToUpperInvariant()} {ParameterType} {FormatToString(DataFormat)} R {ReferenceImpedance}";

    private static string FormatToString(DataFormat format) => format switch
    {
        DataFormat.DecibelAngle => "DB",
        DataFormat.MagnitudeAngle => "MA",
        DataFormat.RealImaginary => "RI",
        _ => "MA"
    };
}
