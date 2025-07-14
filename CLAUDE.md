# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET C# project for a Serilog formatter that outputs compact JSON optimized for AWS CloudWatch metric filters and structured log analysis. The formatter replaces the standard Serilog compact formatter's `@` prefixed properties with `_` prefixed properties to ensure compatibility with CloudWatch metric filters.

## Development Commands

Since this is a .NET project, use standard dotnet CLI commands:

```bash
# Build the solution
dotnet build

# Run tests (xUnit v3 with Microsoft.Testing.Platform)
dotnet run --project test --framework net8.0
dotnet run --project test --framework net9.0

# Restore packages
dotnet restore

# Pack for NuGet
dotnet pack

# Clean build artifacts
dotnet clean
```

## Project Structure

```
compact-json-formatter/
├── src/
│   ├── LayeredCraft.Logging.CompactJsonFormatter.csproj (Main library)
│   └── CompactJsonFormatter.cs (Main formatter implementation)
├── test/
│   ├── LayeredCraft.Logging.CompactJsonFormatter.Tests.csproj (xUnit v3 test project)
│   ├── CompactJsonFormatterTests.cs (26 comprehensive unit tests)
│   ├── TestHelpers.cs (Test utilities and helpers)
│   └── xunit.runner.json (xUnit configuration)
├── .github/
│   ├── workflows/
│   │   ├── build.yaml (CI/CD for main branch)
│   │   └── pr-build.yaml (PR validation)
│   └── dependabot.yml (Dependency management)
├── LayeredCraft.Logging.CompactJsonFormatter.sln (Solution with organized folders)
├── Directory.Build.props (Shared project configuration)
├── README.md (Comprehensive documentation with examples)
├── CLAUDE.md (This file)
├── icon.png (NuGet package icon)
└── .gitignore (Comprehensive exclusions for .NET, macOS, JetBrains IDEs)
```

## Key Information

- **License**: MIT License
- **Organization**: LayeredCraft
- **Target Frameworks**: .NET 8.0 and .NET 9.0
- **Purpose**: High-performance Serilog formatter for compact JSON output
- **Target Use Case**: AWS CloudWatch metric filters and structured log analysis
- **Key Feature**: Replaces `@` prefixed properties with `_` prefixed properties for CloudWatch compatibility

## Development Notes

- **Main Implementation**: `CompactJsonFormatter` class in `LayeredCraft.Logging.CompactJsonFormatter` namespace
- **Dependencies**: Uses `Serilog.Formatting.Compact` and `Microsoft.SourceLink.GitHub`
- **Test Framework**: xUnit v3 with Microsoft.Testing.Platform runner
- **Test Libraries**: NSubstitute for mocking, AwesomeAssertions for fluent assertions
- **Test Coverage**: 26 unit tests covering all major functionality and edge cases
- **Package Configuration**: Auto-generates NuGet package on build with icon and source link

## Test Execution

The project uses xUnit v3 with Microsoft.Testing.Platform. To run tests:

```bash
# Run all tests on .NET 9.0
cd test && dotnet run --framework net9.0

# Run all tests on .NET 8.0
cd test && dotnet run --framework net8.0

# Build and run tests from solution root
dotnet build
dotnet run --project test --framework net9.0
```

## GitHub Integration

- **CI/CD**: Automated build and test workflows
- **PR Validation**: Comprehensive PR checks with test execution
- **Dependency Management**: Dependabot for automatic dependency updates
- **Package Publishing**: Automated NuGet package generation and publishing

## Testing Strategy

The test suite includes:
- **Constructor Tests**: Default and custom JsonValueFormatter scenarios
- **Format Method Tests**: Instance method with newline handling and null validation
- **FormatEvent Static Method Tests**: Core formatting logic with comprehensive scenarios
- **Edge Cases**: CloudWatch compatibility, trace/span support, exception handling
- **Property Handling**: Regular properties and @ prefix conversion
- **JSON Validation**: Proper structure and content validation using System.Text.Json

All tests pass on both target frameworks and provide excellent code coverage.