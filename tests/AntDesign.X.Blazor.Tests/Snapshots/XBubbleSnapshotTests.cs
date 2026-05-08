using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using VerifyXunit;

namespace AntDesign.X.Tests.Snapshots;

public class XBubbleSnapshotTests
{
    [Fact]
    public Task Renders_streaming_bubble_with_aria_live()
    {
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<XBubble>(p => p
            .Add(b => b.Header, "Assistant")
            .Add(b => b.Placement, XBubblePlacement.Start)
            .Add(b => b.Content, "Hello world")
            .Add(b => b.Streaming, true));

        return Verifier.Verify(NormalizeMarkup(cut.Markup), "html");
    }

    [Fact]
    public Task Renders_basic_user_bubble()
    {
        using var ctx = new TestContext();

        var cut = ctx.RenderComponent<XBubble>(p => p
            .Add(b => b.Header, "User")
            .Add(b => b.Placement, XBubblePlacement.End)
            .Add(b => b.Content, "Ping"));

        return Verifier.Verify(NormalizeMarkup(cut.Markup), "html");
    }

    private static string NormalizeMarkup(string markup)
        => markup.Replace("\r\n", "\n").Trim();
}
