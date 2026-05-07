namespace AntDesign.X.Tests;

public class XCssTests
{
    [Fact]
    public void ToCssVariables_returns_empty_for_null()
    {
        XCss.ToCssVariables(null).Should().BeEmpty();
    }

    [Fact]
    public void ToCssVariables_emits_only_set_tokens()
    {
        var tokens = new XThemeTokens
        {
            PrimaryColor = "#ff0000",
            ColorBgChat = "#fafafa",
            MotionDurationFast = "100ms",
        };

        var css = XCss.ToCssVariables(tokens);

        css.Should().Contain("--antdx-color-primary:#ff0000;");
        css.Should().Contain("--antdx-color-bg-chat:#fafafa;");
        css.Should().Contain("--antdx-motion-duration-fast:100ms;");
        css.Should().NotContain("--antdx-radius-md");
    }

    [Fact]
    public void Combine_skips_blank_segments()
    {
        XCss.Combine("a", null, " ", "b").Should().Be("a b");
    }
}
