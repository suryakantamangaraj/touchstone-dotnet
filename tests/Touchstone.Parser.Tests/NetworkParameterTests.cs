using FluentAssertions;
using Touchstone.Parser.Models;
using Touchstone.Parser.Utilities;
using Xunit;

namespace Touchstone.Parser.Tests;

public class NetworkParameterTests
{
    private const double tolerance = 1e-10;

    [Fact]
    public void FromRealImaginary_StoresCorrectly()
    {
        var p = NetworkParameter.FromRealImaginary(3.0, 4.0);
        p.Real.Should().Be(3.0);
        p.Imaginary.Should().Be(4.0);
    }

    [Fact]
    public void FromMagnitudeAngle_0Degrees()
    {
        var p = NetworkParameter.FromMagnitudeAngle(1.0, 0.0);
        p.Real.Should().BeApproximately(1.0, tolerance);
        p.Imaginary.Should().BeApproximately(0.0, tolerance);
    }

    [Fact]
    public void FromMagnitudeAngle_90Degrees()
    {
        var p = NetworkParameter.FromMagnitudeAngle(1.0, 90.0);
        p.Real.Should().BeApproximately(0.0, 1e-6);
        p.Imaginary.Should().BeApproximately(1.0, 1e-6);
    }

    [Fact]
    public void FromDecibelAngle_0dB()
    {
        var p = NetworkParameter.FromDecibelAngle(0.0, 0.0);
        p.Real.Should().BeApproximately(1.0, tolerance);
        p.Imaginary.Should().BeApproximately(0.0, tolerance);
    }

    [Fact]
    public void FromDecibelAngle_Minus20dB()
    {
        var p = NetworkParameter.FromDecibelAngle(-20.0, 0.0);
        p.Magnitude.Should().BeApproximately(0.1, 1e-6);
    }

    [Fact]
    public void Magnitude_3_4_Returns5()
    {
        var p = new NetworkParameter(3.0, 4.0);
        p.Magnitude.Should().BeApproximately(5.0, tolerance);
    }

    [Fact]
    public void MagnitudeDb_Unit_Returns0()
    {
        var p = NetworkParameter.FromMagnitudeAngle(1.0, 45.0);
        p.MagnitudeDb.Should().BeApproximately(0.0, 1e-6);
    }

    [Fact]
    public void MagnitudeDb_Zero_ReturnsNegInfinity()
    {
        NetworkParameter.Zero.MagnitudeDb.Should().Be(double.NegativeInfinity);
    }

    [Fact]
    public void PhaseDegrees_90()
    {
        new NetworkParameter(0.0, 1.0).PhaseDegrees.Should().BeApproximately(90.0, 1e-6);
    }

    [Theory]
    [InlineData(0.5, 0.3)]
    [InlineData(-0.2, 0.8)]
    [InlineData(1.0, 0.0)]
    public void RoundTrip_RI_MA_RI(double re, double im)
    {
        var orig = new NetworkParameter(re, im);
        var rt = NetworkParameter.FromMagnitudeAngle(orig.Magnitude, orig.PhaseDegrees);
        rt.Real.Should().BeApproximately(re, 1e-10);
        rt.Imaginary.Should().BeApproximately(im, 1e-10);
    }

    [Theory]
    [InlineData(0.5, 0.3)]
    [InlineData(-0.2, 0.8)]
    public void RoundTrip_RI_DB_RI(double re, double im)
    {
        var orig = new NetworkParameter(re, im);
        var rt = NetworkParameter.FromDecibelAngle(orig.MagnitudeDb, orig.PhaseDegrees);
        rt.Real.Should().BeApproximately(re, 1e-6);
        rt.Imaginary.Should().BeApproximately(im, 1e-6);
    }

    [Fact]
    public void Conjugate_NegatesImaginary()
    {
        var c = new NetworkParameter(3.0, 4.0).Conjugate();
        c.Real.Should().Be(3.0);
        c.Imaginary.Should().Be(-4.0);
    }

    [Fact]
    public void Reciprocal_Correct()
    {
        var r = new NetworkParameter(3.0, 4.0).Reciprocal();
        r.Real.Should().BeApproximately(0.12, tolerance);
        r.Imaginary.Should().BeApproximately(-0.16, tolerance);
    }

    [Fact]
    public void Reciprocal_Zero_Throws()
    {
        var act = () => NetworkParameter.Zero.Reciprocal();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Add_ReturnsSum()
    {
        var r = new NetworkParameter(1.0, 2.0).Add(new NetworkParameter(3.0, 4.0));
        r.Real.Should().Be(4.0);
        r.Imaginary.Should().Be(6.0);
    }

    [Fact]
    public void Multiply_ReturnsProduct()
    {
        var r = new NetworkParameter(1.0, 2.0).Multiply(new NetworkParameter(3.0, 4.0));
        r.Real.Should().BeApproximately(-5.0, tolerance);
        r.Imaginary.Should().BeApproximately(10.0, tolerance);
    }

    [Fact]
    public void Equality_Same()
    {
        var a = new NetworkParameter(1.0, 2.0);
        var b = new NetworkParameter(1.0, 2.0);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void ApproximatelyEquals_Close()
    {
        var a = new NetworkParameter(1.0, 2.0);
        var b = new NetworkParameter(1.0 + 1e-12, 2.0 - 1e-12);
        a.ApproximatelyEquals(b).Should().BeTrue();
    }
}
