using Aspose.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace SpecialTask
{
    public enum EArgumentType { Int, Color, PseudoBool, String, Texture, Points }

    public static class ArgumentTypesConstroller
    {
		private static readonly Dictionary<string, EArgumentType> stringToType = new();

		static ArgumentTypesConstroller()
		{
			foreach (EArgumentType type in Enum.GetValues<EArgumentType>()) stringToType.Add(type.ToString().ToLower(), type);
		}

        public static EArgumentType ParseType(string? str)
        {
			if (str == null) return EArgumentType.PseudoBool;

            try { return stringToType[str.ToLower()]; }
			catch (KeyNotFoundException) { return EArgumentType.PseudoBool; }		// all that cannot be recognized is bool
        }

        public static object ParseValue(this EArgumentType type, string value)
        {
            return type switch
            {
                EArgumentType.Int		=>	int.Parse(value),
                EArgumentType.Color		=>	ColorsController.Parse(value),
                EArgumentType.String	=>	value,
                EArgumentType.Texture	=>	TextureController.Parse(value),
				EArgumentType.Points	=>	value.ParsePoints(),
                _						=>	value != "false"                   // all true, that not false
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
					else if (possibleArgs.Count > 1 && lastArgument.Length > 0)			// several corresponding arguments
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
				return "";
			}
		}

        public readonly (string, object) CreateArgumentFromString(string argument)
        {
            argument = argument.Trim();         // ParseArguments don`t trim arguments, so we`ll make it here

            if (argument == "-h" || argument == "--help") return ("help", true);

            foreach (ConsoleCommandArgument arg in arguments)
            {
                if (argument.StartsWith(arg.longArgument) || argument.StartsWith(arg.shortArgument))
                {
                    string rawValue = argument.Replace(arg.longArgument, "").Replace(arg.shortArgument, "").Trim();

                    try
                    {
                        object value = arg.type.ParseValue(rawValue);
                        string paramName = arg.commandParameterName;
                        return (paramName, value);
                    }
                    catch (FormatException)     // Error casting string to int or Points. Other types of parameters have default values (EColor.None, etc.)
                    {
						// YANDERE
                        string intOrPoints = arg.type == EArgumentType.Int ? "integer" : "points";
                        MiddleConsole.HighConsole.DisplayError(
                            $"{arg.longArgument} should be {intOrPoints}. {rawValue} is not {intOrPoints}. Try {neededUserInput} --help");

                        throw new ArgumentParsingError();
                    }
                }
            }
            MiddleConsole.HighConsole.DisplayError($"Unknown argument: {argument}. Try {neededUserInput} -- help");
            throw new ArgumentParsingError();
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
			return (from arg in arguments where arg.longArgument.StartsWith(argument) select arg).ToList();
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
	}

	/// <summary>
	/// Processes commands from <see cref="ILowConsole"/>.
	/// Gives <see cref="IHighConsole"/> and <see cref="ILowConsole"/> necessary information about availible commands
	/// </summary>
	static class CommandsParser
	{
		const string XML_WITH_COMMANDS = @"ConsoleCommands.xml";

		static readonly List<ConsoleCommand> consoleCommands = new();
		public static string globalHelp = "[color:yellow]Global help not found![color]";
		private static readonly string projectDir = "";

		static CommandsParser()
		{
			DirectoryInfo? workingDir = Directory.GetParent(Environment.CurrentDirectory);

			if (workingDir == null || (workingDir.Name != "Debug" && workingDir.Name != "Release")) projectDir = Directory.GetCurrentDirectory();
			else
			{
				DirectoryInfo? binDir = workingDir.Parent;
				if (binDir == null)
				{
					LogThatWeAreInRootDirAndExit();
					return;
				}

				projectDir = binDir.Parent?.FullName ?? "";
				if (projectDir.Length == 0)
				{
					LogThatWeAreInRootDirAndExit();
					return;
				}
			}

			try { consoleCommands = ParseCommandsXML(); }
			catch (InvalidResourceFileException)
			{
				Logger.Instance.Fatal("Invalid XML file with commands! exitting...");
			}
		}

		// This method is TOO LONG
		public static void ParseCommand(string userInput)
		{
			int indexOfFirstMinus = userInput.IndexOf('-');

			string commandName;
			string arguments;

			if (indexOfFirstMinus > 0)
			{
				if (indexOfFirstMinus == 0) return;		// input starts with minus: there is no command
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

				if (argumentValues.ContainsKey("help"))		// in theory, user can enter "--help false". It`s still --help
				{
					DisplayHelp(consoleCommand);
                    return;
				}

				ICommand command = CreateCommand(consoleCommand, argumentValues);

				if (consoleCommand.supportsUndo) CommandsFacade.RegisterAndExecute(command);
				else CommandsFacade.ExecuteButDontRegister(command);
			}
			catch (CallOfFictionalCommandException) 
			{
                Logger.Instance.Warning($"Call of the fictional command {consoleCommand.neededUserInput}");
                MiddleConsole.HighConsole.DisplayError(
                    $"You cannot call {consoleCommand.neededUserInput} without \"second-level command\". Try {consoleCommand.neededUserInput} --help");
            }
			catch (ArgumentParsingError) { /* ignore */ }	// Some required argument is missing || Some extra argument is present || Error casting parameter
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
			return "";		// empty input => nothing happened
		}

		private static void LogThatWeAreInRootDirAndExit()
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

		private static List<ConsoleCommand> ParseCommandsXML()
		{
			XDocument commandsFile = XDocument.Load(projectDir + "\\" + XML_WITH_COMMANDS);
			XElement xmlRoot = commandsFile.Root ?? throw new InvalidResourceFileException();

			List<ConsoleCommand> commands = new();

			foreach (XElement elem in xmlRoot.Elements())
			{
				if (elem.Name == "command") { commands.Add(ParseCommandElement(elem)); }
				else if (elem.Name == "help")
				{
					string helpCandidate = elem.Value;
					if (helpCandidate != "") globalHelp = helpCandidate + "\n";
				}
				else Logger.Instance.Warning($"Unexpected XML tag inside the root element: {elem.Name}");
			}

			return commands;
		}

		private static ConsoleCommand ParseCommandElement(XElement elem)
		{
			List<ConsoleCommandArgument> arguments = new();

			string neededUserInput = elem.Attribute("userInput")?.Value ?? "";
            
			bool fictional = (elem.Attribute("fictional")?.Value ?? "false") != "false";

            Type? commandType = Type.GetType("SpecialTask." + elem.Attribute("commandClass")?.Value);
            if (!fictional && commandType == null) throw new InvalidResourceFileException();

            string? help = elem.Value;
			if (help == "") help = null;

			foreach (XElement child in elem.Elements())
			{
				if (child.Name == "argument") arguments.Add(ParseArgumentElement(child));
				else Logger.Instance.Warning($"Unexpected XML tag inside {neededUserInput} command: {child.Name}");
			}

			return new ConsoleCommand
			{
				neededUserInput = neededUserInput,
				help = help,
				commandType = commandType,
				supportsUndo = (elem.Attribute("supportsUndo")?.Value ?? "false") != "false",
				fictional = fictional,
				arguments = arguments
			};
		}

		private static ConsoleCommandArgument ParseArgumentElement(XElement elem)
		{
            EArgumentType type = ArgumentTypesConstroller.ParseType(elem.Attribute("type")?.Value);

            object? defaultValue = null;
            string? defValueString = elem.Attribute("defaultValue")?.Value;
			if (defValueString != null) defaultValue = type.ParseValue(defValueString);		// we leave defaultValue null, if there`s no such attribute

            return new ConsoleCommandArgument
			{
				shortArgument = elem.Attribute("shortArgument")?.Value ?? "",
				longArgument = elem.Attribute("longArgument")?.Value ?? "",
                type = type,
				isNecessary = (elem.Attribute("isNecessary")?.Value ?? "false") != "false",
				commandParameterName = elem.Attribute("commandParameterName")?.Value ?? "",
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
			if (consoleCommand.fictional) throw new CallOfFictionalCommandException();

			// Check that all necessary arguments are present
			foreach(ConsoleCommandArgument argument in consoleCommand.arguments)
			{
				if (!arguments.ContainsKey(argument.commandParameterName))
				{
					if (argument.isNecessary)
					{
						MiddleConsole.HighConsole.DisplayError($"Missing required argument {argument.longArgument}. Try {consoleCommand.neededUserInput} --help");
						throw new ArgumentParsingError();
					}
					else if (argument.defaultValue != null)
					{
						arguments.Add(argument.commandParameterName, argument.defaultValue);
					}
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
				int startOfNextArgument = arguments.IndexOf('-', 2);

				(string, object) pair;
				string arg;
                if (startOfNextArgument > 0)
				{
					arg = arguments[..startOfNextArgument];
					arguments = arguments[startOfNextArgument..];
				}
				else
				{
					arg = arguments;
					arguments = "";
                }
                pair = consoleCommand.CreateArgumentFromString(arg);

                try { argumentPairs.Add(pair.Item1, pair.Item2); }
				catch (ArgumentException)
				{
					MiddleConsole.HighConsole.DisplayError($"Duplicated argument: {pair.Item1}");
				}
			}

			return argumentPairs;
		}

		private static void DisplayHelp(ConsoleCommand command)
		{
			string? help = command.help;
			if (help == null) MiddleConsole.HighConsole.DisplayError($"Help for {command.neededUserInput} not found");
			else MiddleConsole.HighConsole.Display(help);
		}
	}

	/// <summary>
	/// Provides some extensions to <see cref="IList{T}"/> of <see cref="string"/>s
	/// </summary>
	static class ListOfStringsOperations
	{
		/// <summary>
		/// Length of the shortest string in the <paramref name="collection"/>
		/// </summary>
		public static int ShortestLength(this IList<string> collection)
		{
			return (from str in collection select str.Length).Min();
		}

		/// <summary>
		/// The longest common prefix of all strings in <paramref name="collection"/>
		/// </summary>
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

	public struct Point
	{
		public int X;
		public int Y;

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}

		public static Point operator +(Point a, Point b)
		{
			return new(a.X + b.X, a.Y + b.Y);
		}

		public static explicit operator (int, int)(Point p) => (p.X, p.Y);

		public static explicit operator System.Windows.Point(Point p) => new(p.X, p.Y);
	}

	public static class Points
	{
		public static string PointsToString(this List<Point> points)
		{
            return string.Join(", ", from p in points select $"{p.X} {p.Y}");
        }

		public static List<Point> ParsePoints(this string value)
		{
            List<string[]> prePoints = (from prePreP 
									in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    select prePreP.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList();

            return (from preP in prePoints select new Point(int.Parse(preP[0]), int.Parse(preP[1]))).ToList();
        }

		public static Point Center(this List<Point> points)
		{
			int x = (int)(from p in points select p.X).Average();
			int y = (int)(from p in points select p.Y).Average();
			return new Point(x, y);
		}
	}
}
