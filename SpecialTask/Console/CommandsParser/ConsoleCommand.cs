using Mono.Options;
using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Exceptions;
using System.Text.RegularExpressions;
using static SpecialTask.Infrastructure.Extensoins.StringListExtensions;

namespace SpecialTask.Console.CommandsParser
{
    internal partial struct ConsoleCommand
    {
        private DictionaryOptionSet? doSet;

        private static readonly Regex expression = Expression();

        public readonly string AutocompleteArguments(string argumentsInput)
        {
            string lastArgument = SelectLastLongArgument(argumentsInput).Trim();

            return Arguments.Select(x => x.LongArgument).Where(x => x.StartsWith(lastArgument)).ToList().RemovePrefix(lastArgument).LongestCommonPrefix();
        }

        public IReadOnlyDictionary<string, object> ParseArguments(string arguments)
        {
            
            doSet = CreateOptionSet();

            string[] args = SplitByRegex(arguments);
            try
            {
                doSet.ParseToDictionary(args);
            }
            catch (OptionException e) when (e.Message.StartsWith("Missing required value"))
            {
                throw new ArgumentMissingValueException($"Argument {e.OptionName} requires value, but value not provided", e.OptionName);
            }

            if (doSet.NotParsedNecessaryArguments.Count > 0)
            {
                string argKey = doSet.NotParsedNecessaryArguments[0];

                throw new NecessaryArgumentNotPresentedException($"{argKey} is necessary but not present", argKey);
            }

            if (doSet.Extra.Count > 0)
            {
                foreach (string longArg in doSet.Extra)
                {
                    if (longArg != " ") throw new ExtraArgumentException($"{longArg} is extra", longArg);   // check " ", because we added it 32 lines below
                }
            }

            return doSet.ArgumentValues;
        }

        private static string SelectLastLongArgument(string input)
        {
            int indexOfLastSingleMinus = input.LastIndexOf("-");
            int indexOfLastDoubleMinus = input.LastIndexOf("--");

            return indexOfLastSingleMinus > indexOfLastDoubleMinus + 1 || indexOfLastDoubleMinus < 0
                ? string.Empty
                : input[indexOfLastDoubleMinus..];
        }

        private DictionaryOptionSet CreateOptionSet()
        {
            doSet = new();
            foreach (ConsoleCommandArgument argument in Arguments)
            {
                doSet.Add($"{argument.ShortArgument}|{argument.LongArgument}{(argument.Type == ArgumentType.PseudoBool ? string.Empty : "=")}", 
                    argument.Description, argument.CommandParameterName, argument.Type, argument.IsNecessary);
            }

            return doSet;
        }

        private static string[] SplitByRegex(string input)
        {
            string[] preResult = expression.Split(input);
            return preResult.Where(s => s != string.Empty).Select(s => s.Trim()).ToArray();
        }

        public string NeededUserInput { get; set; }

        public string CommandType { get; set; }

        public List<ConsoleCommandArgument> Arguments { get; set; }

        public bool SupportsUndo { get; set; }

        public bool Fictional { get; set; }

        public string Description { get; set; }

        public string Help
        {
            get
            {
                doSet = CreateOptionSet();

                string optionDescriptions = doSet.WriteOptionDescriptions();
                return Description + Environment.NewLine + optionDescriptions;
            }
        }

        [GeneratedRegex("(\\-\\-[^\\-]+)|(\\-[^\\-]+)")]    // I use Regex this way because of two reasons:     1) VS says that it`s faster
        private static partial Regex Expression();                                                          //  2) explanation will be generated
    }

    internal record struct ConsoleCommandArgument(string ShortArgument, string LongArgument, string Description, ArgumentType Type, bool IsNecessary,
        string CommandParameterName, object? DefaultValue);
}
