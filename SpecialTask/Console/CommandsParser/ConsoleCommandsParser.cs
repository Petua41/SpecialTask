using SpecialTask.Console.Commands;
using SpecialTask.Console.Interfaces;
using SpecialTask.Infrastructure.Exceptions;
using static SpecialTask.Infrastructure.Extensoins.StringListExtensions;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTask.Console.CommandsParser
{
    /// <summary>
    /// Processes commands from <see cref="ILowConsole"/>.
    /// Gives <see cref="IHighConsole"/> and <see cref="ILowConsole"/> necessary information about availible commands
    /// </summary>
    internal static class ConsoleCommandsParser
    {
        private static List<ConsoleCommand> consoleCommands = new();

        public static void InitializeCommands()
        {
            string xmlContents = Properties.Resources.ConsoleCommands;

            consoleCommands = new(XMLCommandsParser.ParseCommandsXML(xmlContents));
        }

        // This method is like facade: only calls other methods in the right order and handles exceptions
        public static void ParseCommand(string userInput)
        {
            if (userInput.Length == 0)
            {
                return;
            }

            (string commandName, string arguments) = userInput.SplitToCommandAndArgs();

            int commandNumber = SelectCommand(commandName);
            // If command not found, print global help
            if (commandNumber < 0)
            {
                HighConsole.DisplayGlobalHelp();
                return;
            }

            ConsoleCommand consoleCommand = consoleCommands[commandNumber];

            try
            {
                if (arguments.Contains("-h") || arguments.Contains("--help"))       // don`t parse arguments, if user asks for help
                {
                    HighConsole.NewLine();
                    HighConsole.Display(consoleCommand.Help);
                    return;
                }

                Dictionary<string, object> argumentValues = new(consoleCommand.ParseArguments(arguments));

                ICommand command = CreateCommand(consoleCommand, argumentValues);

                if (consoleCommand.SupportsUndo)
                {
                    CommandsFacade.Register(command);
                }

                CommandsFacade.Execute(command);
            }
            catch (InvalidOperationException)
            {
                Logger.Warning($"Call of the fictional command {consoleCommand.NeededUserInput}");
                HighConsole.DisplayError(
                    $"You cannot call {consoleCommand.NeededUserInput} without \"second-level command\". Try {consoleCommand.NeededUserInput} --help");
            }
            catch (ExtraArgumentException e)
            {
                if (e.LongArgument is not null) HighConsole.DisplayError($"Invalid argument: {e.LongArgument}. Try {commandName} -- help");
                else HighConsole.DisplayError($"Some argument is invalid. Please, contact us and try {commandName} -- help");
                return;
            }
            catch (NecessaryArgumentNotPresentedException e)
            {
                if (e.LongArgument is not null) HighConsole.DisplayError($"{e.LongArgument} is necessary. Try {commandName} -- help");
                else HighConsole.DisplayError($"Some argument is necessary, but not present. Please, contact us and try {commandName} -- help");
                return;
            }
        }

        public static string Autocomplete(string input)
        {
            if (input.Length == 0)
            {
                return string.Empty;       // empty input => nothing happened
            }

            (string commandName, string argumentsStr) = input.SplitToCommandAndArgs();


            int idxOfCommand = SelectCommand(commandName);
            return idxOfCommand >= 0
                ? consoleCommands[idxOfCommand].AutocompleteArguments(argumentsStr)
                : consoleCommands.Select(x => x.NeededUserInput).Where(x => x.StartsWith(input)).ToList().RemovePrefix(input).LongestCommonPrefix();
        }

        public static ICommand CreateCommand(ConsoleCommand consoleCommand, Dictionary<string, object> arguments)
        {
            if (consoleCommand.Fictional)
            {
                throw new InvalidOperationException();
            }

            // Add all default values, that we know
            foreach (ConsoleCommandArgument argument in consoleCommand.Arguments)
            {
                if (!arguments.ContainsKey(argument.CommandParameterName) && argument.DefaultValue is not null)
                {
                    arguments.Add(argument.CommandParameterName, argument.DefaultValue);
                }
            }

            return CommandCreator.CreateCommand(consoleCommand.CommandType, arguments);
        }

        /// <summary>
        /// Finds ConsoleCommand by user input
        /// </summary>
        /// <returns>Index in commands list or -1 if not found</returns>
        private static int SelectCommand(string commandName)
        {
            commandName = commandName.Trim();
            return consoleCommands.FindIndex(t => t.NeededUserInput == commandName);        // ТАК НАДО ВЕЗДЕ		!!!!!!!!!!!!!!!!
        }
    }
}
