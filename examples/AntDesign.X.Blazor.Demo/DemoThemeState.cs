namespace AntDesign.X.Blazor.Demo;

public sealed class DemoThemeState
{
    public string Theme { get; private set; } = "light";

    public bool IsDark => string.Equals(Theme, "dark", StringComparison.OrdinalIgnoreCase);

    public void Toggle()
    {
        Theme = IsDark ? "light" : "dark";
    }
}
