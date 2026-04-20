namespace Touchstone.Parser.Models;

/// <summary>
/// Represents the complete parsed contents of a Touchstone (.sNp) file.
/// Provides strongly typed access to frequency points, network parameters,
/// options, and metadata.
/// </summary>
/// <remarks>
/// This is the primary output of <see cref="Touchstone.Parser.Parsing.TouchstoneParser"/>.
/// All frequency values are normalized to Hertz internally.
/// All parameter values are normalized to real/imaginary representation internally.
/// </remarks>
public sealed class TouchstoneData
{
    /// <summary>
    /// Gets the option line configuration from the file.
    /// </summary>
    public TouchstoneOptions Options { get; }

    /// <summary>
    /// Gets the number of ports in the network (N in .sNp).
    /// </summary>
    public int NumberOfPorts { get; }

    /// <summary>
    /// Gets the ordered list of frequency points with their parameter data.
    /// Frequencies are in strictly increasing order and normalized to Hertz.
    /// </summary>
    public IReadOnlyList<FrequencyPoint> FrequencyPoints { get; }

    /// <summary>
    /// Gets the comment lines extracted from the file (without the '!' prefix).
    /// </summary>
    public IReadOnlyList<string> Comments { get; }

    /// <summary>
    /// Gets the source file name, if available.
    /// </summary>
    public string? FileName { get; }

    /// <summary>
    /// Gets the number of frequency points in the dataset.
    /// </summary>
    public int Count => FrequencyPoints.Count;

    /// <summary>
    /// Gets an enumerable of all frequency values in Hertz.
    /// </summary>
    public IEnumerable<double> Frequencies => FrequencyPoints.Select(fp => fp.FrequencyHz);

    /// <summary>
    /// Initializes a new instance of the <see cref="TouchstoneData"/> class.
    /// </summary>
    /// <param name="options">The parsed option line configuration.</param>
    /// <param name="numberOfPorts">The number of ports.</param>
    /// <param name="frequencyPoints">The list of frequency points.</param>
    /// <param name="comments">The comment lines.</param>
    /// <param name="fileName">The optional source file name.</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when numberOfPorts is less than 1.</exception>
    public TouchstoneData(
        TouchstoneOptions options,
        int numberOfPorts,
        IReadOnlyList<FrequencyPoint> frequencyPoints,
        IReadOnlyList<string> comments,
        string? fileName = null)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        FrequencyPoints = frequencyPoints ?? throw new ArgumentNullException(nameof(frequencyPoints));
        Comments = comments ?? throw new ArgumentNullException(nameof(comments));

        if (numberOfPorts < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfPorts), "Number of ports must be at least 1.");
        }

        NumberOfPorts = numberOfPorts;
        FileName = fileName;
    }

    /// <summary>
    /// Returns the parameter data for a specific S‑parameter (e.g., S11, S21)
    /// across all frequency points.
    /// </summary>
    /// <param name="row">The output port index (0-based).</param>
    /// <param name="col">The input port index (0-based).</param>
    /// <returns>
    /// An enumerable of tuples containing the frequency in Hz and the
    /// <see cref="NetworkParameter"/> value at each frequency point.
    /// </returns>
    public IEnumerable<(double FrequencyHz, NetworkParameter Value)> GetParameter(int row, int col)
    {
        return FrequencyPoints.Select(fp => (fp.FrequencyHz, fp[row, col]));
    }

    /// <summary>
    /// Gets the <see cref="FrequencyPoint"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index.</param>
    /// <returns>The frequency point at the specified index.</returns>
    public FrequencyPoint this[int index] => FrequencyPoints[index];

    /// <inheritdoc/>
    public override string ToString() =>
        $"Touchstone {NumberOfPorts}-port, {Count} points" +
        (FileName != null ? $", file: {FileName}" : string.Empty);
}
