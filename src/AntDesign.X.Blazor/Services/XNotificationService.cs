namespace AntDesign.X;

public interface IXNotificationService
{
    event Action? Changed;

    IReadOnlyList<XNotificationItem> Items { get; }

    int? MaxCount { get; }

    TimeSpan DefaultDuration { get; }

    void Configure(int? maxCount, TimeSpan? defaultDuration);

    Task<XNotificationItem> OpenAsync(XNotificationItem item);

    Task CloseAsync(string key);

    Task ClearAsync();
}

public sealed class XNotificationService : IXNotificationService, IDisposable
{
    private readonly object _gate = new();
    private readonly List<XNotificationItem> _items = [];
    private readonly Dictionary<string, CancellationTokenSource> _timers = [];

    public event Action? Changed;

    public int? MaxCount { get; private set; } = 5;

    public TimeSpan DefaultDuration { get; private set; } = TimeSpan.FromSeconds(4.5);

    public IReadOnlyList<XNotificationItem> Items
    {
        get
        {
            lock (_gate)
            {
                return _items.ToArray();
            }
        }
    }

    public void Configure(int? maxCount, TimeSpan? defaultDuration)
    {
        lock (_gate)
        {
            MaxCount = maxCount;
            if (defaultDuration is not null && defaultDuration.Value > TimeSpan.Zero)
            {
                DefaultDuration = defaultDuration.Value;
            }
        }
    }

    public Task<XNotificationItem> OpenAsync(XNotificationItem item)
    {
        var resolved = Normalize(item);
        CancellationTokenSource? timer = null;
        var duration = resolved.Duration ?? DefaultDuration;

        lock (_gate)
        {
            RemoveLocked(resolved.Key);
            RemoveTaggedLocked(resolved.Tag, resolved.Key);
            _items.Insert(0, resolved);

            if (MaxCount is > 0)
            {
                while (_items.Count > MaxCount.Value)
                {
                    var removed = _items[^1];
                    _items.RemoveAt(_items.Count - 1);
                    CancelLocked(removed.Key);
                }
            }

            if (duration > TimeSpan.Zero)
            {
                timer = new CancellationTokenSource();
                _timers[resolved.Key] = timer;
            }
        }

        NotifyChanged();

        if (timer is not null)
        {
            _ = AutoCloseAsync(resolved.Key, duration, timer.Token);
        }

        return Task.FromResult(resolved);
    }

    public Task CloseAsync(string key)
    {
        lock (_gate)
        {
            RemoveLocked(key);
        }

        NotifyChanged();
        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        lock (_gate)
        {
            foreach (var timer in _timers.Values)
            {
                timer.Cancel();
                timer.Dispose();
            }

            _timers.Clear();
            _items.Clear();
        }

        NotifyChanged();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var timer in _timers.Values)
            {
                timer.Cancel();
                timer.Dispose();
            }

            _timers.Clear();
        }
    }

    private async Task AutoCloseAsync(string key, TimeSpan duration, CancellationToken token)
    {
        try
        {
            await Task.Delay(duration, token);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await CloseAsync(key);
    }

    private XNotificationItem Normalize(XNotificationItem item)
    {
        var key = string.IsNullOrWhiteSpace(item.Key) ? Guid.NewGuid().ToString("N") : item.Key;
        return item with
        {
            Key = key,
            CreatedAt = item.CreatedAt ?? DateTimeOffset.UtcNow,
        };
    }

    private void RemoveLocked(string key)
    {
        var index = _items.FindIndex(item => item.Key == key);
        if (index >= 0)
        {
            _items.RemoveAt(index);
        }

        CancelLocked(key);
    }

    private void RemoveTaggedLocked(string? tag, string exceptKey)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return;
        }

        for (var index = _items.Count - 1; index >= 0; index--)
        {
            var item = _items[index];
            if (item.Key == exceptKey)
            {
                continue;
            }

            if (string.Equals(item.Tag, tag, StringComparison.OrdinalIgnoreCase))
            {
                _items.RemoveAt(index);
                CancelLocked(item.Key);
            }
        }
    }

    private void CancelLocked(string key)
    {
        if (_timers.Remove(key, out var timer))
        {
            timer.Cancel();
            timer.Dispose();
        }
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}
