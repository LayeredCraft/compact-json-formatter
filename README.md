# LayeredCraft.Logging.CompactJsonFormatter

[![Build Status](https://github.com/LayeredCraft/compact-json-formatter/actions/workflows/build.yaml/badge.svg)](https://github.com/LayeredCraft/compact-json-formatter/actions/workflows/build.yaml)
[![NuGet](https://img.shields.io/nuget/v/LayeredCraft.Logging.CompactJsonFormatter.svg)](https://www.nuget.org/packages/LayeredCraft.Logging.CompactJsonFormatter/)
[![Downloads](https://img.shields.io/nuget/dt/LayeredCraft.Logging.CompactJsonFormatter.svg)](https://www.nuget.org/packages/LayeredCraft.Logging.CompactJsonFormatter/)

A high-performance Serilog formatter that outputs compact JSON optimized for AWS CloudWatch metric filters and structured log analysis.

## Overview

The standard Serilog `CompactJsonFormatter` uses `@` prefixed properties for metadata (like `@l` for level, `@t` for timestamp, etc.). However, these `@` prefixed properties are not compatible with AWS CloudWatch metric filters, which cannot parse escaped metadata keys.

This formatter solves that problem by replacing the `@` prefix with `_` (underscore) while maintaining all the performance benefits of the original compact formatter.

## Key Features

- **CloudWatch Compatible**: Uses `_` prefix instead of `@` for metadata fields
- **High Performance**: Based on the official Serilog compact formatter
- **Single-line JSON**: Optimized for log ingestion and analysis
- **Full Serilog Integration**: Drop-in replacement for `CompactJsonFormatter`
- **Trace Support**: Includes trace ID and span ID support
- **Multi-targeting**: Supports .NET 8.0 and .NET 9.0

## Installation

```bash
dotnet add package LayeredCraft.Logging.CompactJsonFormatter
```

## Usage

### Basic Usage

```csharp
using LayeredCraft.Logging.CompactJsonFormatter;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateLogger();

Log.Information("Hello, {Name}!", "World");
```

### With Custom Value Formatter

```csharp
using LayeredCraft.Logging.CompactJsonFormatter;
using Serilog;
using Serilog.Formatting.Json;

var customValueFormatter = new JsonValueFormatter(typeTagName: "$type");

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter(customValueFormatter))
    .CreateLogger();
```

### AWS CloudWatch Integration

```csharp
using LayeredCraft.Logging.CompactJsonFormatter;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;

Log.Logger = new LoggerConfiguration()
    .WriteTo.AmazonCloudWatch(
        logGroup: "/aws/lambda/my-function",
        region: RegionEndpoint.USEast1,
        formatter: new CompactJsonFormatter())
    .CreateLogger();
```

## Output Format

### Standard Serilog CompactJsonFormatter Output
```json
{"@t":"2025-01-14T10:30:00.000Z","@l":"Information","@mt":"Hello, {Name}!","Name":"World"}
```

### LayeredCraft CompactJsonFormatter Output
```json
{"_t":"2025-01-14T10:30:00.000Z","_l":"Information","_mt":"Hello, {Name}!","Name":"World"}
```

## Field Mapping

| Serilog Standard | LayeredCraft | Description |
|------------------|--------------|-------------|
| `@t` | `_t` | Timestamp |
| `@l` | `_l` | Log Level |
| `@mt` | `_mt` | Message Template |
| `@m` | `_m` | Rendered Message |
| `@r` | `_r` | Renderings |
| `@x` | `_x` | Exception |
| `@tr` | `_tr` | Trace ID |
| `@sp` | `_sp` | Span ID |

## CloudWatch Metric Filters

With the underscore prefix, you can easily create CloudWatch metric filters:

```
[_l="Error"]
[_l="Warning" && _t > "2025-01-01"]
```

## Project Structure

```
compact-json-formatter/
├── src/
│   ├── LayeredCraft.Logging.CompactJsonFormatter.csproj
│   └── CompactJsonFormatter.cs
├── test/
│   └── LayeredCraft.Logging.CompactJsonFormatter.Tests.csproj
├── .github/
│   ├── workflows/
│   │   ├── build.yaml
│   │   └── pr-build.yaml
│   └── dependabot.yml
├── Directory.Build.props
├── README.md
├── CLAUDE.md
└── icon.png
```

## Development

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Creating NuGet Package

```bash
dotnet pack
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for your changes
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Related Projects

- [LayeredCraft.StructuredLogging](https://github.com/LayeredCraft/structured-logging) - Structured logging extensions for .NET
