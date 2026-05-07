using Microsoft.Extensions.DependencyInjection;

namespace AntDesign.X.Tests;

public class XLocaleServiceTests
{
    [Fact]
    public void Default_locale_is_zh_CN()
    {
        var svc = new XLocaleService();
        svc.CurrentLocale.Should().Be("zh-CN");
        svc.T("sender.submit").Should().Be("发送");
    }

    [Theory]
    [InlineData("en-US", "Send")]
    [InlineData("ja-JP", "送信")]
    public void Use_switches_active_locale(string locale, string expected)
    {
        var svc = new XLocaleService();
        svc.Use(locale);
        svc.T("sender.submit").Should().Be(expected);
    }

    [Fact]
    public void Unknown_locale_falls_back_to_default()
    {
        var svc = new XLocaleService();
        svc.Use("fr-FR");
        svc.CurrentLocale.Should().Be("zh-CN");
    }

    [Fact]
    public void Missing_key_returns_key_itself()
    {
        var svc = new XLocaleService();
        svc.T("nonexistent.key").Should().Be("nonexistent.key");
    }

    [Fact]
    public void Register_merges_entries()
    {
        var svc = new XLocaleService();
        svc.Register("zh-CN", new Dictionary<string, string> { ["custom.hello"] = "你好" });
        svc.T("custom.hello").Should().Be("你好");
        svc.T("sender.submit").Should().Be("发送");
    }
}
