using SpecialTask.Infrastructure.Enums;
using System.Windows.Media;

namespace SpecialTask.Infrastructure.Extensoins
{
    public static class InternalColorExtensions
    {
        private static readonly Dictionary<InternalColor, Color> wpfColors = new();

        static InternalColorExtensions()
        {
            foreach (InternalColor color in Enum.GetValues<InternalColor>())
            {
                if (color != InternalColor.None)                    // we don`t add None to dictionaries for two reasons:
                {                                                   //		so that None won`t appear in ColorsList
                    wpfColors.Add(color, GetColor((uint)color));    //		None isn`t actually a color. It`s absence of color
                }
            }
        }

        public static Color GetWPFColor(this InternalColor color)
        {
            return wpfColors.TryGetValue(color, out Color result) ? result : Colors.Transparent;
        }

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
