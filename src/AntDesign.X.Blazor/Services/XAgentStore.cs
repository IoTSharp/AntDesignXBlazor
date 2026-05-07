using System.Text.Json;

namespace AntDesign.X;

public sealed class XAgentStore : IDisposable
{
    private readonly IXRequestClient _requestClient;
    private readonly XRequestClientOptions _requestOptions;
    private readonly object _gate = new();
    private readonly List<XBubbleItem> _messages = [];
    private readonly List<XThoughtItem> _thoughts = [];
    private readonly List<XSourceItem> _sources = [];
    private readonly List<XAgentToolCallItem> _toolCalls = [];
    private readonly List<XAgentEventItem> _events = [];
    private CancellationTokenSource? _requestCts;
    private XRequestHandle? _activeHandle;
    private XAgentRequest? _lastRequest;
    private AgentRun? _activeRun;
    private bool _running;
    private bool _disposed;
    private Exception? _lastError;

    public XAgentStore(IXRequestClient requestClient, XRequestClientOptions requestOptions)
    {
        _requestClient = requestClient;
        _requestOptions = requestOptions;
        Reset();
    }

    public event Action? Changed;

    public string AgentKey { get; set; } = "default";

    public IReadOnlyList<XBubbleItem> Messages
    {
        get
        {
            lock (_gate)
            {
                return _messages.ToArray();
            }
        }
    }

    public IReadOnlyList<XThoughtItem> Thoughts
    {
        get
        {
            lock (_gate)
            {
                return _thoughts.ToArray();
            }
        }
    }

    public IReadOnlyList<XSourceItem> Sources
    {
        get
        {
            lock (_gate)
            {
                return _sources.ToArray();
            }
        }
    }

    public IReadOnlyList<XAgentToolCallItem> ToolCalls
    {
        get
        {
            lock (_gate)
            {
                return _toolCalls.ToArray();
            }
        }
    }

    public IReadOnlyList<XActionItem> ToolActions =>
        ToolCalls.Select(item => new XActionItem
        {
            Key = item.Key,
            Label = item.Name,
            Tooltip = item.Arguments,
            Danger = item.Status == XMessageStatus.Error,
        }).ToArray();

    public IReadOnlyList<XAgentEventItem> Events
    {
        get
        {
            lock (_gate)
            {
                return _events.ToArray();
            }
        }
    }

    public bool IsRunning
    {
        get
        {
            lock (_gate)
            {
                return _running;
            }
        }
    }

    public Exception? LastError
    {
        get
        {
            lock (_gate)
            {
                return _lastError;
            }
        }
    }

    public void Reset()
    {
        lock (_gate)
        {
            CancelRequestLocked();
            _messages.Clear();
            _thoughts.Clear();
            _sources.Clear();
            _toolCalls.Clear();
            _events.Clear();
            _activeRun = null;
            _lastRequest = null;
            _running = false;
            _lastError = null;

            _messages.Add(new XBubbleItem
            {
                Key = Guid.NewGuid().ToString("N"),
                Role = "system",
                Header = "Agent",
                Content = "点击 Run agent 生成思考链、工具调用和来源。这里会保留当前一次 trace 的完整闭环。",
                Markdown = true,
                Status = XMessageStatus.Local,
            });
        }

        NotifyChanged();
    }

    public async Task RunAsync(XAgentRequest request, CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            return;
        }

        var run = new AgentRun(
            Prompt: request.Prompt,
            Attachments: request.Attachments.ToArray(),
            UserKey: Guid.NewGuid().ToString("N"),
            AssistantKey: Guid.NewGuid().ToString("N"),
            UserIndex: -1,
            AssistantIndex: -1);

