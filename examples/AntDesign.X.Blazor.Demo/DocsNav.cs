namespace AntDesign.X.Blazor.Demo;

public static class DocsNav
{
    public sealed record TopNavItem(string Text, string Href, bool Exact = false, bool External = false);

    public sealed record SidebarItem(string Text, string Href);

    public sealed record SidebarGroup(string Text, IReadOnlyList<SidebarItem> Items, string? Href = null);

    public sealed record OutlineItem(string Text, string Href, int Level = 2);

    public static readonly IReadOnlyList<TopNavItem> TopLinks =
    [
        new("研发", "development/introduce"),
        new("组件", "component/overview"),
        new("演示", "playground/independent"),
        new("关于", "about", Exact: true),
    ];

    public static readonly IReadOnlyList<TopNavItem> MoreLinks =
    [
        new("Ant Design X of React", "https://x.ant.design/index-cn", External: true),
        new("Ant Design Blazor", "https://antblazor.com/", External: true),
    ];

    public static readonly IReadOnlyList<SidebarGroup> DevelopmentSidebar =
    [
        new("Ant Design X of Blazor", [], "development/introduce"),
        new("样式兼容", [], "development/compatible-style"),
    ];

    public static readonly IReadOnlyList<SidebarGroup> ComponentSidebar =
    [
        new("总览", [], "component/overview"),
        new("通用",
        [
            new("Bubble 对话气泡框", "component/bubble"),
            new("Conversations 管理对话", "component/conversations"),
        ]),
        new("唤醒",
        [
            new("Welcome 欢迎", "component/welcome"),
            new("Prompts 提示集", "component/prompts"),
        ]),
        new("表达",
        [
            new("Sender 输入框", "component/sender"),
            new("Attachments 输入附件", "component/attachments"),
            new("Suggestion 快捷指令", "component/suggestion"),
        ]),
        new("确认",
        [
            new("ThoughtChain 思维链", "component/thought-chain"),
        ]),
        new("反馈",
        [
            new("Actions 操作列表", "component/actions"),
        ]),
        new("工具",
        [
            new("useXAgent 模型调度", "component/use-x-agent"),
            new("useXChat 数据管理", "component/use-x-chat"),
            new("XStream 流", "component/x-stream"),
            new("XRequest 请求", "component/x-request"),
            new("XProvider 全局化配置", "component/x-provider"),
        ]),
    ];

    public static readonly IReadOnlyList<SidebarGroup> PlaygroundSidebar =
    [
        new("样板间",
        [
            new("独立式", "playground/independent"),
            new("助手式", "playground/copilot"),
            new("超现代", "playground/ultramodern"),
        ]),
    ];

    public static IReadOnlyList<SidebarGroup> GetSidebar(string relativePath)
    {
        if (relativePath.StartsWith("development", StringComparison.OrdinalIgnoreCase))
        {
            return DevelopmentSidebar;
        }

        if (relativePath.StartsWith("playground", StringComparison.OrdinalIgnoreCase)
            || relativePath.StartsWith("components/playground", StringComparison.OrdinalIgnoreCase))
        {
            return PlaygroundSidebar;
        }

        return ComponentSidebar;
    }

    public static bool HasSidebar(string relativePath) =>
        !string.IsNullOrWhiteSpace(relativePath)
        && !relativePath.Equals("/", StringComparison.Ordinal)
        && !relativePath.StartsWith("about", StringComparison.OrdinalIgnoreCase);

    public static bool HideAside(string relativePath) =>
        relativePath.StartsWith("playground", StringComparison.OrdinalIgnoreCase)
        || relativePath.StartsWith("components/playground", StringComparison.OrdinalIgnoreCase)
        || relativePath.Equals("component/overview", StringComparison.OrdinalIgnoreCase)
        || relativePath.Equals("components", StringComparison.OrdinalIgnoreCase)
        || relativePath.Equals("components/introduce-cn", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<OutlineItem> GetOutline(string relativePath)
    {
        if (relativePath.StartsWith("development/introduce", StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                new("特性", "#features"),
                new("安装", "#install"),
                new("原子组件", "#components"),
            ];
        }

        if (relativePath.StartsWith("development/compatible-style", StringComparison.OrdinalIgnoreCase))
        {
            return [new("使用说明", "#usage")];
        }

        if (relativePath.StartsWith("component", StringComparison.OrdinalIgnoreCase)
            || relativePath.StartsWith("components", StringComparison.OrdinalIgnoreCase))
        {
            return
            [
                new("何时使用", "#when-to-use"),
                new("代码演示", "#examples"),
                new("API", "#api"),
                new("贡献者", "#contributors"),
            ];
        }

        return [];
    }

    public static bool IsActive(string currentPath, string href)
    {
        var normalizedCurrent = Normalize(currentPath);
        var normalizedHref = Normalize(href);

        if (normalizedCurrent.Equals(normalizedHref, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (normalizedHref.StartsWith("component/", StringComparison.OrdinalIgnoreCase))
        {
            var legacy = "components/" + normalizedHref["component/".Length..];
            return normalizedCurrent.Equals(legacy, StringComparison.OrdinalIgnoreCase);
        }

        if (normalizedHref.StartsWith("playground/", StringComparison.OrdinalIgnoreCase))
        {
            var legacy = "components/playground-" + normalizedHref["playground/".Length..];
            return normalizedCurrent.Equals(legacy, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    public static string Normalize(string value)
    {
        value = value.Trim();
        value = value.TrimStart('/');
        value = value.Split('#', '?')[0];
        return value.TrimEnd('/');
    }
}
