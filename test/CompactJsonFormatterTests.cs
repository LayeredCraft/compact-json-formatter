using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using Serilog.Events;
using Serilog.Formatting.Json;
using Xunit;

namespace LayeredCraft.Logging.CompactJsonFormatter.Tests;

public class CompactJsonFormatterTests
{
    [Fact]
    public void Constructor_WithNullValueFormatter_CreatesDefaultJsonValueFormatter()
    {
        // Act
        var formatter = new CompactJsonFormatter(null);

        // Assert
        formatter.Should().NotBeNull();
        
        // Use reflection to check the internal field
        var field = typeof(CompactJsonFormatter).GetField("_valueFormatter", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.Should().NotBeNull();
        
        var valueFormatter = field!.GetValue(formatter);
        valueFormatter.Should().NotBeNull();
        valueFormatter.Should().BeOfType<JsonValueFormatter>();
    }

    [Fact]
    public void Constructor_WithCustomValueFormatter_StoresValueFormatter()
    {
        // Arrange
        var customFormatter = new JsonValueFormatter(typeTagName: "customType");

        // Act
        var formatter = new CompactJsonFormatter(customFormatter);

        // Assert
        formatter.Should().NotBeNull();
        
        // Use reflection to check the internal field
        var field = typeof(CompactJsonFormatter).GetField("_valueFormatter", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.Should().NotBeNull();
        
        var valueFormatter = field!.GetValue(formatter);
        valueFormatter.Should().BeSameAs(customFormatter);
    }

    [Fact]
    public void Format_WithValidLogEvent_CallsFormatEventAndAddsNewline()
    {
        // Arrange
        var formatter = new CompactJsonFormatter();
        var logEvent = TestHelpers.CreateLogEvent();
        var output = new StringWriter();

        // Act
        formatter.Format(logEvent, output);

        // Assert
        var result = output.ToString();
        result.Should().NotBeEmpty();
        result.Should().EndWith(Environment.NewLine);
        
        // Verify it's valid JSON
        var jsonWithoutNewline = result.TrimEnd('\r', '\n');
        var document = TestHelpers.ParseJson(jsonWithoutNewline);
        TestHelpers.JsonContainsProperty(document, "_t").Should().BeTrue();
        TestHelpers.JsonContainsProperty(document, "_mt").Should().BeTrue();
    }

    [Fact]
    public void Format_WithNullLogEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var formatter = new CompactJsonFormatter();
        var output = new StringWriter();

        // Act & Assert
        Action act = () => formatter.Format(null!, output);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logEvent");
    }

    [Fact]
    public void Format_WithNullOutput_ThrowsArgumentNullException()
    {
        // Arrange
        var formatter = new CompactJsonFormatter();
        var logEvent = TestHelpers.CreateLogEvent();

        // Act & Assert
        Action act = () => formatter.Format(logEvent, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("output");
    }

    [Fact]
    public void FormatEvent_WithNullLogEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act & Assert
        Action act = () => CompactJsonFormatter.FormatEvent(null!, output, valueFormatter);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logEvent");
    }

    [Fact]
    public void FormatEvent_WithNullOutput_ThrowsArgumentNullException()
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEvent();
        var valueFormatter = new JsonValueFormatter();

        // Act & Assert
        Action act = () => CompactJsonFormatter.FormatEvent(logEvent, null!, valueFormatter);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("output");
    }

    [Fact]
    public void FormatEvent_WithNullValueFormatter_ThrowsArgumentNullException()
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEvent();
        var output = new StringWriter();

        // Act & Assert
        Action act = () => CompactJsonFormatter.FormatEvent(logEvent, output, null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("valueFormatter");
    }

    [Fact]
    public void FormatEvent_WithBasicLogEvent_ProducesCorrectJson()
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEvent(
            LogEventLevel.Information,
            "Test message");

        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        // Verify timestamp is present and in correct format
        TestHelpers.JsonContainsProperty(document, "_t").Should().BeTrue();
        var timestamp = TestHelpers.GetJsonProperty(document, "_t");
        timestamp.Should().NotBeEmpty();
        DateTimeOffset.TryParseExact(timestamp, "O", null, System.Globalization.DateTimeStyles.None, out _).Should().BeTrue();
        
