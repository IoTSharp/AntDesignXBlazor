using System.Collections.Concurrent;

namespace AntDesign.X;

/// <summary>
/// 单条本地化字符串。键采用 <c>组件.字段</c> 的扁平命名（如 <c>sender.submit</c>）。
/// </summary>
public sealed record XLocaleEntries(IReadOnlyDictionary<string, string> Entries)
{
    public string this[string key] => Entries.TryGetValue(key, out var value) ? value : key;
}

/// <summary>
/// 多语言资源服务。通过 <c>XProvider.Locale</c> 切换；未命中的 key 回退到默认语言（zh-CN）。
/// </summary>
public interface IXLocaleService
{
    /// <summary>当前激活语言。</summary>
    string CurrentLocale { get; }

    /// <summary>切换激活语言；不存在时静默回退到默认语言。</summary>
    void Use(string? locale);

    /// <summary>注册或合并一个语言资源包。</summary>
    void Register(string locale, IReadOnlyDictionary<string, string> entries);

    /// <summary>读取指定 key 的本地化文本，找不到时返回 key 本身。</summary>
    string T(string key);
}

/// <inheritdoc />
public sealed class XLocaleService : IXLocaleService
{
    private const string DefaultLocale = "zh-CN";
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _resources = new(StringComparer.OrdinalIgnoreCase);
    private string _current = DefaultLocale;

    public XLocaleService()
    {
        Register(DefaultLocale, BuiltIn.ZhCn);
        Register("en-US", BuiltIn.EnUs);
        Register("ja-JP", BuiltIn.JaJp);
    }

    public string CurrentLocale => _current;

    public void Use(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            _current = DefaultLocale;
            return;
        }

        _current = _resources.ContainsKey(locale) ? locale : DefaultLocale;
    }

    public void Register(string locale, IReadOnlyDictionary<string, string> entries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(locale);
        ArgumentNullException.ThrowIfNull(entries);

        _resources.AddOrUpdate(
            locale,
            _ => new Dictionary<string, string>(entries, StringComparer.OrdinalIgnoreCase),
            (_, existing) =>
            {
                foreach (var kvp in entries)
                {
                    existing[kvp.Key] = kvp.Value;
                }
                return existing;
            });
    }

    public string T(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return string.Empty;
        }

        if (_resources.TryGetValue(_current, out var current) && current.TryGetValue(key, out var value))
        {
            return value;
        }

        if (!string.Equals(_current, DefaultLocale, StringComparison.OrdinalIgnoreCase)
            && _resources.TryGetValue(DefaultLocale, out var fallback)
            && fallback.TryGetValue(key, out var fallbackValue))
        {
            return fallbackValue;
        }

        return key;
    }

    internal static class BuiltIn
    {
        public static readonly Dictionary<string, string> ZhCn = new(StringComparer.OrdinalIgnoreCase)
        {
            ["sender.submit"] = "发送",
            ["sender.cancel"] = "停止",
            ["sender.upload"] = "附件",
            ["sender.speech.start"] = "开始语音",
            ["sender.speech.stop"] = "结束语音",
            ["sender.placeholder"] = "请输入消息…",
            ["sender.attachments.placeholder"] = "拖拽文件到这里上传",
            ["bubble.loading"] = "正在思考…",
            ["conversations.new"] = "新建会话",
            ["notification.close"] = "关闭",
        };

        public static readonly Dictionary<string, string> EnUs = new(StringComparer.OrdinalIgnoreCase)
        {
            ["sender.submit"] = "Send",
            ["sender.cancel"] = "Stop",
            ["sender.upload"] = "Attach",
            ["sender.speech.start"] = "Start speaking",
            ["sender.speech.stop"] = "Stop speaking",
            ["sender.placeholder"] = "Type a message…",
            ["sender.attachments.placeholder"] = "Drag files here to upload",
            ["bubble.loading"] = "Thinking…",
            ["conversations.new"] = "New conversation",
            ["notification.close"] = "Close",
        };

        public static readonly Dictionary<string, string> JaJp = new(StringComparer.OrdinalIgnoreCase)
        {
            ["sender.submit"] = "送信",
            ["sender.cancel"] = "停止",
            ["sender.upload"] = "添付",
            ["sender.speech.start"] = "音声開始",
            ["sender.speech.stop"] = "音声終了",
            ["sender.placeholder"] = "メッセージを入力…",
            ["sender.attachments.placeholder"] = "ファイルをここにドラッグしてアップロード",
            ["bubble.loading"] = "考え中…",
            ["conversations.new"] = "新しい会話",
            ["notification.close"] = "閉じる",
        };
    }
}
