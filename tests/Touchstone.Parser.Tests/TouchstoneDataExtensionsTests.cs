using FluentAssertions;
using Touchstone.Parser.Models;
using Touchstone.Parser.Utilities;
using Xunit;

namespace Touchstone.Parser.Tests;

public class TouchstoneDataExtensionsTests
{
    private TouchstoneData LoadBandpassFilter() =>
        Parsing.TouchstoneParser.Parse(Path.Combine("TestData", "bandpass_filter.s2p"));

    [Fact]
    public void GetS11_ReturnsCorrectCount()
    {
        var data = LoadBandpassFilter();
        data.GetS11().Should().HaveCount(6);
    }

    [Fact]
    public void GetS21_ReturnsCorrectValues()
    {
        var data = LoadBandpassFilter();
        var s21 = data.GetS21().ToList();
        // At 1.0 GHz, S21 = -0.5 dB
        s21[2].Value.MagnitudeDb.Should().BeApproximately(-0.5, 0.1);
    }

    [Fact]
    public void GetFrequenciesIn_ConvertsCorrectly()
    {
        var data = LoadBandpassFilter();
        var freqsMhz = data.GetFrequenciesIn(FrequencyUnit.MHz).ToList();
        freqsMhz[0].Should().BeApproximately(500.0, 1.0);
    }

    [Fact]
    public void MinMaxFrequency_Correct()
    {
        var data = LoadBandpassFilter();
        data.MinFrequencyHz().Should().BeApproximately(0.5e9, 1.0);
        data.MaxFrequencyHz().Should().BeApproximately(2.0e9, 1.0);
    }

    [Fact]
    public void ToInsertionLoss_ComputesCorrectly()
    {
        var data = LoadBandpassFilter();
        var il = data.ToInsertionLoss().ToList();
        // At 1.0 GHz, S21 = -0.5 dB → IL = 0.5 dB
        il[2].InsertionLossDb.Should().BeApproximately(0.5, 0.1);
    }

    [Fact]
    public void ToReturnLoss_ComputesCorrectly()
    {
        var data = LoadBandpassFilter();
        var rl = data.ToReturnLoss().ToList();
        // At 1.0 GHz, S11 = -25 dB → RL = 25 dB
        rl[2].ReturnLossDb.Should().BeApproximately(25.0, 0.1);
    }

    [Fact]
    public void ToVswr_ComputesCorrectly()
    {
        var data = LoadBandpassFilter();
        var vswr = data.ToVswr().ToList();
        // All VSWR values should be >= 1
        vswr.Should().OnlyContain(v => v.Vswr >= 1.0);
    }

    [Fact]
    public void InFrequencyRange_FiltersCorrectly()
    {
        var data = LoadBandpassFilter();
        var filtered = data.InFrequencyRange(0.9e9, 1.3e9);
        filtered.Count.Should().Be(2); // 1.0 GHz and 1.2 GHz
    }

    [Fact]
    public void Where_FiltersCorrectly()
    {
        var data = LoadBandpassFilter();
        var filtered = data.Where(fp => fp.FrequencyHz >= 1.5e9);
        filtered.Count.Should().Be(2); // 1.5 GHz and 2.0 GHz
    }

    [Fact]
    public void ToCsvString_ProducesValidCsv()
    {
        var data = LoadBandpassFilter();
        string csv = data.ToCsvString(FrequencyUnit.GHz);

        csv.Should().Contain("Frequency (GHz)");
        csv.Should().Contain("S11_dB");
        csv.Should().Contain("S21_dB");
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(7); // 1 header + 6 data
    }
}
