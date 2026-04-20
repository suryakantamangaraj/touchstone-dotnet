using System.Globalization;

namespace Touchstone.Parser.Parsing;

/// <summary>
/// Tokenizes data lines from Touchstone files into numeric values.
/// Handles whitespace separation, inline comments, and continuation across lines.
/// </summary>
internal sealed class DataLineTokenizer
{
    private readonly List<double> buffer = new List<double>();

    /// <summary>
    /// Gets the number of values currently in the buffer.
    /// </summary>
    public int Count => buffer.Count;

    /// <summary>
    /// Adds the numeric tokens from a single data line to the internal buffer.
    /// </summary>
    /// <param name="line">The data line to tokenize.</param>
    /// <param name="lineNumber">The 1-based line number for error reporting.</param>
    /// <exception cref="TouchstoneParserException">Thrown when a non-numeric token is encountered.</exception>
    public void AddLine(string line, int lineNumber)
    {
        if (line == null) throw new ArgumentNullException(nameof(line));

        // Remove inline comments
        int commentIndex = line.IndexOf('!');
        if (commentIndex >= 0)
        {
            line = line.Substring(0, commentIndex);
        }

        string trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return;
        }

        string[] tokens = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        foreach (string token in tokens)
        {
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                buffer.Add(value);
            }
            else
            {
                throw new TouchstoneParserException(
                    $"Invalid numeric value: '{token}'.", lineNumber);
            }
        }
    }

    /// <summary>
    /// Consumes and returns the specified number of values from the front of the buffer.
    /// </summary>
    /// <param name="count">The number of values to consume.</param>
    /// <returns>An array of consumed values.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there are insufficient values.</exception>
    public double[] Consume(int count)
    {
        if (buffer.Count < count)
        {
            throw new InvalidOperationException(
                $"Expected at least {count} values in buffer, but only {buffer.Count} available.");
        }

        double[] result = new double[count];
        buffer.CopyTo(0, result, 0, count);
        buffer.RemoveRange(0, count);
        return result;
    }

    /// <summary>
    /// Peeks at a value at the specified offset without consuming it.
    /// </summary>
    /// <param name="offset">The zero-based offset into the buffer.</param>
    /// <returns>The value at the specified offset.</returns>
    public double Peek(int offset = 0) => buffer[offset];

    /// <summary>
    /// Returns whether the buffer has at least the specified number of values.
    /// </summary>
    /// <param name="count">The minimum number of values.</param>
    /// <returns>True if the buffer has enough values.</returns>
    public bool HasValues(int count) => buffer.Count >= count;

    /// <summary>
    /// Clears all values from the buffer.
    /// </summary>
    public void Clear() => buffer.Clear();
}
