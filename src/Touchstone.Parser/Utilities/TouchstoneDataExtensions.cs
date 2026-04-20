using System.Globalization;
using Touchstone.Parser.Models;

namespace Touchstone.Parser.Utilities;

/// <summary>
/// LINQ-friendly extension methods for <see cref="TouchstoneData"/>,
/// providing convenient access to common S-parameter queries, RF calculations,
/// and data export functionality.
/// </summary>
public static class TouchstoneDataExtensions
{
    // ──────────────────────────────────────────────────────────────────
    //  Common 2-port parameter shortcuts
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the S11 (input reflection) parameter across all frequency points.
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <returns>An enumerable of (frequency, parameter) tuples.</returns>
    public static IEnumerable<(double FrequencyHz, NetworkParameter Value)> GetS11(this TouchstoneData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.GetParameter(0, 0);
    }

    /// <summary>
    /// Gets the S21 (forward transmission) parameter across all frequency points.
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <returns>An enumerable of (frequency, parameter) tuples.</returns>
    public static IEnumerable<(double FrequencyHz, NetworkParameter Value)> GetS21(this TouchstoneData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.GetParameter(1, 0);
    }

    /// <summary>
    /// Gets the S12 (reverse transmission) parameter across all frequency points.
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <returns>An enumerable of (frequency, parameter) tuples.</returns>
    public static IEnumerable<(double FrequencyHz, NetworkParameter Value)> GetS12(this TouchstoneData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.GetParameter(0, 1);
    }

    /// <summary>
    /// Gets the S22 (output reflection) parameter across all frequency points.
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <returns>An enumerable of (frequency, parameter) tuples.</returns>
    public static IEnumerable<(double FrequencyHz, NetworkParameter Value)> GetS22(this TouchstoneData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.GetParameter(1, 1);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Frequency queries
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets all frequency values converted to the specified unit.
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <param name="unit">The target frequency unit.</param>
    /// <returns>An enumerable of frequency values in the specified unit.</returns>
    public static IEnumerable<double> GetFrequenciesIn(this TouchstoneData data, FrequencyUnit unit)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.FrequencyPoints.Select(fp => FrequencyConverter.FromHz(fp.FrequencyHz, unit));
    }

    /// <summary>
    /// Gets the minimum frequency in Hertz.
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <returns>The minimum frequency.</returns>
    public static double MinFrequencyHz(this TouchstoneData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.FrequencyPoints.Min(fp => fp.FrequencyHz);
    }

    /// <summary>
    /// Gets the maximum frequency in Hertz.
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <returns>The maximum frequency.</returns>
    public static double MaxFrequencyHz(this TouchstoneData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.FrequencyPoints.Max(fp => fp.FrequencyHz);
    }

    // ──────────────────────────────────────────────────────────────────
    //  RF calculations
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Computes the insertion loss (|S21| in dB) across all frequency points.
    /// Insertion loss is typically reported as a positive number (negated S21 in dB).
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <returns>An enumerable of (frequency, insertion loss in dB) tuples.</returns>
    public static IEnumerable<(double FrequencyHz, double InsertionLossDb)> ToInsertionLoss(this TouchstoneData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.GetS21().Select(p => (p.FrequencyHz, -p.Value.MagnitudeDb));
    }

    /// <summary>
    /// Computes the return loss (|S11| in dB) across all frequency points.
    /// Return loss is typically reported as a positive number (negated S11 in dB).
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <returns>An enumerable of (frequency, return loss in dB) tuples.</returns>
    public static IEnumerable<(double FrequencyHz, double ReturnLossDb)> ToReturnLoss(this TouchstoneData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.GetS11().Select(p => (p.FrequencyHz, -p.Value.MagnitudeDb));
    }

    /// <summary>
    /// Computes the Voltage Standing Wave Ratio (VSWR) from S11 across all frequency points.
    /// VSWR = (1 + |S11|) / (1 - |S11|)
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <returns>An enumerable of (frequency, VSWR) tuples.</returns>
    public static IEnumerable<(double FrequencyHz, double Vswr)> ToVswr(this TouchstoneData data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.GetS11().Select(p =>
        {
            double mag = p.Value.Magnitude;
            double vswr = mag >= 1.0 ? double.PositiveInfinity : (1.0 + mag) / (1.0 - mag);
            return (p.FrequencyHz, vswr);
        });
    }

