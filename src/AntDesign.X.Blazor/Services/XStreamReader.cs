using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace AntDesign.X;

public enum XStreamReadMode
{
    Sse = 0,
    Line = 1,
}

public sealed record XStreamReaderOptions
{
    public XStreamReadMode Mode { get; init; } = XStreamReadMode.Sse;
    public bool ParseJsonData { get; init; } = true;
    public JsonSerializerOptions? JsonSerializerOptions { get; init; }
    public bool IgnoreComments { get; init; } = true;
}

public sealed record XStreamChunk
{
    public string Raw { get; init; } = string.Empty;
    public string? Event { get; init; }
    public string? Id { get; init; }
    public string Data { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Fields { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public JsonElement? Json { get; init; }
    public bool IsDone { get; init; }
}

public static class XStreamReader
{
    public static async IAsyncEnumerable<XStreamChunk> ReadAsync(
        Stream stream,
        XStreamReaderOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        options ??= new XStreamReaderOptions();

        if (options.Mode == XStreamReadMode.Line)
        {
            await foreach (var chunk in ReadLineAsync(stream, options, cancellationToken))
            {
                yield return chunk;
            }

            yield break;
        }

        await foreach (var chunk in ReadSseAsync(stream, options, cancellationToken))
        {
            yield return chunk;
        }
    }

    private static async IAsyncEnumerable<XStreamChunk> ReadSseAsync(
        Stream stream,
        XStreamReaderOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);
        var block = new List<string>();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                break;
            }

            if (line.Length == 0)
            {
                if (block.Count > 0)
                {
                    yield return BuildSseChunk(block, options);
                    block.Clear();
                }

                continue;
            }

            block.Add(line);
        }

