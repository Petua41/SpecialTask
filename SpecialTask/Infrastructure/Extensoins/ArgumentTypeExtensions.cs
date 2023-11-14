using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Infrastructure.Extensoins
{
    public static class ArgumentTypeExtensions
    {
        public static object ParseValue(this ArgumentType type, string value)
        {
            return type switch
            {
                ArgumentType.Int => int.Parse(value),
                ArgumentType.Color => value.ParseColor(),
                ArgumentType.String => value,
                ArgumentType.Texture => value.ParseStreakTexture(),
                ArgumentType.Points => value.ParsePoints(),
                _ => value != "false"                   // all true, that not false
            };
        }
    }
}
