using System.Globalization;
using System.Text.RegularExpressions;
using Touchstone.Parser.Models;
using Touchstone.Parser.Utilities;

namespace Touchstone.Parser.Parsing;

/// <summary>
/// Parses Touchstone (.sNp) files into strongly typed <see cref="TouchstoneData"/> objects.
/// Supports Touchstone v1.0/v1.1 format for 1-port through N-port networks.
/// </summary>
/// <remarks>
/// <para>
/// The parser handles all three data formats (RI, MA, DB), all frequency units
/// (Hz, kHz, MHz, GHz), and all parameter types (S, Y, Z, H, G).
/// </para>
/// <para>
/// All frequency values are normalized to Hertz and all parameter values are
/// normalized to real/imaginary representation internally.
/// </para>
/// </remarks>
public static class TouchstoneParser
{
    private static readonly Regex portCountRegex = new Regex(
        @"\.s(\d+)p$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Parses a Touchstone file from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the .sNp file.</param>
    /// <returns>A <see cref="TouchstoneData"/> instance containing the parsed data.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="TouchstoneParserException">Thrown when the file content is invalid.</exception>
    public static TouchstoneData Parse(string filePath)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Touchstone file not found: '{filePath}'.", filePath);
        }

        string fileName = Path.GetFileName(filePath);
        int portCount = DetectPortCount(fileName);