        await StartRunAsync(run, cancellationToken, request.Metadata);
    }

    public async Task RetryAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            return;
        }

        XAgentRequest? request;
        lock (_gate)
        {
            request = _lastRequest;
        }

        if (request is null)
        {
            return;
        }

        await RunAsync(request, cancellationToken);
    }

    public Task AbortAsync()
    {
        CancellationTokenSource? requestCts;
        lock (_gate)
        {
            requestCts = _requestCts;
        }

        requestCts?.Cancel();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            CancelRequestLocked();
        }
    }

    private async Task StartRunAsync(
        AgentRun run,
        CancellationToken cancellationToken,
        IReadOnlyDictionary<string, string> metadata)
    {
        CancellationTokenSource? previousCts = null;
        CancellationTokenSource? requestCts = null;

        lock (_gate)
        {
            if (_running)
            {
                previousCts = _requestCts;
                _activeRun = null;
                _activeHandle = null;
                _running = false;
            }

            _lastError = null;
            _messages.Clear();
            _thoughts.Clear();
            _sources.Clear();
            _toolCalls.Clear();
            _events.Clear();

            _messages.Add(CreateSystemBubble());
            _messages.Add(CreateUserBubble(run));
            run = run with
            {
                UserIndex = 1,
                AssistantIndex = 2,
            };
            _messages.Add(CreateAssistantBubble(run.AssistantKey));
            _lastRequest = new XAgentRequest
            {
                Prompt = run.Prompt,
                Attachments = run.Attachments.ToArray(),
                Metadata = metadata.ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase),
            };
            _activeRun = run;
            _running = true;
            _requestCts = requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        previousCts?.Cancel();
        previousCts?.Dispose();
        NotifyChanged();

        var payload = BuildPayload(run, metadata);
        var requestOptions = new XRequestOptions
        {
            RequestId = run.AssistantKey,
            RequestUri = new Uri(_requestOptions.AgentPath, UriKind.Relative),
            Body = payload,
            Stream = true,
            StreamMode = XStreamReadMode.Sse,
            Metadata = payload.Metadata,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["x-antdx-scenario"] = "agent",
                ["x-antdx-agent"] = AgentKey,
            },
            OnChunk = chunk => ApplyChunkAsync(run, chunk),
        };

        XRequestHandle handle;
        try
        {
            handle = await _requestClient.RequestAsync(requestOptions, requestCts!.Token);
        }
        catch (Exception exception)
        {
            await FinalizeRunAsync(run, exception, aborted: false);
            return;
        }

        lock (_gate)
        {
            _activeHandle = handle;
        }

        _ = TrackCompletionAsync(run, handle);
    }

    private async Task TrackCompletionAsync(AgentRun run, XRequestHandle handle)
    {
        try
        {
            await handle.Completion;
            await FinalizeRunAsync(run, null, aborted: false);
        }
        catch (OperationCanceledException)
        {
            await FinalizeRunAsync(run, null, aborted: true);
        }
        catch (Exception exception)
        {
            await FinalizeRunAsync(run, exception, aborted: false);
        }
    }

    private ValueTask ApplyChunkAsync(AgentRun run, XStreamChunk chunk)
    {
        lock (_gate)
        {
            if (!IsCurrentRun(run))
            {
                return ValueTask.CompletedTask;
            }

            var kind = ResolveEventKind(chunk);
            var eventItem = CreateEventItem(kind, chunk);
            _events.Add(eventItem);

            if (kind is "thought" or "analysis")
            {
                _thoughts.Add(CreateThoughtItem(chunk, eventItem));
            }
            else if (kind is "tool" or "tool_call" or "tool-call")
            {
                _toolCalls.Add(CreateToolCallItem(chunk, eventItem));
            }
            else if (kind is "source" or "citation")
            {
                _sources.Add(CreateSourceItem(chunk, eventItem));
            }
            else if (kind is "message" or "assistant" or "delta")
            {
                AppendAssistantDelta(run, ResolveChunkText(chunk));
            }
        }

        NotifyChanged();
        return ValueTask.CompletedTask;
    }

    private async Task FinalizeRunAsync(AgentRun run, Exception? exception, bool aborted)
    {
        lock (_gate)
        {
            if (!IsCurrentRun(run))
            {
                return;
            }

            if (run.AssistantIndex >= 0 && run.AssistantIndex < _messages.Count)
            {
                var current = _messages[run.AssistantIndex];
                _messages[run.AssistantIndex] = current with
                {
                    Status = aborted ? XMessageStatus.Abort : exception is null ? XMessageStatus.Success : XMessageStatus.Error,
                    Loading = false,
                    Streaming = false,
                };
            }

            if (exception is not null && !aborted)
            {
                _lastError = exception;
            }

            _running = false;
            _activeHandle = null;
            _activeRun = null;
            CancelRequestLocked();
        }

        NotifyChanged();
        await Task.CompletedTask;
    }

    private void AppendAssistantDelta(AgentRun run, string delta)
    {
        if (string.IsNullOrWhiteSpace(delta) ||
            run.AssistantIndex < 0 ||
            run.AssistantIndex >= _messages.Count)
        {
            return;
        }

        var current = _messages[run.AssistantIndex];
        _messages[run.AssistantIndex] = current with
        {
            Content = (current.Content ?? string.Empty) + delta,
            Loading = true,
            Streaming = true,
            Status = XMessageStatus.Updating,
        };
    }

    private bool IsCurrentRun(AgentRun run)
    {
        return _activeRun?.AssistantKey == run.AssistantKey;
    }

    private XAgentRequestPayload BuildPayload(AgentRun run, IReadOnlyDictionary<string, string> metadata)
    {
        List<XRequestMessageDto> messages;
        lock (_gate)
        {
            messages = _messages
                .Where(item => !string.Equals(item.Key, run.AssistantKey, StringComparison.OrdinalIgnoreCase))
                .Select(MapMessage)
                .ToList();
        }

        return new XAgentRequestPayload
        {
            AgentKey = AgentKey,
            Prompt = run.Prompt,
            Messages = messages,
            Attachments = run.Attachments.Select(MapAttachment).ToArray(),
            Metadata = new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase)
            {
                ["scenario"] = "agent",
                ["agent_key"] = AgentKey,
                ["prompt_length"] = run.Prompt.Length.ToString(),
            },
        };
    }

    private static XBubbleItem CreateSystemBubble()
    {
        return new XBubbleItem
        {
            Key = Guid.NewGuid().ToString("N"),
            Role = "system",
            Header = "System",
            Content = "Agent trace will capture thoughts, tool calls, and sources for the current run.",
            Markdown = true,
            Status = XMessageStatus.Local,
        };
    }

    private static XBubbleItem CreateUserBubble(AgentRun run)
    {
        return new XBubbleItem
        {
            Key = run.UserKey,
            Role = "user",
            Placement = XBubblePlacement.End,
            Header = "You",
            Content = run.Prompt,
            Markdown = true,
            Status = XMessageStatus.Local,
            Attachments = run.Attachments.ToArray(),
        };
    }

    private static XBubbleItem CreateAssistantBubble(string key)
    {
        return new XBubbleItem
        {
            Key = key,
            Role = "assistant",
            Placement = XBubblePlacement.Start,
            Header = "Agent",
            Content = string.Empty,
            Markdown = true,
            Loading = true,
            Streaming = true,
            Status = XMessageStatus.Loading,
        };
    }

    private static XRequestMessageDto MapMessage(XBubbleItem item)
    {
        return new XRequestMessageDto
        {
            Key = item.Key,
            Role = item.Role,
            Header = item.Header,
            Content = item.Content,
            Status = item.Status?.ToString(),
            Loading = item.Loading,
            Streaming = item.Streaming,
            Attachments = item.Attachments.Select(MapAttachment).ToArray(),
        };
    }

    private static XRequestAttachmentDto MapAttachment(XAttachmentItem item)
    {
        return new XRequestAttachmentDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Url = item.Url,
            ImageUrl = item.ImageUrl,
            ContentType = item.ContentType,
            Size = item.Size,
        };
    }

    private static XAgentEventItem CreateEventItem(string kind, XStreamChunk chunk)
    {
        var isMessageKind = kind is "message" or "assistant" or "delta";
        var title = ResolveString(chunk, "title") ?? ResolveString(chunk, "name") ?? kind;
        var description = ResolveString(chunk, "description") ?? ResolveString(chunk, "summary");
        var content = ResolveString(chunk, "content");
        if (string.IsNullOrWhiteSpace(content) && isMessageKind)
        {
            content = ResolveChunkText(chunk);
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            content = chunk.IsDone
                ? "done"
                : isMessageKind && chunk.Json is not null
                    ? null
                    : chunk.Data;
        }

        return new XAgentEventItem
        {
            Kind = kind,
            Title = title,
            Description = description,
            Content = content,
            Status = MapSemanticStatus(ResolveString(chunk, "status") ?? ResolveString(chunk, "level")),
            Metadata = ResolveMetadata(chunk),
        };
    }

    private static XThoughtItem CreateThoughtItem(XStreamChunk chunk, XAgentEventItem eventItem)
    {
        return new XThoughtItem
        {
            Key = eventItem.Key,
            Title = eventItem.Title,
            Description = eventItem.Description,
            Content = eventItem.Content,
            Status = eventItem.Status,
        };
    }

    private static XAgentToolCallItem CreateToolCallItem(XStreamChunk chunk, XAgentEventItem eventItem)
    {
        return new XAgentToolCallItem
        {
            Key = eventItem.Key,
            Name = eventItem.Title,
            Arguments = ResolveString(chunk, "arguments"),
            Result = ResolveString(chunk, "result") ?? eventItem.Content,
            Status = MapMessageStatus(ResolveString(chunk, "status")),
        };
    }

    private static XSourceItem CreateSourceItem(XStreamChunk chunk, XAgentEventItem eventItem)
    {
        return new XSourceItem
        {
            Key = eventItem.Key,
            Title = eventItem.Title,
            Description = eventItem.Description,
            Url = ResolveString(chunk, "url"),
            Icon = ResolveString(chunk, "icon"),
        };
    }

    private static string ResolveEventKind(XStreamChunk chunk)
    {
        var kind = chunk.Event;
        if (!string.IsNullOrWhiteSpace(kind))
        {
            return kind!;
        }

        if (TryGetJsonRoot(chunk, out var root) &&
            root.TryGetProperty("type", out var type) &&
            type.ValueKind == JsonValueKind.String)
        {
            return type.GetString() ?? "message";
        }

        return "message";
    }

    private static string ResolveChunkText(XStreamChunk chunk)
    {
        if (XStreamReader.TryReadOpenAiContent(chunk, out var content) &&
            !string.IsNullOrWhiteSpace(content))
        {
            return content;
        }

        if (chunk.Json is not null || LooksLikeJson(chunk.Data))
        {
            return string.Empty;
        }

        return chunk.Data == "[DONE]" ? string.Empty : chunk.Data;
    }

    private static bool LooksLikeJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.TrimStart();
        return trimmed.StartsWith('{') || trimmed.StartsWith('[');
    }

    private static XSemanticStatus MapSemanticStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "processing" or "running" or "working" => XSemanticStatus.Processing,
            "success" or "done" or "succeeded" => XSemanticStatus.Success,
            "warning" or "warn" => XSemanticStatus.Warning,
            "error" or "failed" or "fail" => XSemanticStatus.Error,
            _ => XSemanticStatus.Default,
        };
    }

    private static XMessageStatus MapMessageStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "loading" => XMessageStatus.Loading,
            "updating" or "processing" => XMessageStatus.Updating,
            "success" or "done" or "succeeded" => XMessageStatus.Success,
            "error" or "failed" or "fail" => XMessageStatus.Error,
            "abort" or "aborted" => XMessageStatus.Abort,
            _ => XMessageStatus.Local,
        };
    }

    private static IReadOnlyDictionary<string, string> ResolveMetadata(XStreamChunk chunk)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in chunk.Fields)
        {
            metadata[field.Key] = field.Value;
        }

        if (TryGetJsonRoot(chunk, out var root) &&
            root.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    metadata[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }
        }

        return metadata;
    }

    private static string? ResolveString(XStreamChunk chunk, string propertyName)
    {
        if (TryGetJsonRoot(chunk, out var root) &&
            root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
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

    private void CancelRequestLocked()
    {
        _requestCts?.Cancel();
        _requestCts?.Dispose();
        _requestCts = null;
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }

    private sealed record AgentRun(
        string Prompt,
        IReadOnlyList<XAttachmentItem> Attachments,
        string UserKey,
        string AssistantKey,
        int UserIndex,
        int AssistantIndex);
}
