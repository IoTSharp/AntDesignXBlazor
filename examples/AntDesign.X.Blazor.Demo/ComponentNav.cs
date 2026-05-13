namespace AntDesign.X.Blazor.Demo;

/// <summary>
/// Mirrors the ant-design-x-vue docs sidebar. Extra Blazor-only demos stay out of this primary map.
/// </summary>
public static class ComponentNav
{
    public sealed record NavItem(string Slug, string Title, string EnglishTitle);

    public sealed record NavGroup(string Title, IReadOnlyList<NavItem> Items);

    public static readonly IReadOnlyList<NavGroup> Groups =
    [
        new("通用",
        [
            new("bubble", "Bubble 对话气泡框", "Bubble"),
            new("conversations", "Conversations 管理对话", "Conversations"),
        ]),
        new("唤醒",
        [
            new("welcome", "Welcome 欢迎", "Welcome"),
            new("prompts", "Prompts 提示集", "Prompts"),
        ]),
        new("表达",
        [
            new("sender", "Sender 输入框", "Sender"),
            new("attachments", "Attachments 输入附件", "Attachments"),
            new("suggestion", "Suggestion 快捷指令", "Suggestion"),
        ]),
        new("确认",
        [
            new("thought-chain", "ThoughtChain 思维链", "ThoughtChain"),
        ]),
        new("反馈",
        [
            new("actions", "Actions 操作列表", "Actions"),
        ]),
        new("工具",
        [
            new("use-x-agent", "useXAgent 模型调度", "useXAgent"),
            new("use-x-chat", "useXChat 数据管理", "useXChat"),
            new("x-stream", "XStream 流", "XStream"),
            new("x-request", "XRequest 请求", "XRequest"),
            new("x-provider", "XProvider 全局化配置", "XProvider"),
        ]),
    ];
}
