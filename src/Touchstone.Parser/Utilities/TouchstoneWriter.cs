using System.Globalization;
using Touchstone.Parser.Models;

namespace Touchstone.Parser.Utilities;

/// <summary>
/// Writes <see cref="TouchstoneData"/> to Touchstone (.sNp) file format.
/// Produces valid Touchstone v1.0 output.
/// </summary>
public static class TouchstoneWriter
{
    /// <summary>
    /// Writes Touchstone data to a file.
    /// </summary>
    /// <param name="data">The Touchstone data to write.</param>
    /// <param name="filePath">The output file path.</param>
    /// <param name="options">Optional output options. If null, uses the original options from the data.</param>
    public static void Write(TouchstoneData data, string filePath, TouchstoneOptions? options = null)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        using var writer = new StreamWriter(filePath);
        Write(data, writer, options);
    }

    /// <summary>
    /// Writes Touchstone data to a <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="data">The Touchstone data to write.</param>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="options">Optional output options. If null, uses the original options from the data.</param>
    /// <exception cref="ArgumentNullException">Thrown when data or writer is null.</exception>
    public static void Write(TouchstoneData data, TextWriter writer, TouchstoneOptions? options = null)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (writer == null) throw new ArgumentNullException(nameof(writer));

        options ??= data.Options;

        // Write comments
        foreach (string comment in data.Comments)
        {
            writer.WriteLine($"! {comment}");
        }

        // Write option line
        writer.WriteLine(options.ToString());

        // Write data
        int n = data.NumberOfPorts;
        foreach (var fp in data.FrequencyPoints)
        {
            double freq = FrequencyConverter.FromHz(fp.FrequencyHz, options.FrequencyUnit);

            if (n <= 2)
            {
                // 1-port and 2-port: single line per frequency
                var parts = new List<string>
                {
                    FormatValue(freq)
                };

                if (n == 1)
                {
                    AppendParameter(parts, fp[0, 0], options.DataFormat);
                }
                else
                {
                    // 2-port legacy ordering: S11, S21, S12, S22
                    AppendParameter(parts, fp[0, 0], options.DataFormat);
                    AppendParameter(parts, fp[1, 0], options.DataFormat);
                    AppendParameter(parts, fp[0, 1], options.DataFormat);
                    AppendParameter(parts, fp[1, 1], options.DataFormat);
                }

                writer.WriteLine(string.Join(" ", parts));
            }
            else
            {
                // N-port (N >= 3): row-major ordering
                // First line starts with frequency, then as many pairs as fit
                // Subsequent lines for remaining rows
                bool firstRow = true;
                for (int row = 0; row < n; row++)
                {
                    var parts = new List<string>();

                    if (firstRow)
                    {
                        parts.Add(FormatValue(freq));
                        firstRow = false;
                    }

                    for (int col = 0; col < n; col++)
                    {
                        AppendParameter(parts, fp[row, col], options.DataFormat);
                    }

                    writer.WriteLine(string.Join(" ", parts));
                }
            }
        }
    }

    /// <summary>
    /// Writes Touchstone data to a string.
    /// </summary>
    /// <param name="data">The Touchstone data to write.</param>
    /// <param name="options">Optional output options.</param>
    /// <returns>The Touchstone content as a string.</returns>
    public static string WriteToString(TouchstoneData data, TouchstoneOptions? options = null)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));

        using var writer = new StringWriter();
        Write(data, writer, options);
        return writer.ToString();
    }

    private static void AppendParameter(List<string> parts, NetworkParameter param, DataFormat format)
    {
        switch (format)
        {
            case DataFormat.RealImaginary:
                parts.Add(FormatValue(param.Real));
                parts.Add(FormatValue(param.Imaginary));
                break;
            case DataFormat.MagnitudeAngle:
                parts.Add(FormatValue(param.Magnitude));
                parts.Add(FormatValue(param.PhaseDegrees));
                break;
            case DataFormat.DecibelAngle:
                parts.Add(FormatValue(param.MagnitudeDb));
                parts.Add(FormatValue(param.PhaseDegrees));
                break;
        }
    }

    private static string FormatValue(double value) =>
        value.ToString("G10", CultureInfo.InvariantCulture);
}
