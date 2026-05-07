namespace AntDesign.X;

/// <summary>
/// Placement of an <c>XBubble</c> within a conversation flow.
/// </summary>
public enum XBubblePlacement
{
    /// <summary>Aligned to the start (typically assistant / system messages).</summary>
    Start = 0,

    /// <summary>Aligned to the end (typically the current user).</summary>
    End = 1,
}
