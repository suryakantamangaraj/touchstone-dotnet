# Touchstone.Parser Documentation

Welcome to the **Touchstone.Parser** API documentation.

This library provides a clean, modular, and enterprise-ready way to parse Touchstone (`.sNp`) files in .NET.

## Getting Started

Install the package:

```bash
dotnet add package Touchstone.Parser
```

Then parse your first file:

```csharp
using Touchstone.Parser.Parsing;
using Touchstone.Parser.Utilities;

var data = TouchstoneParser.Parse("filter.s2p");

foreach (var (freqHz, param) in data.GetS21())
{
    Console.WriteLine($"{freqHz / 1e9:F3} GHz → S21 = {param.MagnitudeDb:F2} dB");
}
```

For more details, see the [README on GitHub](https://github.com/suryakantamangaraj/touchstone-dotnet).

## Key Features

- **Parse `.sNp` files** into strongly typed C# classes.
- **Multi-port support** (1-port through N-port).
- **All data formats** (RI, MA, DB).
- **All frequency units** (Hz, kHz, MHz, GHz).
- **LINQ-friendly APIs** for querying and transformations.
- **RF calculations** (Insertion Loss, Return Loss, VSWR).

## Explore the API

Browse the full [API Reference](api/index.md) to see all available types, methods, and extension methods.
