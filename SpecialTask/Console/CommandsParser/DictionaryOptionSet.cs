using Mono.Options;
using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Exceptions;
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
            Add(prototype, description, x =>
            {
                try 
                { 
                    object value = argType.ParseValue(x); 
                }
                catch (FormatException e) 
                { 
                    throw new ArgumentValueFormatException($"Value for {key} should be {argType}. {x} is not {argType}", e, key, x); 
                }
                argValues.TryAdd(key, argType.ParseValue(x));     // ignore duplicated arguments
            });

            if (isNecessary && !necessaryArgs.Contains(key))
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
