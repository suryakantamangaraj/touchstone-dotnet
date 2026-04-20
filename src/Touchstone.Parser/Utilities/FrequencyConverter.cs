using Touchstone.Parser.Models;

namespace Touchstone.Parser.Utilities;

/// <summary>
/// Provides methods for converting frequency values between different units.
/// </summary>
public static class FrequencyConverter
{
    /// <summary>
    /// Converts a frequency value from one unit to another.
    /// </summary>
    /// <param name="value">The frequency value to convert.</param>
    /// <param name="from">The source frequency unit.</param>
    /// <param name="to">The target frequency unit.</param>
    /// <returns>The converted frequency value.</returns>
    public static double Convert(double value, FrequencyUnit from, FrequencyUnit to)
    {
        if (from == to)
        {
            return value;
        }

        // Convert to Hz first, then to the target unit
        double hz = ToHz(value, from);
        return FromHz(hz, to);
    }

    /// <summary>
    /// Converts a frequency value from the specified unit to Hertz.
    /// </summary>
    /// <param name="value">The frequency value.</param>
    /// <param name="unit">The source frequency unit.</param>
    /// <returns>The frequency in Hertz.</returns>
    public static double ToHz(double value, FrequencyUnit unit)
    {
        return value * GetMultiplier(unit);
    }

    /// <summary>
    /// Converts a frequency value from Hertz to the specified unit.
    /// </summary>
    /// <param name="value">The frequency in Hertz.</param>
    /// <param name="unit">The target frequency unit.</param>
    /// <returns>The frequency in the target unit.</returns>
    public static double FromHz(double value, FrequencyUnit unit)
    {
        return value / GetMultiplier(unit);
    }

    /// <summary>
    /// Gets the multiplier to convert from the specified unit to Hertz.
    /// </summary>
    /// <param name="unit">The frequency unit.</param>
    /// <returns>The Hz multiplier.</returns>
    public static double GetMultiplier(FrequencyUnit unit)
    {
        return unit switch
        {
            FrequencyUnit.Hz => 1.0,
            FrequencyUnit.KHz => 1.0e3,
            FrequencyUnit.MHz => 1.0e6,
            FrequencyUnit.GHz => 1.0e9,
            _ => throw new ArgumentOutOfRangeException(nameof(unit), $"Unknown frequency unit: {unit}.")
        };
    }
}
