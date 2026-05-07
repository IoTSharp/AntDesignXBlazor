# AntDesign.X.Blazor

[Ant Design X](https://x.ant.design/) 的 Blazor 门面实现。项目目标是在 **纯 Blazor + AntDesign.Blazor** 中复现 Ant Design X 的 AI 对话产品语义，并提供一个可运行的示例应用，让使用者能逐个查看组件、交互和代码写法。

官方 Ant Design X 是面向 React 生态的 AI 组件库，除 UI 组件外也提供接入 AI 服务的 API 方案。其组件总览按 Common、Wake、Express、Confirmation、Feedback、Others 分组，覆盖 Bubble、Conversations、Notification、Welcome、Prompts、Attachments、Sender、Suggestion、Think、ThoughtChain、Actions、CodeHighlighter、FileCard、Folder、Mermaid、Sources、XProvider。本项目据此建立 Blazor 映射。

> 设计参考：Ant Design X 官方站点当前公开的 2.x 组件矩阵，以及 X SDK 中的 XRequest / XStream / useXChat / useXAgent 方向。

## 项目原则

1. **协同 AntDesign.Blazor**：基础视觉和通用控件继续使用 AntDesign.Blazor，AntDesign.X.Blazor 只补齐 AI 对话、输入、来源、思考链、文件卡片等 X 语义组件。
2. **保持原汁原味**：组件命名、交互结构和视觉层级尽量贴近 Ant Design X；Blazor API 使用 C# 事件、`RenderFragment`、强类型模型表达。
3. **纯 Blazor**：不引入 React / Vue / Svelte 工程。Mermaid 仅作为浏览器端图渲染库可选接入。
4. **样式隔离**：CSS 类统一使用 `antdx-` 前缀，降低对宿主应用的污染。
5. **示例即文档**：`examples/AntDesign.X.Blazor.Demo` 是可演示应用，也是组件使用手册。

## 当前已实现

- 显示：`XBubble`、`XBubbleList`、`XWelcome`、`XThoughtChain`、`XThink`、`XSources`、`XFolder`
- 输入：`XSender`、`XAttachments`、`XFileCard`、`XSuggestion`、`XPrompts`
- 操作与反馈：`XActions`、`XNotification`
- 内容渲染：`XMarkdown`、`XCodeHighlighter`、`XMermaid`
- 全局门面：`XProvider`、浅色/深色主题 token、`antdx-` 样式令牌
- 示例应用：完整 AI 工作台、组件分区展示、代码片段展示、附件选择、通知与暗色主题切换

## 目录结构

```text
external/AntDesignXBlazor/
  src/AntDesign.X.Blazor/
    Components/
    Models/
    Utilities/
    wwwroot/css/antdesign-x.css
    wwwroot/js/antdesign-x-module.js
  examples/AntDesign.X.Blazor.Demo/
  ROADMAP.md
  README.md
```

## 在 Blazor 中使用

```xml
<ProjectReference Include="external/AntDesignXBlazor/src/AntDesign.X.Blazor/AntDesign.X.Blazor.csproj" />
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

如需 `XMermaid`：

```html
<script src="https://cdn.jsdelivr.net/npm/mermaid@11/dist/mermaid.min.js"></script>
```

```razor
@using AntDesign.X
@using AntDesign.X.Components

<XProvider Theme="light">
    <XBubble Placement="XBubblePlacement.Start"
             AvatarIcon="robot"
             Header="Assistant"
             Content="Hello from **Ant Design X Blazor**"
             Markdown="true" />
</XProvider>
```

## 完整输入闭环

```razor
<XBubbleList Items="@messages" OnAction="@HandleAction" />

<XSender @bind-Value="@draft"
         Attachments="@attachments"
         Actions="@senderActions"
         OnSubmit="@Submit"
         OnFilesSelected="@AddFiles"
         OnAttachmentRemove="@RemoveAttachment" />
```

```csharp
private string? draft;
private readonly List<XAttachmentItem> attachments = [];

private Task Submit(XSenderRequest request)
{
    messages.Add(new XBubbleItem
    {
        Role = "You",
        Placement = XBubblePlacement.End,
        AvatarIcon = "user",
        Content = request.Text,
        Attachments = request.Attachments
    });

    draft = string.Empty;
    return Task.CompletedTask;
}
```

## 示例应用

示例应用位于：

```text
examples/AntDesign.X.Blazor.Demo/
```

运行方式：

```powershell
dotnet run --project external/AntDesignXBlazor/examples/AntDesign.X.Blazor.Demo/AntDesign.X.Blazor.Demo.csproj
```

> 在 Camel.NET 仓库协作规则下，AI 默认不主动执行本机 build/run；用户可在需要时自行运行上述示例命令。子模块本身是独立 Blazor 组件库，不改变 Camel.NET 后端 Docker Compose 验收基线。

## 与官方 Ant Design X 的映射

| 官方能力 | Blazor 门面 | 当前状态 |
| --- | --- | --- |
| Bubble / Bubble.List | `XBubble` / `XBubbleList` | 已实现基础渲染、role config、divider、auto scroll |
| Sender | `XSender` | 已实现输入、附件、停止、动作、autoSize、clear、折叠 |
| Conversations | `XConversations` | 已实现分组、激活、计数、菜单、重命名、删除、排序 |
| Prompts | `XPrompts` | 已实现卡片、嵌套展开与选择 |
| Attachments / FileCard | `XAttachments` / `XFileCard` | 已实现选择、列表、移除、beforeUpload、目录、拖拽、粘贴 |
| Welcome | `XWelcome` | 已实现 |
| Actions | `XActions` | 已实现 |
| Suggestion | `XSuggestion` | 已实现过滤与选择 |
| ThoughtChain / Think | `XThoughtChain` / `XThink` | 已实现 |
| Sources | `XSources` | 已实现 |
| Folder | `XFolder` | 已实现 |
| Notification | `XNotification` | 已实现列表渲染、队列宿主与全局服务 |
| XMarkdown | `XMarkdown` | 已实现 Markdig 渲染与 code fence 高亮委托 |
| CodeHighlighter | `XCodeHighlighter` | 已实现代码框与复制 |
| Mermaid | `XMermaid` | 已实现 Mermaid 渲染、主题跟随、代码/图切换与 fallback |
| XProvider | `XProvider` | 已实现主题 token 门面 |
| XRequest / XStream / useXChat / useXAgent | 待设计为 Blazor services/hooks-like patterns | 路线图中 |

## 许可证

MIT
