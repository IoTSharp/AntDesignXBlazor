using VerifyXunit;

namespace AntDesign.X.Tests.Snapshots;

public class XCssSnapshotTests
{
    [Fact]
    public Task FullTokens_emits_stable_css_variable_string()
    {
        var tokens = new XThemeTokens
        {
            PrimaryColor = "#1677ff",
            BorderRadius = "8px",
            ColorBgChat = "#f6f7f9",
            ColorBgBubbleUser = "#1677ff",
            ColorBgBubbleAi = "#ffffff",
            ColorBgBubbleSystem = "#fff7e6",
            ColorBorderBubble = "#e5e7eb",
            ColorTextBubbleUser = "#ffffff",
            ColorTextBubbleAi = "#1f2937",
            ColorTextThink = "#6b7280",
            ColorBgThink = "#f3f4f6",
            ColorBgThoughtChain = "#fafafa",
            PaddingChat = "16px",
            PaddingBubble = "12px",
            MotionDurationFast = "100ms",
            MotionDuration = "200ms",
            MotionDurationSlow = "300ms",
            MotionEaseInOut = "cubic-bezier(.4,0,.2,1)",
            MotionEaseOut = "cubic-bezier(0,0,.2,1)",
            MotionEaseIn = "cubic-bezier(.4,0,1,1)",
            FontFamily = "Inter, sans-serif",
            FontSize = "14px",
            FontSizeSm = "12px",
            FontSizeLg = "16px",
            FontSizeXl = "20px",
            LineHeight = "1.5",
            LineHeightSm = "1.4",
            LineHeightLg = "1.6",
        };

        return Verifier.Verify(XCss.ToCssVariables(tokens));
    }
}
