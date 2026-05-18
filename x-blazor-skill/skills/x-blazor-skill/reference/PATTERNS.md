# AntDesign.X.Blazor Patterns

Use this reference for full-page composition and streaming flows.

## App Registration Pattern

Register AntDesign.Blazor first, then AntDesign.X.Blazor:

```csharp
using AntDesign.X;

builder.Services.AddAntDesign();
builder.Services.AddAntDesignX(options =>
{
    options.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    options.ChatPath = "/api/chat";
});
```

Include static assets once in the host document:

```html
<link rel="stylesheet" href="_content/AntDesign/css/ant-design-blazor.css" />
<link rel="stylesheet" href="_content/AntDesign.X.Blazor/css/antdesign-x.css" />
<script src="_content/AntDesign/js/ant-design-blazor.js"></script>
```

## Manual Local Chat Pattern

Use this pattern when no server call is needed yet.

```razor
@using AntDesign.X
@using AntDesign.X.Components
@using Microsoft.AspNetCore.Components.Forms

<XProvider Theme="@theme">
    <XWelcome Title="AntDesign.X.Blazor"
              Description="Ask a question to start." />

    <XPrompts Items="@prompts" OnSelect="@UsePrompt" />

    <XBubbleList Items="@messages" />

    <XSender @bind-Value="@draft"
             Attachments="@attachments"
             ClearAfterSubmit="true"
             OnSubmit="@Submit"
             OnFilesSelected="@AddFiles"
             OnAttachmentRemove="@RemoveAttachment" />
</XProvider>

@code {
    private string theme = "light";
    private string? draft;
    private readonly List<XBubbleItem> messages = [];
    private readonly List<XAttachmentItem> attachments = [];

    private readonly IReadOnlyList<XPromptItem> prompts =
    [
        new() { Key = "summary", Title = "Summarize", Description = "Summarize the current document" },
        new() { Key = "ideas", Title = "Ideas", Description = "Suggest follow-up questions" }
    ];

    private Task UsePrompt(XPromptItem item)
    {
        draft = item.Description ?? item.Title;
        return Task.CompletedTask;
    }

    private Task Submit(XSenderRequest request)
    {
        messages.Add(new XBubbleItem
        {
            Role = "user",
            Content = request.Text,
            Attachments = request.Attachments
        });

        messages.Add(new XBubbleItem
        {
            Role = "assistant",
            Header = "Assistant",
            AvatarIcon = "robot",
            Content = "Received: " + request.Text,
            Markdown = true
        });

        draft = string.Empty;
        return Task.CompletedTask;
    }

    private Task AddFiles(IReadOnlyList<IBrowserFile> files)
    {
        attachments.AddRange(files.Select(file => new XAttachmentItem
        {
            Name = file.Name,
            ContentType = file.ContentType,
            Size = file.Size
        }));

        return Task.CompletedTask;
    }

    private Task RemoveAttachment(string id)
    {
        attachments.RemoveAll(item => item.Id == id);
        return Task.CompletedTask;
    }
}
```

## Store-Based Streaming Pattern

Use `XChatStore` for normal chat screens. It appends the user message, creates an assistant placeholder, streams chunks into the assistant message, and exposes `Changed`.

```razor
@implements IDisposable
@inject XChatStore Chat

<XProvider>
    <XBubbleList Items="@Chat.Messages"
                 Loading="@Chat.IsLoading"
                 OnAction="@HandleAction" />

    <XSender @bind-Value="@draft"
             Loading="@Chat.IsLoading"
             OnSubmit="@Submit"
             OnCancel="@Chat.AbortAsync" />
</XProvider>

@code {
    private string? draft;

    protected override void OnInitialized()
    {
        Chat.Changed += StateHasChanged;
    }

    public void Dispose()
    {
        Chat.Changed -= StateHasChanged;
    }

    private async Task Submit(XSenderRequest request)
    {
        await Chat.SubmitAsync(request);
        draft = string.Empty;
    }

    private Task HandleAction(string actionKey)
    {
        return actionKey switch
        {
            "retry" => Chat.RetryAsync(),
            "abort" => Chat.AbortAsync(),
            _ => Task.CompletedTask
        };
    }
}
```

## Agent Store Pattern

Use `XAgentStore` when the screen is agent-first rather than chat-first. Bind `IsRunning` to sender loading state and subscribe to `Changed` just like `XChatStore`.

```razor
@implements IDisposable
@inject XAgentStore Agent

<XThoughtChain Items="@Agent.Events.Select(ToThought).ToArray()" />
<XBubbleList Items="@Agent.Messages" Loading="@Agent.IsRunning" />
<XSender @bind-Value="@prompt"
         Loading="@Agent.IsRunning"
         OnSubmit="@RunAgent"
         OnCancel="@Agent.AbortAsync" />

@code {
    private string? prompt;

    protected override void OnInitialized()
    {
        Agent.Changed += StateHasChanged;
    }

    public void Dispose()
    {
        Agent.Changed -= StateHasChanged;
    }

    private Task RunAgent(XSenderRequest request)
    {
        return Agent.RunAsync(new XAgentRequest
        {
            Prompt = request.Text,
            Attachments = request.Attachments
        });
    }

    private static XThoughtItem ToThought(XAgentEventItem item) => new()
    {
        Key = item.Key,
        Title = item.Title,
        Description = item.Description,
        Content = item.Content,
        Status = item.Status
    };
}
```

## Server Boundary Pattern

Keep provider API keys and model credentials on the server. Point the client to an app-owned endpoint through `XRequestClientOptions`.

```csharp
builder.Services.AddAntDesignX(options =>
{
    options.ChatPath = "/api/ai/chat";
    options.AgentPath = "/api/ai/agent";
    options.Timeout = TimeSpan.FromMinutes(5);
});
```

Use server-side middleware or API routes to translate `XChatRequestPayload` or `XAgentRequestPayload` into your provider-specific request.

## React Ant Design X Migration Pattern

Map React concepts to Blazor concepts directly:

| React Ant Design X | AntDesign.X.Blazor |
| --- | --- |
| `XProvider` | `XProvider` Razor component |
| `Bubble.List` | `XBubbleList` |
| `Bubble` | `XBubble` |
| `Sender` | `XSender` |
| `Conversations` | `XConversations` |
| `Prompts` | `XPrompts` |
| `Attachments` | `XAttachments` |
| `ThoughtChain` | `XThoughtChain` |
| `Think` | `XThink` |
| `Sources` | `XSources` |
| `useXChat` | `XChatStore` |
| `useXAgent` | `XAgentStore` |
| `XRequest` | `IXRequestClient` |
| JSX slots | `RenderFragment` parameters |
| event props | `EventCallback` parameters |

Do not copy JSX examples verbatim. Rebuild them as Razor markup with C# model records.
