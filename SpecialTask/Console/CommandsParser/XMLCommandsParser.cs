using SpecialTask.Console.Commands;
using SpecialTask.Infrastructure;
using SpecialTask.Infrastructure.Enums;
using System.Xml.Linq;

namespace SpecialTask.Console.CommandsParser
{
    internal static class XMLCommandsParser
    {
        // Tags:
        private const string COMMAND = "command";
        private const string HELP = "help";
        private const string ARGUMENT = "argument";

        // Attributes:
        //  Command:
        private const string USER_INPUT = "userInput";
        private const string FICTIONAL = "fictional";
        private const string COMMAND_CLASS = "commandClass";
        private const string SUPPORTS_UNDO = "supportsUndo";
        //  Argument:
        private const string DEFAULT_VALUE = "defaultValue";
        private const string SHORT_ARGUMENT = "shortArgument";
        private const string LONG_ARGUMENT = "longArgument";
        private const string IS_NECESSARY = "isNecessary";
        private const string COMMAND_PARAMETER_NAME = "commandParameterName";

        // Values:
        private const string FALSE = "false";

        public static IReadOnlyList<ConsoleCommand> ParseCommandsXML(string xmlContent)
        {
            XDocument commandsFile = XDocument.Parse(xmlContent);
            XElement xmlRoot = commandsFile.Root ?? throw new InvalidResourceFileException();

            List<ConsoleCommand> commands = new();

            foreach (XElement elem in xmlRoot.Elements())
            {
                if (elem.Name == COMMAND) { commands.Add(ParseCommandElement(elem)); }
                else if (elem.Name == HELP)
                {
                    string helpCandidate = elem.Value;
                    if (helpCandidate != string.Empty)
                    {
                        GlobalHelp = helpCandidate + Environment.NewLine;
                    }
                }
                else
                {
                    Logger.Warning($"Unexpected XML tag inside the root element: {elem.Name}");
                }
            }

            return commands;
        }

        public static ICommand CreateCommand(ConsoleCommand consoleCommand, Dictionary<string, object> arguments)
        {
            if (consoleCommand.fictional)
            {
                throw new InvalidOperationException();
            }

            // Check that all necessary arguments are present
            foreach (ConsoleCommandArgument argument in consoleCommand.arguments)
            {
                if (!arguments.ContainsKey(argument.CommandParameterName))
                {
                    if (argument.IsNecessary)
                    {
                        HighConsole.DisplayError($"Missing required argument {argument.LongArgument}. Try {consoleCommand.neededUserInput} --help");
                        throw new ArgumentParsingError();
                    }
                    else if (argument.DefaultValue is not null)
                    {
                        arguments.Add(argument.CommandParameterName, argument.DefaultValue);
                    }
                }
            }

            return CommandCreator.CreateCommand(consoleCommand.commandType, arguments);
        }

        private static ConsoleCommand ParseCommandElement(XElement elem)
        {
            List<ConsoleCommandArgument> arguments = new();

            string neededUserInput = elem.Attribute(USER_INPUT)?.Value ?? string.Empty;

            bool fictional = (elem.Attribute(FICTIONAL)?.Value ?? FALSE) != FALSE;

            string? help = elem.Value;
            if (help == string.Empty)
            {
                help = null;
            }

            foreach (XElement child in elem.Elements())
            {
                if (child.Name == ARGUMENT)
                {
                    arguments.Add(ParseArgumentElement(child));
                }
                else
                {
                    Logger.Warning($"Unexpected XML tag inside {neededUserInput} command: {child.Name}");
                }
            }

            return new ConsoleCommand
            {
                neededUserInput = neededUserInput,
                help = help,
                commandType = elem.Attribute(COMMAND_CLASS)?.Value ?? string.Empty,
                supportsUndo = (elem.Attribute(SUPPORTS_UNDO)?.Value ?? FALSE) != FALSE,
                fictional = fictional,
                arguments = arguments
            };
        }

        private static ConsoleCommandArgument ParseArgumentElement(XElement elem)
        {
            EArgumentType type = ArgumentTypesConstroller.ParseType(elem.Attribute("type")?.Value);

            object? defaultValue = null;
            string? defValueString = elem.Attribute(DEFAULT_VALUE)?.Value;
            if (defValueString is not null)
            {
                defaultValue = type.ParseValue(defValueString);     // we leave defaultValue null, if there`s no such attribute
            }

            return new ConsoleCommandArgument
            {
                ShortArgument = elem.Attribute(SHORT_ARGUMENT)?.Value ?? string.Empty,
                LongArgument = elem.Attribute(LONG_ARGUMENT)?.Value ?? string.Empty,
                Type = type,
                IsNecessary = (elem.Attribute(IS_NECESSARY)?.Value ?? FALSE) != FALSE,
                CommandParameterName = elem.Attribute(COMMAND_PARAMETER_NAME)?.Value ?? string.Empty,
                DefaultValue = defaultValue
            };
        }

        public static string GlobalHelp { get; private set; } = "[color:purple]Global help not found![purple]";
    }
}
