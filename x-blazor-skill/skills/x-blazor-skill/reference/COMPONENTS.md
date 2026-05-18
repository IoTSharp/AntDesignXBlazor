# AntDesign.X.Blazor Components

Use this reference when selecting components or checking the main data model for a UI surface.

## Component Groups

| Stage | Components | Blazor data |
| --- | --- | --- |
| General chat | `XBubble`, `XBubbleList`, `XConversations`, `XNotification` | `XBubbleItem`, `XConversationItem`, `XNotificationItem` |
| Wake / prompt | `XWelcome`, `XPrompts` | `XPromptItem` |
| Express / input | `XSender`, `XAttachments`, `XSuggestion` | `XSenderRequest`, `XAttachmentItem` |
| Thinking | `XThoughtChain`, `XThink` | `XThoughtItem` |
| Feedback / evidence | `XActions`, `XFileCard`, `XSources`, `XFolder` | `XActionItem`, `XSourceItem`, `XFolderItem` |
| Content rendering | `XMarkdown`, `XCodeHighlighter`, `XMermaid` | string content plus optional templates |
| Global | `XProvider` | `XThemeTokens` |

## XProvider

Wrap the AI experience in `XProvider` when you need theme, locale, or token support.

```razor
<XProvider Theme="dark"
           Locale="en-US"
           Tokens="@tokens">
    @ChildContent
</XProvider>

@code {
    private readonly XThemeTokens tokens = new()
    {
        PrimaryColor = "#1677ff",
        BubbleStartBackground = "#f6f8fa",
        BubbleEndBackground = "#e6f4ff"
    };
}
```

Use `Theme="light"` or `Theme="dark"`. `Tokens` become CSS custom properties through the `antdx-` styling layer.

## XBubble and XBubbleList

Use `XBubbleList` for normal chat history:

```razor
<XBubbleList Items="@messages"
             Loading="@chat.IsLoading"
             AutoScroll="true"
             OnAction="@HandleAction" />
```

`XBubbleList` applies default role config for:

| Role | Default behavior |
| --- | --- |
| `assistant` / `ai` | start placement, filled, round, Markdown enabled |
| `user` | end placement, filled, corner, Markdown enabled |
| `system` | start placement, outlined, Markdown enabled |
| `tool` | start placement, borderless, Markdown enabled |
| `divider` | renders as a divider row |

Use `XBubble` when rendering a single message or a fully custom slot:

```razor
<XBubble Role="assistant"
         Header="Assistant"
         AvatarIcon="robot"
         Content="@markdown"
         Markdown="true"
         Streaming="@isStreaming" />
```

Key properties:

| Property | Notes |
| --- | --- |
| `Role` | Semantic role used for styling and defaults. |
| `Placement` | `Start` or `End`. User messages normally use `End`. |
| `Variant` | Filled, outlined, borderless, and other enum values from `XBubbleVariant`. |
| `Shape` | Default, round, or corner shape enum. |
| `Content` | Plain text or Markdown string. |
| `ContentTemplate` | Use for rich Razor UI. |
| `Actions` | `IReadOnlyList<XActionItem>` shown in the footer. |
| `Attachments` | `IReadOnlyList<XAttachmentItem>` rendered compactly below content. |
| `Loading` / `Streaming` / `Status` | Drive loading and stream state. |

## XSender

Use `XSender` for the composer. Bind draft text with `@bind-Value`; handle send with `OnSubmit`.

```razor
<XSender @bind-Value="@draft"
         Attachments="@attachments"
         Loading="@chat.IsLoading"
         ClearAfterSubmit="true"
         AutoSize="true"
         AutoSizeMinRows="2"
         AutoSizeMaxRows="8"
         OnSubmit="@chat.SubmitAsync"
         OnCancel="@chat.AbortAsync"
         OnFilesSelected="@AddFiles"
         OnAttachmentRemove="@RemoveAttachment" />
```

Key properties:

| Property | Notes |
| --- | --- |
| `Value` / `ValueChanged` | Text draft binding. |
| `OnSubmit` | Receives `XSenderRequest` with `Text` and `Attachments`. |
| `Loading` | Shows stop behavior and disables submit. |
| `SubmitMode` | Controls Enter-key behavior via `XSenderSubmitMode`. |
| `AttachmentsEnabled` | Enables the built-in file input surface. |
| `BeforeUpload` | `Func<IBrowserFile, ValueTask<bool>>` filter. |
| `Actions` / `OnAction` | Custom composer buttons. |
| `HeaderTemplate`, `PrefixTemplate`, `SuffixTemplate`, `FooterTemplate` | Rich slots. |
| `AllowSpeech` | Enables browser speech input when supported. |

## Navigation and Prompt Components

Use `XConversations` for session lists, `XPrompts` for suggested tasks, and `XSuggestion` for inline command hints.

```razor
<XConversations Items="@conversations"
                ActiveKey="@activeConversation"
                ActiveKeyChanged="@SwitchConversation"
                OnSelect="@SelectConversation" />

<XPrompts Items="@prompts"
          OnSelect="@UsePrompt" />
```

Create stable keys for conversation and prompt items so Blazor can preserve state across renders.

## Attachments and Files

Use `XAttachments` when the attachment list is independent from `XSender`; otherwise use the built-in attachment support on `XSender`.

```razor
<XAttachments Items="@attachments"
              UploadEnabled="true"
              Multiple="true"
              Pastable="true"
              DragHover="true"
              OnFilesSelected="@AddFiles"
              OnRemove="@RemoveAttachment" />
```

Represent files with `XAttachmentItem`; include `Name`, `ContentType`, `Size`, `Status`, and optional `Percent`.

## Thinking, Sources, and Feedback

Use `XThoughtChain` for multi-step agent/tool progress and `XThink` for one collapsible reasoning block.

```razor
<XThoughtChain Items="@thoughts" />
<XSources Items="@sources" />
<XActions Items="@actions" OnAction="@HandleAction" />
```

Use `XSources` for citations and retrieved documents. Use `XActions` for copy, retry, like/dislike, or custom operations under a message.

## Content Rendering

Use `XMarkdown` for Markdown strings, `XCodeHighlighter` for explicit code blocks, and `XMermaid` for diagrams.

```razor
<XMarkdown Content="@markdown" />
<XCodeHighlighter Language="csharp" Code="@code" />
<XMermaid Definition="@diagram" />
```

If content is part of a bubble, prefer `XBubble Markdown="true"` for simple Markdown and `ContentTemplate` for mixed Markdown/components.

## Notifications

Use `IXNotificationService` with `XNotification` host when the page needs queued status messages.

```razor
<XNotification />
```

Inject the service where actions occur:

```csharp
[Inject] private IXNotificationService Notifications { get; set; } = default!;
```
