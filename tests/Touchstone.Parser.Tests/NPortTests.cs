using FluentAssertions;
using Touchstone.Parser.Models;
using Touchstone.Parser.Parsing;
using Xunit;

namespace Touchstone.Parser.Tests;

public class NPortTests
{
    private static string GetTestDataPath(string fileName) =>
        Path.Combine("TestData", fileName);

    [Fact]
    public void Parse_4Port_ParsesCorrectly()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("filter_4port.s4p"));

        data.NumberOfPorts.Should().Be(4);
        data.Count.Should().Be(2);
        
        // Check first frequency point
        var fp1 = data.FrequencyPoints[0];
        fp1.FrequencyHz.Should().Be(1.0e9);
        
        // S11 = 0.1 + 0.0j
        fp1[0, 0].Real.Should().Be(0.1);
        // S12 = 0.9 + 0.0j (row 0, col 1)
        fp1[0, 1].Real.Should().Be(0.9);
        // S44 = 0.1 + 0.0j (row 3, col 3)
        fp1[3, 3].Real.Should().Be(0.1);
    }

    [Fact]
    public void Parse_4Port_SecondPoint()
    {
        var data = Parsing.TouchstoneParser.Parse(GetTestDataPath("filter_4port.s4p"));
        var fp2 = data.FrequencyPoints[1];
        
        fp2.FrequencyHz.Should().Be(2.0e9);
        fp2[0, 0].Real.Should().Be(0.2);
        fp2[0, 1].Real.Should().Be(0.8);
    }
}
