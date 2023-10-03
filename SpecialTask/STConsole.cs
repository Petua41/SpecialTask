﻿using System.Collections.Generic;
using System.Linq;

namespace SpecialTask
{
    enum EConsoleCommandArgumentTypes { Int, Color, PseudoBool, String, Texture }

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
                        try { lastColor = ColorsController.GetColorFromString(colorName); }
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
                    string currentPartOfMessage = message[..(indexOfNextColorChange - 1)];
                    message = message[indexOfNextColorChange..];
                    messageSplittedByColors.Add(currentPartOfMessage, lastColor);
                }
            } while (message.Length > 0);

            return messageSplittedByColors;
        }

        public void DisplayCommandParsingError(string message)
        {
            Display("[color:red]" + message + "\n");
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
