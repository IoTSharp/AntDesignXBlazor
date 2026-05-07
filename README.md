# AntDesign.X.Blazor

[Ant Design X](https://x.ant.design/) 的 Blazor 实现，与 [AntDesign.Blazor](https://antblazor.com/) 协同工作。

> Ant Design X 官方仅提供 React 实现。本项目用 **纯 Blazor + AntDesign.Blazor** 复现其"对话原语"组件（Bubble / BubbleList / Welcome / Prompts / Sender / Conversations / ThoughtChain / Attachments / Actions），并提供 Light / Dark 主题适配。

## 目标

1. **协同而非替代** AntDesign.Blazor —— 复用其 `Card / Tag / Button / Avatar / Icon / Menu / Timeline / TextArea` 等已有组件，只补齐 X 专属的对话语义层。
2. **纯 Blazor，无 React/Vue/Svelte 依赖**。
3. **无侵入命名空间**：所有 CSS 类一律以 `antdx-` 前缀。

## 项目结构

```
src/
  AntDesign.X.Blazor/
    Components/                # X 系列组件（XBubble、XSender、…）
    wwwroot/css/antdesign-x.css
    AntDesign.X.Blazor.csproj  # Razor Class Library (net10.0)
```

## 在 Blazor 项目中使用

```xml
<!-- YourApp.csproj -->
<ProjectReference Include="path/to/AntDesign.X.Blazor.csproj" />
```

```html
<!-- App.razor / index.html，放在 ant-design-blazor.css 之后 -->
<link rel="stylesheet" href="_content/AntDesign.X.Blazor/css/antdesign-x.css" />
```

```razor
@using AntDesign.X
@using AntDesign.X.Components

<XBubble Placement="XBubblePlacement.Start"
         AvatarIcon="robot"
         Header="助手"
         ContentTemplate="@(@<text>你好，我是 AI 助手。</text>)" />
```

## 路线图（Phase 1）

- [x] XBubble + 设计令牌
- [ ] XBubbleList
- [ ] XWelcome / XPromptCard
- [ ] XSender（含 InputFile 拖放、附件预览、长按语音）
- [ ] XConversations（基于 AntD `Menu`）
- [ ] XThoughtChain（基于 AntD `Timeline`）
- [ ] XAttachments / XActions

## License

MIT
