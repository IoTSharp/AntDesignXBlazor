using Microsoft.AspNetCore.Components;

namespace AntDesign.X;

/// <summary>
/// 插件化内容渲染注册中心。允许在 <c>XCodeHighlighter</c>、<c>XBubble</c> 等组件中按
/// 语言/类型注入自定义渲染（LaTeX、Mermaid、Chart、Artifact 等）。
/// </summary>
/// <remarks>
/// 使用示例：
/// <code>
/// services.AddSingleton&lt;IXRendererRegistry&gt;(_ =>
/// {
///     var registry = new XRendererRegistry();
///     registry.Register("latex", source => @&lt;div class="my-latex"&gt;@source&lt;/div&gt;);
///     return registry;
/// });
/// </code>
/// </remarks>
public interface IXRendererRegistry
{
    /// <summary>注册指定语言/类型的渲染器。</summary>
    void Register(string language, RenderFragment<string> renderer);

    /// <summary>移除一个渲染器，便于热替换。</summary>
    bool Unregister(string language);

    /// <summary>解析渲染器；未注册时返回 <see langword="null"/>。</summary>
    RenderFragment<string>? Resolve(string? language);

    /// <summary>当前所有已注册的语言。</summary>
    IReadOnlyCollection<string> Languages { get; }
}

/// <inheritdoc />
public sealed class XRendererRegistry : IXRendererRegistry
{
    private readonly Dictionary<string, RenderFragment<string>> _renderers =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(string language, RenderFragment<string> renderer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        ArgumentNullException.ThrowIfNull(renderer);
        _renderers[language] = renderer;
    }

    public bool Unregister(string language)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);
        return _renderers.Remove(language);
    }

    public RenderFragment<string>? Resolve(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return null;
        }

        return _renderers.TryGetValue(language, out var fragment) ? fragment : null;
    }

    public IReadOnlyCollection<string> Languages => _renderers.Keys;
}
