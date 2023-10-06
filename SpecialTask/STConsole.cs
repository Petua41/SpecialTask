using System.Collections.Generic;
using System.Linq;

namespace SpecialTask
{
    enum EArgumentType { Int, Color, PseudoBool, String, Texture }

    static class ArgumentTypesConstroller
    {
        public static EArgumentType GetArgumentTypeFromString(string str)
        {
            return str.ToLower() switch
            {
                "int" => EArgumentType.Int,
                "color" => EArgumentType.Color,
                "string" => EArgumentType.String,
                "texture" => EArgumentType.Texture,
                _ => EArgumentType.PseudoBool
            };
        }

        public static object ParseValue(this EArgumentType type, string value)
        {
            return type switch
            {
                EArgumentType.Int => int.Parse(value),
                EArgumentType.Color => ColorsController.Parse(value),
                EArgumentType.String => value,
                EArgumentType.Texture => TextureController.Parse(value),
                _ => value != "false"                   // all true, that not false
            };
        }
    }

    /// <summary>
    /// Бизнес-класс консоли. Выступает посредником между классом-обёрткой WPFConsole и остальной бизнес-логикой приложения
    /// </summary>
    public class STConsole
    {
        private static STConsole? singleton;
        private const EColor defaultColor = EColor.White;

        private STConsole()
        {
            if (singleton != null) throw new SingletonError();
        }

        public static STConsole Instance
        {
            get
            {
                singleton ??= new STConsole();
                return singleton;
            }
        }

        public void DisplayGlobalHelp()
        {
            Display(CommandsParser.globalHelp);
        }

        public void Display(string message)
        {
            MyMap<string, EColor> messageSplittedByColors = SplitMessageByColors(message);
            foreach (KeyValuePair<string, EColor> kvp in messageSplittedByColors)
            {
                WPFConsole.Instance.Display(kvp.Key, kvp.Value);
            }
        }

        public void ProcessInput(string inputString)
        {
            CommandsParser.ParseCommand(inputString);
        }

        public string Autocomplete(string inputString)
        {
            /* сначала обращаемся к CommandsParser, чтобы понять, что введено
			{
				ничего => ""
				часть команды => обращаемся к CommandsParser за остатком команды
				команда целиком => CommandsParser передаёт запрос дальше в ConsoleCommand
				{
					ничего => ""
					часть аргумента => ConsoleCommand возвращает остаток аргумента
					аргумент целиком => зависит от типа аргумента
					{
						PseudoBool => возвращаем пустую строку
						Остальное (есть значение по-умолчанию) => возвращаем значение по-умолчанию
						Остальное (нет значения по-умолчанию) => возвращаем пустую строку
					}
				}
			}

			Это всё не здесь. Это всё передаётся по цепочке обязанностей. Здесь только передаём запрос в CommandsParser */

            return CommandsParser.Autocomplete(inputString);
        }

        public static MyMap<string, EColor> SplitMessageByColors(string message)        // This must be private, but I wanna test it
        {
            MyMap<string, EColor> messageSplittedByColors = new();

            EColor lastColor = defaultColor;
            do
            {
                int indexOfNextColorChange = message.IndexOf("[color");
                if (indexOfNextColorChange == -1)
                {
                    messageSplittedByColors.Add(message, lastColor);
                    message = "";
                }
                else if (indexOfNextColorChange == 0)
                {
                    int endOfColorSequence = message.IndexOf("]");
                    string colorSequence = message[..(endOfColorSequence + 1)];
                    if (colorSequence == "[color]") lastColor = defaultColor;
                    else
                    {
                        string colorName = colorSequence[7..^1];
                        try { lastColor = ColorsController.Parse(colorName); }
                        catch (ColorExcepttion)
                        {
                            Logger.Instance.Error(string.Format("Invalid color name in escape sequence: {0}", colorName));
                            throw new EscapeSequenceParsingError();
                        }
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

        public void DisplayError(string message)
        {
            Display("[color:red]" + message + "\n");
        }

        /// <summary>
        /// Displays message in yellow and new line symbol
        /// </summary>
        /// <param name="message"></param>
        public void DisplayWarning(string message)
        {
            Display("[color:Yellow]" + message + "\n");
        }

        /// <summary>
        /// Displays message in yellow and space
        /// </summary>
        /// <param name="message"></param>
        public void DisplayWarningWithoutSlashN(string message)
        {
            Display("[color:Yellow]" + message + " ");
        }

        public string TransferredString => WPFConsole.Instance.GetTransferredString();

        public char? TransferredChar => WPFConsole.Instance.GetTransferredChar();

        public ESpecialKeyCombinations TransferredCombination => WPFConsole.Instance.GetTransferredCombination();

        public bool TransferringInput
        {
            get => WPFConsole.Instance.TransferringInput;
            set => WPFConsole.Instance.TransferringInput = value;
        }

        public bool InputBlocked
        {
            get => WPFConsole.Instance.InputBlocked;
            set => WPFConsole.Instance.InputBlocked = value;
        }

        public void DisplayPrompt()
        {
            WPFConsole.Instance.DisplayPrompt();
        }
    }

    /// <summary>
    /// Представляет упорядоченную коллекцию пар "ключ-значение", не требующую уникальность ключей
    /// </summary>
    public class MyMap<K, V> : List<KeyValuePair<K, V>>
    {
        private readonly List<KeyValuePair<K, V>> map;

        public MyMap()
        {
            map = new();
        }

        public MyMap(IEnumerable<KeyValuePair<K, V>> oldMap) : this()
        {
            foreach (KeyValuePair<K, V> pair in oldMap) Add(pair.Key, pair.Value);
        }

        public void Add(K key, V value)
        {
            map.Add(new KeyValuePair<K, V>(key, value));
        }

        public new void RemoveAt(int index)
        {
            map.RemoveAt(index);
        }

        public new int Count => map.Count;

        public new int Capacity => map.Capacity;

        public new KeyValuePair<K, V> this[int index]
        {
            get => map[index];
            set => map[index] = value;
        }

        public new IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return map.GetEnumerator();
        }

        public List<K> Keys
        {
            get => (from kvp in map select kvp.Key).ToList();
        }

        public List<V> Values
        {
            get => (from kvp in map select kvp.Value).ToList();
        }

        public override bool Equals(object? obj)
        {
            if (obj is MyMap<K, V> otherMyMap)
            {
                return Keys == otherMyMap.Keys && Values == otherMyMap.Values;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return map.GetHashCode();
        }

        public static bool operator ==(MyMap<K, V> a, object? b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MyMap<K, V> a, object? b)
        {
            return !(a == b);
        }
    }
}
