using FluentAssertions;
using Touchstone.Parser.Models;
using Touchstone.Parser.Parsing;
using Xunit;

namespace Touchstone.Parser.Tests;

public class EdgeCaseTests
{
    [Fact]
    public void Parse_EmptyString_WithKnownPortCount_ReturnsEmptyData()
    {
        var data = Parsing.TouchstoneParser.ParseString("", "test.s1p");
        data.NumberOfPorts.Should().Be(1);
        data.Count.Should().Be(0);
    }

    [Fact]
    public void Parse_EmptyString_WithoutFileName_ThrowsException()
    {
        var act = () => Parsing.TouchstoneParser.ParseString("");
        act.Should().Throw<TouchstoneParserException>();
    }

    [Fact]
    public void Parse_OnlyComments_WithKnownPortCount_ReturnsEmptyData()
    {
        string content = "! comment 1\n! comment 2\n";
        var data = Parsing.TouchstoneParser.ParseString(content, "test.s1p");
        data.NumberOfPorts.Should().Be(1);
        data.Count.Should().Be(0);
        data.Comments.Should().HaveCount(2);
    }

    [Fact]
    public void Parse_OnlyComments_WithoutFileName_ThrowsException()
    {
        string content = "! comment 1\n! comment 2\n";
        var act = () => Parsing.TouchstoneParser.ParseString(content);
        act.Should().Throw<TouchstoneParserException>();
    }

    [Fact]
    public void Parse_DuplicateOptionLine_ThrowsException()
    {
        string content = "# GHZ S MA R 50\n# MHZ S RI R 75\n1.0 0.1 0.0";
        var act = () => Parsing.TouchstoneParser.ParseString(content, "test.s1p");
        act.Should().Throw<TouchstoneParserException>()
            .Which.Message.Should().Contain("Multiple option lines");
    }

    [Fact]
    public void Parse_InvalidNumericData_ThrowsException()
    {
        string content = "# GHZ S RI R 50\n1.0 abc 0.5";
        var act = () => Parsing.TouchstoneParser.ParseString(content, "test.s1p");
        act.Should().Throw<TouchstoneParserException>()
            .Which.Message.Should().Contain("Invalid numeric");
    }

    [Fact]
    public void FrequencyPoint_NegativeFrequency_Throws()
    {
        var act = () => new FrequencyPoint(-1.0, new NetworkParameter[1, 1]);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void FrequencyPoint_NonSquareMatrix_Throws()
    {
        var act = () => new FrequencyPoint(1.0, new NetworkParameter[2, 3]);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FrequencyPoint_IndexOutOfRange_Throws()
    {
        var matrix = new NetworkParameter[1, 1];
        matrix[0, 0] = NetworkParameter.Zero;
        var fp = new FrequencyPoint(1.0, matrix);

        var act = () => fp[1, 0];
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TouchstoneData_ZeroPorts_Throws()
    {
        var act = () => new TouchstoneData(
            TouchstoneOptions.Default, 0,
            new List<FrequencyPoint>(), new List<string>(), null);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TouchstoneData_NullOptions_Throws()
    {
        var act = () => new TouchstoneData(
            null!, 1, new List<FrequencyPoint>(), new List<string>(), null);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Parse_InlineCommentInData_HandledCorrectly()
    {
        string content = "# GHZ S RI R 50\n1.0 0.1 0.2 ! inline comment";
        var data = Parsing.TouchstoneParser.ParseString(content, "test.s1p");
        data.Count.Should().Be(1);
        data.FrequencyPoints[0][0, 0].Real.Should().BeApproximately(0.1, 1e-10);
    }

    [Fact]
    public void Parse_MixedCaseOptions_ParsesCorrectly()
    {
        string content = "# mHz s ri r 50\n100 0.5 0.3";
        var data = Parsing.TouchstoneParser.ParseString(content, "test.s1p");
        data.Options.FrequencyUnit.Should().Be(FrequencyUnit.MHz);
    }

    [Fact]
    public void Parse_ExtraWhitespace_ParsesCorrectly()
    {
        string content = "  #   GHZ   S   RI   R   50  \n  1.0     0.1    0.2  ";
        var data = Parsing.TouchstoneParser.ParseString(content, "test.s1p");
        data.Count.Should().Be(1);
        data.FrequencyPoints[0][0, 0].Real.Should().BeApproximately(0.1, 1e-10);
    }

    [Fact]
    public void Parse_NullPath_ThrowsArgumentNullException()
    {
        var act = () => Parsing.TouchstoneParser.Parse((string)null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OptionLineParser_NullLine_ThrowsArgumentNullException()
    {
        var act = () => OptionLineParser.Parse(null!, 1);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DetectPortCount_NullFileName_ThrowsArgumentNullException()
    {
        var act = () => Parsing.TouchstoneParser.DetectPortCount(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
