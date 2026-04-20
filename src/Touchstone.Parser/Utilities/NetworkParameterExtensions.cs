using Touchstone.Parser.Models;

namespace Touchstone.Parser.Utilities;

/// <summary>
/// Extension methods for <see cref="NetworkParameter"/> providing
/// convenient access to magnitude, phase, and complex arithmetic.
/// </summary>
public static class NetworkParameterExtensions
{
    /// <summary>
    /// Converts the parameter to decibel magnitude.
    /// Equivalent to <see cref="NetworkParameter.MagnitudeDb"/>.
    /// </summary>
    /// <param name="parameter">The network parameter.</param>
    /// <returns>The magnitude in dB.</returns>
    public static double ToDecibels(this NetworkParameter parameter) =>
        parameter.MagnitudeDb;

    /// <summary>
    /// Converts the parameter to linear magnitude.
    /// Equivalent to <see cref="NetworkParameter.Magnitude"/>.
    /// </summary>
    /// <param name="parameter">The network parameter.</param>
    /// <returns>The linear magnitude.</returns>
    public static double ToMagnitude(this NetworkParameter parameter) =>
        parameter.Magnitude;

    /// <summary>
    /// Gets the phase angle in radians.
    /// Equivalent to <see cref="NetworkParameter.PhaseRadians"/>.
    /// </summary>
    /// <param name="parameter">The network parameter.</param>
    /// <returns>The phase in radians.</returns>
    public static double ToPhaseRadians(this NetworkParameter parameter) =>
        parameter.PhaseRadians;

    /// <summary>
    /// Gets the phase angle in degrees.
    /// Equivalent to <see cref="NetworkParameter.PhaseDegrees"/>.
    /// </summary>
    /// <param name="parameter">The network parameter.</param>
    /// <returns>The phase in degrees.</returns>
    public static double ToPhaseDegrees(this NetworkParameter parameter) =>
        parameter.PhaseDegrees;

    /// <summary>
    /// Adds two network parameters (complex addition).
    /// </summary>
    /// <param name="a">The first parameter.</param>
    /// <param name="b">The second parameter.</param>
    /// <returns>The sum as a new <see cref="NetworkParameter"/>.</returns>
    public static NetworkParameter Add(this NetworkParameter a, NetworkParameter b) =>
        new NetworkParameter(a.Real + b.Real, a.Imaginary + b.Imaginary);

    /// <summary>
    /// Subtracts one network parameter from another (complex subtraction).
    /// </summary>
    /// <param name="a">The first parameter.</param>
    /// <param name="b">The parameter to subtract.</param>
    /// <returns>The difference as a new <see cref="NetworkParameter"/>.</returns>
    public static NetworkParameter Subtract(this NetworkParameter a, NetworkParameter b) =>
        new NetworkParameter(a.Real - b.Real, a.Imaginary - b.Imaginary);

    /// <summary>
    /// Multiplies two network parameters (complex multiplication).
    /// </summary>
    /// <param name="a">The first parameter.</param>
    /// <param name="b">The second parameter.</param>
    /// <returns>The product as a new <see cref="NetworkParameter"/>.</returns>
    public static NetworkParameter Multiply(this NetworkParameter a, NetworkParameter b) =>
        new NetworkParameter(
            a.Real * b.Real - a.Imaginary * b.Imaginary,
            a.Real * b.Imaginary + a.Imaginary * b.Real);

    /// <summary>
    /// Checks if two network parameters are approximately equal within a tolerance.
    /// </summary>
    /// <param name="a">The first parameter.</param>
    /// <param name="b">The second parameter.</param>
    /// <param name="tolerance">The maximum allowed difference per component.</param>
    /// <returns>True if the parameters are approximately equal.</returns>
    public static bool ApproximatelyEquals(this NetworkParameter a, NetworkParameter b, double tolerance = 1e-10) =>
        Math.Abs(a.Real - b.Real) <= tolerance && Math.Abs(a.Imaginary - b.Imaginary) <= tolerance;
}
