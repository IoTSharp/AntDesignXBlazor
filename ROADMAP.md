# AntDesign.X.Blazor Roadmap

本路线图把 [Ant Design X](https://ant-design-x.antgroup.com/) 官方 React 设计系统完整映射到 Blazor。状态只描述本子模块真实进展，不把目标能力写成已完成。

## 进度总览（最新）

图例：✅ 已完成 ｜ 🚀 进行中 ｜ ⏳ 待开始 ｜ 🧪 验证中

| # | 里程碑 | 状态 | 摘要 |
| --- | --- | --- | --- |
| 1 | 组件矩阵闭环（19 个） | ✅ | 17 个 X 组件 + Markdown + Icon 全部上线 |
| 2 | DemoCard / ComponentLayout / ComponentNav 框架 | ✅ | 每组件独立路由 `/components/{slug}` |
| 3 | ROADMAP 与官方 2.x 对齐表 | ✅ | 见下方差异核对 |
| 4 | 默认 `dotnet build` 释放文件锁 | ✅ | 旧 demo 进程 PID 69332 已结束 |
| 5 | XSender 高级特性（语音 / focus / 折叠 / 停止） | ✅ | SpeechRecognition、FocusAsync/BlurAsync、recording 动效已落地 |
| 6 | XBubble 流式打字机 + loading 三态 | ✅ | BubbleDemo 覆盖 placement / variant / typing / loading |
| 7 | XAttachments 拖拽 / 粘贴 / 缩略图增强 | ✅ | AttachmentsDemo 覆盖拖拽 / 粘贴 / 缩略图 |
| 8 | XConversations 分组 / 菜单 / groupable | ✅ | ConversationsDemo 覆盖 group / activeKey / menu |
| 9 | XThoughtChain 折叠 / 状态图标 / 嵌套 | ✅ | XThoughtChain 重写 + Children + ThoughtChainDemo |
| 10 | useXAgent / useXChat 等价 .NET API 评估 | ✅ | XAgentStore / XChatStore 提供 abort / regenerate / streaming chunk 回调，README 含 React→C# 对照 |
| 11 | 单元 + 快照测试骨架（bUnit + Verify） | ✅ | bUnit + xUnit + Verify.Xunit，16 个用例（XLocaleService / XRendererRegistry / XCss + XBubble 快照） |
| 12 | NuGet 打包元数据 / CI 构建脚本 | ✅ | SourceLink + EnablePackageValidation + GitHub Actions 工作流 |
| 13 | 14 个组件 demo 页全部接通 | ✅ | Welcome / Prompts / Suggestion / Sources / Folder / Notification / Think / FileCard / Bubble / Sender / Attachments / Conversations / ThoughtChain / Actions |
| 14 | DeepSeek 实时对话 Demo | ✅ | `/components/live-chat` 真接 OpenAI 兼容 SSE，API key 从 `.env` 读取（已 `.gitignore`） |
| 15 | AG-UI 协议适配 | 🚀 | 准备接入 [AG-UI](https://github.com/ag-ui-protocol/ag-ui) 事件协议，先做可选 adapter，再映射到现有 X 组件与 Store |

## 与官方 2.x 的差异核对（最新一次审计）

参照 [组件总览](https://ant-design-x.antgroup.com/components/overview-cn) 与 [github.com/ant-design/x](https://github.com/ant-design/x) 的 demo 源码：

| 维度 | 现状 | 官方 2.x 基线 | 差距 |
| --- | --- | --- | --- |
| 组件矩阵 | 17 个 X 组件全部具备 + 辅助 `XMarkdown`、`XIcon` | 17 个组件分 6 类（通用 3 / 唤醒 2 / 确认 2 / 表达 3 / 反馈 6 / 其他 1） | 组件齐 |
| 演示组织 | 单页 [Home.razor](examples/AntDesign.X.Blazor.Demo/Components/Pages/Home.razor) 总览 | 每个组件独立路由，每页 4–10 个 demo card + API 表 + TS 接口 | 缺组件级路由与 demo 卡片矩阵 |
| 设计 token | `antdx-` 前缀 + light/dark 两套 | 与 antd 5 token 体系对齐，X 增加 `colorBgChat`、`colorBgBubbleUser` 等专属 token | 待对齐命名/色阶 |
| 动效 | 静态过渡 | 气泡进入、Sender 折叠、ThoughtChain 展开、Notification 队列均带 motion | 缺动效 token 与 motion class |
| API 文档 | 仅 README 映射表 | 每页 `<API>` 段落，含参数、类型、默认值 | 需要从源码生成 |
| i18n | 内置中文文案 | 全套 LocaleProvider，配 zh-CN/en-US/ja-JP/... | 暂未做 |
| 可访问性 | 基础 button/input | 键盘导航、aria-live 播报流式 token | 待审计 |
| 测试 | 无 | jest + cypress | 待补 bUnit + Playwright |

结论：**组件层完整，缺口主要在「演示页粒度」「设计语言对齐」「文档与质量」**。下面里程碑围绕这三件事展开。

## 设计原则（不变）

1. 协同 AntDesign.Blazor，X 只补 AI 对话语义。
2. 命名、交互、视觉层级贴近官方；Blazor API 用 C# 事件、`RenderFragment`、强类型模型。
3. 纯 Blazor，不引入 React/Vue。Mermaid 仅作浏览器渲染依赖。
4. CSS 统一 `antdx-` 前缀。
5. 示例即文档：`examples/AntDesign.X.Blazor.Demo` 是手册。

## 里程碑 A：组件闭环 MVP — 完成

- [x] `XProvider`、`XBubble`、`XBubbleList`、`XWelcome`、`XPrompts`、`XConversations`
- [x] `XSender`、`XAttachments`、`XFileCard`、`XActions`、`XSuggestion`
- [x] `XThoughtChain`、`XThink`、`XSources`、`XFolder`
- [x] `XNotification`、`XMarkdown`、`XCodeHighlighter`、`XMermaid`
- [x] 单页示例工作台

## 里程碑 B：官方 API 对齐 — 完成

- [x] role config、autoSize、清空、Header/Footer 折叠、上传、目录、粘贴、拖拽、菜单、重命名、排序、通知队列、Mermaid 主题等

## 里程碑 C：Blazor X SDK — 完成

- [x] `IXRequestClient`、`XStreamReader`、`XChatStore`、`XAgentStore`、mock streaming demo

## 里程碑 D：演示页对齐官方（进行中）

将单页 Home 拆分为「分类导航 + 每组件一个独立路由」，并为每个组件复刻官方 demo 矩阵。脚手架：
[examples/AntDesign.X.Blazor.Demo/Components/Pages/Components/](examples/AntDesign.X.Blazor.Demo/Components/Pages/Components/)。

约定：

- 每个组件页路由为 `/components/{name}`。
- 每个示例用统一的 `<DemoCard Title Description SourceCode>` 包装，自带可复制源码。
- 每页底部留一份「待补 demo」清单，逐项勾选。
- 页面可由社区分别认领。

### 通用 / Common

#### Bubble — `/components/bubble`

- [x] 基本（placement / variant / shape）
- [x] 头像与名字
- [x] Header 与 Footer
- [x] 加载中
- [x] 打字效果（typing）
- [x] Markdown 内容
- [x] 文件附件气泡
- [x] Bubble.List + roles
- [x] 自定义 message render（messageRender）
- [x] 语义化 classNames / styles

#### Conversations — `/components/conversations`

- [x] 基本
- [x] 受控 activeKey
- [x] 分组 group
- [x] 菜单（menu / items）
- [x] 重命名 / 删除
- [x] 自定义图标与计数
- [x] 排序 slot

#### Notification — `/components/notification`

- [x] 基本
- [x] 队列 / maxCount
- [x] duration 与 pauseOnHover
- [x] 四角 placement
- [x] 全局 service：`IXNotificationService.OpenAsync`
- [x] 状态：success / info / warning / error / processing

### 唤醒 / Wake

#### Welcome — `/components/welcome`

- [x] 基本
- [x] Variant（filled / borderless）
- [x] 自定义 icon / extra
- [x] 与 Prompts 组合

#### Prompts — `/components/prompts`

- [x] 基本
- [x] 嵌套 children
- [x] 垂直布局 vertical
- [x] 卡片样式
- [x] 禁用项
- [x] 默认展开 / 收起

### 确认 / Confirmation

#### Think — `/components/think`

- [x] 基本
- [x] 状态：default / processing / success / error
- [x] 自定义内容渲染

#### ThoughtChain — `/components/thought-chain`

- [x] 基本
- [x] 大小 small / middle / large
- [x] 嵌套 children
- [x] 折叠 collapsible
- [x] 自定义 icon / extra

### 表达 / Express

#### Attachments — `/components/attachments`

- [x] 基本
- [x] 上传中 / 完成 / 失败
- [x] 拖拽 hover
- [x] 粘贴文件
- [x] 文件夹（directory）
- [x] beforeUpload 校验
- [x] 自定义渲染

#### Sender — `/components/sender`

- [x] 基本
- [x] 提交方式（Enter / Shift+Enter / Ctrl+Enter）
- [x] 受控 value
- [x] Header & Footer 折叠
- [ ] 引用功能（reference）
- [x] 语音输入按钮（Web Speech API）
- [x] 工具动作（action 列表）
- [x] 停止生成（loading + cancel）
- [x] AutoSize / clearable
- [x] 编程式 `FocusAsync` / `BlurAsync`

#### Suggestion — `/components/suggestion`

- [x] 基本
- [x] 自定义触发字符
- [x] 多级 children
- [x] 与 Sender 联动

### 反馈 / Feedback

#### Actions — `/components/actions`

- [x] 基本
- [ ] 子菜单
- [ ] 危险动作
- [ ] icon-only / size

#### CodeHighlighter — `/components/code-highlighter`

- [x] 基本
- [x] 流式 streaming
- [ ] 多语言语法
- [x] 复制 / 行号

#### FileCard — `/components/file-card`

- [x] 基本
- [x] 上传中 percent
- [x] 错误状态
- [x] 图片预览
- [x] 自定义 icon

#### Folder — `/components/folder`

- [x] 基本
- [x] 卡片 variant
- [x] 受控 activeKey
- [x] 自定义渲染
- [x] 状态徽标

#### Mermaid — `/components/mermaid`

- [x] 基本
- [ ] 流式 / 增量
- [ ] 主题跟随
- [x] 错误 fallback

#### Sources — `/components/sources`

- [x] 基本
- [x] 卡片样式
- [x] 多列布局
- [x] 链接打开方式

### 其他 / Others

#### XProvider — `/components/x-provider`

- [x] 基本
- [x] 主题 token
- [x] 暗色模式
- [x] 嵌套覆盖

### 实战 / Live

#### LiveChat — `/components/live-chat`

- [x] 真接 DeepSeek （OpenAI 兼容 SSE）
- [x] API key 从 `.env` / 环境变量读取，`.gitignore` 已排除 `*.env`
- [x] 流式渲染 + 取消 + 错误状态
- [x] 未配置时页面内提示 `.env` 填写说明

### Playground 实战范例 / Playground

复刻官方 [`/docs/playground/*`](https://ant-design-x.antgroup.com/docs/playground/ultramodern) 三套整页范例，独立路由 + 沿用 mock streaming：

#### Ultramodern — `/components/playground-ultramodern`

- [x] 左侧 Conversations 分组（Today / Yesterday）+ New chat
- [x] 右侧聊天主区：空态启动页、流式 Bubble.List、可取消
- [x] Sender 底部 Deep Think 切换 chip

#### Independent — `/components/playground-independent`

- [x] 居中 Welcome（👋 Hello, I'm Ant Design X） + 顶部操作按钮
- [x] Hot Topics 列表（rank-1/2/3 高亮）
- [x] Design Guide 四宫格（Intention / Role / Chat / Interface）
- [x] 底部快捷 prompts + Sender

#### Copilot — `/components/playground-copilot`

- [x] 双栏：左阅读区 (XMarkdown) + 右可折叠 AI Copilot 面板
- [x] 折叠时正文撑满宽度，提供「✨ AI Copilot」唤回按钮
- [x] 面板内空态：Welcome (Compact) + 垂直 Prompts；非空态：Bubble.List
- [x] 面板底部快捷 prompts + Sender，新建会话清空消息

## 里程碑 E：设计语言（design token）校对

- [x] 比对 [`@ant-design/x` x-provider tokens](https://github.com/ant-design/x/tree/main/components/x-provider) 与本仓库 `wwwroot/css/antdesign-x.css` 中变量
- [x] 引入官方 X 专属 token：`colorBgChat`、`colorBgBubbleUser`、`colorBgBubbleAi`、`colorBorderBubble`、`colorTextThink`、`paddingChat` 等
- [x] `XThemeTokens` + `XCss.ToCssVariables` 同步扩展，形成 C# API 入口
- [x] 实现 motion token（duration / easing）并应用于 Bubble 进入、Sender 折叠、Notification 入场
- [x] 暗色模式 token 覆盖与 `prefers-reduced-motion` 兜底
- [x] 字体梯度（fontSize、lineHeight）token 对齐与截图回归

## 里程碑 F：质量与发布

- [x] bUnit + xUnit 测试项目骨架（`tests/AntDesign.X.Blazor.Tests`）
- [x] NuGet metadata、SourceLink、`EnablePackageValidation`、symbol 包
- [x] GitHub Actions CI（build / test / pack 矩阵 ubuntu+windows）
- [x] 版本策略：跟随 Ant Design X 主版本（README 标注）
- [ ] Playwright 截图测试：每个 demo 页 desktop / mobile / dark
- [ ] XML doc + 自动化生成 API 表（doc-gen 工具）

## 里程碑 G：高级体验

- [x] BubbleList 虚拟化（`Virtual` 参数 + `Microsoft.AspNetCore.Components.Web.Virtualization`）
- [x] 流式 diff（`XStreamingText` 前缀 diff 渲染，配合 motion token 实现淡入光标）
- [x] 插件化 renderer：`IXRendererRegistry`（按语言注册 RenderFragment）
- [x] i18n：`IXLocaleService`，内置 zh-CN / en-US / ja-JP，`XProvider.Locale` 联动
- [ ] 可访问性 audit（aria-live / 键盘导航完整覆盖）
- [ ] 与 Camel.NET AI 工作台逐步替换式接入

## 里程碑 H：AG-UI 协议适配（准备开始）

[AG-UI](https://github.com/ag-ui-protocol/ag-ui) 是面向 Agent 后端与前端应用的事件协议，重点覆盖 agent run 生命周期、流式文本、工具调用、共享状态、activity、reasoning 与 custom events。AntDesign.X.Blazor 的适配目标不是引入另一套 UI，而是在现有 `XBubbleList`、`XSender`、`XThoughtChain`、`XSources`、`XActions`、`XChatStore`、`XAgentStore` 之上增加一层可选协议 adapter。

设计边界：

- 保持组件层稳定：不让 AG-UI 事件类型渗透到 `XBubble` / `XSender` 等基础组件参数。
- 保持纯 Blazor / C#：第一阶段不依赖 React、CopilotKit 前端包或 TypeScript runtime。
- 优先使用现有 `IXRequestClient`、`XStreamReader`、`XAgentStore` 能力，避免重复实现通用 SSE/HTTP streaming。
- AG-UI .NET SDK 稳定前，先在本库内定义最小 C# 模型与 mapper；后续可桥接官方 SDK。
- adapter 允许与 OpenAI 兼容 SSE、DeepSeek demo 并存，用户按 endpoint/protocol 选择。

### H0：协议审计与范围锁定

- [ ] 对照 AG-UI docs 的 Events / Messages / Tools / State / Reasoning，整理本库第一版必须支持的事件清单。
- [ ] 明确首版传输只支持 HTTP + SSE；WebSocket、webhook、binary/protobuf 放到后续。
- [ ] 确认 AG-UI input payload 与当前 `XAgentRequestPayload` / `XChatRequestPayload` 的差异。
- [ ] 建立事件命名策略：保持 AG-UI 原始 `type` 字符串，同时提供 C# enum/常量便于消费。
- [ ] 写一份 `docs/ag-ui-adapter.md` 草案，记录事件映射、限制与示例 payload。

### H1：Core 模型与事件解析

- [ ] 新增 `Models/AgUi/` 或 `Services/AgUi/` 命名空间，放置协议最小模型。
- [ ] 定义 `XAgUiEvent` 基类/record：`Type`、`Timestamp`、`RawEvent`、`Metadata`。
- [ ] 覆盖 lifecycle：`RUN_STARTED`、`RUN_FINISHED`、`RUN_ERROR`、`STEP_STARTED`、`STEP_FINISHED`。
- [ ] 覆盖 message：`TEXT_MESSAGE_START`、`TEXT_MESSAGE_CONTENT`、`TEXT_MESSAGE_END`、`TEXT_MESSAGE_CHUNK`。
- [ ] 覆盖 tool call：`TOOL_CALL_START`、`TOOL_CALL_ARGS`、`TOOL_CALL_END`、`TOOL_CALL_RESULT`。
- [ ] 覆盖 state：`STATE_SNAPSHOT`、`STATE_DELTA`。
- [ ] 覆盖 activity / reasoning / custom events 的通用 fallback，未知事件必须保留 raw JSON。
- [ ] 为 `XStreamReader` 增加 AG-UI 解析辅助，或新增 `XAgUiStreamReader` 包装现有 `XStreamChunk`。

### H2：事件到 X 语义模型映射

- [ ] 新增 `XAgUiEventMapper`，把 AG-UI 事件映射为本库已有 UI 模型。
- [ ] `TEXT_MESSAGE_*` → `XBubbleItem` assistant 内容增量、loading、streaming、success/error 状态。
- [ ] `REASONING_*` → `XThoughtItem` / `XThink`，只展示 summary/content，不暴露 encrypted reasoning 原文。
- [ ] `TOOL_CALL_*` → `XAgentToolCallItem` + `XActionItem`，支持参数流式拼接、结果回填和失败状态。
- [ ] `STATE_SNAPSHOT` / `STATE_DELTA` → `XAgentEventItem.Metadata` 或可选 shared state 字典。
- [ ] `RUN_*` / `STEP_*` → `XAgentEventItem`，驱动整体运行状态、进度和错误提示。
- [ ] `CUSTOM` / 未知事件 → `XAgentEventItem`，允许 demo 页以原始事件列表展示。

### H3：Client 与 Store 集成

- [ ] 新增 `IXAgUiClient` / `XAgUiClient`，封装 AG-UI HTTP endpoint、headers、session/thread id、abort。
- [ ] 新增 `XAgUiClientOptions`，支持 `BaseAddress`、`RunPath`、`DefaultHeaders`、`UseCredentials`、`ProtocolVersion`。
- [ ] 提供 DI 扩展：`services.AddAntDesignXAgUi(...)` 或 `services.AddAntDesignX(options => options.UseAgUi(...))`。
- [ ] 在 `XAgentStore` 增加可选运行入口，或新增 `XAgUiAgentStore`，避免破坏现有 OpenAI 兼容路径。
- [ ] 支持把当前 `XBubbleItem` 历史消息转换为 AG-UI messages input。
- [ ] 支持 abort/retry/regenerate，与现有 `XSender.Loading` / stop 按钮一致。
- [ ] 支持多会话：`ConversationKey` / `AgentKey` 映射到 AG-UI thread/session/run metadata。

### H4：Demo 与文档

- [ ] 新增 demo 路由 `/components/ag-ui-agent`。
- [ ] 页面布局：左侧 Conversations，中间 `XBubbleList` + `XSender`，右侧 trace 面板展示 events / tool calls / state。
- [ ] 提供 mock AG-UI SSE 服务，避免 demo 依赖外部 token。
- [ ] 支持用户配置真实 AG-UI endpoint，通过 `.env` / 环境变量读取。
- [ ] README 增加 “AG-UI adapter” 小节，说明它是可选协议层。
- [ ] Roadmap 与组件导航中加入 AG-UI demo 入口。

### H5：测试与兼容性验收

- [ ] 单元测试：事件 JSON 反序列化、未知事件 fallback、timestamp/rawEvent 保留。
- [ ] 单元测试：message/tool/reasoning/state 映射到 X 模型的增量行为。
- [ ] bUnit 测试：`XAgUiAgentStore` 推动 `XBubbleList` 流式更新。
- [ ] 快照测试：AG-UI demo 的空态、运行中、完成、错误状态。
- [ ] CI 覆盖 `dotnet test`，保证 adapter 不影响现有 OpenAI 兼容 demo。
- [ ] 发布前验收：mock SSE、真实 AG-UI endpoint、abort/retry、工具调用、reasoning、state delta 均有最小闭环。

首版交付标准：

- [ ] 可以连接一个 AG-UI HTTP SSE endpoint，并驱动 Blazor UI 流式显示 assistant 消息。
- [ ] 可以展示 run lifecycle、tool calls、reasoning summary、state snapshot/delta。
- [ ] 可以中断运行，并保持 Store 状态一致。
- [ ] 没有 AG-UI endpoint 时，demo 使用本地 mock 仍可完整演示。
- [ ] 不破坏当前 `XChatStore`、`XAgentStore`、DeepSeek LiveChat 与 NuGet API 兼容性。

## 贡献指引

1. 选一个未勾选的 demo 项，在对应 `Pages/Components/{Name}Demo.razor` 中添加 `<DemoCard>`。
2. demo 源码用 `SourceCode` 参数同步贴出，便于复制。
3. 同步在本路线图把对应项打勾。
4. 涉及 token / 动效改动，需顺手补 `wwwroot/css/antdesign-x.css` 的 dark 模式覆盖。
