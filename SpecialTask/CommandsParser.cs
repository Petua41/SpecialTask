using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using SpecialTask.Commands;

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
		public string commandType;
		public List<ConsoleCommandArgument> arguments;
		public bool supportsUndo;
		public bool fictional;                                  // Только для help. При вызове печатает что-то типа "invalid command"

		public readonly string AutocompleteArguments(string argumentsInput)
		{
			string lastArgument = SelectLastLongArgument(argumentsInput).Trim();

			return (from arg in arguments where arg.longArgument.StartsWith(lastArgument) 
												 select arg.longArgument).ToList().RemovePrefix(lastArgument).LongestCommonPrefix();
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
					catch (FormatException)     // Error casting string
					{
						string argType = arg.type.ToString();
						MiddleConsole.HighConsole.DisplayError( 
							$"{arg.longArgument} should be {argType}. {rawValue} is not {argType}. Try {neededUserInput} --help");

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
		const string XML_PATH = @"..\ConsoleCommands.xml";
		const string XML_ALT_PATH = @"..\..\..\ConsoleCommands.xml";
		// string xml = Properties.Resources.file_name;

		static readonly List<ConsoleCommand> consoleCommands = new();
		public static string globalHelp = "[color:purple]Global help not found![purple]";

		static CommandsParser()
		{
			try
			{
				if (File.Exists(XML_PATH)) consoleCommands = ParseCommandsXML(XML_PATH);
				else if (File.Exists(XML_ALT_PATH)) consoleCommands = ParseCommandsXML(XML_ALT_PATH);
				else Logger.Instance.Fatal("Cannot find XML file with commands!");
			}
			catch (InvalidResourceFileException)
			{
				Logger.Instance.Fatal("Invalid XML file with commands!");
			}
		}

		// This method is like facade: only calls other methods in the right order and handles exceptions
		public static void ParseCommand(string userInput)
		{
			if (userInput.Length == 0) return;

			(string commandName, string arguments) = userInput.SplitToCommandAndArgs();

			int commandNumber = SelectCommand(commandName);
			// Если команда не найдена, выводим глобальную помощь (help и ? тоже не будут найдены)
			if (commandNumber < 0)
			{
				MiddleConsole.HighConsole.DisplayGlobalHelp();
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
			catch (InvalidOperationException) 
			{
				Logger.Instance.Warning($"Call of the fictional command {consoleCommand.neededUserInput}");
				MiddleConsole.HighConsole.DisplayError(
					$"You cannot call {consoleCommand.neededUserInput} without \"second-level command\". Try {consoleCommand.neededUserInput} --help");
			}
			catch (ArgumentParsingError) { /* ignore */ }	// Some required argument is missing || Some extra argument is present || Error casting parameter
		}

		public static string Autocomplete(string input)
		{
			if (input.Length == 0) return "";       // empty input => nothing happened

            (string commandName, string argumentsStr) = input.SplitToCommandAndArgs();


            int idxOfCommand = SelectCommand(commandName);
			if (idxOfCommand >= 0)
			{														// complete command => pass request on
				return consoleCommands[idxOfCommand].AutocompleteArguments(argumentsStr);
			}
			else
			{
				return (from comm in consoleCommands where comm.neededUserInput.StartsWith(input) 
						select comm.neededUserInput).ToList().RemovePrefix(input).LongestCommonPrefix();
			}
		}

		private static List<ConsoleCommand> ParseCommandsXML(string xmlWithCommands)
		{
			XDocument commandsFile = XDocument.Load(xmlWithCommands);
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
				commandType = elem.Attribute("commandClass")?.Value.Replace("Command", "") ?? "",
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
			return consoleCommands.FindIndex(t => t.neededUserInput == commandName);		// ТАК НАДО ВЕЗДЕ		!!!!!!!!!!!!!!!!
		}

		private static ICommand CreateCommand(ConsoleCommand consoleCommand, Dictionary<string, object> arguments)
		{
			if (consoleCommand.fictional) throw new InvalidOperationException();

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

			return CommandCreator.CreateCommand(consoleCommand.commandType, arguments);
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

		private static (string, string) SplitToCommandAndArgs(this string input)
		{
			int indexOfFirstMinus = input.IndexOf('-');

			string commandName;
			string arguments;

			if (indexOfFirstMinus > 0)
			{
				if (indexOfFirstMinus == 0) return ("", "");     // input starts with minus: there is no command
				commandName = input[..(indexOfFirstMinus - 1)];
				arguments = input[indexOfFirstMinus..];
			}
			else
			{
				commandName = input;
                arguments = "";
			}

			return (commandName, arguments);
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
		/// The longest common prefix of all strings in <paramref name="collection"/> or <see cref="String.Empty"/> if <paramref name="collection"/> is empty
		/// </summary>
		public static string LongestCommonPrefix(this IList<string> collection)
		{
			if (collection.Count == 0) return string.Empty;

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

		/// <summary>
		/// Removes prefix from all <see cref="string"/>s in <paramref name="collection"/>
		/// </summary>
		/// 
		public static IList<string> RemovePrefix(this IList<string> collection, string prefix)
		{
			return collection.Select(st => st.StartsWith(prefix) ? st[prefix.Length..] : st).ToList();
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

		public static implicit operator (int, int)(Point p) => (p.X, p.Y);

		public static explicit operator System.Windows.Point(Point p) => new(p.X, p.Y);

		public static implicit operator Point((int, int) tp) => new(tp.Item1, tp.Item2);

		public readonly void Deconstruct(out int x, out int y)
		{
			x = X;
			y = Y;
		}
	}

	/// <summary>
	/// Provides some extensions to <see cref="List{T}"/> of <see cref="Point"/>s
	/// </summary>
	public static class PointListExtensions
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
