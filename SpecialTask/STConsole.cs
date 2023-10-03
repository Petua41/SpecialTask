using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;

namespace SpecialTask
{
	enum EConsoleCommandArgumentTypes { Int, Color, PseudoBool, String, Texture }

	/// <summary>
	/// Бизнес-класс консоли. Выступает посредником между классом-обёрткой WPFConsole и остальной бизнес-логикой приложения
	/// </summary>
	public class STConsole
	{
		private static STConsole? singleton;

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
	}

	struct ConsoleCommand
	{
		public string neededUserInput;
		public string? help;
		public Type? commandType;
		public List<ConsoleCommandArgument> argumetns;
		public bool supportsUndo;
		public bool fictional;                                  // Только для help. При вызове печатает что-то типа "недопустимая команда"

		public readonly string AutocompleteArguments(string input)
		{
			if (input.StartsWith(neededUserInput))
			{
				string lastArgument = SelectLastArgument(input);
				if (lastArgument.Length == 0)
				{
					// нет аргументов
					return "";
				}
				else if (IsArgumentFull(lastArgument))
				{
					// аргумент введён целиком
					return ChooseArgument(lastArgument).TryToAutocompleteParameter();
				}
				else
				{
					// аргумент введён не целиком
					List<ConsoleCommandArgument> possibleArgs = TryToAutocompleteArgs(lastArgument);
					if (possibleArgs.Count == 0) return "";		// нет подходящих аргументов
					if (possibleArgs.Count == 1)				// один подходящий аргумент
					{
						if (lastArgument.StartsWith("--")) return possibleArgs.Single().longArgument;
						return possibleArgs.Single().shortArgument;
					}
					return "";									// невозможно однозначно определить (больше одного подходящего аргумента)
				}

			}
			else
			{
				Logger.Instance.Error("Query passed to ConsoleCommand, but input doesn`t contain this command");
				throw new ChainOfResponsibilityException();
			}
		}

		private readonly string SelectLastArgument(string input)
		{
			int indexOfLastSingleMinus = input.LastIndexOf("-");
			int indexOfLastDoubleMinus = input.LastIndexOf("--");
			if (indexOfLastSingleMinus < 0 && indexOfLastDoubleMinus < 0)
			{
				// нет аргументов
				return "";
			}
			if (indexOfLastSingleMinus + 1 > indexOfLastDoubleMinus)
			{
				// последний аргумент -- короткий
				return input[indexOfLastSingleMinus..];
			}
			return input[indexOfLastDoubleMinus..];
		}

		private readonly List<ConsoleCommandArgument> TryToAutocompleteArgs(string argument)
		{
			return (from arg in argumetns where (arg.shortArgument.StartsWith(argument) || arg.longArgument.StartsWith(argument)) select arg).ToList();
		}

		private readonly bool IsArgumentFull(string argument)
		{
			return (from arg in argumetns where (arg.shortArgument == argument.Trim() || arg.longArgument == argument.Trim()) select arg).Any();
		}

		private readonly ConsoleCommandArgument ChooseArgument(string input)
		{
			try { return (from arg in argumetns where (arg.shortArgument == input.Trim() || arg.longArgument == input.Trim()) select arg).First(); }
			catch (InvalidOperationException)
			{
				Logger.Instance.Error("IsArgumetFull said that argument is full, but it isn`t");
				throw new ChainOfResponsibilityException();
			}
        }
	}

	struct ConsoleCommandArgument
	{
		public string shortArgument;
		public string longArgument;
		public EConsoleCommandArgumentTypes type;
		public bool isNecessary;
		public string commandParameterName;
		public object? defaultValue;

		public readonly string TryToAutocompleteParameter()
		{
			if (type == EConsoleCommandArgumentTypes.PseudoBool) return "";
			if (defaultValue != null) return defaultValue.ToString();
			return "";
		}

        public readonly object ParseParameter(string value)
        {
            return type switch
            {
                EConsoleCommandArgumentTypes.Int => int.Parse(value),
                EConsoleCommandArgumentTypes.Color => ColorsController.GetColorFromString(value),
                EConsoleCommandArgumentTypes.String => value,
                EConsoleCommandArgumentTypes.Texture => TextureController.GetTextureFromString(value),
                _ => value != "false"                   // здесь тоже всё true, что не false
            };
        }
    }

	/// <summary>
	/// Обрабатывает команды, полученные от STConsole, а также сообщает STConsole всю необходимую информацию
	/// </summary>
	static class CommandsParser
	{
		const string XML_WITH_COMMANDS = @"ConsoleCommands.xml";

        static List<ConsoleCommand> consoleCommands = new();
		public static string globalHelp = "[color:yellow]Global help not found![color]";
		private static string projectDir;

