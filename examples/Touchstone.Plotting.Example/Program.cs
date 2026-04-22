using ScottPlot;
using Touchstone.Parser.Models;
using Touchstone.Parser.Parsing;
using Touchstone.Parser.Utilities;

// ════════════════════════════════════════════════════════════════════
//  Touchstone.Parser — Plotting Example
//  Demonstrates how to visualize S-parameter data using ScottPlot
// ════════════════════════════════════════════════════════════════════

Console.WriteLine("📊 Touchstone Plotting Demo");
Console.WriteLine("══════════════════════════");

// 1. Parse the Touchstone file
string filePath = Path.Combine("SampleData", "wifi_filter.s2p");
var data = TouchstoneParser.Parse(filePath);

Console.WriteLine($"Loaded {filePath} ({data.Count} points)");

// 2. Prepare data for plotting
var frequencies = data.GetFrequenciesIn(FrequencyUnit.GHz).ToArray();
var s11Db = data.GetS11().Select(p => p.Value.MagnitudeDb).ToArray();
var s21Db = data.GetS21().Select(p => p.Value.MagnitudeDb).ToArray();

// 3. Create a plot
var plt = new ScottPlot.Plot();

// Add S11 and S21 curves
var sig11 = plt.Add.Scatter(frequencies, s11Db);
sig11.LegendText = "S11 (Return Loss)";
sig11.LineWidth = 2;

var sig21 = plt.Add.Scatter(frequencies, s21Db);
sig21.LegendText = "S21 (Insertion Loss)";
sig21.LineWidth = 2;

// Customize plot appearance
plt.Title("Filter S-Parameters");
plt.XLabel("Frequency (GHz)");
plt.YLabel("Magnitude (dB)");
plt.ShowLegend();

// Set axis limits if needed
plt.Axes.SetLimitsY(-60, 5);

// 4. Save the plot
string outputPath = "s_parameters_plot.png";
plt.SavePng(outputPath, 800, 600);

Console.WriteLine($"✅ Plot saved to: {Path.GetFullPath(outputPath)}");
