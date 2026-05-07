namespace AntDesign.X;

public sealed class XChatStore : IDisposable
{
    private readonly IXRequestClient _requestClient;
    private readonly XRequestClientOptions _requestOptions;
    private readonly object _gate = new();
    private readonly List<XBubbleItem> _messages = [];
    private CancellationTokenSource? _requestCts;
    private XRequestHandle? _activeHandle;
    private ChatSubmission? _lastSubmission;
    private ChatSubmission? _activeSubmission;
    private bool _loading;
    private Exception? _lastError;
    private bool _disposed;

    public XChatStore(IXRequestClient requestClient, XRequestClientOptions requestOptions)
    {
        _requestClient = requestClient;
        _requestOptions = requestOptions;
    }

    public event Action? Changed;

    public string ConversationKey { get; set; } = "default";

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

    public bool IsLoading
    {
        get
        {
            lock (_gate)
            {
                return _loading;
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

    public string? ActiveRequestId
    {
        get
        {
            lock (_gate)
            {
                return _activeHandle?.RequestId;
            }
        }
    }

    public void ReplaceMessages(IEnumerable<XBubbleItem> messages)
    {
        CancellationTokenSource? requestCts;
        lock (_gate)
        {
            requestCts = _requestCts;
            _requestCts = null;
            _activeHandle = null;
            _activeSubmission = null;
            _messages.Clear();
            _messages.AddRange(messages);
            _lastSubmission = null;
            _loading = false;
            _lastError = null;
        }

        requestCts?.Cancel();
        requestCts?.Dispose();
        NotifyChanged();
    }

    public async Task SubmitAsync(XSenderRequest request, CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(request.Text) && request.Attachments.Count == 0)
        {
            return;
        }

        var submission = new ChatSubmission(
            Prompt: request.Text,
            Attachments: request.Attachments.ToArray(),
            UserKey: Guid.NewGuid().ToString("N"),
            AssistantKey: Guid.NewGuid().ToString("N"),
            UserIndex: -1,
            AssistantIndex: -1);

        await StartSubmissionAsync(submission, appendUserMessage: true, cancellationToken);
    }

    public async Task RetryAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            return;
        }

        ChatSubmission? submission;
        lock (_gate)
        {
            submission = _lastSubmission;
        }

        if (submission is null)
        {
            return;
        }

        await StartSubmissionAsync(submission, appendUserMessage: false, cancellationToken);
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
            _requestCts?.Cancel();
            _requestCts?.Dispose();
            _requestCts = null;
        }
    }

    private async Task StartSubmissionAsync(
        ChatSubmission submission,
        bool appendUserMessage,
        CancellationToken cancellationToken)
    {
        CancellationTokenSource? previousCts = null;
        CancellationTokenSource? requestCts = null;
        lock (_gate)
        {
            if (_loading)
            {
                previousCts = _requestCts;
                _requestCts = null;
                _activeHandle = null;
                _activeSubmission = null;
                _loading = false;
            }

            _lastError = null;

            if (appendUserMessage)
            {
                var userBubble = CreateUserBubble(submission);
                _messages.Add(userBubble);
                submission = submission with { UserIndex = _messages.Count - 1 };
            }

            if (!appendUserMessage && submission.AssistantIndex >= 0 && submission.AssistantIndex < _messages.Count)
            {
                _messages[submission.AssistantIndex] = CreateAssistantBubble(submission.AssistantKey);
            }
            else
            {
                _messages.Add(CreateAssistantBubble(submission.AssistantKey));
                submission = submission with { AssistantIndex = _messages.Count - 1 };
            }

            _lastSubmission = submission;
            _activeSubmission = submission;
            _loading = true;

            _requestCts = requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        previousCts?.Cancel();
        previousCts?.Dispose();
        NotifyChanged();

        var payload = BuildPayload(submission);
        var requestOptions = new XRequestOptions
        {
            RequestId = submission.AssistantKey,
            RequestUri = new Uri(_requestOptions.ChatPath, UriKind.Relative),
            Body = payload,
            Stream = true,
            StreamMode = XStreamReadMode.Sse,
            Metadata = payload.Metadata,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["x-antdx-scenario"] = "chat",
                ["x-antdx-conversation"] = ConversationKey,
            },
            OnChunk = chunk => ApplyChunkAsync(submission, chunk),
        };

        XRequestHandle handle;
        try
        {
            handle = await _requestClient.RequestAsync(requestOptions, requestCts!.Token);
        }
        catch (Exception exception)
        {
            await FinalizeFailureAsync(submission, exception, aborted: false);
            return;
        }

        lock (_gate)
        {
            _activeHandle = handle;
        }

        _ = TrackCompletionAsync(submission, handle);
    }

    private async Task TrackCompletionAsync(ChatSubmission submission, XRequestHandle handle)
    {
        try
        {
            await handle.Completion;
            await FinalizeSuccessAsync(submission);
        }
        catch (OperationCanceledException)
        {
            await FinalizeFailureAsync(submission, null, aborted: true);
        }
        catch (Exception exception)
        {
            await FinalizeFailureAsync(submission, exception, aborted: false);
        }
    }

    private ValueTask ApplyChunkAsync(ChatSubmission submission, XStreamChunk chunk)
    {
        lock (_gate)
        {
            if (!IsCurrentSubmission(submission))
            {
                return ValueTask.CompletedTask;
            }

            if (submission.AssistantIndex < 0 || submission.AssistantIndex >= _messages.Count)
            {
                return ValueTask.CompletedTask;
            }

            var current = _messages[submission.AssistantIndex];
            var nextContent = current.Content ?? string.Empty;
            var delta = ResolveChunkText(chunk);

            if (!string.IsNullOrWhiteSpace(delta))
            {
                nextContent += delta;
            }

            _messages[submission.AssistantIndex] = current with
            {
                Content = nextContent,
                Loading = true,
                Streaming = true,
                Status = XMessageStatus.Updating,
            };
        }

        NotifyChanged();
        return ValueTask.CompletedTask;
    }

    private async Task FinalizeSuccessAsync(ChatSubmission submission)
    {
        lock (_gate)
        {
            if (!IsCurrentSubmission(submission))
            {
                return;
            }

            UpdateAssistantState(submission, XMessageStatus.Success, loading: false, streaming: false);
            _loading = false;
            _lastError = null;
            _activeHandle = null;
            _activeSubmission = null;
            DisposeRequestCtsLocked();
        }

        NotifyChanged();
        await Task.CompletedTask;
    }

    private async Task FinalizeFailureAsync(ChatSubmission submission, Exception? exception, bool aborted)
    {
        lock (_gate)
        {
            if (!IsCurrentSubmission(submission))
            {
                return;
            }

            UpdateAssistantState(
                submission,
                aborted ? XMessageStatus.Abort : XMessageStatus.Error,
                loading: false,
                streaming: false);
            _loading = false;
            _lastError = exception;
            _activeHandle = null;
            _activeSubmission = null;
            DisposeRequestCtsLocked();
        }

        NotifyChanged();
        await Task.CompletedTask;
    }

    private void UpdateAssistantState(ChatSubmission submission, XMessageStatus status, bool loading, bool streaming)
    {
        if (submission.AssistantIndex < 0 || submission.AssistantIndex >= _messages.Count)
        {
            return;
        }

        var current = _messages[submission.AssistantIndex];
        _messages[submission.AssistantIndex] = current with
        {
            Status = status,
            Loading = loading,
            Streaming = streaming,
        };
    }

    private bool IsCurrentSubmission(ChatSubmission submission)
    {
        return _activeSubmission?.AssistantKey == submission.AssistantKey;
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

    private XChatRequestPayload BuildPayload(ChatSubmission submission)
    {
        List<XRequestMessageDto> messages;
        lock (_gate)
        {
            messages = _messages
                .Where(item => !string.Equals(item.Key, submission.AssistantKey, StringComparison.OrdinalIgnoreCase))
                .Select(MapMessage)
                .ToList();
        }

        return new XChatRequestPayload
        {
            ConversationKey = ConversationKey,
            Prompt = submission.Prompt,
            Messages = messages,
            Attachments = submission.Attachments.Select(MapAttachment).ToArray(),
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["scenario"] = "chat",
                ["conversation"] = ConversationKey,
                ["prompt_length"] = submission.Prompt.Length.ToString(),
            },
        };
    }

    private static XBubbleItem CreateUserBubble(ChatSubmission submission)
    {
        return new XBubbleItem
        {
            Key = submission.UserKey,
            Role = "user",
            Placement = XBubblePlacement.End,
            Header = "You",
            Content = submission.Prompt,
            Markdown = true,
            Status = XMessageStatus.Local,
            Attachments = submission.Attachments.ToArray(),
        };
    }

    private static XBubbleItem CreateAssistantBubble(string key)
    {
        return new XBubbleItem
        {
            Key = key,
            Role = "assistant",
            Placement = XBubblePlacement.Start,
            Header = "Assistant",
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

    private void DisposeRequestCtsLocked()
    {
        _requestCts?.Dispose();
        _requestCts = null;
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }

    private sealed record ChatSubmission(
        string Prompt,
        IReadOnlyList<XAttachmentItem> Attachments,
        string UserKey,
        string AssistantKey,
        int UserIndex,
        int AssistantIndex);
}
