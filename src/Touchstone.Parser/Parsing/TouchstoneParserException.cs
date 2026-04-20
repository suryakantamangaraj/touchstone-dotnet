namespace Touchstone.Parser.Parsing;

/// <summary>
/// Exception thrown when a Touchstone file cannot be parsed.
/// Includes the line number where the error occurred, when available.
/// </summary>
public sealed class TouchstoneParserException : Exception
{
    /// <summary>
    /// Gets the line number (1-based) where the parsing error occurred, if known.
    /// </summary>
    public int? LineNumber { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TouchstoneParserException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TouchstoneParserException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TouchstoneParserException"/> class
    /// with a line number.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="lineNumber">The 1-based line number where the error occurred.</param>
    public TouchstoneParserException(string message, int lineNumber)
        : base($"Line {lineNumber}: {message}")
    {
        LineNumber = lineNumber;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TouchstoneParserException"/> class
    /// with a line number and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="lineNumber">The 1-based line number where the error occurred.</param>
    /// <param name="innerException">The inner exception.</param>
    public TouchstoneParserException(string message, int lineNumber, Exception innerException)
        : base($"Line {lineNumber}: {message}", innerException)
    {
        LineNumber = lineNumber;
    }
}
