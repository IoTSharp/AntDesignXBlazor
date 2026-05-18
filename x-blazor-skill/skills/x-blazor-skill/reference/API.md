# AntDesign.X.Blazor API Notes

Use this reference for service registration, core records, and request/streaming APIs.

## Namespaces

```razor
@using AntDesign.X
@using AntDesign.X.Components
```

```csharp
using AntDesign.X;
```

Component classes live under `AntDesign.X.Components`; models, services, and extension methods live under `AntDesign.X`.

## Service Registration

`AddAntDesignX` registers:

| Service | Lifetime | Purpose |
| --- | --- | --- |
| `XRequestClientOptions` | singleton | Request paths, base address, timeout, headers |
| `IXNotificationService` | scoped | Notification queue |
| `IXRequestClient` | scoped | Low-level request and stream client |
| `XChatStore` | scoped | Chat message state and streaming lifecycle |
| `XAgentStore` | scoped | Agent message/event/tool-call state |
| `IXRendererRegistry` | singleton | Render delegation registry |
| `IXLocaleService` | scoped | Component text locale service |

```csharp
builder.Services.AddAntDesignX(options =>
{
    options.BaseAddress = new Uri("https://example.com");
    options.ChatPath = "/api/chat";
    options.AgentPath = "/api/agent";
    options.Timeout = TimeSpan.FromMinutes(2);
    options.DefaultHeaders["x-client"] = "blazor";
});
```

## Core Records

| Record | Use |
| --- | --- |
| `XBubbleItem` | Message rows for `XBubbleList`. |
| `XBubbleRoleConfig` | Role-specific placement, avatar, Markdown, shape, and template defaults. |
| `XAttachmentItem` | Files displayed by `XSender`, `XAttachments`, `XFileCard`, or message bubbles. |
| `XActionItem` | Button/action metadata for `XActions`, `XBubble`, and `XSender`. |
| `XConversationItem` | Conversation list item. |
| `XPromptItem` | Prompt card item, including nested children. |
| `XSourceItem` | Citation/source row. |
| `XThoughtItem` | Thought chain node. |
| `XFolderItem` | File/folder tree item. |
| `XNotificationItem` | Notification payload. |
| `XSenderRequest` | Submitted text plus attachments. |
| `XThemeTokens` | Theme custom properties for `XProvider`. |

Prefer these records over anonymous objects so code remains strongly typed.

## XBubbleItem Example

```csharp
var message = new XBubbleItem
{
    Role = "assistant",
    Header = "Assistant",
    AvatarIcon = "robot",
    Content = "Here is **Markdown** content.",
    Markdown = true,
    Streaming = false,
    Status = XMessageStatus.Success,
    Actions =
    [
        new XActionItem { Key = "copy", Icon = "copy", Tooltip = "Copy" },
        new XActionItem { Key = "retry", Icon = "reload", Tooltip = "Retry" }
    ]
};
```

## Request Client

Use `IXRequestClient` when you need custom control below `XChatStore` or `XAgentStore`.

```csharp
var handle = await RequestClient.RequestAsync(new XRequestOptions
{
    RequestId = Guid.NewGuid().ToString("N"),
    RequestUri = new Uri("/api/chat", UriKind.Relative),
    Body = payload,
    Stream = true,
    StreamMode = XStreamReadMode.Sse,
    OnChunk = chunk =>
    {
        // Append chunk content to the active assistant message.
        return Task.CompletedTask;
    }
}, cancellationToken);

await handle.Completion;
```

Use the stores first unless the app needs a custom protocol or message lifecycle.

## Chat Store

`XChatStore` exposes:

| Member | Use |
| --- | --- |
| `Messages` | Snapshot for `XBubbleList.Items`. |
| `IsLoading` | Bind to `XSender.Loading` and `XBubbleList.Loading`. |
| `LastError` | Render errors or notifications. |
| `ActiveRequestId` | Track the running assistant request. |
| `Changed` | Subscribe and call `StateHasChanged`. |
| `SubmitAsync(XSenderRequest, ct)` | Append user message and stream assistant response. |
| `RetryAsync(ct)` | Re-run the last submission. |
| `AbortAsync()` | Cancel the active request. |
| `ReplaceMessages(...)` | Reset or hydrate conversation state. |

Always unsubscribe from `Changed` in `Dispose`.

## Agent Store

`XAgentStore` follows the same UI subscription pattern as `XChatStore` and is intended for agent workflows. Use it when the UI shows tool calls, events, or agent-specific progress alongside chat messages.

Typical binding:

```razor
<XThoughtChain Items="@thoughtItems" />
<XBubbleList Items="@Agent.Messages" Loading="@Agent.IsRunning" />
<XSender Loading="@Agent.IsRunning" OnSubmit="@RunAgent" OnCancel="@Agent.AbortAsync" />
```

## Static Assets

Include:

```html
<link rel="stylesheet" href="_content/AntDesign/css/ant-design-blazor.css" />
<link rel="stylesheet" href="_content/AntDesign.X.Blazor/css/antdesign-x.css" />
<script src="_content/AntDesign/js/ant-design-blazor.js"></script>
```

Add Mermaid only when rendering `XMermaid`:

```html
<script src="https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js"></script>
```

## File Upload Notes

Blazor file input uses `IBrowserFile`. Use `BeforeUpload` to filter files before they enter the attachment list.

```csharp
private ValueTask<bool> BeforeUpload(IBrowserFile file)
{
    var accepted = file.Size <= 10 * 1024 * 1024;
    return ValueTask.FromResult(accepted);
}
```

Do not trust browser-provided file metadata for security decisions on the server.
