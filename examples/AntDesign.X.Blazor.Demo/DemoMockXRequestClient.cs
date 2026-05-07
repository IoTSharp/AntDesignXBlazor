using System.Net;
using System.Text;
using System.Text.Json;

namespace AntDesign.X.Blazor.Demo;

public sealed class DemoMockXRequestClient : IXRequestClient
{
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
        XRequestOptions request,
        CancellationTokenSource linkedCts,
        TaskCompletionSource completion)
    {
        try
        {
            var scenario = ResolveScenario(request);
            var script = scenario == "agent"
                ? BuildAgentScript(requestId, request)
                : BuildChatScript(requestId, request);

            var payload = BuildPayload(script);
            await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));

            await foreach (var chunk in XStreamReader.ReadAsync(
                               stream,
                               new XStreamReaderOptions { Mode = XStreamReadMode.Sse },
                               linkedCts.Token))
            {
                if (request.OnChunk is not null)
                {
                    await request.OnChunk(chunk);
                }

                if (chunk.IsDone)
                {
                    break;
                }

                await Task.Delay(110, linkedCts.Token);
            }

            if (request.OnCompleted is not null)
            {
                await request.OnCompleted(new XRequestCompletedContext
                {
                    RequestId = requestId,
                    StatusCode = HttpStatusCode.OK,
                    Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["x-demo-stream"] = ["true"],
                    },
                });
            }

            completion.TrySetResult();
        }
        catch (OperationCanceledException exception)
        {
            if (request.OnError is not null)
            {
                await request.OnError(new XRequestErrorContext
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
            if (request.OnError is not null)
            {
                await request.OnError(new XRequestErrorContext
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

    private static string ResolveScenario(XRequestOptions request)
    {
        if (request.Metadata.TryGetValue("scenario", out var scenario) &&
            !string.IsNullOrWhiteSpace(scenario))
        {
            return scenario;
        }

        return request.Body switch
        {
            XAgentRequestPayload => "agent",
            XChatRequestPayload => "chat",
            _ => "chat",
        };
    }

    private static IEnumerable<SseEvent> BuildChatScript(string requestId, XRequestOptions request)
    {
        var payload = request.Body as XChatRequestPayload;
        var prompt = payload?.Prompt ?? string.Empty;
        var attachmentCount = payload?.Attachments.Count ?? 0;
        var fragments = new[]
        {
            attachmentCount > 0
                ? $"收到 {attachmentCount} 个附件。"
                : "已收到输入。",
            "我会先归纳上下文，",
            "再给出一个可流式更新的回答。",
            string.IsNullOrWhiteSpace(prompt)
                ? "当前没有额外提示。"
                : $"当前提示是：{prompt}"
        };

        yield return new SseEvent("message", CreateOpenAiDelta(requestId, "assistant", null));

        foreach (var fragment in fragments)
        {
            yield return new SseEvent("message", CreateOpenAiDelta(requestId, null, fragment));
        }

        yield return new SseEvent("done", "[DONE]");
    }

    private static IEnumerable<SseEvent> BuildAgentScript(string requestId, XRequestOptions request)
    {
        var payload = request.Body as XAgentRequestPayload;
        var prompt = payload?.Prompt ?? string.Empty;
        var attachmentCount = payload?.Attachments.Count ?? 0;

        yield return new SseEvent("thought", JsonSerializer.Serialize(new
        {
            title = "识别意图",
            description = string.IsNullOrWhiteSpace(prompt) ? "检查默认 agent 路径" : prompt,
            content = string.IsNullOrWhiteSpace(prompt) ? "检查默认 agent 路径" : prompt,
            status = "processing",
        }));

        yield return new SseEvent("thought", JsonSerializer.Serialize(new
        {
            title = "规划工具",
            description = attachmentCount > 0
                ? $"准备处理 {attachmentCount} 个附件并查询上下文"
                : "准备查询上下文与示例知识",
            content = attachmentCount > 0
                ? $"准备处理 {attachmentCount} 个附件并查询上下文"
                : "准备查询上下文与示例知识",
            status = "processing",
        }));

        yield return new SseEvent("tool_call", JsonSerializer.Serialize(new
        {
            name = "search_memory",
            arguments = $"{{\"query\":\"{EscapeJson(prompt)}\"}}",
            result = "2 hits",
            content = "search_memory => 2 hits",
            status = "success",
        }));

        yield return new SseEvent("source", JsonSerializer.Serialize(new
        {
            title = "workspace notes",
            description = "会话上下文与 SDK 状态",
            url = "https://example.invalid/workspace",
            icon = "folder",
            content = "workspace notes",
            status = "success",
        }));

        yield return new SseEvent("message", CreateOpenAiDelta(requestId, "assistant", null));
        yield return new SseEvent("message", CreateOpenAiDelta(requestId, null, "Agent 已完成 trace。"));
        yield return new SseEvent("message", CreateOpenAiDelta(requestId, null, " 这次输出包含思考、工具调用和来源。"));
        yield return new SseEvent("done", "[DONE]");
    }

    private static string CreateOpenAiDelta(string requestId, string? role, string? content)
    {
        return JsonSerializer.Serialize(new
        {
            id = requestId,
            choices = new[]
            {
                new
                {
                    delta = new
                    {
                        role,
                        content,
                    },
                },
            },
        });
    }

    private static string BuildPayload(IEnumerable<SseEvent> events)
    {
        var builder = new StringBuilder();

        foreach (var item in events)
        {
            if (!string.IsNullOrWhiteSpace(item.Event))
            {
                builder.Append("event: ").AppendLine(item.Event);
            }

            var lines = item.Data.Split('\n');
            foreach (var line in lines)
            {
                builder.Append("data: ").AppendLine(line);
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string EscapeJson(string? text)
    {
        return string.IsNullOrEmpty(text)
            ? string.Empty
            : text.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private sealed record SseEvent(string? Event, string Data);
}
