using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

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
    struct ConsoleCommand
	{
		public string neededUserInput;
		public string? help;
		public Type? commandType;
		public List<ConsoleCommandArgument> arguments;
		public bool supportsUndo;
		public bool fictional;                                  // Только для help. При вызове печатает что-то типа "invalid command"

		public readonly string AutocompleteArguments(string input)
		{
			if (input.StartsWith(neededUserInput))				// check that it`s right command
			{
				string lastArgument = SelectLastLongArgument(input).Trim();

				if (!IsLongArgumentFull(lastArgument))          // the argument is not full
                {
					List<ConsoleCommandArgument> possibleArgs = TryToAutocompleteLongArgs(lastArgument);

					if (possibleArgs.Count == 1) return possibleArgs.Single().longArgument.Replace(lastArgument, "");		// one corresponding argument
					else if (possibleArgs.Count > 1)			// several corresponding arguments
					{
						return (from arg in possibleArgs select arg.longArgument.Replace(lastArgument, "")).ToList().LongestCommonPrefix();
					}
					return "";									// no corresponding arguments
				}
				return "";										// the argument is already full

			}
			else
			{
				Logger.Instance.Error("Query passed to ConsoleCommand, but input doesn`t contain this command");
				throw new ChainOfResponsibilityException();
			}
		}

		private static string SelectLastLongArgument(string input)
		{
			int indexOfLastSingleMinus = input.LastIndexOf("-");
			int indexOfLastDoubleMinus = input.LastIndexOf("--");

			if (indexOfLastSingleMinus > indexOfLastDoubleMinus + 1 || indexOfLastDoubleMinus < 0) return "";

			return input[indexOfLastDoubleMinus..];
		}

		private readonly List<ConsoleCommandArgument> TryToAutocompleteLongArgs(string argument)
		{
			//return (from arg in argumetns where arg.longArgument.StartsWith(argument) select arg).ToList();

			List<ConsoleCommandArgument> result = new();

			foreach (ConsoleCommandArgument arg in arguments)
			{
				if (arg.longArgument.StartsWith(argument)) result.Add(arg);
			}
			return result;
		}

		private readonly bool IsLongArgumentFull(string argument)
		{
			return (from arg in arguments where arg.longArgument == argument.Trim() select arg).Any();
		}
	}

	struct ConsoleCommandArgument
	{
		public string shortArgument;
		public string longArgument;
		public EArgumentType type;
		public bool isNecessary;
		public string commandParameterName;
		public object? defaultValue;

		public readonly object ParseParameter(string value)
		{
			return type switch
			{
				EArgumentType.Int => int.Parse(value),
				EArgumentType.Color => ColorsController.Parse(value),
				EArgumentType.String => value,
				EArgumentType.Texture => TextureController.Parse(value),
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

		static readonly List<ConsoleCommand> consoleCommands = new();
		public static string globalHelp = "[color:yellow]Global help not found![color]";
		private static readonly string projectDir = "";

		static CommandsParser()
		{
			DirectoryInfo? workingDir = Directory.GetParent(Environment.CurrentDirectory);      // GetParent на самом деле получает не Parent, а саму директорию
			if (workingDir == null)
			{
				LogThatWeAreInRootDir();
				return;
			}

			DirectoryInfo? binDir = workingDir.Parent;
			if (binDir == null)
			{
				LogThatWeAreInRootDir();
				return;
			}

			projectDir = binDir.Parent?.FullName ?? "";
			if (projectDir.Length == 0)
			{
				LogThatWeAreInRootDir();
				return;
			}

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

		// TODO: this method is TOO long
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
			if (commandNumber < 0)                        // Если команда не найдена, выводим глобальную помощь (help и ? тоже не будут найдены)
			{
				if (userInput.Length > 0) MiddleConsole.HighConsole.DisplayGlobalHelp();	// if user just pressed enter, don`t help him
				return;
			}

			ConsoleCommand consoleCommand = consoleCommands[commandNumber];

			try
			{
				Dictionary<string, object> argumentValues = ParseArguments(consoleCommand, arguments);

				if (argumentValues.ContainsKey("help") && (bool)argumentValues["help"])
				{
					DisplayHelp(consoleCommand);
                    return;
				}

				ICommand command = CreateCommand(consoleCommand, argumentValues);

				if (consoleCommand.supportsUndo) CommandsFacade.RegisterAndExecute(command);
				else CommandsFacade.ExecuteButDontRegister(command);
			}
			catch (CallOfFictionalCommandException) { }			// Nothing happened
			catch (ArgumentParsingError) { }					// Some required argument is missing || Some extra argument is present || Error casting parameter
		}

		public static string Autocomplete(string input)
		{
			if (input.Length > 0)
			{
				int idxOfCommand = TryToSelectCommand(input);
				if (idxOfCommand >= 0)
				{														// complete command => pass request on
					ConsoleCommand command = consoleCommands[idxOfCommand];
					return command.AutocompleteArguments(input);
				}
				else
				{
					List<ConsoleCommand> possibleCommands = GetListOfCommandsToComplete(input);
					if (possibleCommands.Count == 0) return "";			// no candidates
					else if (possibleCommands.Count == 1)				// one candidate
					{
						string shouldBe = possibleCommands.Single().neededUserInput;
						return shouldBe.Replace(input, "");
					}
					else												// several candidates
                    {
                        return (from command in possibleCommands select command.neededUserInput.Replace(input, "")).ToList().LongestCommonPrefix();
					}
				}
			}

			return "";		// empty input => nothing
		}

		private static void LogThatWeAreInRootDir()
		{
			Logger.Instance.Warning("Application is running in the root directory!");
			Logger.Instance.Error("Cannot get current project directory");
			Logger.Instance.Fatal("Cannot get XML file with commands! exitting...");
		}

		private static int TryToSelectCommand(string input)
		{
			int indexOfFirstMinus = input.IndexOf('-');
			if (indexOfFirstMinus > 0) input = input[..indexOfFirstMinus];
			return SelectCommand(input);
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
						supportsUndo = attr.Value != "false";       // считаем, что всё true, что не false
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
				arguments = arguments
			};
		}

		private static ConsoleCommandArgument ParseArgumentElement(XElement elem)
		{
			string shortArgument = "";
			string longArgument = "";
			EArgumentType type = EArgumentType.PseudoBool;
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
						type = ArgumentTypesConstroller.GetArgumentTypeFromString(attr.Value);
						break;
					case "isNecessary":
						isNecessary = attr.Value != "false";
						break;
					case "commandParameterName":
						commandParameterName = attr.Value;
						break;
					case "defaultValue":
						defaultValue = type.ParseValue(attr.Value);
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

		/// <summary>
		/// Finds ConsoleCommand by user input
		/// </summary>
		/// <param name="commandName">Часть введённой строки до аргументов</param>
		/// <returns>Индекс в списке команд или -1, если команда не найдена</returns>
		private static int SelectCommand(string commandName)
		{
			commandName = commandName.Trim();

			for (int i = 0; i < consoleCommands.Count; i++)
			{
				if (consoleCommands[i].neededUserInput == commandName) return i;
			}
			return -1;
		}

		private static ICommand CreateCommand(ConsoleCommand consoleCommand, Dictionary<string, object> arguments)
		{
			if (consoleCommand.fictional)
			{
				Logger.Instance.Warning(string.Format("Call of the fictional command {0}", consoleCommand.neededUserInput));
				MiddleConsole.HighConsole.DisplayError(string.Format("You cannot call {0} without \"second-level command\". Try {0} --help",
					consoleCommand.neededUserInput));
				throw new CallOfFictionalCommandException();
			}

			for (int i = 0; i < consoleCommand.arguments.Count; i++)
			{
				ConsoleCommandArgument argument = consoleCommand.arguments[i];
				bool isPresent = arguments.ContainsKey(argument.commandParameterName);
				if (!isPresent && argument.isNecessary)
				{
					MiddleConsole.HighConsole.DisplayError(string.Format("Missing required argument {0}. Try {1} --help",
						argument.longArgument, consoleCommand.neededUserInput));
					throw new ArgumentParsingError();
				}
			}

			Type type = consoleCommand.commandType ?? throw new CallOfFictionalCommandException();
			try
			{
				ConstructorInfo constructor = type.GetConstructors().Single();
				return (ICommand)constructor.Invoke(new object[] { arguments });
			}
			catch (InvalidOperationException)
			{
				Logger.Instance.Error("{0} must contain exactly one constructor!");
				throw new InvalidCommandClassException();
			}
			catch (InvalidCastException)
			{
				Logger.Instance.Error("{0} must implement ICommand!");
				throw new InvalidCommandClassException();
			}
		}

		private static Dictionary<string, object> ParseArguments(ConsoleCommand consoleCommand, string arguments)
		{
			Dictionary<string, object> argumentPairs = new();

			while (arguments.Length > 0)
			{
				int startOfNextArgument = arguments.IndexOf('-', 2);		// TODO: this way we cannot parse negative numbers. Maybe we don`t need negative numbers?
				if (startOfNextArgument > 0)
				{
					string arg = arguments[..startOfNextArgument];
					arguments = arguments[startOfNextArgument..];
					KeyValuePair<string, object> kvp = CreateArgumentFromString(arg, consoleCommand);
					argumentPairs.Add(kvp.Key, kvp.Value);
				}
				else
				{
					KeyValuePair<string, object> kvp = CreateArgumentFromString(arguments, consoleCommand);
					arguments = "";
					argumentPairs.Add(kvp.Key, kvp.Value);
				}
			}

			return argumentPairs;
		}

		private static KeyValuePair<string, object> CreateArgumentFromString(string argument, ConsoleCommand command)
		{
			argument = argument.Trim();         // ParseArguments don`t trim arguments, so we`ll make it here

			if (argument == "-h" || argument == "--help") return new("help", true);

			foreach (ConsoleCommandArgument arg in command.arguments)
			{
				if (argument.StartsWith(arg.longArgument) || argument.StartsWith(arg.shortArgument))
				{
					string rawValue = argument.Replace(arg.longArgument, "").Replace(arg.shortArgument, "").Trim();
					try
					{
						object value = arg.ParseParameter(rawValue);
						string paramName = arg.commandParameterName;
						return new(paramName, value);
					}
					catch (FormatException)		// Error casting string to int. Other types of parameters have default values (EColor.None, etc.)
					{
						MiddleConsole.HighConsole.DisplayError(string.Format("{0} should be integer. {1} is not integer. Try {2} --help",
							arg.longArgument, rawValue, command.neededUserInput));
						throw new ArgumentParsingError();
					}
				}
			}
			MiddleConsole.HighConsole.DisplayError(string.Format("Unknown argument: {0}. Try {1} -- help", argument, command.neededUserInput));
			throw new ArgumentParsingError();
		}

		private static void DisplayHelp(ConsoleCommand command)
		{
			string? help = command.help;
			if (help == null) MiddleConsole.HighConsole.DisplayError("Help not found");
			else MiddleConsole.HighConsole.Display(help);
		}
	}

	static class ListOfStringsOperations
	{
		public static int ShortestLength(this IList<string> collection)
		{
			return (from str in collection select str.Length).Min();
		}

		public static string LongestCommonPrefix(this IList<string> collection)
		{
			string lastPrefix = "";
			for (int i = 0; i < collection.ShortestLength(); i++)
			{
				string commonPrefix = collection[0][..(i + 1)];
				foreach (string str in collection)
				{
					if (!str.StartsWith(commonPrefix)) return lastPrefix;
				}
				lastPrefix = commonPrefix;
			}
			return lastPrefix;
		}
	}
}
