# AntDesign.X.Blazor Roadmap

本路线图用于把 Ant Design X 官方 React 设计系统完整映射到 Blazor。状态只描述本子模块真实进展，不把目标能力写成已完成。

## 设计分析结论

Ant Design X 的核心不是普通聊天 UI，而是一套 AI 产品原语：

- 官方组件总览按 **Common / Wake / Express / Confirmation / Feedback / Others** 分层：Common 是全局配置和会话容器，Wake 是欢迎与提示词唤醒，Express 是输入和建议，Confirmation 是思考过程，Feedback 是操作、文件、通知与来源，Others 是 Markdown、代码、图和目录等富内容。
- **会话容器**：Conversations 管理会话列表，Bubble/Bubble.List 管理消息流，Welcome/Prompts 管理空态与引导。
- **输入编排**：Sender 不是简单 textarea，而是输入、附件、工具、停止生成、快捷提交的组合器。
- **AI 过程可视化**：ThoughtChain、Think、Sources、FileCard、Folder 用于表达推理过程、引用来源、文件上下文与中间结果。
- **可组合反馈**：Actions、Suggestion、Notification 支撑复制、重试、喜欢、命令建议和局部反馈。
- **内容渲染与 SDK**：XMarkdown、CodeHighlighter、Mermaid 负责 AI 输出格式化；XRequest、XStream、useXChat、useXAgent 负责请求、流式、会话状态和 agent 状态。

Blazor 实现策略：

- 组件层使用 Razor + 强类型模型 + `EventCallback` + `RenderFragment`。
- 通用视觉复用 AntDesign.Blazor，X 专属语义用 `antdx-` CSS。
- SDK hooks 映射为可注入服务、状态容器和 streaming helpers，而不是照搬 React hooks。

## 里程碑 A：组件闭环 MVP

- [x] `XProvider`：主题、token、暗色模式门面
- [x] `XBubble`：placement、variant、avatar、header、footer、markdown、loading、actions、attachments
- [x] `XBubbleList`：items 渲染、loading bubble、自定义 item template
- [x] `XWelcome`：图标、标题、描述、extra、child content
- [x] `XPrompts`：提示卡片、分组子项、tag、select event
- [x] `XConversations`：分组、active key、count、updated time
- [x] `XSender`：输入、提交、停止、附件、工具动作、快捷键
- [x] `XAttachments`：上传入口、文件列表、移除事件
- [x] `XFileCard`：文件类型、图片预览、状态、进度
- [x] `XActions`：icon/text action、danger、disabled
- [x] `XSuggestion`：按 query 过滤、选择事件
- [x] `XThoughtChain`：状态时间线
- [x] `XThink`：可折叠推理块
- [x] `XSources`：来源引用卡片
- [x] `XFolder`：文件树 / 上下文集合
- [x] `XNotification`：固定位置通知
- [x] `XMarkdown`：Markdig Markdown 渲染
- [x] `XCodeHighlighter`：代码框、复制按钮
- [x] `XMermaid`：Mermaid 渲染与 fallback
- [x] 示例应用：完整 AI 工作台和逐组件展示

## 里程碑 B：官方 API 对齐

- [x] 对照官方 2.x 文档补齐每个组件的参数表和命名别名
- [x] 为 `XBubble` 增加官方 roles 风格的 role config 映射
- [x] 为 `XSender` 增加 header/footer 折叠、clear、autoSize 细节
- [x] 为 `XAttachments` 增加 beforeUpload、directory、drag hover、paste file
- [x] 为 `XPrompts` 增加 nested prompt 展开体验
- [x] 为 `XConversations` 增加菜单动作、重命名、删除、排序 slot
- [x] 为 `XNotification` 增加队列、duration、全局 service
- [x] 为 `XMarkdown` 增加代码块自动委托给 `XCodeHighlighter`
- [x] 为 `XMermaid` 增加主题同步和服务端 prerender安全策略

## 里程碑 C：Blazor X SDK

- [x] `IXRequestClient`：映射官方 XRequest，请求、取消、错误、headers
- [x] `XStreamReader`：映射 XStream，支持 SSE / fetch stream / OpenAI-compatible chunks
- [x] `XChatStore`：映射 useXChat，管理 messages、loading、submit、retry、abort
- [x] `XAgentStore`：映射 useXAgent，管理 agent 状态、tool call、intermediate events
- [x] 与 `IHttpClientFactory`、`CancellationToken`、Blazor Server circuit 生命周期对齐
- [x] 示例应用接入 mock streaming service，演示真实流式输出

## 里程碑 D：质量与发布

- [ ] bUnit 组件测试：渲染、事件、状态分支
- [ ] Playwright 示例站截图测试：桌面 / 移动 / dark mode
- [ ] XML docs 和 README 参数表
- [ ] NuGet metadata、icon、package validation
- [ ] 示例站增加每个组件的独立路由与代码 tab
- [ ] 版本策略：跟随 Ant Design X 主版本建立兼容说明

## 里程碑 E：高级体验

- [ ] 虚拟化 BubbleList，支持长会话
- [ ] 消息级 stream diff，减少 Blazor Server 高频渲染成本
- [ ] 插件化 renderers：LaTeX、diagram、chart、artifact preview
- [ ] 可访问性 audit：keyboard、aria、focus management
- [ ] 国际化资源：zh-CN / en-US
- [ ] 与 Camel.NET AI 工作台逐步替换式接入，但保持子模块独立发布
