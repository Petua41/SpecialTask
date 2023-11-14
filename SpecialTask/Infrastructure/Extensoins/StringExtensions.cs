using SpecialTask.Infrastructure.Collections;
using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Infrastructure.Extensoins
{
    public static class StringExtensions
    {
        private const InternalColor defaultColor = InternalColor.White; 

        private static readonly Dictionary<string, ArgumentType> stringToType = new();
        private static readonly Dictionary<string, InternalColor> colorNames = new();

        static StringExtensions()
        {
            // init ArgumentTypes:
            foreach (ArgumentType type in Enum.GetValues<ArgumentType>())
            {
                stringToType.Add(type.ToString().ToLower(), type);
            }

            // init InternalColors:
            foreach (InternalColor color in Enum.GetValues<InternalColor>())
            {
                if (color != InternalColor.None)                                   // we don`t add None to dictionaries for two reasons:
                {                                                           //		so that None won`t appear in ColorsList
                    colorNames.Add(color.ToString().ToLower(), color);      //		None isn`t actually a color. It`s absence of color
                }
            }
        }

        // This method is TOO LONG
        public static Pairs<string, InternalColor> SplitByColors(this string message)
        {
            Pairs<string, InternalColor> messageSplittedByColors = new();

            InternalColor lastColor = defaultColor;
            do
            {
                int indexOfNextColorChange = message.IndexOf("[color");

                if (indexOfNextColorChange == -1)
                {
                    messageSplittedByColors.Add(message, lastColor);
                    message = string.Empty;
                }
                else if (indexOfNextColorChange == 0)
                {
                    int endOfColorSequence = message.IndexOf("]");
                    string colorSequence = message[..(endOfColorSequence + 1)];
                    if (colorSequence == "[color]")
                    {
                        lastColor = defaultColor;
                    }
                    else
                    {
                        string colorName = colorSequence[7..^1];
                        lastColor = colorName.ParseColor();
                    }
                    message = message[(endOfColorSequence + 1)..];
                }
                else
                {
                    string currentPartOfMessage = message[..indexOfNextColorChange];
                    message = message[indexOfNextColorChange..];
                    messageSplittedByColors.Add(currentPartOfMessage, lastColor);
                }
            } while (message.Length > 0);

            return messageSplittedByColors;
        }

        public static string[] SplitInsensitive(this string str, char separator)
        {
            return str.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public static (string, string) SplitToCommandAndArgs(this string input)
        {
            int indexOfFirstMinus = input.IndexOf('-');

            string commandName;
            string arguments;

            if (indexOfFirstMinus > 0)
            {
                if (indexOfFirstMinus == 0)
                {
                    return (string.Empty, input);     // input starts with minus: there is no command
                }

                commandName = input[..(indexOfFirstMinus - 1)];
                arguments = input[indexOfFirstMinus..];
            }
            else
            {
                commandName = input;
                arguments = string.Empty;
            }

            return (commandName, arguments);
        }

        public static ArgumentType ParseArgumentType(this string? str)
        {
            if (str is not null && stringToType.TryGetValue(str, out ArgumentType type)) return type;
            return ArgumentType.PseudoBool;     // all that cannot be recognized is PseudoBool
        }

        public static InternalColor ParseColor(this string colorString)
        {
            colorString = colorString.Trim().ToLower();

            if (colorNames.TryGetValue(colorString, out InternalColor result)) return result;
            else return InternalColor.None;
        }

        public static StreakTexture ParseStreakTexture(this string textureName)
        {
            return textureName.ToLower() switch
            {
                "solid" or "solidcolor" or "color" or "sc" => StreakTexture.SolidColor,
                "horizontallines" or "hl" => StreakTexture.HorizontalLines,
                "verticallines" or "vl" => StreakTexture.VerticalLines,
                "horizontaltransparencygradient" or "htg" or "horizontaltransparenttocolorgradient" => StreakTexture.HorizontalTransparentToColorGradient,
                "horizontalrainbow" or "rainbow" or "hrb" => StreakTexture.HorizontalRainbow,
                "radialtarnsparencygradient" or "rtg" or "radialcolortotransparentgradient" => StreakTexture.RadialColorToTransparentGradient,
                "watertexture" or "water" or "wt" => StreakTexture.Water,
                "dots" => StreakTexture.Dots,
                "holes" or "tc" or "transparentcircles" => StreakTexture.TransparentCircles,
                _ => StreakTexture.None
            };
        }
    }
}
