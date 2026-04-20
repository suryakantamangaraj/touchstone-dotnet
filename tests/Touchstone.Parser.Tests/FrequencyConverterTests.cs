using FluentAssertions;
using Touchstone.Parser.Models;
using Touchstone.Parser.Utilities;
using Xunit;

namespace Touchstone.Parser.Tests;

public class FrequencyConverterTests
{
    [Fact]
    public void Convert_GhzToHz()
    {
        FrequencyConverter.Convert(1.0, FrequencyUnit.GHz, FrequencyUnit.Hz)
            .Should().Be(1e9);
    }

    [Fact]
    public void Convert_MhzToGhz()
    {
        FrequencyConverter.Convert(1000.0, FrequencyUnit.MHz, FrequencyUnit.GHz)
            .Should().BeApproximately(1.0, 1e-10);
    }

    [Fact]
    public void Convert_HzToKhz()
    {
        FrequencyConverter.Convert(1000.0, FrequencyUnit.Hz, FrequencyUnit.KHz)
            .Should().Be(1.0);
    }

    [Fact]
    public void Convert_SameUnit_ReturnsSame()
    {
        FrequencyConverter.Convert(42.0, FrequencyUnit.MHz, FrequencyUnit.MHz)
            .Should().Be(42.0);
    }

    [Fact]
    public void ToHz_GHz()
    {
        FrequencyConverter.ToHz(2.4, FrequencyUnit.GHz).Should().Be(2.4e9);
    }

    [Fact]
    public void FromHz_MHz()
    {
        FrequencyConverter.FromHz(100e6, FrequencyUnit.MHz).Should().Be(100.0);
    }

    [Fact]
    public void GetMultiplier_AllUnits()
    {
        FrequencyConverter.GetMultiplier(FrequencyUnit.Hz).Should().Be(1.0);
        FrequencyConverter.GetMultiplier(FrequencyUnit.KHz).Should().Be(1e3);
        FrequencyConverter.GetMultiplier(FrequencyUnit.MHz).Should().Be(1e6);
        FrequencyConverter.GetMultiplier(FrequencyUnit.GHz).Should().Be(1e9);
    }
}
