---
name: x-blazor-skill
description: Build AI chat, agent workspace, and content-generation interfaces with AntDesign.X.Blazor. Use when working in Blazor projects that need Ant Design X concepts such as XProvider, XBubbleList, XSender, XConversations, XPrompts, XMarkdown, XMermaid, XChatStore, XAgentStore, IXRequestClient, streaming replies, attachments, sources, thought chains, or React Ant Design X to Blazor migration guidance.
---

# X Blazor Skill

## Overview

Use this skill to build Ant Design X style AI experiences in pure Blazor with `AntDesign.X.Blazor` and `AntDesign.Blazor`.

This library maps Ant Design X concepts to C# records, Razor components, `EventCallback`, `RenderFragment`, and scoped stores. Do not use React hooks, JSX, or `@ant-design/x` APIs in Blazor code.

## Setup

Install and register both AntDesign.Blazor and AntDesign.X.Blazor:

```powershell
dotnet add package AntDesignX.Blazor
```

```html
<link rel="stylesheet" href="_content/AntDesign/css/ant-design-blazor.css" />
<link rel="stylesheet" href="_content/AntDesign.X.Blazor/css/antdesign-x.css" />
<script src="_content/AntDesign/js/ant-design-blazor.js"></script>
```

```csharp
using AntDesign.X;

builder.Services.AddAntDesign();
builder.Services.AddAntDesignX();
```

In Razor files, add:

```razor
@using AntDesign.X
@using AntDesign.X.Components
```

For `XMermaid`, include Mermaid separately:

```html
<script src="https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js"></script>
```

## Decision Guide

| If you need to... | Start with |
| --- | --- |
| Render chat messages | `XBubbleList` with `IReadOnlyList<XBubbleItem>` |
| Render one-off assistant/user content | `XBubble` |
| Build the chat composer | `XSender` and `XSenderRequest` |
| Keep chat state and stream assistant replies | `XChatStore` |
| Call an agent endpoint directly | `XAgentStore` or `IXRequestClient` |
| Show sessions or history | `XConversations` and `XConversationItem` |
| Show suggested prompts | `XPrompts` and `XPromptItem` |
| Support file input | `XSender` attachments or `XAttachments` |
| Display citations | `XSources` |
| Display tool or reasoning progress | `XThoughtChain` or `XThink` |
| Render Markdown, code, or diagrams | `XMarkdown`, `XCodeHighlighter`, `XMermaid` |
| Apply X theme tokens | `XProvider` with optional `XThemeTokens` |

## Workflow

1. Read the host app's existing Blazor patterns before changing UI.
2. Register services and static assets once at the app boundary.
3. Wrap the AI surface with `XProvider`.
4. Use model records from `AntDesign.X` instead of anonymous dictionaries for component data.
5. Use `XBubbleList` for message collections; reserve `XBubble` for single messages or custom templates.
6. Bind `XSender` through `@bind-Value` and handle `OnSubmit(XSenderRequest request)`.
7. For streaming, update `XBubbleItem.Content`, `Streaming`, `Loading`, and `Status` through `XChatStore` or `XAgentStore` rather than ad hoc UI flags.
8. Check reference files only when needed:
   - [COMPONENTS.md](reference/COMPONENTS.md) for component choice and key props.
   - [PATTERNS.md](reference/PATTERNS.md) for full-page composition examples.
   - [API.md](reference/API.md) for model records, service APIs, and React-to-Blazor mappings.

## Development Rules

- Prefer `XProvider` around chat/workspace surfaces so theme tokens and locale services are available.
- Prefer `XBubbleList` over manually looping `XBubble` for normal chat logs; it handles role defaults, loading rows, auto-scroll, and optional virtualization.
- Use `Role` values such as `user`, `assistant`, `system`, and `tool`; role config defaults are case-insensitive.
- Use `ContentTemplate`, `HeaderTemplate`, `FooterTemplate`, and item templates for rich Razor content. Do not serialize complex Blazor UI into strings.
- Set `Markdown="true"` only when the content should be parsed as Markdown.
- Treat `Streaming=true` as a transient state and set it to `false` when the final chunk arrives.
- Use `XSender.Loading` plus `OnCancel` for stop behavior while a request is running.
- Keep user secrets and model API keys on the server. Do not embed provider keys in browser-delivered Blazor WebAssembly code.
- Keep custom CSS under the host app's scope; AntDesign.X.Blazor classes use the `antdx-` prefix.
- Prefer examples under `examples/AntDesign.X.Blazor.Demo/Components/Pages/Components` when exact syntax is uncertain.

## Minimal Example

```razor
@using AntDesign.X
@using AntDesign.X.Components
@using Microsoft.AspNetCore.Components.Forms

<XProvider Theme="light">
    <XBubbleList Items="@messages" Loading="@loading" OnAction="@HandleAction" />

    <XSender @bind-Value="@draft"
             Attachments="@attachments"
             Loading="@loading"
             OnSubmit="@Submit"
             OnCancel="@Cancel"
             OnFilesSelected="@AddFiles"
             OnAttachmentRemove="@RemoveAttachment" />
</XProvider>

@code {
    private string? draft;
    private bool loading;
    private readonly List<XBubbleItem> messages = [];
    private readonly List<XAttachmentItem> attachments = [];

    private Task Submit(XSenderRequest request)
    {
        messages.Add(new XBubbleItem
        {
            Role = "user",
            Content = request.Text,
            Attachments = request.Attachments
        });

        draft = string.Empty;
        return Task.CompletedTask;
    }

    private Task Cancel() => Task.CompletedTask;
    private Task HandleAction(string key) => Task.CompletedTask;
    private Task AddFiles(IReadOnlyList<IBrowserFile> files) => Task.CompletedTask;
    private Task RemoveAttachment(string id) => Task.CompletedTask;
}
```
