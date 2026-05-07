using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace AntDesign.X;

public sealed class XRequestClientOptions
{
    public string HttpClientName { get; set; } = "AntDesignX";
    public Uri? BaseAddress { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(120);
    public string ChatPath { get; set; } = "/v1/chat/completions";
    public string AgentPath { get; set; } = "/v1/agents/run";
    public Dictionary<string, string> DefaultHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed record XRequestOptions
{
    public string? RequestId { get; init; }
    public Uri RequestUri { get; init; } = new("/", UriKind.Relative);
    public HttpMethod Method { get; init; } = HttpMethod.Post;
    public object? Body { get; init; }
    public bool Stream { get; init; } = true;
    public XStreamReadMode? StreamMode { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public JsonSerializerOptions? JsonSerializerOptions { get; init; }
    public Func<HttpRequestMessage, ValueTask>? ConfigureRequest { get; init; }
    public Func<XStreamChunk, ValueTask>? OnChunk { get; init; }
    public Func<XRequestErrorContext, ValueTask>? OnError { get; init; }
    public Func<XRequestCompletedContext, ValueTask>? OnCompleted { get; init; }
}

public sealed record XRequestCompletedContext
{
    public string RequestId { get; init; } = string.Empty;
    public HttpStatusCode? StatusCode { get; init; }
    public string? ResponseText { get; init; }
    public IReadOnlyDictionary<string, string[]> Headers { get; init; } = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
}

public sealed record XRequestErrorContext
{
    public string RequestId { get; init; } = string.Empty;
    public Exception Exception { get; init; } = new InvalidOperationException("Request failed.");
    public bool IsCanceled { get; init; }
    public HttpStatusCode? StatusCode { get; init; }
    public string? ResponseText { get; init; }
    public IReadOnlyDictionary<string, string[]> Headers { get; init; } = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
}

public sealed class XRequestHandle
{
    private readonly Func<ValueTask> _abort;

    public XRequestHandle(string requestId, Func<ValueTask> abort, Task completion)
    {
        RequestId = requestId;
        _abort = abort;
        Completion = completion;
    }

    public string RequestId { get; }

    public bool IsRequesting => !Completion.IsCompleted;

    public Task Completion { get; }

    public ValueTask AbortAsync()
    {
        return _abort();
    }
}

public interface IXRequestClient
{
    Task<XRequestHandle> RequestAsync(XRequestOptions request, CancellationToken cancellationToken = default);
}

public sealed class XRequestClient : IXRequestClient
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly XRequestClientOptions _options;

    public XRequestClient(IHttpClientFactory clientFactory, XRequestClientOptions options)
    {
        _clientFactory = clientFactory;
        _options = options;
    }

    public Task<XRequestHandle> RequestAsync(XRequestOptions request, CancellationToken cancellationToken = default)
    {
        var requestId = string.IsNullOrWhiteSpace(request.RequestId)
            ? Guid.NewGuid().ToString("N")
            : request.RequestId!;
        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = new XRequestHandle(
            requestId,
            () =>
            {
                try
                {
                    linkedCts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                }

                return ValueTask.CompletedTask;
            },
            completion.Task);

        _ = ExecuteAsync(requestId, request, linkedCts, completion);
        return Task.FromResult(handle);
    }

    private async Task ExecuteAsync(
        string requestId,
        XRequestOptions options,
        CancellationTokenSource linkedCts,
        TaskCompletionSource completion)
    {
        try
        {
            using var client = _clientFactory.CreateClient(_options.HttpClientName);
            ConfigureClient(client);

            using var request = BuildRequestMessage(requestId, options);
            if (options.ConfigureRequest is not null)
            {
                await options.ConfigureRequest(request);
            }

            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token);
            var responseHeaders = CaptureHeaders(response);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await ReadResponseTextAsync(response, linkedCts.Token);
                var exception = new HttpRequestException(
                    $"XRequest returned {(int)response.StatusCode} {response.ReasonPhrase}.",
                    null,
                    response.StatusCode);

                if (options.OnError is not null)
                {
                    await options.OnError(new XRequestErrorContext
                    {
                        RequestId = requestId,
                        Exception = exception,
                        StatusCode = response.StatusCode,
                        ResponseText = errorText,
                        Headers = responseHeaders,
                    });
                }

                completion.TrySetException(exception);
                return;
            }

            if (options.Stream)
            {
                var stream = await response.Content.ReadAsStreamAsync(linkedCts.Token);
                var streamMode = options.StreamMode ?? ResolveStreamMode(response);
                await foreach (var chunk in XStreamReader.ReadAsync(
                                   stream,
                                   new XStreamReaderOptions
                                   {
                                       Mode = streamMode,
                                       JsonSerializerOptions = options.JsonSerializerOptions,
                                   },
                                   linkedCts.Token))
                {
                    if (options.OnChunk is not null)
                    {
                        await options.OnChunk(chunk);
                    }

                    if (chunk.IsDone)
                    {
                        break;
                    }
                }
            }
            else
            {
                var responseText = await response.Content.ReadAsStringAsync(linkedCts.Token);
                if (options.OnChunk is not null)
                {
                    await options.OnChunk(new XStreamChunk
                    {
                        Raw = responseText,
                        Event = "message",
                        Data = responseText,
                        IsDone = true,
                    });
                }
            }

            if (options.OnCompleted is not null)
            {
                await options.OnCompleted(new XRequestCompletedContext
                {
                    RequestId = requestId,
                    StatusCode = response.StatusCode,
                    Headers = responseHeaders,
                });
            }

            completion.TrySetResult();
        }
        catch (OperationCanceledException exception)
        {
            if (options.OnError is not null)
            {
                await options.OnError(new XRequestErrorContext
                {
                    RequestId = requestId,
                    Exception = exception,
                    IsCanceled = true,
                });
            }

            completion.TrySetCanceled(linkedCts.Token);
        }
        catch (Exception exception)
        {
            if (options.OnError is not null)
            {
                await options.OnError(new XRequestErrorContext
                {
                    RequestId = requestId,
                    Exception = exception,
                });
            }

            completion.TrySetException(exception);
        }
        finally
        {
            linkedCts.Dispose();
        }
    }

