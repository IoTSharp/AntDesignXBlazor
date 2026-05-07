using Microsoft.AspNetCore.Components;

namespace AntDesign.X;

public sealed record XActionItem
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string? Label { get; init; }
    public string? Icon { get; init; }
    public string? Tooltip { get; init; }
    public bool Disabled { get; init; }
    public bool Danger { get; init; }
    public RenderFragment? Template { get; init; }
}

public sealed record XAttachmentItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Url { get; init; }
    public string? ImageUrl { get; init; }
    public string? ContentType { get; init; }
    public long? Size { get; init; }
    public XFileCardStatus Status { get; init; }
    public int? Percent { get; init; }
    public bool Removable { get; init; } = true;
}

public sealed record XBubbleItem
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string? Role { get; init; }
    public string? Header { get; init; }
    public string? Content { get; init; }
    public RenderFragment? ContentTemplate { get; init; }
    public string? AvatarIcon { get; init; }
    public string? AvatarUrl { get; init; }
    public XBubblePlacement Placement { get; init; }
    public XBubbleVariant Variant { get; init; }
    public bool Loading { get; init; }
    public IReadOnlyList<XAttachmentItem> Attachments { get; init; } = Array.Empty<XAttachmentItem>();
    public IReadOnlyList<XActionItem> Actions { get; init; } = Array.Empty<XActionItem>();
}

public sealed record XConversationItem
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Group { get; init; }
    public int? Count { get; init; }
    public bool Disabled { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public sealed record XPromptItem
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public string? Tag { get; init; }
    public bool Disabled { get; init; }
    public IReadOnlyList<XPromptItem> Children { get; init; } = Array.Empty<XPromptItem>();
}

public sealed record XSourceItem
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Url { get; init; }
    public string? Icon { get; init; }
}

public sealed record XThoughtItem
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Content { get; init; }
    public RenderFragment? ContentTemplate { get; init; }
    public string? Icon { get; init; }
    public XSemanticStatus Status { get; init; }
}

public sealed record XFolderItem
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public bool Disabled { get; init; }
    public IReadOnlyList<XFolderItem> Children { get; init; } = Array.Empty<XFolderItem>();
}

public sealed record XNotificationItem
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public XSemanticStatus Status { get; init; }
    public bool Closable { get; init; } = true;
}

public sealed record XSenderRequest
{
    public string Text { get; init; } = string.Empty;
    public IReadOnlyList<XAttachmentItem> Attachments { get; init; } = Array.Empty<XAttachmentItem>();
}

public sealed record XThemeTokens
{
    public string? PrimaryColor { get; init; }
    public string? BorderRadius { get; init; }
    public string? BubbleStartBackground { get; init; }
    public string? BubbleEndBackground { get; init; }
    public string? PageBackground { get; init; }
    public string? ComponentBackground { get; init; }
}
