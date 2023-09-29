using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SpecialTask
{
	enum EConsoleCommandArgumentTypes { Int, Color, PseudoBool, String, Path, Texture }

	/// <summary>
	/// Бизнес-класс консоли. Принимает запросы от класса-обёртки WPFConsole
	/// </summary>
	public class STConsole
	{
		private static STConsole? singleton;

		private WPFConsole wpfConsole;

		private STConsole()
		{
			if (singleton != null) throw new SingletonError();
			wpfConsole = WPFConsole.Instance;
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
		}

		public void ProcessInput(string inputString)
		{
			CommandsParser.ParseCommand(inputString);
		}

		public MyMap<string, EColor> SplitMessageByColors(string message)		// This must be private, but I wanna test it
		{
			MyMap<string, EColor> messageSplittedByColors = new();

			EColor lastColor = EColor.None;
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
					if (colorSequence == "[color]") lastColor = EColor.None;
					else
					{
						string colorName = colorSequence[7..^1];
						try { lastColor = ColorsController.GetColorFromString(colorName); }
						catch (ColorExcepttion)
						{
							Logger.Error(string.Format("Invalid color name in escape sequence: {0}", colorName));
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
	}

	struct ConsoleCommand
	{
		public string neededUserInput;
		public string? help;
		public Type? commandType;
		public List<ConsoleCommandArgument> argumetns;
		public bool supportsUndo;
		public bool fictional;									// Только для help. При вызове печатает что-то типа "недопустимая команда"
	}

	struct ConsoleCommandArgument
	{
		public string shortArgument;
		public string longArgument;
		public EConsoleCommandArgumentTypes type;
		public bool isNecessary;
		public string commandParameterName;
		public object? defaultValue;
	}

	/// <summary>
	/// Обрабатывает команды, полученные от STConsole, а также сообщает STConsole всю необходимую информацию
	/// </summary>
	static class CommandsParser
	{
		const string XML_WITH_COMMANDS = @"ConsoleCommands.xml";

		static List<ConsoleCommand> consoleCommands;
		public static string globalHelp;

		static CommandsParser()
		{
			(consoleCommands, globalHelp) = ParseCommandsXML();
		}

		public static void ParseCommand(string userInput)
		{
			bool thereAreArguments = true;
			int indexOfFirstMinus = userInput.IndexOf('-');
			if (indexOfFirstMinus == -1) thereAreArguments = false;

			string commandName;
			string arguments;

			if (thereAreArguments)
			{
				commandName = userInput[..(indexOfFirstMinus - 1)];
				arguments = userInput[indexOfFirstMinus..];
			}
			else
			{
				commandName = userInput;
				arguments = "";
			}

			int commandNumber = SelectCommand(commandName);
			if (commandNumber == -1)						// Если команда не найдена, выводим глобальную помощь
			{
				STConsole.Instance.DisplayGlobalHelp();
				return;
			}
			bool supportsUndo = consoleCommands[commandNumber].supportsUndo;
			ICommand command = ParseArguments(commandNumber, arguments);
			if (supportsUndo) CommandsFacade.RegisterAndExecute(command);
			else CommandsFacade.ExecuteButDontRegister(command);
		}

		private static (List<ConsoleCommand>, string) ParseCommandsXML()
		{
			XDocument commandsFile = XDocument.Load(XML_WITH_COMMANDS);
			XElement xmlRoot = commandsFile.Root ?? throw new InvalidResourceFileException();

			List<ConsoleCommand> commands = new();

			string globalHelp = "Global help not found!";

			foreach (XElement elem in xmlRoot.Elements())
			{
				if (elem.Name == "command") { commands.Add(ParseCommandElement(elem)); }
				else if (elem.Name == "help")
				{
					string helpCandidate = elem.Value;
					if (helpCandidate != "") globalHelp = helpCandidate;
				}
				else Logger.Warning(string.Format("Unexpected XML tag in root element: {0}", elem.Name));
			}

			return (commands, globalHelp);
		}

		private static ConsoleCommand ParseCommandElement(XElement elem)
		{
			string neededUserInput;
			string? help;
			Type commandType;
			List<ConsoleCommandArgument> arguments = new();
			bool supportsUndo;
			bool fictional;

			XAttribute[] attrs = elem.Attributes().ToArray();
			neededUserInput = (from attr in attrs where attr.Name == XName.Get("userInput") select attr.Value).Single();
			help = elem.Value;
			if (help == "") help = null;
			commandType = Type.GetType((from attr in attrs where attr.Name == XName.Get("commandClass") select attr.Value).Single()) ?? 
				throw new InvalidResourceFileException();
			supportsUndo = (from attr in attrs where attr.Name == XName.Get("supportsUndo") select (attr.Value == "true")).SingleOrDefault(false);
			fictional = (from attr in attrs where attr.Name == XName.Get("fictional") select (attr.Value == "true")).SingleOrDefault(false);

			foreach (XElement child in elem.Elements())
			{
				if (child.Name == "argument") arguments.Add(ParseArgumentElement(child));
				else Logger.Warning(string.Format("Unexpected XML tag inside {0} command: {1}", neededUserInput, child.Name));
			}

			return new ConsoleCommand
			{
				neededUserInput = neededUserInput,
				help = help,
				commandType = commandType,
				supportsUndo = supportsUndo,
				fictional = fictional,
				argumetns = arguments
			};
		}

		private static ConsoleCommandArgument ParseArgumentElement(XElement elem)
		{
			string shortArgument;
			string longArgument;
			EConsoleCommandArgumentTypes type;
			bool isNecessary;
			string commandParameterName;
			object? defaultValue;

			XAttribute[] attrs = elem.Attributes().ToArray();
			shortArgument = (from attr in attrs where attr.Name == XName.Get("shortArgument") select attr.Value).Single();
			longArgument = (from attr in attrs where attr.Name == XName.Get("longArgument") select attr.Value).SingleOrDefault(shortArgument);
			type = GetArgumentTypeFromString((from attr in attrs where attr.Name == XName.Get("type") select attr.Value).SingleOrDefault(""));
			isNecessary = (from attr in attrs where attr.Name == XName.Get("isNecessary") select (attr.Value == "true")).SingleOrDefault(false);
			commandParameterName = (from attr in attrs where attr.Name == XName.Get("commandParameterName") select attr.Value).Single();
			defaultValue = (from attr in attrs where attr.Name == XName.Get("defaultValue") select attr.Value == "true").SingleOrDefault(null);

			return new ConsoleCommandArgument
			{
				shortArgument = shortArgument,
				longArgument = longArgument,
				type = type,
				isNecessary = isNecessary,
				commandParameterName = commandParameterName,
				defaultValue = defaultValue
			};
		}

		private static EConsoleCommandArgumentTypes GetArgumentTypeFromString(string str)
		{
			return str switch
			{
				"Int" => EConsoleCommandArgumentTypes.Int,
				"Color" => EConsoleCommandArgumentTypes.Color,
				"String" => EConsoleCommandArgumentTypes.String,
				"Path" => EConsoleCommandArgumentTypes.Path,
				"Texture" => EConsoleCommandArgumentTypes.Texture,
				_ => EConsoleCommandArgumentTypes.PseudoBool
			};
		}

		/// <summary>
		/// Находит команду по введённой пользователем строке
		/// </summary>
		/// <param name="commandName">Часть введённой строки до аргументов</param>
		/// <returns>Индекс в списке команд или -1, если команда не найдена</returns>
		private static int SelectCommand(string commandName)	// возвращает индекс в списке
		{
			for (int i = 0; i < consoleCommands.Count; i++)
			{
				if (consoleCommands[i].neededUserInput == commandName) return i;
			}
			return -1;
		}
		
		private static ICommand ParseArguments(int commandNumber, string arguments)
		{
			if (arguments == "") return ParseArguments(commandNumber);
			// TODO

			throw new NotImplementedException();
		}

		private static ICommand ParseArguments(int commandNumber)
		{
			// TODO: здесь надо создавать ICommand

			throw new NotImplementedException();
		}

		private static string GetHelp(int indexInList)
		{
			return consoleCommands[indexInList].help;
		}
	}

	/// <summary>
	/// Представляет упорядоченную коллекцию пар "ключ-значение", не требующую уникальность ключей
	/// </summary>
	public class MyMap<K, V>: List<KeyValuePair<K, V>>
	{
		private List<KeyValuePair<K, V>> map;

		public MyMap()
		{
			map = new();
		}

		public MyMap(IEnumerable<KeyValuePair<K, V>> oldMap): this()
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

		public new int Count
		{
			get { return map.Count; }
		}

		public new KeyValuePair<K, V> this[int index]
		{
			get => map[index];
			set => map[index] = value;
		}

		public List<K> Keys
		{
			get => (from kvp in map select kvp.Key).ToList();
		}

		public List <V> Values
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