    private void ConfigureClient(HttpClient client)
    {
        if (_options.BaseAddress is not null && client.BaseAddress is null)
        {
            client.BaseAddress = _options.BaseAddress;
        }

        if (_options.Timeout > TimeSpan.Zero)
        {
            client.Timeout = _options.Timeout;
        }
    }

    private HttpRequestMessage BuildRequestMessage(string requestId, XRequestOptions options)
    {
        var request = new HttpRequestMessage(options.Method, options.RequestUri)
        {
            Content = CreateContent(options),
        };

        var headers = new Dictionary<string, string>(_options.DefaultHeaders, StringComparer.OrdinalIgnoreCase)
        {
            ["x-antdx-request-id"] = requestId,
        };

        foreach (var header in options.Headers)
        {
            headers[header.Key] = header.Value;
        }

        if (options.Stream && !headers.ContainsKey("Accept"))
        {
            headers["Accept"] = "text/event-stream";
        }

        ApplyHeaders(request, headers);
        return request;
    }

    private HttpContent? CreateContent(XRequestOptions options)
    {
        if (options.Body is null || options.Method == HttpMethod.Get)
        {
            return null;
        }

        return options.Body switch
        {
            HttpContent content => content,
            string text => new StringContent(text, Encoding.UTF8, "text/plain"),
            byte[] bytes => new ByteArrayContent(bytes),
            Stream stream => new StreamContent(stream),
            _ => JsonContent.Create(options.Body, options: options.JsonSerializerOptions),
        };
    }

    private static void ApplyHeaders(HttpRequestMessage request, IReadOnlyDictionary<string, string> headers)
    {
        foreach (var header in headers)
        {
            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value) &&
                request.Content is not null)
            {
                request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }

    private static XStreamReadMode ResolveStreamMode(HttpResponseMessage response)
    {
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        return string.Equals(mediaType, "text/event-stream", StringComparison.OrdinalIgnoreCase)
            ? XStreamReadMode.Sse
            : XStreamReadMode.Line;
    }

    private static async Task<string?> ReadResponseTextAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    private static IReadOnlyDictionary<string, string[]> CaptureHeaders(HttpResponseMessage response)
    {
        var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        foreach (var header in response.Headers)
        {
            headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in response.Content.Headers)
        {
            headers[header.Key] = header.Value.ToArray();
        }

        return headers;
    }
}