        TestHelpers.GetJsonProperty(document, "_mt").Should().Be("Test message");
        
        // Information level should be omitted
        TestHelpers.JsonContainsProperty(document, "_l").Should().BeFalse();
    }

    [Theory]
    [InlineData(LogEventLevel.Verbose, "Verbose")]
    [InlineData(LogEventLevel.Debug, "Debug")]
    [InlineData(LogEventLevel.Warning, "Warning")]
    [InlineData(LogEventLevel.Error, "Error")]
    [InlineData(LogEventLevel.Fatal, "Fatal")]
    public void FormatEvent_WithNonInformationLevel_IncludesLevelInJson(LogEventLevel level, string expectedLevel)
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEvent(level, "Test message");
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.GetJsonProperty(document, "_l").Should().Be(expectedLevel);
    }

    [Fact]
    public void FormatEvent_WithInformationLevel_OmitsLevelFromJson()
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEvent(LogEventLevel.Information, "Test message");
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "_l").Should().BeFalse();
    }

    [Fact]
    public void FormatEvent_WithException_IncludesExceptionInJson()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        var logEvent = TestHelpers.CreateLogEvent(LogEventLevel.Error, "Error occurred", exception);
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "_x").Should().BeTrue();
        TestHelpers.GetJsonProperty(document, "_x").Should().Contain("Test exception");
        TestHelpers.GetJsonProperty(document, "_x").Should().Contain("InvalidOperationException");
    }

    [Fact]
    public void FormatEvent_WithoutException_OmitsExceptionFromJson()
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEvent(LogEventLevel.Information, "Test message");
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "_x").Should().BeFalse();
    }

    [Fact]
    public void FormatEvent_WithTraceId_IncludesTraceIdInJson()
    {
        // Arrange
        var traceId = ActivityTraceId.CreateRandom();
        var logEvent = TestHelpers.CreateLogEventWithTracing(
            LogEventLevel.Information,
            "Test message",
            traceId);
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "_tr").Should().BeTrue();
        TestHelpers.GetJsonProperty(document, "_tr").Should().Be(traceId.ToHexString());
    }

    [Fact]
    public void FormatEvent_WithSpanId_IncludesSpanIdInJson()
    {
        // Arrange
        var spanId = ActivitySpanId.CreateRandom();
        var logEvent = TestHelpers.CreateLogEventWithTracing(
            LogEventLevel.Information,
            "Test message",
            spanId: spanId);
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "_sp").Should().BeTrue();
        TestHelpers.GetJsonProperty(document, "_sp").Should().Be(spanId.ToHexString());
    }

    [Fact]
    public void FormatEvent_WithBothTraceAndSpanIds_IncludesBothInJson()
    {
        // Arrange
        var traceId = ActivityTraceId.CreateRandom();
        var spanId = ActivitySpanId.CreateRandom();
        var logEvent = TestHelpers.CreateLogEventWithTracing(
            LogEventLevel.Information,
            "Test message",
            traceId,
            spanId);
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "_tr").Should().BeTrue();
        TestHelpers.JsonContainsProperty(document, "_sp").Should().BeTrue();
        TestHelpers.GetJsonProperty(document, "_tr").Should().Be(traceId.ToHexString());
        TestHelpers.GetJsonProperty(document, "_sp").Should().Be(spanId.ToHexString());
    }

    [Fact]
    public void FormatEvent_WithoutTraceAndSpanIds_OmitsTraceAndSpanFromJson()
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEvent();
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "_tr").Should().BeFalse();
        TestHelpers.JsonContainsProperty(document, "_sp").Should().BeFalse();
    }

    [Fact]
    public void FormatEvent_WithRegularProperties_IncludesPropertiesInJson()
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEvent(
            LogEventLevel.Information,
            "Test message",
            null,
            ("UserId", 123),
            ("UserName", "john.doe"));
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "UserId").Should().BeTrue();
        TestHelpers.JsonContainsProperty(document, "UserName").Should().BeTrue();
        TestHelpers.GetJsonPropertyElement(document, "UserId").GetInt32().Should().Be(123);
        TestHelpers.GetJsonProperty(document, "UserName").Should().Be("john.doe");
    }

    [Fact]
    public void FormatEvent_WithAtPrefixedProperties_ConvertsToUnderscorePrefix()
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEvent(
            LogEventLevel.Information,
            "Test message",
            null,
            ("@timestamp", "2025-01-14"),
            ("@level", "custom"));
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "_timestamp").Should().BeTrue();
        TestHelpers.JsonContainsProperty(document, "_level").Should().BeTrue();
        TestHelpers.GetJsonProperty(document, "_timestamp").Should().Be("2025-01-14");
        TestHelpers.GetJsonProperty(document, "_level").Should().Be("custom");
        
        // Original @ properties should not exist
        TestHelpers.JsonContainsProperty(document, "@timestamp").Should().BeFalse();
        TestHelpers.JsonContainsProperty(document, "@level").Should().BeFalse();
    }

    [Fact]
    public void FormatEvent_WithFormattedProperties_IncludesRenderingsArray()
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEventWithFormattedProperties(
            "Value: {Value:D4}",
            ("Value", 42));
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "_r").Should().BeTrue();
        TestHelpers.AssertJsonProperty(document, "_r", JsonValueKind.Array);
        
        var renderingsArray = TestHelpers.GetJsonPropertyElement(document, "_r");
        renderingsArray.GetArrayLength().Should().Be(1);
        renderingsArray[0].GetString().Should().Be("0042");
    }

    [Fact]
    public void FormatEvent_WithoutFormattedProperties_OmitsRenderingsArray()
    {
        // Arrange
        var logEvent = TestHelpers.CreateLogEvent(
            LogEventLevel.Information,
            "Simple message without formatting");
        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(logEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        TestHelpers.JsonContainsProperty(document, "_r").Should().BeFalse();
    }

    [Fact]
    public void FormatEvent_WithComplexScenario_ProducesCorrectJson()
    {
        // Arrange
        var exception = new ArgumentException("Invalid argument");
        var traceId = ActivityTraceId.CreateRandom();
        var spanId = ActivitySpanId.CreateRandom();
        
        // Create log event with exception using TestHelpers method
        var logEvent = TestHelpers.CreateLogEvent(
            LogEventLevel.Error,
            "Complex scenario occurred",
            exception);

        // Create new LogEvent with tracing using reflection or direct construction
        var template = logEvent.MessageTemplate;
        var properties = logEvent.Properties.Select(p => new LogEventProperty(p.Key, p.Value));
        
        var complexLogEvent = new LogEvent(
            logEvent.Timestamp,
            LogEventLevel.Error,
            exception,
            template,
            properties,
            traceId,
            spanId);

        var output = new StringWriter();
        var valueFormatter = new JsonValueFormatter();

        // Act
        CompactJsonFormatter.FormatEvent(complexLogEvent, output, valueFormatter);

        // Assert
        var result = output.ToString();
        var document = TestHelpers.ParseJson(result);
        
        // Verify all components are present
        TestHelpers.JsonContainsProperty(document, "_t").Should().BeTrue();
        TestHelpers.JsonContainsProperty(document, "_mt").Should().BeTrue();
        TestHelpers.JsonContainsProperty(document, "_l").Should().BeTrue();
        TestHelpers.JsonContainsProperty(document, "_x").Should().BeTrue();
        TestHelpers.JsonContainsProperty(document, "_tr").Should().BeTrue();
        TestHelpers.JsonContainsProperty(document, "_sp").Should().BeTrue();
        
        TestHelpers.GetJsonProperty(document, "_mt").Should().Be("Complex scenario occurred");
        TestHelpers.GetJsonProperty(document, "_l").Should().Be("Error");
        TestHelpers.GetJsonProperty(document, "_x").Should().Contain("ArgumentException");
        TestHelpers.GetJsonProperty(document, "_tr").Should().Be(traceId.ToHexString());
        TestHelpers.GetJsonProperty(document, "_sp").Should().Be(spanId.ToHexString());
    }
}