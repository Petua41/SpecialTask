using SpecialTask.Exceptions;
using SpecialTask.Infrastructure.Extensoins;

namespace SpecialTask.Infrastructure
{
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

            return arguments.Select(x => x.LongArgument).Where(x => x.StartsWith(lastArgument)).ToList().RemovePrefix(lastArgument).LongestCommonPrefix();
        }

        public readonly (string, object) CreateArgumentFromString(string argument)
        {
            argument = argument.Trim();         // ParseArguments don`t trim arguments, so we`ll make it here

            if (argument == "-h" || argument == "--help") return ("help", true);

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

                        throw new ArgumentParsingError();
                    }
                }
            }
            HighConsole.DisplayError($"Unknown argument: {argument}. Try {neededUserInput} -- help");
            throw new ArgumentParsingError();
        }

        private static string SelectLastLongArgument(string input)
        {
            int indexOfLastSingleMinus = input.LastIndexOf("-");
            int indexOfLastDoubleMinus = input.LastIndexOf("--");

            if (indexOfLastSingleMinus > indexOfLastDoubleMinus + 1 || indexOfLastDoubleMinus < 0) return string.Empty;

            return input[indexOfLastDoubleMinus..];
        }
    }

    record struct ConsoleCommandArgument(string ShortArgument, string LongArgument, EArgumentType Type, bool IsNecessary,
        string CommandParameterName, object? DefaultValue);
}
