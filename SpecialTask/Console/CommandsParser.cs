using SpecialTask.Console.Commands;
using SpecialTask.Exceptions;
using SpecialTask.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static SpecialTask.Helpers.Extensoins.StringListExtensions;

namespace SpecialTask.Console
{
    /// <summary>
    /// Processes commands from <see cref="ILowConsole"/>.
    /// Gives <see cref="IHighConsole"/> and <see cref="ILowConsole"/> necessary information about availible commands
    /// </summary>
    static class CommandsParser
    {
        static readonly List<ConsoleCommand> consoleCommands = new();
        public static string globalHelp = "[color:purple]Global help not found![purple]";

        static CommandsParser()
        {
            string xmlContents = Properties.Resources.ConsoleCommands;

            try
            {
                consoleCommands = ParseCommandsXML(xmlContents);
            }
            catch (InvalidResourceFileException)
            {
                Logger.Instance.Fatal($"Invalid XML file with commands!{Environment.NewLine}Please, contact us");
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

                if (argumentValues.ContainsKey("help"))     // in theory, user can enter "--help false". It`s still --help
                {
                    DisplayHelp(consoleCommand);
                    return;
                }

                ICommand command = CreateCommand(consoleCommand, argumentValues);

                if (consoleCommand.supportsUndo) CommandsFacade.Register(command);

                CommandsFacade.Execute(command);
            }
            catch (InvalidOperationException)
            {
                Logger.Instance.Warning($"Call of the fictional command {consoleCommand.neededUserInput}");
                MiddleConsole.HighConsole.DisplayError(
                    $"You cannot call {consoleCommand.neededUserInput} without \"second-level command\". Try {consoleCommand.neededUserInput} --help");
            }
            catch (ArgumentParsingError) { /* ignore */ }   // Some required argument is missing || Some extra argument is present || Error casting parameter
        }

        public static string Autocomplete(string input)
        {
            if (input.Length == 0) return "";       // empty input => nothing happened

            (string commandName, string argumentsStr) = input.SplitToCommandAndArgs();


            int idxOfCommand = SelectCommand(commandName);
            if (idxOfCommand >= 0)
            {                                                       // complete command => pass request on
                return consoleCommands[idxOfCommand].AutocompleteArguments(argumentsStr);
            }
            else
            {
                return (from comm in consoleCommands
                        where comm.neededUserInput.StartsWith(input)
                        select comm.neededUserInput).ToList().RemovePrefix(input).LongestCommonPrefix();
            }
        }

        private static List<ConsoleCommand> ParseCommandsXML(string xmlContent)
        {
            XDocument commandsFile = XDocument.Parse(xmlContent);
            XElement xmlRoot = commandsFile.Root ?? throw new InvalidResourceFileException();

            List<ConsoleCommand> commands = new();

            foreach (XElement elem in xmlRoot.Elements())
            {
                if (elem.Name == "command") { commands.Add(ParseCommandElement(elem)); }
                else if (elem.Name == "help")
                {
                    string helpCandidate = elem.Value;
                    if (helpCandidate != "") globalHelp = helpCandidate + Environment.NewLine;
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
                commandType = elem.Attribute("commandClass")?.Value ?? "",
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
            if (defValueString != null) defaultValue = type.ParseValue(defValueString);     // we leave defaultValue null, if there`s no such attribute

            return new ConsoleCommandArgument
            {
                ShortArgument = elem.Attribute("shortArgument")?.Value ?? "",
                LongArgument = elem.Attribute("longArgument")?.Value ?? "",
                Type = type,
                IsNecessary = (elem.Attribute("isNecessary")?.Value ?? "false") != "false",
                CommandParameterName = elem.Attribute("commandParameterName")?.Value ?? "",
                DefaultValue = defaultValue
            };
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

        private static ICommand CreateCommand(ConsoleCommand consoleCommand, Dictionary<string, object> arguments)
        {
            if (consoleCommand.fictional) throw new InvalidOperationException();

            // Check that all necessary arguments are present
            foreach (ConsoleCommandArgument argument in consoleCommand.arguments)
            {
                if (!arguments.ContainsKey(argument.CommandParameterName))
                {
                    if (argument.IsNecessary)
                    {
                        MiddleConsole.HighConsole.DisplayError($"Missing required argument {argument.LongArgument}. Try {consoleCommand.neededUserInput} --help");
                        throw new ArgumentParsingError();
                    }
                    else if (argument.DefaultValue != null)
                    {
                        arguments.Add(argument.CommandParameterName, argument.DefaultValue);
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
}
