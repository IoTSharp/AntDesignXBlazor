namespace AntDesign.X.Blazor.Demo;

public static class DemoShellLinks
{
    public const string GitHubRepositoryUrl = "https://github.com/IoTSharp/AntDesignXBlazor";
    public const string GiteeRepositoryUrl = "https://gitee.com/IoTSharp/AntDesignXBlazor";
    public const string RepositoryUrl = GitHubRepositoryUrl;

    public sealed record NavLinkItem(string Href, string Text, string Icon, bool Exact = false, string? EnglishTitle = null);

    public static readonly IReadOnlyList<NavLinkItem> PrimaryLinks =
    [
        new("", "首页", "home", true),
        new("development/introduce", "研发", "code"),
        new("component/overview", "组件", "appstore"),
        new("playground/independent", "演示", "experiment"),
    ];

    public static readonly IReadOnlyList<NavLinkItem> HomeSections =
    [
        new("#rich", "RICH 范式", "bulb"),
        new("#scenarios", "场景范例", "desktop"),
        new("#components", "组件矩阵", "appstore"),
        new("#experience", "完整工作台体验", "message"),
        new("#display", "展示组件", "appstore"),
        new("#input", "输入与建议", "paper-clip"),
        new("#feedback", "反馈与状态", "bell"),
        new("#sdk", "SDK 闭环", "robot"),
        new("#markdown", "Markdown、代码与图", "file-text"),
        new("#code", "最小使用代码", "code"),
    ];

    public static readonly IReadOnlyList<NavLinkItem> PlaygroundLinks =
    [
        new("playground/independent", "Independent", "experiment"),
        new("playground/copilot", "Copilot", "experiment"),
    ];
}
