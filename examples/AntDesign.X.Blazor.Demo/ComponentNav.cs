namespace AntDesign.X.Blazor.Demo;

/// <summary>
/// 与官方 ant-design-x 总览一致的组件分组：通用 / 唤醒 / 确认 / 表达 / 反馈 / 其他。
/// </summary>
public static class ComponentNav
{
    public sealed record NavItem(string Slug, string Title, string EnglishTitle);

    public sealed record NavGroup(string Title, IReadOnlyList<NavItem> Items);

    public static readonly IReadOnlyList<NavGroup> Groups =
    [
        new("通用", new NavItem[]
        {
            new("bubble", "对话气泡", "Bubble"),
            new("conversations", "管理对话", "Conversations"),
            new("notification", "系统通知", "Notification"),
        }),
        new("唤醒", new NavItem[]
        {
            new("welcome", "欢迎", "Welcome"),
            new("prompts", "提示集", "Prompts"),
        }),
        new("确认", new NavItem[]
        {
            new("think", "思考过程", "Think"),
            new("thought-chain", "思维链", "ThoughtChain"),
        }),
        new("表达", new NavItem[]
        {
            new("attachments", "输入附件", "Attachments"),
            new("sender", "输入框", "Sender"),
            new("suggestion", "快捷指令", "Suggestion"),
        }),
        new("反馈", new NavItem[]
        {
            new("actions", "操作列表", "Actions"),
            new("code-highlighter", "代码高亮", "CodeHighlighter"),
            new("file-card", "文件卡片", "FileCard"),
            new("folder", "文件夹", "Folder"),
            new("mermaid", "图表工具", "Mermaid"),
            new("sources", "来源引用", "Sources"),
        }),
        new("其他", new NavItem[]
        {
            new("markdown", "Markdown 渲染", "Markdown"),
            new("icon", "图标", "Icon"),
            new("x-provider", "全局化配置", "XProvider"),
        }),
#if !DEMO_WASM
        new("实战", new NavItem[]
        {
            new("live-chat", "DeepSeek 实时对话", "LiveChat"),
        }),
#endif
    ];
}
