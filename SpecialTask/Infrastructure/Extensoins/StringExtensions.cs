using SpecialTask.Infrastructure.Collections;
using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Infrastructure.Extensoins
{
    public static class StringExtensions
    {
        private const EColor defaultColor = EColor.White;

        // This method is TOO LONG
        public static Pairs<string, EColor> SplitByColors(this string message)
        {
            Pairs<string, EColor> messageSplittedByColors = new();

            EColor lastColor = defaultColor;
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
                        lastColor = ColorsController.Parse(colorName);
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
    }
}
