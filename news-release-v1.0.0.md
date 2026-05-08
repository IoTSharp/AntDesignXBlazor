# 🎉 AntDesignX.Blazor v1.0.0 正式发布！在 Blazor 中拥抱 AI 交互体验

> 将 Ant Design X 的 AI 产品语义完整带入 .NET Blazor 生态。

---

## 🚀 项目简介

**AntDesignX.Blazor** 是 [Ant Design X](https://x.ant.design/) 在 Blazor 生态中的实现，面向 **AI 对话、智能工作台和内容生成类产品**。项目在纯 Blazor + AntDesign.Blazor 技术栈上，完整复现了 Ant Design X 的 AI 交互组件矩阵，为 .NET 开发者提供开箱即用的 AI 产品 UI 解决方案。

目前已发布 **v1.0.0** 正式版本，基于 **.NET 10** 构建，采用 **MIT 开源协议**。

---

## ✨ 核心特性

### 🤖 完整 AI 交互组件

| 分类 | 组件 |
|------|------|
| 💬 对话显示 | `XBubble`、`XBubbleList`、`XWelcome`、`XConversations` |
| ⌨️ 输入交互 | `XSender`、`XAttachments`、`XFileCard`、`XSuggestion`、`XPrompts` |
| 🧠 思考链 | `XThoughtChain`、`XThink`、`XSources`、`XFolder` |
| 🎨 内容渲染 | `XMarkdown`、`XCodeHighlighter`、`XMermaid` |
| ⚙️ 操作反馈 | `XActions`、`XNotification` |
| 🏗️ 全局门面 | `XProvider`（深浅主题 token） |

### 🔌 内置 SDK，直连 AI 服务

- `IXRequestClient` / `XStreamReader` — 适配任意 OpenAI 兼容端点
- `XChatStore` / `XAgentStore` — 对标 React `useXChat` / `useXAgent`，支持流式响应、重试和中断
- 开箱即用的 **DeepSeek SSE 接入示例**

### 🎯 设计原则

- **协同 AntDesign.Blazor**：补全 AI 语义组件，不重复造轮子
- **纯 Blazor 实现**：零 React/Vue 依赖，C# 事件 + `RenderFragment` 原生表达
- **样式隔离**：`antdx-` 统一前缀，不污染宿主应用
- **示例即文档**：Demo 站既是可演示应用，也是组件使用手册

---

## 📦 快速开始

```powershell
dotnet add package AntDesignX.Blazor
```

```csharp
builder.Services.AddAntDesign();
builder.Services.AddAntDesignX();
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

---

## 🔗 相关地址汇总

| 资源 | 地址 |
|------|------|
| 🐙 **GitHub 源码** | <https://github.com/IoTSharp/AntDesignXBlazor> |
| 🏯 **Gitee 镜像** | <https://gitee.com/IoTSharp/AntDesignXBlazor> |
| 📦 **NuGet 包** | <https://www.nuget.org/packages/AntDesignX.Blazor/> |
| 🎨 **Ant Design X 官方** | <https://x.ant.design/> |
| 📖 **AntDesign.Blazor** | <https://github.com/ant-design-blazor/ant-design-blazor> |

---

## 💬 加入交流

欢迎扫码加入企业微信群，与作者和其他开发者一起交流：

![企业微信群二维码](https://raw.githubusercontent.com/IoTSharp/AntDesignXBlazor/main/examples/AntDesign.X.Blazor.Demo/wwwroot/enterprise-wechat-qr.png)

---

## 🧪 在线演示

克隆项目后运行示例应用：

```powershell
git clone https://github.com/IoTSharp/AntDesignXBlazor.git
dotnet run --project AntDesignXBlazor/examples/AntDesign.X.Blazor.Demo/AntDesign.X.Blazor.Demo.csproj
```

Demo 站包含完整 AI 工作台：对话列表、欢迎页、提示集、消息流、附件选择、暗色主题切换和 mock streaming 闭环演示。

---

## 📋 技术栈

- **框架**：.NET 10 + Blazor（Server / WebAssembly 双模式）
- **基础组件**：AntDesign v1.6.0
- **Markdown 渲染**：Markdig
- **图表渲染**：Mermaid（可选 CDN 引入）
- **许可证**：MIT

---

**如果你正在用 Blazor 构建 AI 产品，AntDesignX.Blazor 就是你最值得关注的组件库！** 🌟 Star 支持一下，让更多 .NET 开发者看到这个项目！

---

*AntDesignX.Blazor — 让 Blazor 开发者的 AI 产品之路更加顺畅 🚀*

---

*本文由项目方授权发布，转载请注明出处。*
