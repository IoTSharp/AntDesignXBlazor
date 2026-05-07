namespace AntDesign.X;

public enum XBubblePlacement
{
    Start = 0,
    End = 1,
}

public enum XBubbleVariant
{
    Filled = 0,
    Borderless = 1,
    Outlined = 2,
    Shadow = 3,
}

public enum XBubbleShape
{
    Default = 0,
    Round = 1,
    Corner = 2,
}

public enum XBubbleFooterPlacement
{
    OuterStart = 0,
    OuterEnd = 1,
    InnerStart = 2,
    InnerEnd = 3,
}

public enum XSemanticStatus
{
    Default = 0,
    Processing = 1,
    Success = 2,
    Warning = 3,
    Error = 4,
}

public enum XMessageStatus
{
    Local = 0,
    Loading = 1,
    Updating = 2,
    Success = 3,
    Error = 4,
    Abort = 5,
}

public enum XFileCardStatus
{
    Default = 0,
    Uploading = 1,
    Done = 2,
    Error = 3,
    Removed = 4,
}

public enum XFolderVariant
{
    Card = 0,
    Ghost = 1,
    Borderless = 2,
}

public enum XNotificationPlacement
{
    TopRight = 0,
    TopLeft = 1,
    BottomRight = 2,
    BottomLeft = 3,
}

public enum XSenderSubmitMode
{
    Enter = 0,
    ShiftEnter = 1,
    ModEnter = 2,
}

public enum XMermaidRenderType
{
    Image = 0,
    Code = 1,
}
