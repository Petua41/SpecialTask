namespace SpecialTask.Infrastructure.Enums
{
    public enum InternalColor : uint   // it`s standard ANSI colors (values are similar to ones in xterm) with None and Purple added
    {
        None,
        Purple = 0x800080,

        Black = 0x010101,           // so that it has other number than None
        Red = 0xCD0000,
        Green = 0x00CD00,
        Yellow = 0xCDCD00,
        Blue = 0x0000EE,
        Magenta = 0xCD00CD,
        Cyan = 0x00CDCD,
        White = 0xE5E5E5,
        Gray = 0x7E7E7E,
        BrightRed = 0xFF0000,
        BrightGreen = 0x00FF00,
        BrightYellow = 0xFFFF00,
        BrightBlue = 0x5C5CFF,
        BrightMagenta = 0xFF00FF,
        BrightCyan = 0x00FFFF,
        BrightWhite = 0xFFFFFF
    }
}
