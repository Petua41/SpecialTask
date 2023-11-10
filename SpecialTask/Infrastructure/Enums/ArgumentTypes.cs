using SpecialTask.Infrastructure.Extensoins;

namespace SpecialTask.Infrastructure.Enums
{
    public enum ArgumentType { Int, Color, PseudoBool, String, Texture, Points }

    public static class ArgumentTypesConstroller
    {
        private static readonly Dictionary<string, ArgumentType> stringToType = new();

        static ArgumentTypesConstroller()
        {
            foreach (ArgumentType type in Enum.GetValues<ArgumentType>())
            {
                stringToType.Add(type.ToString().ToLower(), type);
            }
        }

        public static ArgumentType ParseType(string? str)
        {
            if (str is null)
            {
                return ArgumentType.PseudoBool;
            }

            try { return stringToType[str.ToLower()]; }
            catch (KeyNotFoundException) { return ArgumentType.PseudoBool; }       // all that cannot be recognized is bool
        }

        public static object ParseValue(this ArgumentType type, string value)
        {
            return type switch
            {
                ArgumentType.Int => int.Parse(value),
                ArgumentType.Color => ColorsController.Parse(value),
                ArgumentType.String => value,
                ArgumentType.Texture => TextureController.Parse(value),
                ArgumentType.Points => value.ParsePoints(),
                _ => value != "false"                   // all true, that not false
            };
        }
    }
}
