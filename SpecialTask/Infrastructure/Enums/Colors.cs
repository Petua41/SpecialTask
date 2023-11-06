using System.Windows.Media;

namespace SpecialTask.Infrastructure
{
    public enum EColor : uint   // it`s standard ANSI colors (values are similar to ones in xterm) with None and Purple added
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

    public static class ColorsController
    {
        private static readonly Dictionary<string, EColor> colorNames = new();
        private static readonly Dictionary<EColor, Color> wpfColors = new();

        static ColorsController()
        {
            foreach (EColor color in Enum.GetValues<EColor>())
            {
                if (color != EColor.None)                                   // we don`t add None to dictionaries for two reasons:
                {                                                           //		so that None won`t appear in ColorsList
                    colorNames.Add(color.ToString().ToLower(), color);      //		None isn`t actually a color. It`s absence of color

                    wpfColors.Add(color, GetColor((uint)color));
                }
            }
        }

        public static Color GetWPFColor(this EColor color)
        {
            try { return wpfColors[color]; }
            catch (KeyNotFoundException) { return Colors.Transparent; }
        }

        public static EColor Parse(string colorString)
        {
            colorString = colorString.Trim().ToLower();
            try { return colorNames[colorString]; }
            catch (KeyNotFoundException) { return EColor.None; }
        }

        public static IReadOnlyList<string> ColorsList => colorNames.Keys.ToList();

        public static (byte, byte, byte) SplitHexValue(uint hexValue)         // it`s private, but I wanna test it
        {
            byte first = (byte)(hexValue >>> 16);
            byte second = (byte)((hexValue & 0x00FF00) >>> 8);
            byte third = (byte)(hexValue & 0x0000FF);
            return (first, second, third);
        }

        private static Color GetColor(uint hexValue)
        {
            (byte, byte, byte) values = SplitHexValue(hexValue);
            return Color.FromRgb(values.Item1, values.Item2, values.Item3);
        }
    }
}
