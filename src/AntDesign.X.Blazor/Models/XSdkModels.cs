namespace AntDesign.X;

public sealed record XRequestAttachmentDto
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Url { get; init; }
    public string? ImageUrl { get; init; }
    public string? ContentType { get; init; }
    public long? Size { get; init; }
}

public sealed record XRequestMessageDto
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string? Role { get; init; }
    public string? Header { get; init; }
    public string? Content { get; init; }
    public string? Status { get; init; }
    public bool? Loading { get; init; }
    public bool? Streaming { get; init; }
    public IReadOnlyList<XRequestAttachmentDto> Attachments { get; init; } = Array.Empty<XRequestAttachmentDto>();
}

public sealed record XChatRequestPayload
{
    public string ConversationKey { get; init; } = string.Empty;
    public string Prompt { get; init; } = string.Empty;
    public IReadOnlyList<XRequestMessageDto> Messages { get; init; } = Array.Empty<XRequestMessageDto>();
    public IReadOnlyList<XRequestAttachmentDto> Attachments { get; init; } = Array.Empty<XRequestAttachmentDto>();
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed record XAgentRequestPayload
{
    public string AgentKey { get; init; } = string.Empty;
    public string Prompt { get; init; } = string.Empty;
    public IReadOnlyList<XRequestMessageDto> Messages { get; init; } = Array.Empty<XRequestMessageDto>();
    public IReadOnlyList<XRequestAttachmentDto> Attachments { get; init; } = Array.Empty<XRequestAttachmentDto>();
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed record XAgentRequest
{
    public string Prompt { get; init; } = string.Empty;
    public IReadOnlyList<XAttachmentItem> Attachments { get; init; } = Array.Empty<XAttachmentItem>();
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public sealed record XAgentToolCallItem
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string? Arguments { get; init; }
    public string? Result { get; init; }
    public XMessageStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed record XAgentEventItem
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string Kind { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Content { get; init; }
    public XSemanticStatus Status { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
