namespace AntDesign.X.Blazor.Demo;

public static class DemoShellLinks
{
    public const string RepositoryUrl = "https://github.com/IoTSharp/AntDesignXBlazor";

    public sealed record NavLinkItem(string Href, string Text, string Icon, bool Exact = false);

    public static readonly IReadOnlyList<NavLinkItem> PrimaryLinks =
    [
        new("/", "首页", "home", true),
        new("/components", "组件总览", "appstore"),
        new("/about", "关于", "info-circle"),
    ];

    public static readonly IReadOnlyList<NavLinkItem> HomeSections =
    [
        new("/#experience", "工作台", "message"),
        new("/#display", "展示", "appstore"),
        new("/#input", "输入", "paper-clip"),
        new("/#feedback", "反馈", "bell"),
        new("/#sdk", "SDK", "robot"),
        new("/#markdown", "文档", "file-text"),
        new("/#code", "源码", "code"),
    ];
}
