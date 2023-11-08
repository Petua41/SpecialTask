using SpecialTask.Console.Commands;
using SpecialTask.Console.Interfaces;
using SpecialTask.Infrastructure.Exceptions;
using static SpecialTask.Console.CommandsParser.XMLCommandsParser;
using static SpecialTask.Infrastructure.Extensoins.StringListExtensions;

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

            consoleCommands = new(ParseCommandsXML(xmlContents));
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
                Dictionary<string, object> argumentValues = ParseArguments(consoleCommand, arguments);

                if (argumentValues.ContainsKey("help"))     // in theory, user can enter "--help false". It`s still --help
                {
                    DisplayHelp(consoleCommand);
                    return;
                }

                ICommand command = CreateCommand(consoleCommand, argumentValues);

                if (consoleCommand.supportsUndo)
                {
                    CommandsFacade.Register(command);
                }

                CommandsFacade.Execute(command);
            }
            catch (InvalidOperationException)
            {
                Logger.Warning($"Call of the fictional command {consoleCommand.neededUserInput}");
                HighConsole.DisplayError(
                    $"You cannot call {consoleCommand.neededUserInput} without \"second-level command\". Try {consoleCommand.neededUserInput} --help");
            }
            catch (ArgumentParsingError) { /* ignore */ }   // Some required argument is missing || Some extra argument is present || Error casting parameter
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
                : consoleCommands.Select(x => x.neededUserInput).Where(x => x.StartsWith(input)).ToList().RemovePrefix(input).LongestCommonPrefix();
        }

        /// <summary>
        /// Finds ConsoleCommand by user input
        /// </summary>
        /// <returns>Index in commands list or -1 if not found</returns>
        private static int SelectCommand(string commandName)
        {
            commandName = commandName.Trim();
            return consoleCommands.FindIndex(t => t.neededUserInput == commandName);        // ТАК НАДО ВЕЗДЕ		!!!!!!!!!!!!!!!!
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
                    arguments = string.Empty;
                }
                pair = consoleCommand.CreateArgumentFromString(arg);

                if (!argumentPairs.TryAdd(pair.Item1, pair.Item2)) HighConsole.DisplayError($"Duplicated argument: {pair.Item1}");
            }

            return argumentPairs;
        }

        private static void DisplayHelp(ConsoleCommand command)
        {
            string? help = command.help;
            if (help is null)
            {
                HighConsole.DisplayError($"Help for {command.neededUserInput} not found");
            }
            else
            {
                HighConsole.Display(help);
            }
        }

        private static (string, string) SplitToCommandAndArgs(this string input)
        {
            int indexOfFirstMinus = input.IndexOf('-');

            string commandName;
            string arguments;

            if (indexOfFirstMinus > 0)
            {
                if (indexOfFirstMinus == 0)
                {
                    return (string.Empty, string.Empty);     // input starts with minus: there is no command
                }

                commandName = input[..(indexOfFirstMinus - 1)];
                arguments = input[indexOfFirstMinus..];
            }
            else
            {
                commandName = input;
                arguments = string.Empty;
            }

            return (commandName, arguments);
        }
    }
}
