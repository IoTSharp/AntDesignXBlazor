namespace AntDesign.X.Tests;

public class XRendererRegistryTests
{
    [Fact]
    public void Register_and_resolve_returns_fragment()
    {
        var registry = new XRendererRegistry();
        Microsoft.AspNetCore.Components.RenderFragment<string> fragment = src => __ => { };

        registry.Register("latex", fragment);

        registry.Resolve("latex").Should().BeSameAs(fragment);
        registry.Languages.Should().Contain("latex");
    }

    [Fact]
    public void Resolve_is_case_insensitive()
    {
        var registry = new XRendererRegistry();
        Microsoft.AspNetCore.Components.RenderFragment<string> fragment = _ => __ => { };
        registry.Register("Mermaid", fragment);

        registry.Resolve("mermaid").Should().NotBeNull();
        registry.Resolve("MERMAID").Should().NotBeNull();
    }

    [Fact]
    public void Resolve_unknown_returns_null()
    {
        var registry = new XRendererRegistry();
        registry.Resolve("unknown").Should().BeNull();
        registry.Resolve(null).Should().BeNull();
    }

    [Fact]
    public void Unregister_removes_entry()
    {
        var registry = new XRendererRegistry();
        registry.Register("chart", _ => __ => { });

        registry.Unregister("chart").Should().BeTrue();
        registry.Resolve("chart").Should().BeNull();
    }
}
