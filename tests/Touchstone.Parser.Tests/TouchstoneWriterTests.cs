using FluentAssertions;
using Touchstone.Parser.Models;
using Touchstone.Parser.Utilities;
using Xunit;

namespace Touchstone.Parser.Tests;

public class TouchstoneWriterTests
{
    [Fact]
    public void RoundTrip_1Port_RI()
    {
        var original = Parsing.TouchstoneParser.Parse(
            Path.Combine("TestData", "simple.s1p"));

        string output = TouchstoneWriter.WriteToString(original);
        var reparsed = Parsing.TouchstoneParser.ParseString(output, "round.s1p");

        reparsed.NumberOfPorts.Should().Be(original.NumberOfPorts);
        reparsed.Count.Should().Be(original.Count);

        for (int i = 0; i < original.Count; i++)
        {
            reparsed.FrequencyPoints[i].FrequencyHz
                .Should().BeApproximately(original.FrequencyPoints[i].FrequencyHz, 1.0);
            reparsed.FrequencyPoints[i][0, 0]
                .ApproximatelyEquals(original.FrequencyPoints[i][0, 0], 1e-4)
                .Should().BeTrue();
        }
    }

    [Fact]
    public void RoundTrip_2Port_DB()
    {
        var original = Parsing.TouchstoneParser.Parse(
            Path.Combine("TestData", "bandpass_filter.s2p"));

        string output = TouchstoneWriter.WriteToString(original);
        var reparsed = Parsing.TouchstoneParser.ParseString(output, "round.s2p");

        reparsed.NumberOfPorts.Should().Be(2);
        reparsed.Count.Should().Be(original.Count);

        for (int i = 0; i < original.Count; i++)
        {
            var origPt = original.FrequencyPoints[i];
            var rePt = reparsed.FrequencyPoints[i];
            rePt[0, 0].MagnitudeDb.Should().BeApproximately(origPt[0, 0].MagnitudeDb, 0.1);
            rePt[1, 0].MagnitudeDb.Should().BeApproximately(origPt[1, 0].MagnitudeDb, 0.1);
        }
    }

    [Fact]
    public void Write_WithDifferentOptions_ConvertsFormat()
    {
        var data = Parsing.TouchstoneParser.Parse(
            Path.Combine("TestData", "amplifier.s2p"));

        var outputOptions = new TouchstoneOptions(
            FrequencyUnit.MHz, ParameterType.S, DataFormat.MagnitudeAngle, 50.0);

        string output = TouchstoneWriter.WriteToString(data, outputOptions);

        output.Should().Contain("# MHZ S MA R 50");
    }

    [Fact]
    public void Write_PreservesComments()
    {
        var data = Parsing.TouchstoneParser.Parse(
            Path.Combine("TestData", "comments_only.s2p"));

        string output = TouchstoneWriter.WriteToString(data);

        output.Should().Contain("! Keysight PNA-X N5247B");
    }

    [Fact]
    public void RoundTrip_3Port()
    {
        var original = Parsing.TouchstoneParser.Parse(
            Path.Combine("TestData", "coupler.s3p"));

        string output = TouchstoneWriter.WriteToString(original);
        var reparsed = Parsing.TouchstoneParser.ParseString(output, "round.s3p");

        reparsed.NumberOfPorts.Should().Be(3);
        reparsed.Count.Should().Be(original.Count);
    }
}
