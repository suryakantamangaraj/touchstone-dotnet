namespace Touchstone.Parser.Models;

/// <summary>
/// Represents a single complex network parameter value (e.g., S11, S21).
/// Internally stored as real and imaginary parts for lossless conversion.
/// </summary>
/// <remarks>
/// All factory methods normalize input to the internal real/imaginary representation.
/// Use the computed properties to access magnitude, phase, and dB values.
/// </remarks>
public readonly struct NetworkParameter : IEquatable<NetworkParameter>
{
    /// <summary>
    /// Gets the real part of the complex parameter.
    /// </summary>
    public double Real { get; }

    /// <summary>
    /// Gets the imaginary part of the complex parameter.
    /// </summary>
    public double Imaginary { get; }

    /// <summary>
    /// Gets the linear magnitude: |S| = sqrt(Re² + Im²).
    /// </summary>
    public double Magnitude => Math.Sqrt(Real * Real + Imaginary * Imaginary);

    /// <summary>
    /// Gets the magnitude in decibels: 20 * log10(|S|).
    /// </summary>
    public double MagnitudeDb
    {
        get
        {
            double mag = Magnitude;
            return mag > 0 ? 20.0 * Math.Log10(mag) : double.NegativeInfinity;
        }
    }

    /// <summary>
    /// Gets the phase angle in degrees.
    /// </summary>
    public double PhaseDegrees => Math.Atan2(Imaginary, Real) * (180.0 / Math.PI);

    /// <summary>
    /// Gets the phase angle in radians.
    /// </summary>
    public double PhaseRadians => Math.Atan2(Imaginary, Real);

    /// <summary>
    /// Initializes a new <see cref="NetworkParameter"/> from real and imaginary components.
    /// </summary>
    /// <param name="real">The real part.</param>
    /// <param name="imaginary">The imaginary part.</param>
    public NetworkParameter(double real, double imaginary)
    {
        Real = real;
        Imaginary = imaginary;
    }

    /// <summary>
    /// Creates a <see cref="NetworkParameter"/> from real and imaginary parts.
    /// </summary>
    /// <param name="real">The real part.</param>
    /// <param name="imaginary">The imaginary part.</param>
    /// <returns>A new <see cref="NetworkParameter"/>.</returns>
    public static NetworkParameter FromRealImaginary(double real, double imaginary) =>
        new NetworkParameter(real, imaginary);

    /// <summary>
    /// Creates a <see cref="NetworkParameter"/> from magnitude and angle (in degrees).
    /// </summary>
    /// <param name="magnitude">The linear magnitude.</param>
    /// <param name="angleDegrees">The angle in degrees.</param>
    /// <returns>A new <see cref="NetworkParameter"/>.</returns>
    public static NetworkParameter FromMagnitudeAngle(double magnitude, double angleDegrees)
    {
        double angleRadians = angleDegrees * (Math.PI / 180.0);
        return new NetworkParameter(
            magnitude * Math.Cos(angleRadians),
            magnitude * Math.Sin(angleRadians));
    }

    /// <summary>
    /// Creates a <see cref="NetworkParameter"/> from decibel magnitude and angle (in degrees).
    /// </summary>
    /// <param name="magnitudeDb">The magnitude in dB.</param>
    /// <param name="angleDegrees">The angle in degrees.</param>
    /// <returns>A new <see cref="NetworkParameter"/>.</returns>
    public static NetworkParameter FromDecibelAngle(double magnitudeDb, double angleDegrees)
    {
        double magnitude = Math.Pow(10.0, magnitudeDb / 20.0);
        return FromMagnitudeAngle(magnitude, angleDegrees);
    }

    /// <summary>
    /// Returns the complex conjugate of this parameter.
    /// </summary>
    /// <returns>A new <see cref="NetworkParameter"/> with negated imaginary part.</returns>
    public NetworkParameter Conjugate() => new NetworkParameter(Real, -Imaginary);

    /// <summary>
    /// Returns the reciprocal (1/z) of this parameter.
    /// </summary>
    /// <returns>A new <see cref="NetworkParameter"/> representing the reciprocal.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the parameter is zero.</exception>
    public NetworkParameter Reciprocal()
    {
        double denominator = Real * Real + Imaginary * Imaginary;
        if (denominator == 0)
        {
            throw new InvalidOperationException("Cannot compute reciprocal of a zero-valued parameter.");
        }

        return new NetworkParameter(Real / denominator, -Imaginary / denominator);
    }

    /// <summary>
    /// Returns a zero-valued network parameter.
    /// </summary>
    public static NetworkParameter Zero { get; } = new NetworkParameter(0, 0);

    /// <inheritdoc/>
    public bool Equals(NetworkParameter other) =>
        Real.Equals(other.Real) && Imaginary.Equals(other.Imaginary);

    /// <inheritdoc/>
    public override bool Equals(object? obj) =>
        obj is NetworkParameter other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Real, Imaginary);

    /// <summary>
    /// Determines whether two <see cref="NetworkParameter"/> values are equal.
    /// </summary>
    public static bool operator ==(NetworkParameter left, NetworkParameter right) => left.Equals(right);

    /// <summary>
    /// Determines whether two <see cref="NetworkParameter"/> values are not equal.
    /// </summary>
    public static bool operator !=(NetworkParameter left, NetworkParameter right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"({Real:G6}, {Imaginary:G6}j)";
}
