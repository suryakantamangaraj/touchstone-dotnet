using FluentAssertions;
using Touchstone.Parser.Parsing;
using Xunit;

namespace Touchstone.Parser.Tests;

public class StressTests
{
    [Fact]
    public void Parse_LargeFile_10000Points_ParsesCorrectly()
    {
        // Generate 10,000 points
        int pointCount = 10000;
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# GHZ S RI R 50");
        for (int i = 0; i < pointCount; i++)
        {
            sb.AppendLine($"{i * 0.001:F3} 0.1 0.1 0.9 0.0 0.9 0.0 0.1 0.1");
        }

        var data = TouchstoneParser.ParseString(sb.ToString(), "stress.s2p");

        data.Count.Should().Be(pointCount);
        data.NumberOfPorts.Should().Be(2);
        data.FrequencyPoints[pointCount - 1].FrequencyHz.Should().BeApproximately((pointCount - 1) * 0.001 * 1e9, 1.0);
    }
}
