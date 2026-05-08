namespace AntDesign.X.Blazor.Demo;

public static class DemoShellLinks
{
    public const string GitHubRepositoryUrl = "https://github.com/IoTSharp/AntDesignXBlazor";
    public const string GiteeRepositoryUrl = "https://gitee.com/IoTSharp/AntDesignXBlazor";
    public const string RepositoryUrl = GitHubRepositoryUrl;

    public sealed record NavLinkItem(string Href, string Text, string Icon, bool Exact = false, string? EnglishTitle = null);

    public static readonly IReadOnlyList<NavLinkItem> PrimaryLinks =
    [
        new("/", "首页", "home", true),
        new("/components", "组件总览", "appstore"),
        new("/about", "关于", "info-circle"),
    ];

    public static readonly IReadOnlyList<NavLinkItem> HomeSections =
    [
        new("/#experience", "完整工作台体验", "message"),
        new("/#display", "展示组件", "appstore"),
        new("/#input", "输入与建议", "paper-clip"),
        new("/#feedback", "反馈与状态", "bell"),
        new("/#sdk", "SDK 闭环", "robot"),
        new("/#markdown", "Markdown、代码与图", "file-text"),
        new("/#code", "最小使用代码", "code"),
    ];

    public static readonly IReadOnlyList<NavLinkItem> HomePlaygroundLinks =
    [
        new("/components/playground-ultramodern", "Ultramodern", "experiment", false, "Ultramodern"),
        new("/components/playground-independent", "Independent", "experiment", false, "Independent"),
        new("/components/playground-copilot", "Copilot", "experiment", false, "Copilot"),
    ];
}
