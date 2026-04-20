using FluentAssertions;
using Touchstone.Parser.Models;
using Touchstone.Parser.Parsing;
using Xunit;

namespace Touchstone.Parser.Tests;

public class EdgeCaseTests
{
    [Fact]
    public void Parse_EmptyString_ReturnsEmptyData()
    {
        var act = () => Parsing.TouchstoneParser.ParseString("", "test.s1p");
        act.Should().Throw<TouchstoneParserException>();
    }

    [Fact]
    public void Parse_OnlyComments_ThrowsException()
    {
        string content = "! comment 1\n! comment 2\n";
        var act = () => Parsing.TouchstoneParser.ParseString(content, "test.s1p");
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
}