        using var reader = new StreamReader(filePath);
        return ParseInternal(reader, portCount, fileName);
    }

    /// <summary>
    /// Parses a Touchstone file from a stream.
    /// </summary>
    /// <param name="stream">The stream containing the Touchstone data.</param>
    /// <param name="fileName">Optional file name for port count detection and metadata.</param>
    /// <returns>A <see cref="TouchstoneData"/> instance containing the parsed data.</returns>
    /// <exception cref="TouchstoneParserException">Thrown when the content is invalid.</exception>
    public static TouchstoneData Parse(Stream stream, string? fileName = null)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));
        int portCount = fileName != null ? DetectPortCount(fileName) : 0;
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, true, 1024, leaveOpen: true);
        return ParseInternal(reader, portCount, fileName);
    }

    /// <summary>
    /// Parses Touchstone data from a <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The text reader providing the Touchstone data.</param>
    /// <param name="fileName">Optional file name for port count detection and metadata.</param>
    /// <returns>A <see cref="TouchstoneData"/> instance containing the parsed data.</returns>
    /// <exception cref="TouchstoneParserException">Thrown when the content is invalid.</exception>
    public static TouchstoneData Parse(TextReader reader, string? fileName = null)
    {
        if (reader == null) throw new ArgumentNullException(nameof(reader));
        int portCount = fileName != null ? DetectPortCount(fileName) : 0;
        return ParseInternal(reader, portCount, fileName);
    }

    /// <summary>
    /// Asynchronously parses a Touchstone file from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the .sNp file.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A <see cref="TouchstoneData"/> instance containing the parsed data.</returns>
    public static async Task<TouchstoneData> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (filePath == null) throw new ArgumentNullException(nameof(filePath));

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Touchstone file not found: '{filePath}'.", filePath);
        }

        string fileName = Path.GetFileName(filePath);
        int portCount = DetectPortCount(fileName);

        // Read all lines asynchronously, then parse
        string[] lines = await ReadAllLinesAsync(filePath, cancellationToken).ConfigureAwait(false);
        using var reader = new StringReader(string.Join("\n", lines));
        return ParseInternal(reader, portCount, fileName);
    }

    /// <summary>
    /// Parses Touchstone data from a raw string.
    /// </summary>
    /// <param name="content">The Touchstone file content as a string.</param>
    /// <param name="fileName">Optional file name for port count detection and metadata.</param>
    /// <returns>A <see cref="TouchstoneData"/> instance containing the parsed data.</returns>
    public static TouchstoneData ParseString(string content, string? fileName = null)
    {
        if (content == null) throw new ArgumentNullException(nameof(content));
        int portCount = fileName != null ? DetectPortCount(fileName) : 0;
        using var reader = new StringReader(content);
        return ParseInternal(reader, portCount, fileName);
    }

    /// <summary>
    /// Detects the number of ports from the file extension.
    /// </summary>
    /// <param name="fileName">The file name (e.g., "filter.s2p").</param>
    /// <returns>The number of ports, or 0 if it cannot be determined.</returns>
    public static int DetectPortCount(string fileName)
    {
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));
        var match = portCountRegex.Match(fileName);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int ports))
        {
            return ports;
        }
        return 0;
    }

    private static TouchstoneData ParseInternal(TextReader reader, int portCount, string? fileName)
    {
        var comments = new List<string>();
        TouchstoneOptions? options = null;
        var dataLines = new List<(string Line, int LineNumber)>();

        string? line;
        int lineNumber = 0;
        bool optionLineParsed = false;

        while ((line = reader.ReadLine()) != null)
        {
            lineNumber++;
            string trimmed = line.Trim();

            // Skip empty lines
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            // Comment line
            if (trimmed.StartsWith("!", StringComparison.Ordinal))
            {
                comments.Add(trimmed.Substring(1).Trim());
                continue;
            }

            // Option line
            if (trimmed.StartsWith("#", StringComparison.Ordinal))
            {
                if (optionLineParsed)
                {
                    throw new TouchstoneParserException(
                        "Multiple option lines found. Only one '#' line is allowed.", lineNumber);
                }
                options = OptionLineParser.Parse(trimmed, lineNumber);
                optionLineParsed = true;
                continue;
            }

            // Data line — collect for batch processing
            dataLines.Add((trimmed, lineNumber));
        }

        // Use default options if none found
        options ??= TouchstoneOptions.Default;

        // Parse data lines
        var frequencyPoints = ParseDataLines(dataLines, options, portCount);

        // Infer port count from data if not detected from filename
        if (portCount == 0 && frequencyPoints.Count > 0)
        {
            portCount = frequencyPoints[0].NumberOfPorts;
        }

        if (portCount == 0)
        {
            throw new TouchstoneParserException("Unable to determine the number of ports.");
        }

        return new TouchstoneData(options, portCount, frequencyPoints, comments, fileName);
    }

    private static List<FrequencyPoint> ParseDataLines(
        List<(string Line, int LineNumber)> dataLines,
        TouchstoneOptions options,
        int portCount)
    {
        if (dataLines.Count == 0)
        {
            return new List<FrequencyPoint>();
        }

        var tokenizer = new DataLineTokenizer();
        var frequencyPoints = new List<FrequencyPoint>();

        // Feed all lines into the tokenizer
        foreach (var (line, lineNum) in dataLines)
        {
            tokenizer.AddLine(line, lineNum);
        }

        // Determine port count from data if not known
        if (portCount == 0)
        {
            portCount = InferPortCount(tokenizer.Count, dataLines.Count);
        }

        // Number of parameter pairs per frequency point = N*N
        int pairsPerPoint = portCount * portCount;
        // Total values per frequency point: 1 (frequency) + 2*N*N (pairs of real/imag or mag/angle)
        int valuesPerPoint = 1 + 2 * pairsPerPoint;

        while (tokenizer.HasValues(valuesPerPoint))
        {
            double[] values = tokenizer.Consume(valuesPerPoint);

            // First value is the frequency
            double rawFrequency = values[0];
            double frequencyHz = FrequencyConverter.ToHz(rawFrequency, options.FrequencyUnit);

            // Parse the N*N parameter matrix
            var parameters = new NetworkParameter[portCount, portCount];

            if (portCount == 2)
            {
                // 2-port uses legacy ordering: S11, S21, S12, S22
                parameters[0, 0] = CreateParameter(values[1], values[2], options.DataFormat);
                parameters[1, 0] = CreateParameter(values[3], values[4], options.DataFormat);
                parameters[0, 1] = CreateParameter(values[5], values[6], options.DataFormat);
                parameters[1, 1] = CreateParameter(values[7], values[8], options.DataFormat);
            }
            else
            {
                // 1-port and N-port (N>=3) use row-major ordering
                int valueIndex = 1;
                for (int row = 0; row < portCount; row++)
                {
                    for (int col = 0; col < portCount; col++)
                    {
                        parameters[row, col] = CreateParameter(
                            values[valueIndex], values[valueIndex + 1], options.DataFormat);
                        valueIndex += 2;
                    }
                }
            }

            frequencyPoints.Add(new FrequencyPoint(frequencyHz, parameters));
        }

        return frequencyPoints;
    }

    private static NetworkParameter CreateParameter(double value1, double value2, DataFormat format)
    {
        return format switch
        {
            DataFormat.RealImaginary => NetworkParameter.FromRealImaginary(value1, value2),
            DataFormat.MagnitudeAngle => NetworkParameter.FromMagnitudeAngle(value1, value2),
            DataFormat.DecibelAngle => NetworkParameter.FromDecibelAngle(value1, value2),
            _ => throw new TouchstoneParserException($"Unsupported data format: {format}.")
        };
    }

    private static int InferPortCount(int totalValues, int totalLines)
    {
        // Try common port counts: 1, 2, 3, 4
        // For N ports, each frequency needs 1 + 2*N*N values
        for (int n = 1; n <= 8; n++)
        {
            int valuesPerPoint = 1 + 2 * n * n;
            if (totalValues % valuesPerPoint == 0)
            {
                return n;
            }
        }

        // Default to 2-port if we can't figure it out
        return 2;
    }

    private static async Task<string[]> ReadAllLinesAsync(string filePath, CancellationToken cancellationToken)
    {
        var lines = new List<string>();
        using var reader = new StreamReader(filePath);
        string? line;
        while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lines.Add(line);
        }
        return lines.ToArray();
    }
}
