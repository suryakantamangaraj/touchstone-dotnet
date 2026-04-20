using FluentAssertions;
using Touchstone.Parser.Models;
using Touchstone.Parser.Parsing;
using Xunit;

namespace Touchstone.Parser.Tests;

/// <summary>
/// Integration tests for <see cref="Parsing.TouchstoneParser"/>.
/// </summary>
public class TouchstoneParserTests
{
    private static string GetTestDataPath(string fileName) =>
        Path.Combine("TestData", fileName);

    // ──────────────────────────────────────────────────────────────────
    //  1-Port Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_Simple1Port_ParsesCorrectly()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("simple.s1p"));

        data.NumberOfPorts.Should().Be(1);
        data.Count.Should().Be(5);
        data.Options.FrequencyUnit.Should().Be(FrequencyUnit.MHz);
        data.Options.DataFormat.Should().Be(DataFormat.RealImaginary);
        data.Options.ReferenceImpedance.Should().Be(50.0);
        data.FileName.Should().Be("simple.s1p");
    }

    [Fact]
    public void Parse_Simple1Port_FrequenciesAreInHz()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("simple.s1p"));

        // File uses MHz, first point is 100 MHz = 100e6 Hz
        data.FrequencyPoints[0].FrequencyHz.Should().Be(100e6);
        data.FrequencyPoints[1].FrequencyHz.Should().Be(200e6);
        data.FrequencyPoints[4].FrequencyHz.Should().Be(2000e6);
    }

    [Fact]
    public void Parse_Simple1Port_ParameterValuesAreCorrect()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("simple.s1p"));

        // First point: S11 = 0.01 + 0.02j (RI format)
        var s11 = data.FrequencyPoints[0][0, 0];
        s11.Real.Should().BeApproximately(0.01, 1e-10);
        s11.Imaginary.Should().BeApproximately(0.02, 1e-10);
    }

    [Fact]
    public void Parse_Simple1Port_CommentsExtracted()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("simple.s1p"));

        data.Comments.Should().HaveCountGreaterOrEqualTo(3);
        data.Comments[0].Should().Contain("Simple 1-port");
    }

    // ──────────────────────────────────────────────────────────────────
    //  2-Port Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_BandpassFilter_ParsesCorrectly()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("bandpass_filter.s2p"));

        data.NumberOfPorts.Should().Be(2);
        data.Count.Should().Be(6);
        data.Options.FrequencyUnit.Should().Be(FrequencyUnit.GHz);
        data.Options.DataFormat.Should().Be(DataFormat.DecibelAngle);
    }

    [Fact]
    public void Parse_BandpassFilter_2PortOrdering()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("bandpass_filter.s2p"));

        // At 1.0 GHz (3rd point): S11=-25dB, S21=-0.5dB, S12=-0.6dB, S22=-24dB
        var point = data.FrequencyPoints[2];
        point[0, 0].MagnitudeDb.Should().BeApproximately(-25.0, 0.1);  // S11
        point[1, 0].MagnitudeDb.Should().BeApproximately(-0.5, 0.1);   // S21
        point[0, 1].MagnitudeDb.Should().BeApproximately(-0.6, 0.1);   // S12
        point[1, 1].MagnitudeDb.Should().BeApproximately(-24.0, 0.1);  // S22
    }

    [Fact]
    public void Parse_Amplifier_RiFormat()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("amplifier.s2p"));

        data.NumberOfPorts.Should().Be(2);
        data.Count.Should().Be(5);
        data.Options.DataFormat.Should().Be(DataFormat.RealImaginary);

        // First point: S11 = 0.3926 - 0.1211j
        var s11 = data.FrequencyPoints[0][0, 0];
        s11.Real.Should().BeApproximately(0.3926, 1e-4);
        s11.Imaginary.Should().BeApproximately(-0.1211, 1e-4);
    }

    // ──────────────────────────────────────────────────────────────────
    //  3-Port Tests
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_Coupler3Port_ParsesCorrectly()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("coupler.s3p"));

        data.NumberOfPorts.Should().Be(3);
        data.Count.Should().Be(2);
        data.Options.DataFormat.Should().Be(DataFormat.MagnitudeAngle);
    }

    [Fact]
    public void Parse_Coupler3Port_MatrixValues()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("coupler.s3p"));

        // First frequency point: S11 magnitude = 0.1
        var s11 = data.FrequencyPoints[0][0, 0];
        s11.Magnitude.Should().BeApproximately(0.1, 1e-6);

        // S12 magnitude = 0.95
        var s12 = data.FrequencyPoints[0][0, 1];
        s12.Magnitude.Should().BeApproximately(0.95, 1e-6);
    }

    // ──────────────────────────────────────────────────────────────────
    //  Special Cases
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Parse_CommentsFile_ExtractsAllComments()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("comments_only.s2p"));

        data.Comments.Should().HaveCountGreaterOrEqualTo(10);
        data.Comments.Should().Contain(c => c.Contains("Keysight PNA-X"));
    }

    [Fact]
    public void Parse_MinimalFile_ParsesCorrectly()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("minimal.s2p"));

        data.NumberOfPorts.Should().Be(2);
        data.Count.Should().Be(1);
        data.Comments.Should().BeEmpty();
    }

    [Fact]
    public void Parse_NoOptionLine_UsesDefaults()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("no_option_line.s1p"));

        data.NumberOfPorts.Should().Be(1);
        data.Count.Should().Be(3);
        data.Options.FrequencyUnit.Should().Be(FrequencyUnit.GHz);
        data.Options.DataFormat.Should().Be(DataFormat.MagnitudeAngle);
        data.Options.ReferenceImpedance.Should().Be(50.0);
    }

    // ──────────────────────────────────────────────────────────────────
    //  API Overloads
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void ParseString_ParsesCorrectly()
    {
        string content = @"# MHZ S RI R 50
100 0.5 0.3";

        var data = Parsing.TouchstoneParser.ParseString(content, "test.s1p");

        data.NumberOfPorts.Should().Be(1);
        data.Count.Should().Be(1);
        data.FrequencyPoints[0][0, 0].Real.Should().BeApproximately(0.5, 1e-10);
    }

    [Fact]
    public void Parse_Stream_ParsesCorrectly()
    {
        string content = @"# GHZ S RI R 50
1.0 0.1 0.2";

        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        var data = Parsing.TouchstoneParser.Parse(stream, "test.s1p");

        data.NumberOfPorts.Should().Be(1);
        data.Count.Should().Be(1);
    }

    [Fact]
    public async Task ParseAsync_ParsesCorrectly()
    {
        var data = await Parsing.TouchstoneParser.ParseAsync(GetTestDataPath("simple.s1p"));

        data.NumberOfPorts.Should().Be(1);
        data.Count.Should().Be(5);
    }

    [Fact]
    public void Parse_NonExistentFile_ThrowsFileNotFoundException()
    {
        var act = () => Parsing.TouchstoneParser.Parse("nonexistent.s2p");
        act.Should().Throw<FileNotFoundException>();
    }

    // ──────────────────────────────────────────────────────────────────
    //  Port Detection
    // ──────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("test.s1p", 1)]
    [InlineData("test.s2p", 2)]
    [InlineData("test.s3p", 3)]
    [InlineData("test.s4p", 4)]
    [InlineData("test.S2P", 2)]
    [InlineData("test.s12p", 12)]
    [InlineData("test.txt", 0)]
    public void DetectPortCount_CorrectlyIdentifiesPorts(string fileName, int expectedPorts)
    {
        int result = Parsing.TouchstoneParser.DetectPortCount(fileName);
        result.Should().Be(expectedPorts);
    }

    // ──────────────────────────────────────────────────────────────────
    //  LINQ-friendly API
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public void GetParameter_ReturnsCorrectEnumerable()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("simple.s1p"));

        var s11Points = data.GetParameter(0, 0).ToList();

        s11Points.Should().HaveCount(5);
        s11Points[0].FrequencyHz.Should().Be(100e6);
        s11Points[0].Value.Real.Should().BeApproximately(0.01, 1e-10);
    }

    [Fact]
    public void Frequencies_ReturnsAllFrequencies()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("simple.s1p"));

        var frequencies = data.Frequencies.ToList();

        frequencies.Should().HaveCount(5);
        frequencies.Should().BeInAscendingOrder();
    }
}