		static CommandsParser()
		{
			DirectoryInfo? workingDir = Directory.GetParent(Environment.CurrentDirectory);      // GetParent на самом деле получает не Parent, а саму директорию
			if (workingDir == null) LogThatWeAreInRootDir();

			DirectoryInfo? binDir = workingDir.Parent;
			if (binDir == null) LogThatWeAreInRootDir();

			projectDir = binDir.Parent.FullName;

			try { (consoleCommands, globalHelp) = ParseCommandsXML(); }
			catch (CannotFindResourceFileException)
			{
				Logger.Instance.Fatal("Cannot get XML file with commands! exitting...");
			}
			catch (InvalidResourceFileException)
			{
				Logger.Instance.Fatal("Invalid XML file with commands! exitting...");
			}
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
			if (commandNumber < 0)                        // Если команда не найдена, выводим глобальную помощь
			{
				STConsole.Instance.DisplayGlobalHelp();
				return;
			}

			ConsoleCommand consoleCommand = consoleCommands[commandNumber];

			Dictionary<string, object> argumentValues = ParseArguments(consoleCommand, arguments);
			ICommand command = CreateCommand(consoleCommand, argumentValues);

			if (consoleCommand.supportsUndo) CommandsFacade.RegisterAndExecute(command);
			else CommandsFacade.ExecuteButDontRegister(command);
		}

		public static string Autocomplete(string input)
		{
			if (input.Length == 0)
			{
				return "";
			}
			else if (!input.Contains(' '))
			{
				if (IsThereACompeteCommand(input))
				{
					ConsoleCommand command = consoleCommands[SelectCommand(input)];
					return command.AutocompleteArguments(input);
				}
				else
				{
					List<ConsoleCommand> possibleCommands = GetListOfCommandsToComplete(input);
					if (possibleCommands.Count == 0) return "";
					else if (possibleCommands.Count == 1)
					{
						string shouldBe = possibleCommands.Single().neededUserInput;
						return shouldBe.Replace(input, "");
					}
					else
					{
						return "";
					}
				}
			}

			throw new NotImplementedException();
		}

		private static void LogThatWeAreInRootDir()
		{
			Logger.Instance.Warning("Application is running in the root directory!");
			Logger.Instance.Error("Cannot get current project directory");
            Logger.Instance.Fatal("Cannot get XML file with commands! exitting...");
        }

		private static bool IsThereACompeteCommand(string input)
		{
			if (SelectCommand(input) > 0) return true;
			return false;
		}

		private static List<ConsoleCommand> GetListOfCommandsToComplete(string input)
		{
			return (from command in consoleCommands where command.neededUserInput.StartsWith(input) select command).ToList();
		}

		private static (List<ConsoleCommand>, string) ParseCommandsXML()
		{
			XDocument commandsFile = XDocument.Load(projectDir + "\\" + XML_WITH_COMMANDS);
			XElement xmlRoot = commandsFile.Root ?? throw new CannotFindResourceFileException();

			List<ConsoleCommand> commands = new();

			foreach (XElement elem in xmlRoot.Elements())
			{
				if (elem.Name == "command") { commands.Add(ParseCommandElement(elem)); }
				else if (elem.Name == "help")
				{
					string helpCandidate = elem.Value;
					if (helpCandidate != "") globalHelp = helpCandidate + "\n";
				}
				else Logger.Instance.Warning(string.Format("Unexpected XML tag in root element: {0}", elem.Name));
			}

			return (commands, globalHelp);
		}

