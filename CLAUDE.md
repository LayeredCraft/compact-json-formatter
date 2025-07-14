# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET C# project for a Serilog formatter that outputs compact JSON optimized for AWS CloudWatch metric filters and structured log analysis. The formatter replaces the standard Serilog compact formatter's `@` prefixed properties with `_` prefixed properties to ensure compatibility with CloudWatch metric filters.

## Development Commands

Since this is a .NET project, use standard dotnet CLI commands:

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Restore packages
dotnet restore

# Pack for NuGet
dotnet pack

# Clean build artifacts
dotnet clean
```

## Project Structure

- **src/LayeredCraft.Logging.CompactJsonFormatter.csproj**: Main class library project
- **test/LayeredCraft.Logging.CompactJsonFormatter.Tests.csproj**: xUnit test project
- **LayeredCraft.Logging.CompactJsonFormatter.sln**: Visual Studio solution file with src and test solution folders
- **src/CompactJsonFormatter.cs**: Main formatter implementation
- **README.md**: Project description with badges

## Key Information

- **License**: MIT License
- **Organization**: LayeredCraft
- **Target Frameworks**: .NET 8.0 and .NET 9.0
- **Purpose**: High-performance Serilog formatter for compact JSON output
- **Target Use Case**: AWS CloudWatch metric filters and structured log analysis
- **Key Feature**: Replaces `@` prefixed properties with `_` prefixed properties for CloudWatch compatibility

## Development Notes

- The main formatter class is `CompactJsonFormatter` in the `LayeredCraft.Logging.CompactJsonFormatter` namespace
- Uses standard Serilog dependencies: `Serilog` and `Serilog.Formatting.Compact`
- Test project uses xUnit v2 (latest stable version)
- Package is configured to generate on build with proper NuGet metadata