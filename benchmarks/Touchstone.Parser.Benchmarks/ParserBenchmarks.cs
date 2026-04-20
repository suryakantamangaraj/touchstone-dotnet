using BenchmarkDotNet.Attributes;
using Touchstone.Parser.Parsing;

namespace Touchstone.Parser.Benchmarks;

[MemoryDiagnoser]
public class ParserBenchmarks
{
    private string _s2pContent = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        // Generate a synthetic 2-port file with 1000 points
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# GHZ S RI R 50");
        for (int i = 0; i < 1000; i++)
        {
            sb.AppendLine($"{i * 0.1:F1} 0.1 0.2 0.9 0.0 0.9 0.0 0.1 0.2");
        }
        _s2pContent = sb.ToString();
    }

    [Benchmark]
    public void ParseS2pString()
    {
        TouchstoneParser.ParseString(_s2pContent, "benchmark.s2p");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkDotNet.Running.BenchmarkRunner.Run<ParserBenchmarks>();
    }
}
