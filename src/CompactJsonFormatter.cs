using System.Globalization;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Parsing;

namespace LayeredCraft.Logging.CompactJsonFormatter;

/// <summary>
/// A high-performance Serilog formatter that emits structured JSON logs
/// using field names compatible with AWS CloudWatch Logs.
/// <para>
/// This formatter is a customized variant of <see cref="CompactJsonFormatter"/> 
/// that avoids reserved field names like <c>@l</c>, <c>@t</c>, etc. to support
/// CloudWatch metric filters and tooling that cannot parse escaped metadata keys.
/// </para>
/// <para>
/// Output is single-line JSON, optimized for ingestion and filtering.
/// </para>
/// </summary>
public sealed class CompactJsonFormatter : ITextFormatter
{
    // JSON property names for CloudWatch-compatible metadata fields
    /// <summary>Timestamp property name (replaces standard @t)</summary>
    private const string TimestampProperty = "_t";
    /// <summary>Message template property name (replaces standard @mt)</summary>
    private const string MessageTemplateProperty = "_mt";
    /// <summary>Log level property name (replaces standard @l)</summary>
    private const string LevelProperty = "_l";
    /// <summary>Exception property name (replaces standard @x)</summary>
    private const string ExceptionProperty = "_x";
    /// <summary>Renderings array property name (replaces standard @r)</summary>
    private const string RenderingsProperty = "_r";
    /// <summary>Trace ID property name (replaces standard @tr)</summary>
    private const string TraceIdProperty = "_tr";
    /// <summary>Span ID property name (replaces standard @sp)</summary>
    private const string SpanIdProperty = "_sp";
    
    readonly JsonValueFormatter _valueFormatter;

    /// <summary>
    /// Constructs an instance of <see cref="CompactJsonFormatter"/>, optionally
    /// providing a custom value formatter.
    /// </summary>
    /// <param name="valueFormatter">
    /// A formatter for <see cref="LogEventPropertyValue"/>s, or null to use the default.
    /// </param>
    public CompactJsonFormatter(JsonValueFormatter? valueFormatter = null)
    {
        _valueFormatter = valueFormatter ?? new JsonValueFormatter(typeTagName: "$type");
    }

    /// <summary>
    /// Format the log event into the output. Subsequent events will be newline-delimited.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    public void Format(LogEvent logEvent, TextWriter output)
    {
        FormatEvent(logEvent, output, _valueFormatter);
        output.WriteLine();
    }

    /// <summary>
    /// Format the log event into the output.
    /// </summary>
    /// <param name="logEvent">The event to format.</param>
    /// <param name="output">The output.</param>
    /// <param name="valueFormatter">A value formatter for <see cref="LogEventPropertyValue"/>s on the event.</param>
    public static void FormatEvent(LogEvent logEvent, TextWriter output, JsonValueFormatter valueFormatter)
    {
        if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
        if (output == null) throw new ArgumentNullException(nameof(output));
        if (valueFormatter == null) throw new ArgumentNullException(nameof(valueFormatter));

        output.Write($"{{\"{TimestampProperty}\":\"");
        output.Write(logEvent.Timestamp.UtcDateTime.ToString("O"));
        output.Write($"\",\"{MessageTemplateProperty}\":");
        JsonValueFormatter.WriteQuotedJsonString(logEvent.MessageTemplate.Text, output);

        var tokensWithFormat = logEvent.MessageTemplate.Tokens
            .OfType<PropertyToken>()
            .Where(pt => pt.Format != null)
            .ToList();

        if (tokensWithFormat.Count > 0)
        {
            output.Write($",\"{RenderingsProperty}\":[");
            var delim = "";
            foreach (var r in tokensWithFormat)
            {
                output.Write(delim);
                delim = ",";
                var space = new StringWriter();
                r.Render(logEvent.Properties, space, CultureInfo.InvariantCulture);
                JsonValueFormatter.WriteQuotedJsonString(space.ToString(), output);
            }

            output.Write(']');
        }

        if (logEvent.Level != LogEventLevel.Information)
        {
            output.Write($",\"{LevelProperty}\":\"");
            output.Write(logEvent.Level);
            output.Write('\"');
        }

        if (logEvent.Exception != null)
        {
            output.Write($",\"{ExceptionProperty}\":");
            JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
        }

        if (logEvent.TraceId != null)
        {
            output.Write($",\"{TraceIdProperty}\":\"");
            output.Write(logEvent.TraceId.Value.ToHexString());
            output.Write('\"');
        }

        if (logEvent.SpanId != null)
        {
            output.Write($",\"{SpanIdProperty}\":\"");
            output.Write(logEvent.SpanId.Value.ToHexString());
            output.Write('\"');
        }

        foreach (var property in logEvent.Properties)
        {
            var name = property.Key;
            if (name.Length > 0 && name[0] == '@')
            {
                // Escape first '@' by converting to '_'
                name = '_' + name[1..];
            }

            output.Write(',');
            JsonValueFormatter.WriteQuotedJsonString(name, output);
            output.Write(':');
            valueFormatter.Format(property.Value, output);
        }

        output.Write('}');
    }
}