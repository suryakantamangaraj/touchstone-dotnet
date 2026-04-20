using FluentAssertions;
using Touchstone.Parser.Models;
using Touchstone.Parser.Parsing;
using Xunit;

namespace Touchstone.Parser.Tests;

/// <summary>
/// Tests for <see cref="OptionLineParser"/>.
/// </summary>
public class OptionLineParserTests
{
    [Fact]
    public void Parse_FullOptionLine_ReturnsCorrectOptions()
    {
        var result = OptionLineParser.Parse("# GHZ S DB R 50", 1);

        result.FrequencyUnit.Should().Be(FrequencyUnit.GHz);
        result.ParameterType.Should().Be(ParameterType.S);
        result.DataFormat.Should().Be(DataFormat.DecibelAngle);
        result.ReferenceImpedance.Should().Be(50.0);
    }

    [Fact]
    public void Parse_MhzRiFormat_ReturnsCorrectOptions()
    {
        var result = OptionLineParser.Parse("# MHZ S RI R 75", 1);

        result.FrequencyUnit.Should().Be(FrequencyUnit.MHz);
        result.ParameterType.Should().Be(ParameterType.S);
        result.DataFormat.Should().Be(DataFormat.RealImaginary);
        result.ReferenceImpedance.Should().Be(75.0);
    }

    [Fact]
    public void Parse_KhzMaFormat_ReturnsCorrectOptions()
    {
        var result = OptionLineParser.Parse("# KHZ Y MA R 50", 1);

        result.FrequencyUnit.Should().Be(FrequencyUnit.KHz);
        result.ParameterType.Should().Be(ParameterType.Y);
        result.DataFormat.Should().Be(DataFormat.MagnitudeAngle);
    }

    [Fact]
    public void Parse_HzZParameters_ReturnsCorrectOptions()
    {
        var result = OptionLineParser.Parse("# HZ Z RI R 100", 1);

        result.FrequencyUnit.Should().Be(FrequencyUnit.Hz);
        result.ParameterType.Should().Be(ParameterType.Z);
        result.DataFormat.Should().Be(DataFormat.RealImaginary);
        result.ReferenceImpedance.Should().Be(100.0);
    }

    [Fact]
    public void Parse_LowerCase_ParsesCorrectly()
    {
        var result = OptionLineParser.Parse("# ghz s db r 50", 1);

        result.FrequencyUnit.Should().Be(FrequencyUnit.GHz);
        result.ParameterType.Should().Be(ParameterType.S);
        result.DataFormat.Should().Be(DataFormat.DecibelAngle);
    }

    [Fact]
    public void Parse_WithInlineComment_IgnoresComment()
    {
        var result = OptionLineParser.Parse("# GHZ S MA R 50 ! this is a comment", 1);

        result.FrequencyUnit.Should().Be(FrequencyUnit.GHz);
        result.ParameterType.Should().Be(ParameterType.S);
        result.DataFormat.Should().Be(DataFormat.MagnitudeAngle);
        result.ReferenceImpedance.Should().Be(50.0);
    }

    [Fact]
    public void Parse_MinimalOptionLine_UsesDefaults()
    {
        var result = OptionLineParser.Parse("#", 1);

        result.FrequencyUnit.Should().Be(FrequencyUnit.GHz);
        result.ParameterType.Should().Be(ParameterType.S);
        result.DataFormat.Should().Be(DataFormat.MagnitudeAngle);
        result.ReferenceImpedance.Should().Be(50.0);
    }

    [Fact]
    public void Parse_HybridParameters_ReturnsCorrectType()
    {
        var result = OptionLineParser.Parse("# GHZ H RI R 50", 1);
        result.ParameterType.Should().Be(ParameterType.H);
    }

    [Fact]
    public void Parse_InverseHybridParameters_ReturnsCorrectType()
    {
        var result = OptionLineParser.Parse("# GHZ G MA R 50", 1);
        result.ParameterType.Should().Be(ParameterType.G);
    }

    [Fact]
    public void Parse_InvalidImpedance_ThrowsException()
    {
        var act = () => OptionLineParser.Parse("# GHZ S DB R abc", 1);
        act.Should().Throw<TouchstoneParserException>()
            .Which.Message.Should().Contain("impedance");
    }

    [Fact]
    public void Parse_MissingImpedanceValue_ThrowsException()
    {
        var act = () => OptionLineParser.Parse("# GHZ S DB R", 1);
        act.Should().Throw<TouchstoneParserException>()
            .Which.Message.Should().Contain("impedance");
    }

    [Fact]
    public void Parse_ToString_ProducesValidOptionLine()
    {
        var options = new TouchstoneOptions(FrequencyUnit.GHz, ParameterType.S, DataFormat.DecibelAngle, 50.0);
        string output = options.ToString();

        output.Should().Be("# GHZ S DB R 50");
    }
}
