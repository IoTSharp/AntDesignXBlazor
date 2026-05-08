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

## 贡献指引

1. 选一个未勾选的 demo 项，在对应 `Pages/Components/{Name}Demo.razor` 中添加 `<DemoCard>`。
2. demo 源码用 `SourceCode` 参数同步贴出，便于复制。
3. 同步在本路线图把对应项打勾。
4. 涉及 token / 动效改动，需顺手补 `wwwroot/css/antdesign-x.css` 的 dark 模式覆盖。
