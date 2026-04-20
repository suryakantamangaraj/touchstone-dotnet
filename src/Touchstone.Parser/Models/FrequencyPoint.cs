namespace Touchstone.Parser.Models;

/// <summary>
/// Represents the network parameter data at a single frequency point.
/// Contains the frequency value (in Hz) and the N×N matrix of parameter values.
/// </summary>
public sealed class FrequencyPoint
{
    private readonly NetworkParameter[,] _parameters;

    /// <summary>
    /// Gets the frequency in Hertz.
    /// </summary>
    public double FrequencyHz { get; }

    /// <summary>
    /// Gets the number of ports (N) in this measurement.
    /// </summary>
    public int NumberOfPorts { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FrequencyPoint"/> class.
    /// </summary>
    /// <param name="frequencyHz">The frequency in Hertz.</param>
    /// <param name="parameters">The N×N matrix of network parameter values.</param>
    /// <exception cref="ArgumentException">Thrown when the parameter matrix is not square.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when frequency is negative.</exception>
    public FrequencyPoint(double frequencyHz, NetworkParameter[,] parameters)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));
        if (frequencyHz < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frequencyHz), "Frequency must be non-negative.");
        }

        if (parameters.GetLength(0) != parameters.GetLength(1))
        {
            throw new ArgumentException("Parameter matrix must be square (N×N).", nameof(parameters));
        }

        FrequencyHz = frequencyHz;
        _parameters = (NetworkParameter[,])parameters.Clone();
        NumberOfPorts = _parameters.GetLength(0);
    }

    /// <summary>
    /// Gets the network parameter value for the specified port indices.
    /// </summary>
    /// <param name="row">The output port index (0-based).</param>
    /// <param name="col">The input port index (0-based).</param>
    /// <returns>The <see cref="NetworkParameter"/> at the specified port indices.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when indices are out of range.</exception>
    public NetworkParameter this[int row, int col]
    {
        get
        {
            ValidateIndex(row, nameof(row));
            ValidateIndex(col, nameof(col));
            return _parameters[row, col];
        }
    }

    /// <summary>
    /// Gets the full N×N parameter matrix as a cloned 2D array.
    /// </summary>
    /// <returns>A copy of the parameter matrix.</returns>
    public NetworkParameter[,] GetParameterMatrix() => (NetworkParameter[,])_parameters.Clone();

    /// <inheritdoc/>
    public override string ToString() =>
        $"f={FrequencyHz:G6} Hz, {NumberOfPorts}-port";

    private void ValidateIndex(int index, string paramName)
    {
        if (index < 0 || index >= NumberOfPorts)
        {
            throw new ArgumentOutOfRangeException(paramName,
                $"Index must be between 0 and {NumberOfPorts - 1}.");
        }
    }
}
