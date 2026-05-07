using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Components;
using Markdig;

namespace AntDesign.X;

internal static class XCss
{
    private static readonly MarkdownPipeline MarkdownPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    public static string Combine(params string?[] values)
    {
        var parts = new List<string>();

        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                parts.AddRange(value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
            }
        }

        return string.Join(" ", parts);
    }

    public static string Status(XSemanticStatus status)
    {
        return status.ToString().ToLowerInvariant();
    }

    public static string FileStatus(XFileCardStatus status)
    {
        return status.ToString().ToLowerInvariant();
    }

    public static string ToCssVariables(XThemeTokens? tokens)
    {
        if (tokens is null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        Append(builder, "--antdx-color-primary", tokens.PrimaryColor);
        Append(builder, "--antdx-radius-md", tokens.BorderRadius);
        Append(builder, "--antdx-bubble-bg-start", tokens.BubbleStartBackground);
        Append(builder, "--antdx-bubble-bg-end", tokens.BubbleEndBackground);
        Append(builder, "--antdx-page-bg", tokens.PageBackground);
        Append(builder, "--antdx-component-bg", tokens.ComponentBackground);
        Append(builder, "--antdx-color-bg-chat", tokens.ColorBgChat);
        Append(builder, "--antdx-color-bg-bubble-user", tokens.ColorBgBubbleUser);
        Append(builder, "--antdx-color-bg-bubble-ai", tokens.ColorBgBubbleAi);
        Append(builder, "--antdx-color-bg-bubble-system", tokens.ColorBgBubbleSystem);
        Append(builder, "--antdx-color-border-bubble", tokens.ColorBorderBubble);
        Append(builder, "--antdx-color-text-bubble-user", tokens.ColorTextBubbleUser);
        Append(builder, "--antdx-color-text-bubble-ai", tokens.ColorTextBubbleAi);
        Append(builder, "--antdx-color-text-think", tokens.ColorTextThink);
        Append(builder, "--antdx-color-bg-think", tokens.ColorBgThink);
        Append(builder, "--antdx-color-bg-thought-chain", tokens.ColorBgThoughtChain);
        Append(builder, "--antdx-padding-chat", tokens.PaddingChat);
        Append(builder, "--antdx-padding-bubble", tokens.PaddingBubble);
        Append(builder, "--antdx-motion-duration-fast", tokens.MotionDurationFast);
        Append(builder, "--antdx-motion-duration", tokens.MotionDuration);
        Append(builder, "--antdx-motion-duration-slow", tokens.MotionDurationSlow);
        Append(builder, "--antdx-motion-ease-in-out", tokens.MotionEaseInOut);
        Append(builder, "--antdx-motion-ease-out", tokens.MotionEaseOut);
        Append(builder, "--antdx-motion-ease-in", tokens.MotionEaseIn);
        return builder.ToString();
    }

    public static string CombineStyles(params string?[] values)
    {
        var builder = new StringBuilder();

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            var trimmed = value.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (builder.Length > 0 && builder[builder.Length - 1] != ';')
            {
                builder.Append(';');
            }

            builder.Append(trimmed);

            if (builder[builder.Length - 1] != ';')
            {
                builder.Append(';');
            }
        }

        return builder.ToString();
    }

    public static MarkupString Markdown(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return new MarkupString(string.Empty);
        }

        var html = Markdig.Markdown.ToHtml(markdown, MarkdownPipeline);
        return new MarkupString(html);
    }

    public static string FormatSize(long? size)
    {
        if (size is null)
        {
            return string.Empty;
        }

        var value = Convert.ToDouble(size.Value, CultureInfo.InvariantCulture);
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var unit = 0;

        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return unit == 0
            ? $"{value:0} {units[unit]}"
            : $"{value:0.#} {units[unit]}";
    }

    private static void Append(StringBuilder builder, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            builder.Append(name).Append(':').Append(value).Append(';');
        }
    }
}
