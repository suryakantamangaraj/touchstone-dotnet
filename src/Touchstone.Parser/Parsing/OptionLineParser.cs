using System.Globalization;
using Touchstone.Parser.Models;

namespace Touchstone.Parser.Parsing;

/// <summary>
/// Parses the Touchstone option line (starting with '#') into a
/// <see cref="TouchstoneOptions"/> instance.
/// </summary>
public static class OptionLineParser
{
    /// <summary>
    /// Parses an option line string into <see cref="TouchstoneOptions"/>.
    /// </summary>
    /// <param name="line">The option line text (with or without the leading '#').</param>
    /// <param name="lineNumber">The 1-based line number for error reporting.</param>
    /// <returns>A <see cref="TouchstoneOptions"/> instance.</returns>
    /// <exception cref="TouchstoneParserException">Thrown when the option line is malformed.</exception>
    public static TouchstoneOptions Parse(string line, int lineNumber)
    {
        // Remove leading '#' and trim
        string content = line.TrimStart();
        if (content.StartsWith("#", StringComparison.Ordinal))
        {
            content = content.Substring(1).Trim();
        }

        // Remove inline comments
        int commentIndex = content.IndexOf('!');
        if (commentIndex >= 0)
        {
            content = content.Substring(0, commentIndex).Trim();
        }

        // Default values per the Touchstone specification
        var frequencyUnit = FrequencyUnit.GHz;
        var parameterType = ParameterType.S;
        var dataFormat = DataFormat.MagnitudeAngle;
        double referenceImpedance = 50.0;

        string[] tokens = content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        int i = 0;
        while (i < tokens.Length)
        {
            string token = tokens[i].ToUpperInvariant();

            // Try to parse as frequency unit
            if (TryParseFrequencyUnit(token, out var parsedFreqUnit))
            {
                frequencyUnit = parsedFreqUnit;
                i++;
                continue;
            }

            // Try to parse as parameter type
            if (TryParseParameterType(token, out var parsedParamType))
            {
                parameterType = parsedParamType;
                i++;
                continue;
            }

            // Try to parse as data format
            if (TryParseDataFormat(token, out var parsedFormat))
            {
                dataFormat = parsedFormat;
                i++;
                continue;
            }

            // Try to parse reference impedance (R <value>)
            if (token == "R")
            {
                i++;
                if (i < tokens.Length)
                {
                    if (double.TryParse(tokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out double impedance))
                    {
                        referenceImpedance = impedance;
                        i++;
                    }
                    else
                    {
                        throw new TouchstoneParserException(
                            $"Invalid reference impedance value: '{tokens[i]}'.", lineNumber);
                    }
                }
                else
                {
                    throw new TouchstoneParserException(
                        "Expected impedance value after 'R' in option line.", lineNumber);
                }
                continue;
            }

            // Unknown token — skip (lenient parsing)
            i++;
        }

        return new TouchstoneOptions(frequencyUnit, parameterType, dataFormat, referenceImpedance);
    }

    private static bool TryParseFrequencyUnit(string token, out FrequencyUnit unit)
    {
        switch (token)
        {
            case "HZ":
                unit = FrequencyUnit.Hz;
                return true;
            case "KHZ":
                unit = FrequencyUnit.KHz;
                return true;
            case "MHZ":
                unit = FrequencyUnit.MHz;
                return true;
            case "GHZ":
                unit = FrequencyUnit.GHz;
                return true;
            default:
                unit = default;
                return false;
        }
    }

    private static bool TryParseParameterType(string token, out ParameterType type)
    {
        switch (token)
        {
            case "S":
                type = ParameterType.S;
                return true;
            case "Y":
                type = ParameterType.Y;
                return true;
            case "Z":
                type = ParameterType.Z;
                return true;
            case "H":
                type = ParameterType.H;
                return true;
            case "G":
                type = ParameterType.G;
                return true;
            default:
                type = default;
                return false;
        }
    }

    private static bool TryParseDataFormat(string token, out DataFormat format)
    {
        switch (token)
        {
            case "DB":
                format = DataFormat.DecibelAngle;
                return true;
            case "MA":
                format = DataFormat.MagnitudeAngle;
                return true;
            case "RI":
                format = DataFormat.RealImaginary;
                return true;
            default:
                format = default;
                return false;
        }
    }
}