        if (block.Count > 0)
        {
            yield return BuildSseChunk(block, options);
        }
    }

    private static async IAsyncEnumerable<XStreamChunk> ReadLineAsync(
        Stream stream,
        XStreamReaderOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, true, leaveOpen: true);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var line = await reader.ReadLineAsync();
            if (line is null)
            {
                yield break;
            }

            if (line.Length == 0)
            {
                continue;
            }

            yield return BuildChunk(
                raw: line,
                eventName: "message",
                id: null,
                data: line,
                fields: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                options: options,
                isDone: string.Equals(line, "[DONE]", StringComparison.OrdinalIgnoreCase));
        }
    }

    private static XStreamChunk BuildSseChunk(IReadOnlyList<string> lines, XStreamReaderOptions options)
    {
        string? eventName = null;
        string? id = null;
        var dataLines = new List<string>();
        var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            if (line.StartsWith(":", StringComparison.Ordinal))
            {
                if (!options.IgnoreComments)
                {
                    fields[line] = string.Empty;
                }

                continue;
            }

            var separatorIndex = line.IndexOf(':');
            var name = separatorIndex >= 0 ? line[..separatorIndex] : line;
            var value = separatorIndex >= 0 ? line[(separatorIndex + 1)..].TrimStart() : string.Empty;

            switch (name)
            {
                case "event":
                    eventName = value;
                    break;
                case "id":
                    id = value;
                    break;
                case "data":
                    dataLines.Add(value);
                    break;
                default:
                    fields[name] = value;
                    break;
            }
        }

        var data = string.Join("\n", dataLines);
        return BuildChunk(
            raw: string.Join("\n", lines),
            eventName: eventName,
            id: id,
            data: data,
            fields: fields,
            options: options,
            isDone: string.Equals(data, "[DONE]", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(eventName, "done", StringComparison.OrdinalIgnoreCase));
    }

    private static XStreamChunk BuildChunk(
        string raw,
        string? eventName,
        string? id,
        string data,
        IReadOnlyDictionary<string, string> fields,
        XStreamReaderOptions options,
        bool isDone)
    {
        JsonElement? json = null;

        if (options.ParseJsonData &&
            !isDone &&
            !string.IsNullOrWhiteSpace(data))
        {
            try
            {
                using var document = JsonDocument.Parse(data);
                json = document.RootElement.Clone();
            }
            catch (JsonException)
            {
            }
        }

        return new XStreamChunk
        {
            Raw = raw,
            Event = eventName,
            Id = id,
            Data = data,
            Fields = fields,
            Json = json,
            IsDone = isDone,
        };
    }

    public static bool TryReadOpenAiContent(XStreamChunk chunk, out string? content)
    {
        content = null;

        if (chunk.IsDone)
        {
            return false;
        }

        if (TryGetJsonRoot(chunk, out var root))
        {
            if (TryReadChoicesContent(root, out content))
            {
                return !string.IsNullOrWhiteSpace(content);
            }

            if (TryReadOutputContent(root, out content))
            {
                return !string.IsNullOrWhiteSpace(content);
            }

            if (root.ValueKind == JsonValueKind.String)
            {
                content = root.GetString();
                return !string.IsNullOrWhiteSpace(content);
            }

            return false;
        }

        if (!string.IsNullOrWhiteSpace(chunk.Data) &&
            !string.Equals(chunk.Data, "[DONE]", StringComparison.OrdinalIgnoreCase))
        {
            content = chunk.Data;
            return true;
        }

        return false;
    }

    public static bool TryReadOpenAiFinishReason(XStreamChunk chunk, out string? finishReason)
    {
        finishReason = null;

        if (!TryGetJsonRoot(chunk, out var root))
        {
            return false;
        }

        if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array)
        {
            foreach (var choice in choices.EnumerateArray())
            {
                if (choice.TryGetProperty("finish_reason", out var reason) &&
                    reason.ValueKind == JsonValueKind.String)
                {
                    finishReason = reason.GetString();
                    return !string.IsNullOrWhiteSpace(finishReason);
                }
            }
        }

        if (root.TryGetProperty("status", out var status) &&
            status.ValueKind == JsonValueKind.String)
        {
            finishReason = status.GetString();
            return !string.IsNullOrWhiteSpace(finishReason);
        }

        return false;
    }

    private static bool TryReadChoicesContent(JsonElement root, out string? content)
    {
        content = null;

        if (!root.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var choice in choices.EnumerateArray())
        {
            if (choice.TryGetProperty("delta", out var delta) &&
                delta.TryGetProperty("content", out var deltaContent) &&
                deltaContent.ValueKind == JsonValueKind.String)
            {
                content = deltaContent.GetString();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    return true;
                }
            }

            if (choice.TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var messageContent) &&
                messageContent.ValueKind == JsonValueKind.String)
            {
                content = messageContent.GetString();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryReadOutputContent(JsonElement root, out string? content)
    {
        content = null;

        if (!root.TryGetProperty("output", out var output) ||
            output.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in output.EnumerateArray())
        {
            if (item.TryGetProperty("content", out var contentParts) &&
                contentParts.ValueKind == JsonValueKind.Array)
            {
                foreach (var part in contentParts.EnumerateArray())
                {
                    if (part.TryGetProperty("text", out var text) &&
                        text.ValueKind == JsonValueKind.String)
                    {
                        content = text.GetString();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            return true;
                        }
                    }
                }
            }

            if (item.TryGetProperty("text", out var itemText) &&
                itemText.ValueKind == JsonValueKind.String)
            {
                content = itemText.GetString();
                if (!string.IsNullOrWhiteSpace(content))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryGetJsonRoot(XStreamChunk chunk, out JsonElement root)
    {
        if (chunk.Json is JsonElement json)
        {
            root = json;
            return true;
        }

        if (string.IsNullOrWhiteSpace(chunk.Data) ||
            string.Equals(chunk.Data, "[DONE]", StringComparison.OrdinalIgnoreCase))
        {
            root = default;
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(chunk.Data);
            root = document.RootElement.Clone();
            return true;
        }
        catch (JsonException)
        {
            root = default;
            return false;
        }
    }
}
