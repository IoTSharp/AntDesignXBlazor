using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AntDesign.X.Blazor.Demo.Services;

/// <summary>
/// 真实接入 DeepSeek（OpenAI 兼容协议）的最小化流式聊天客户端。
/// API key 从配置 "DeepSeek:ApiKey" 读取，不写入仓库。
/// </summary>
public sealed class DeepSeekChatClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly IHttpClientFactory _factory;
    private readonly DeepSeekOptions _options;

    public DeepSeekChatClient(IHttpClientFactory factory, DeepSeekOptions options)
    {
        _factory = factory;
        _options = options;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public string Model => _options.Model;

    public string Endpoint => _options.Endpoint;

    public async IAsyncEnumerable<DeepSeekDelta> StreamAsync(
        IReadOnlyList<DeepSeekMessage> messages,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException(
                "未配置 DeepSeek ApiKey。请在仓库根目录创建 .env，并写入 DEEPSEEK_API_KEY=sk-xxx。");
        }

        using var client = _factory.CreateClient("DeepSeek");
        client.Timeout = TimeSpan.FromMinutes(2);

        var payload = new DeepSeekChatRequest
        {
            Model = _options.Model,
            Stream = true,
            Messages = messages,
            Temperature = _options.Temperature,
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Endpoint)
        {
            Content = JsonContent.Create(payload, options: JsonOptions),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.Accept.ParseAdd("text/event-stream");

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"DeepSeek 调用失败 {(int)response.StatusCode} {response.ReasonPhrase}: {body}",
                null,
                response.StatusCode);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                yield break;
            }

            if (line.Length == 0)
            {
                continue;
            }

            if (!line.StartsWith("data:", StringComparison.Ordinal))
            {
                continue;
            }

            var data = line[5..].Trim();
            if (data is "[DONE]")
            {
                yield break;
            }

            DeepSeekStreamChunk? chunk = null;
            try
            {
                chunk = JsonSerializer.Deserialize<DeepSeekStreamChunk>(data, JsonOptions);
            }
            catch (JsonException)
            {
                // skip malformed chunk
            }

            if (chunk?.Choices is { Count: > 0 } choices)
            {
                var first = choices[0];
                yield return new DeepSeekDelta(
                    first.Delta?.Content,
                    first.Delta?.ReasoningContent,
                    first.FinishReason);
            }
        }
    }
}

public sealed class DeepSeekOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://api.deepseek.com/chat/completions";
    public string Model { get; set; } = "deepseek-chat";
    public double Temperature { get; set; } = 0.7;
}

public sealed record DeepSeekMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);

public sealed record DeepSeekDelta(string? Content, string? Reasoning, string? FinishReason);

internal sealed class DeepSeekChatRequest
{
    [JsonPropertyName("model")] public string Model { get; set; } = "deepseek-chat";
    [JsonPropertyName("messages")] public IReadOnlyList<DeepSeekMessage> Messages { get; set; } = Array.Empty<DeepSeekMessage>();
    [JsonPropertyName("stream")] public bool Stream { get; set; } = true;
    [JsonPropertyName("temperature")] public double Temperature { get; set; } = 0.7;
}

internal sealed class DeepSeekStreamChunk
{
    [JsonPropertyName("choices")] public List<DeepSeekStreamChoice>? Choices { get; set; }
}

internal sealed class DeepSeekStreamChoice
{
    [JsonPropertyName("delta")] public DeepSeekStreamDelta? Delta { get; set; }
    [JsonPropertyName("finish_reason")] public string? FinishReason { get; set; }
}

internal sealed class DeepSeekStreamDelta
{
    [JsonPropertyName("role")] public string? Role { get; set; }
    [JsonPropertyName("content")] public string? Content { get; set; }
    [JsonPropertyName("reasoning_content")] public string? ReasoningContent { get; set; }
}
