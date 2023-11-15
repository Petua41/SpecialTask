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
            doSet.ParseToDictionary(args);

            if (doSet.NotParsedNecessaryArguments.Count > 0)        // this property contains KEYS (NeededUserInput`s) of arguments
            {
                string argKey = doSet.NotParsedNecessaryArguments[0];
                string longArg = GetLongArgumentByName(argKey);

                if (longArg.Length > 0) throw new NecessaryArgumentNotPresentedException($"{longArg} is necessary but not present", longArg);
                throw new NecessaryArgumentNotPresentedException($"Unknown argument is necessary, but not present");
            }

            if (doSet.Extra.Count > 0)                             // this property contains USER INPUT (LongArgument`s) of arguments
            {
                string longArg = doSet.Extra[0];

                throw new ExtraArgumentException($"{longArg} is extra", longArg);
            }

            return doSet.ArgumentValues;
        }

        /// <returns><see cref="ConsoleCommandArgument.LongArgument"/> if found, <see cref="string.Empty"/> otherwise</returns>
        private readonly string GetLongArgumentByName(string name)
        {
            int indexOfArg = Arguments.FindIndex(arg => arg.CommandParameterName == name);
            if (indexOfArg > 0)
            {
                return Arguments[indexOfArg].LongArgument;
            }

            return string.Empty;
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
                doSet.Add($"{argument.ShortArgument}|{argument.LongArgument}=", argument.Description, argument.CommandParameterName,
                    argument.Type, argument.IsNecessary);
            }

            return doSet;
        }

        private static string[] SplitByRegex(string input)
        {
            string[] preResult = expression.Split(input);
            return preResult.Where(s => s != string.Empty).ToArray();
        }

        public string NeededUserInput { get; set; }

        public string CommandType { get; set; }

        public List<ConsoleCommandArgument> Arguments { get; set; }

        public bool SupportsUndo { get; set; }

        public bool Fictional { get; set; }         // only for --help. Doesn`t support execution

        public string Description { get; set; }

        public readonly string Help
        {
            get
            {
                return Description
                    + Environment.NewLine
                    + (doSet is null ? string.Empty : Environment.NewLine
                    + doSet.WriteOptionDescriptions());
            }
        }

        [GeneratedRegex("(\\-\\-[^\\-]+)|(\\-[^\\-]+)")]    // I use Regex this way because of two reasons:     1) VS says that it`s faster
        private static partial Regex Expression();                                                          //  2) explanation will be generated
    }

    internal record struct ConsoleCommandArgument(string ShortArgument, string LongArgument, string Description, ArgumentType Type, bool IsNecessary,
        string CommandParameterName, object? DefaultValue);
}