		private static ConsoleCommand ParseCommandElement(XElement elem)
		{
			string neededUserInput = "";
			string? help;
			Type? commandType = typeof(ICommand);
			List<ConsoleCommandArgument> arguments = new();
			bool supportsUndo = false;
			bool fictional = false;

			XAttribute[] attrs = elem.Attributes().ToArray();
			foreach (XAttribute attr in attrs)
			{
				string attrName = attr.Name.LocalName;
				switch (attrName)
				{
					case "userInput":
						neededUserInput = attr.Value;
						break;
					case "commandClass":
						commandType = Type.GetType("SpecialTask." + attr.Value);
                        if (commandType == null) throw new InvalidResourceFileException();
						break;
					case "supportsUndo":
                        supportsUndo = attr.Value != "false";		// считаем, что всё true, что не false
						break;
					case "fictional":
						fictional = attr.Value != "false";
						break;
					default:
						Logger.Instance.Warning(string.Format("Unknown attribute {0} in command", attrName));
						break;
                }
			}
			
			help = elem.Value;
			if (help == "") help = null;
			else help += "\n";
			
			foreach (XElement child in elem.Elements())
			{
				if (child.Name == "argument") arguments.Add(ParseArgumentElement(child));
				else Logger.Instance.Warning(string.Format("Unexpected XML tag inside {0} command: {1}", neededUserInput, child.Name));
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
			string shortArgument = "";
			string longArgument = "";
			EConsoleCommandArgumentTypes type = EConsoleCommandArgumentTypes.PseudoBool;
			bool isNecessary = false;
			string commandParameterName = "";
			object? defaultValue = null;

			XAttribute[] attrs = elem.Attributes().ToArray();

			foreach (XAttribute attr in attrs)
			{
				string attrName = attr.Name.LocalName;
				switch (attrName)
				{
					case "shortArgument":
						shortArgument = attr.Value;
						break;
					case "longArgument":
						longArgument = attr.Value;
						break;
					case "type":
						type = GetArgumentTypeFromString(attr.Value);
						break;
					case "isNecessary":
						isNecessary = attr.Value != "false";
						break;
					case "commandParameterName":
						commandParameterName = attr.Value;
						break;
					case "defaultValue":
						defaultValue = ParseDefaultValue(attr.Value, type);
						break;
					default:
						Logger.Instance.Warning(string.Format("Unknown attribute {0} in arguments", attr.Name));
						break;
				}
			}

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
				"Texture" => EConsoleCommandArgumentTypes.Texture,
				_ => EConsoleCommandArgumentTypes.PseudoBool
			};
		}

		private static object ParseDefaultValue(string value, EConsoleCommandArgumentTypes type)
		{
			return type switch
			{
				EConsoleCommandArgumentTypes.Int => int.Parse(value),
				EConsoleCommandArgumentTypes.Color => ColorsController.GetColorFromString(value),
				EConsoleCommandArgumentTypes.String => value,
				EConsoleCommandArgumentTypes.Texture => TextureController.GetTextureFromString(value),
				_ => value != "false"					// здесь тоже всё true, что не false
			};
		}

		/// <summary>
		/// Находит команду по введённой пользователем строке
		/// </summary>
		/// <param name="commandName">Часть введённой строки до аргументов</param>
		/// <returns>Индекс в списке команд или -1, если команда не найдена</returns>
		private static int SelectCommand(string commandName)
		{
			for (int i = 0; i < consoleCommands.Count; i++)
			{
				if (consoleCommands[i].neededUserInput == commandName) return i;
			}
			return -1;
		}

		private static ICommand CreateCommand(ConsoleCommand consoleCommand, Dictionary<string, object> arguments)
		{
			// check that all necessary arguments are present
			List<bool> arePresent = (from arg in consoleCommand.argumetns select false).ToList();
			for (int i = 0; i < consoleCommand.argumetns.Count; i++)
			{
				ConsoleCommandArgument argument = consoleCommand.argumetns[i];
				bool isPresent = arguments.ContainsKey(argument.commandParameterName);
				if (isPresent || !argument.isNecessary) arePresent[i] = true;
			}
			if (arePresent.Any(x => !x)) throw new ArgumentParsingError();		// не хватает какого-то обязательного аргумента


		}

		private static Dictionary<string, object> ParseArguments(ConsoleCommand consoleCommand, string arguments)
		{
			Dictionary<string, object> argumentPairs = new();

			while (arguments.Length > 0)
			{
				int startOfNextArgument = arguments.IndexOf('-', 2);
				if (startOfNextArgument > 0)
				{
					string arg = arguments[..startOfNextArgument];
					arguments = arguments[startOfNextArgument..];
					KeyValuePair<string, object> kvp = CreateArgumentFromString(arg, consoleCommand);
					argumentPairs.Add(kvp.Key, kvp.Value);
				}
			}

			return argumentPairs;
		}

		private static KeyValuePair<string, object> CreateArgumentFromString(string argument, ConsoleCommand command)
		{
			foreach (ConsoleCommandArgument arg in command.argumetns)
			{
				if (argument.StartsWith(arg.longArgument))
				{
					string rawValue = argument.Replace(arg.longArgument, "").Trim();
					object value = arg.ParseParameter(rawValue);
					string paramName = arg.commandParameterName;
                    return new(paramName, value);
                }
			}
			throw new ArgumentParsingError();
		}

		private static string GetHelp(int indexInList)
		{
			return consoleCommands[indexInList].help;
		}
	}

	/// <summary>
	/// Представляет упорядоченную коллекцию пар "ключ-значение", не требующую уникальность ключей
	/// </summary>
	public class MyMap<K, V> : List<KeyValuePair<K, V>>
	{
		private List<KeyValuePair<K, V>> map;

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
