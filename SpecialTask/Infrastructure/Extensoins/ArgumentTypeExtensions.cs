using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Exceptions;

namespace SpecialTask.Infrastructure.Extensoins
{
    public static class ArgumentTypeExtensions
    {
        public static object ParseValue(this ArgumentType type, string value)
        {
            return type switch
            {
                ArgumentType.Int => Math.Clamp(int.Parse(value), 0, 1_000_000),     // clamp value, so that all numbers are positive and not too big
                ArgumentType.Color => value.ParseColor(),
                ArgumentType.String => value,
                ArgumentType.Texture => value.ParseStreakTexture(),
                ArgumentType.Points => value.ParsePoints(),
                _ => value != "false"                   // all true, that not false
            };
        }
    }
}
