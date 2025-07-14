using System.Diagnostics;
using System.Text.Json;
using Serilog.Events;
using Serilog.Parsing;

namespace LayeredCraft.Logging.CompactJsonFormatter.Tests;

internal static class TestHelpers
{
    public static LogEvent CreateLogEvent(
        LogEventLevel level = LogEventLevel.Information,
        string messageTemplate = "Test message",
        Exception? exception = null,
        params (string name, object value)[] properties)
    {
        var template = new MessageTemplateParser().Parse(messageTemplate);
        var eventProperties = new List<LogEventProperty>();
        
        foreach (var (name, value) in properties)
        {
            eventProperties.Add(new LogEventProperty(name, new ScalarValue(value)));
        }

        return new LogEvent(
            DateTimeOffset.UtcNow,
            level,
            exception,
            template,
            eventProperties);
    }

    public static LogEvent CreateLogEventWithTracing(
        LogEventLevel level = LogEventLevel.Information,
        string messageTemplate = "Test message",
        ActivityTraceId? traceId = null,
        ActivitySpanId? spanId = null)
    {
        var template = new MessageTemplateParser().Parse(messageTemplate);
        var eventProperties = new List<LogEventProperty>();

        return new LogEvent(
            DateTimeOffset.UtcNow,
            level,
            null,
            template,
            eventProperties,
            traceId ?? default,
            spanId ?? default);
    }

    public static LogEvent CreateLogEventWithFormattedProperties(
        string messageTemplate = "Value: {Value:D4}",
        params (string name, object value)[] properties)
    {
        var template = new MessageTemplateParser().Parse(messageTemplate);
        var eventProperties = new List<LogEventProperty>();
        
        foreach (var (name, value) in properties)
        {
            eventProperties.Add(new LogEventProperty(name, new ScalarValue(value)));
        }

        return new LogEvent(
            DateTimeOffset.UtcNow,
            LogEventLevel.Information,
            null,
            template,
            eventProperties);
    }

    public static JsonDocument ParseJson(string json)
    {
        return JsonDocument.Parse(json);
    }

    public static void AssertJsonProperty(JsonDocument document, string propertyName, string expectedValue)
    {
        if (!document.RootElement.TryGetProperty(propertyName, out var property))
        {
            throw new ArgumentException($"Property '{propertyName}' not found in JSON");
        }

        if (property.GetString() != expectedValue)
        {
            throw new ArgumentException($"Property '{propertyName}' expected '{expectedValue}' but got '{property.GetString()}'");
        }
    }

    public static void AssertJsonProperty(JsonDocument document, string propertyName, JsonValueKind expectedType)
    {
        if (!document.RootElement.TryGetProperty(propertyName, out var property))
        {
            throw new ArgumentException($"Property '{propertyName}' not found in JSON");
        }

        if (property.ValueKind != expectedType)
        {
            throw new ArgumentException($"Property '{propertyName}' expected type '{expectedType}' but got '{property.ValueKind}'");
        }
    }

    public static bool JsonContainsProperty(JsonDocument document, string propertyName)
    {
        return document.RootElement.TryGetProperty(propertyName, out _);
    }

    public static string GetJsonProperty(JsonDocument document, string propertyName)
    {
        if (!document.RootElement.TryGetProperty(propertyName, out var property))
        {
            throw new ArgumentException($"Property '{propertyName}' not found in JSON");
        }

        return property.GetString() ?? string.Empty;
    }

    public static JsonElement GetJsonPropertyElement(JsonDocument document, string propertyName)
    {
        if (!document.RootElement.TryGetProperty(propertyName, out var property))
        {
            throw new ArgumentException($"Property '{propertyName}' not found in JSON");
        }

        return property;
    }
}