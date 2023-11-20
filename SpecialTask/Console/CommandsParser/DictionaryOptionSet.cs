using Mono.Options;
using SpecialTask.Infrastructure.Enums;
using System.IO;
using System.Text;
using static SpecialTask.Infrastructure.Extensoins.ArgumentTypeExtensions;

namespace SpecialTask.Console.CommandsParser
{
    /// <summary>
    /// OptionSet, that writes result to Dictionary. You can access this dictionary through <see cref="ArgumentValues"/> property
    /// </summary>
    internal class DictionaryOptionSet : OptionSet
    {
        private readonly List<string> necessaryArgs = new();

        private readonly Dictionary<string, object> argValues = new();

        public void Add(string prototype, string description, string key, ArgumentType argType, bool isNecessary)
        {
            Add(prototype, description, x => argValues.Add(key, argType.ParseValue(x)));

            if (isNecessary)
            {
                necessaryArgs.Add(key);
            }
        }

        public void ParseToDictionary(string[] args)
        {
            Extra = Parse(args);
        }

        public string WriteOptionDescriptions()
        {
            StringBuilder sb = new();
            using (TextWriter writer = new StringWriter(sb))
            {
                WriteOptionDescriptions(writer);
            }
            return sb.ToString();
        }

        public IReadOnlyDictionary<string, object> ArgumentValues => argValues;

        /// <summary>
        /// List of keys (see <see cref="Add(string, string, string, ArgumentType, bool)"/>) of arguments, that cannot be found
        /// </summary>
        public List<string> NotParsedNecessaryArguments
        {
            get
            {
                List<string> result = new();

                ICollection<string> actualArgs = argValues.Keys;
                foreach (string expectedArg in necessaryArgs)
                {
                    if (!actualArgs.Contains(expectedArg))
                    {
                        result.Add(expectedArg);
                    }
                }

                return result;
            }
        }

        public List<string> Extra { get; private set; } = new();
    }
}
