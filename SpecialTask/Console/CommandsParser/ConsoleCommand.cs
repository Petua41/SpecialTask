using SpecialTask.Infrastructure.Exceptions;
using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Extensoins;

namespace SpecialTask.Console.CommandsParser
{
    internal struct ConsoleCommand
    {
        public string neededUserInput;
        public string? help;
        public string commandType;
        public List<ConsoleCommandArgument> arguments;
        public bool supportsUndo;
        public bool fictional;                                  // Only for --help

        public readonly string AutocompleteArguments(string argumentsInput)
        {
            string lastArgument = SelectLastLongArgument(argumentsInput).Trim();

            return arguments.Select(x => x.LongArgument).Where(x => x.StartsWith(lastArgument)).ToList().RemovePrefix(lastArgument).LongestCommonPrefix();
        }

        public readonly (string, object) CreateArgumentFromString(string argument)
        {
            argument = argument.Trim();         // ParseArguments don`t trim arguments, so we`ll make it here

            if (argument is "-h" or "--help")
            {
                return ("help", true);
            }

            foreach (ConsoleCommandArgument arg in arguments)
            {
                if (argument.StartsWith(arg.LongArgument) || argument.StartsWith(arg.ShortArgument))
                {
                    string rawValue = argument.Replace(arg.LongArgument, string.Empty).Replace(arg.ShortArgument, string.Empty).Trim();

                    try
                    {
                        object value = arg.Type.ParseValue(rawValue);
                        string paramName = arg.CommandParameterName;
                        return (paramName, value);
                    }
                    catch (FormatException)     // Error casting string
                    {
                        string argType = arg.Type.ToString();
                        HighConsole.DisplayError(
                            $"{arg.LongArgument} should be {argType}. {rawValue} is not {argType}. Try {neededUserInput} --help");

                        throw new ArgumentParsingError($"{arg.LongArgument} should be {argType}. {rawValue} is not {argType}.", arg.LongArgument);
                    }
                }
            }
            HighConsole.DisplayError($"Unknown argument: {argument}. Try {neededUserInput} -- help");
            throw new ArgumentParsingError($"Unknown argument: {argument}.", argument);
        }

        private static string SelectLastLongArgument(string input)
        {
            int indexOfLastSingleMinus = input.LastIndexOf("-");
            int indexOfLastDoubleMinus = input.LastIndexOf("--");

            return indexOfLastSingleMinus > indexOfLastDoubleMinus + 1 || indexOfLastDoubleMinus < 0
                ? string.Empty
                : input[indexOfLastDoubleMinus..];
        }
    }

    internal record struct ConsoleCommandArgument(string ShortArgument, string LongArgument, EArgumentType Type, bool IsNecessary,
        string CommandParameterName, object? DefaultValue);
}
