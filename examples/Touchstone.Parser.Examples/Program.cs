using Touchstone.Parser.Models;
using Touchstone.Parser.Parsing;
using Touchstone.Parser.Utilities;

// ════════════════════════════════════════════════════════════════════
//  Touchstone.Parser — Example Application
//  Demonstrates parsing, querying, and exporting S-parameter data
// ════════════════════════════════════════════════════════════════════

Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║  Touchstone.Parser — S-Parameter Analysis Demo          ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
Console.WriteLine();

// ── 1. Parse a Touchstone file ────────────────────────────────────
string filePath = Path.Combine("SampleData", "wifi_filter.s2p");
Console.WriteLine($"📂 Parsing: {filePath}");

var data = TouchstoneParser.Parse(filePath);

Console.WriteLine($"   ├─ Ports: {data.NumberOfPorts}");
Console.WriteLine($"   ├─ Frequency points: {data.Count}");
Console.WriteLine($"   ├─ Format: {data.Options.DataFormat}");
Console.WriteLine($"   ├─ Reference impedance: {data.Options.ReferenceImpedance} Ω");
Console.WriteLine($"   └─ Comments: {data.Comments.Count}");
Console.WriteLine();

// ── 2. Display file comments ──────────────────────────────────────
Console.WriteLine("💬 File Comments:");
foreach (string comment in data.Comments)
{
    Console.WriteLine($"   {comment}");
}
Console.WriteLine();

// ── 3. Query S21 (insertion loss) with LINQ ───────────────────────
Console.WriteLine("📊 S21 Insertion Loss across frequency:");
Console.WriteLine("   ┌────────────────┬────────────┬────────────┐");
Console.WriteLine("   │ Frequency (GHz)│  S21 (dB)  │   IL (dB)  │");
Console.WriteLine("   ├────────────────┼────────────┼────────────┤");

foreach (var (freqHz, param) in data.GetS21())
{
    double freqGhz = FrequencyConverter.FromHz(freqHz, FrequencyUnit.GHz);
    double s21Db = param.MagnitudeDb;
    double insertionLoss = -s21Db;
    Console.WriteLine($"   │ {freqGhz,14:F3} │ {s21Db,10:F2} │ {insertionLoss,10:F2} │");
}

Console.WriteLine("   └────────────────┴────────────┴────────────┘");
Console.WriteLine();

// ── 4. Find passband performance ──────────────────────────────────
Console.WriteLine("🎯 Passband Analysis (2.2 – 2.6 GHz):");
var passband = data.InFrequencyRange(2.2e9, 2.6e9);

var ilValues = passband.ToInsertionLoss().ToList();
var rlValues = passband.ToReturnLoss().ToList();
var vswrValues = passband.ToVswr().ToList();

Console.WriteLine($"   ├─ Frequency points in band: {passband.Count}");
Console.WriteLine($"   ├─ Max insertion loss: {ilValues.Max(x => x.InsertionLossDb):F2} dB");
Console.WriteLine($"   ├─ Min return loss: {rlValues.Min(x => x.ReturnLossDb):F2} dB");
Console.WriteLine($"   └─ Max VSWR: {vswrValues.Max(x => x.Vswr):F3}");
Console.WriteLine();

// ── 5. VSWR table ─────────────────────────────────────────────────
Console.WriteLine("📈 VSWR across frequency:");
Console.WriteLine("   ┌────────────────┬────────────┬────────────┐");
Console.WriteLine("   │ Frequency (GHz)│  S11 (dB)  │    VSWR    │");
Console.WriteLine("   ├────────────────┼────────────┼────────────┤");

foreach (var (freqHz, vswr) in data.ToVswr())
{
    double freqGhz = FrequencyConverter.FromHz(freqHz, FrequencyUnit.GHz);
    double s11Db = data.GetS11()
        .First(p => Math.Abs(p.FrequencyHz - freqHz) < 1.0)
        .Value.MagnitudeDb;
    Console.WriteLine($"   │ {freqGhz,14:F3} │ {s11Db,10:F2} │ {vswr,10:F3} │");
}

Console.WriteLine("   └────────────────┴────────────┴────────────┘");
Console.WriteLine();

// ── 6. Export to CSV ──────────────────────────────────────────────
string csvPath = "wifi_filter_output.csv";
using (var writer = new StreamWriter(csvPath))
{
    data.ToCsv(writer, FrequencyUnit.GHz, DataFormat.DecibelAngle);
}
Console.WriteLine($"💾 CSV exported to: {csvPath}");

// ── 7. Re-export as Touchstone ────────────────────────────────────
string exportPath = "wifi_filter_exported.s2p";
var exportOptions = new TouchstoneOptions(
    FrequencyUnit.MHz, ParameterType.S, DataFormat.RealImaginary, 50.0);
TouchstoneWriter.Write(data, exportPath, exportOptions);
Console.WriteLine($"💾 Re-exported as Touchstone (MHz, RI): {exportPath}");
Console.WriteLine();

Console.WriteLine("✅ Demo complete!");