    // ──────────────────────────────────────────────────────────────────
    //  Filtering
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Filters frequency points by a predicate and returns a new <see cref="TouchstoneData"/>
    /// containing only the matching points.
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <param name="predicate">The filter predicate.</param>
    /// <returns>A new <see cref="TouchstoneData"/> with filtered frequency points.</returns>
    public static TouchstoneData Where(this TouchstoneData data, Func<FrequencyPoint, bool> predicate)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var filtered = data.FrequencyPoints.Where(predicate).ToList();
        return new TouchstoneData(data.Options, data.NumberOfPorts, filtered, data.Comments, data.FileName);
    }

    /// <summary>
    /// Filters frequency points to a specific frequency range (inclusive).
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <param name="minHz">The minimum frequency in Hz.</param>
    /// <param name="maxHz">The maximum frequency in Hz.</param>
    /// <returns>A new <see cref="TouchstoneData"/> with filtered frequency points.</returns>
    public static TouchstoneData InFrequencyRange(this TouchstoneData data, double minHz, double maxHz)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        return data.Where(fp => fp.FrequencyHz >= minHz && fp.FrequencyHz <= maxHz);
    }

    // ──────────────────────────────────────────────────────────────────
    //  CSV Export
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Exports the Touchstone data to CSV format.
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <param name="writer">The text writer to write CSV content to.</param>
    /// <param name="frequencyUnit">The frequency unit for the output (default: Hz).</param>
    /// <param name="dataFormat">The data format for the output (default: dB/angle).</param>
    public static void ToCsv(
        this TouchstoneData data,
        TextWriter writer,
        FrequencyUnit frequencyUnit = FrequencyUnit.Hz,
        DataFormat dataFormat = DataFormat.DecibelAngle)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (writer == null) throw new ArgumentNullException(nameof(writer));

        int n = data.NumberOfPorts;
// ... (rest of the file remains same, skipping for brevity in replacement but I'll ensure it's complete)

        // Write header
        var headers = new List<string> { $"Frequency ({frequencyUnit})" };
        for (int row = 0; row < n; row++)
        {
            for (int col = 0; col < n; col++)
            {
                string paramName = $"S{row + 1}{col + 1}";
                switch (dataFormat)
                {
                    case DataFormat.DecibelAngle:
                        headers.Add($"{paramName}_dB");
                        headers.Add($"{paramName}_Deg");
                        break;
                    case DataFormat.MagnitudeAngle:
                        headers.Add($"{paramName}_Mag");
                        headers.Add($"{paramName}_Deg");
                        break;
                    case DataFormat.RealImaginary:
                        headers.Add($"{paramName}_Re");
                        headers.Add($"{paramName}_Im");
                        break;
                }
            }
        }
        writer.WriteLine(string.Join(",", headers));

        // Write data rows
        foreach (var fp in data.FrequencyPoints)
        {
            var values = new List<string>
            {
                FrequencyConverter.FromHz(fp.FrequencyHz, frequencyUnit)
                    .ToString("G10", CultureInfo.InvariantCulture)
            };

            for (int row = 0; row < n; row++)
            {
                for (int col = 0; col < n; col++)
                {
                    var param = fp[row, col];
                    switch (dataFormat)
                    {
                        case DataFormat.DecibelAngle:
                            values.Add(param.MagnitudeDb.ToString("G10", CultureInfo.InvariantCulture));
                            values.Add(param.PhaseDegrees.ToString("G10", CultureInfo.InvariantCulture));
                            break;
                        case DataFormat.MagnitudeAngle:
                            values.Add(param.Magnitude.ToString("G10", CultureInfo.InvariantCulture));
                            values.Add(param.PhaseDegrees.ToString("G10", CultureInfo.InvariantCulture));
                            break;
                        case DataFormat.RealImaginary:
                            values.Add(param.Real.ToString("G10", CultureInfo.InvariantCulture));
                            values.Add(param.Imaginary.ToString("G10", CultureInfo.InvariantCulture));
                            break;
                    }
                }
            }

            writer.WriteLine(string.Join(",", values));
        }
    }

    /// <summary>
    /// Exports the Touchstone data to a CSV string.
    /// </summary>
    /// <param name="data">The Touchstone data.</param>
    /// <param name="frequencyUnit">The frequency unit (default: Hz).</param>
    /// <param name="dataFormat">The data format (default: dB/angle).</param>
    /// <returns>The CSV content as a string.</returns>
    public static string ToCsvString(
        this TouchstoneData data,
        FrequencyUnit frequencyUnit = FrequencyUnit.Hz,
        DataFormat dataFormat = DataFormat.DecibelAngle)
    {
        using var writer = new StringWriter();
        data.ToCsv(writer, frequencyUnit, dataFormat);
        return writer.ToString();
    }
}
