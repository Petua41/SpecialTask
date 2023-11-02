using SpecialTask.Drawing;
using SpecialTask.Helpers.Extensoins;
using System;
using System.Collections.Generic;

namespace SpecialTask.Helpers
{
    public enum EArgumentType { Int, Color, PseudoBool, String, Texture, Points }

    public static class ArgumentTypesConstroller
    {
        private static readonly Dictionary<string, EArgumentType> stringToType = new();

        static ArgumentTypesConstroller()
        {
            foreach (EArgumentType type in Enum.GetValues<EArgumentType>()) stringToType.Add(type.ToString().ToLower(), type);
        }

        public static EArgumentType ParseType(string? str)
        {
            if (str == null) return EArgumentType.PseudoBool;

            try { return stringToType[str.ToLower()]; }
            catch (KeyNotFoundException) { return EArgumentType.PseudoBool; }       // all that cannot be recognized is bool
        }

        public static object ParseValue(this EArgumentType type, string value)
        {
            return type switch
            {
                EArgumentType.Int => int.Parse(value),
                EArgumentType.Color => ColorsController.Parse(value),
                EArgumentType.String => value,
                EArgumentType.Texture => TextureController.Parse(value),
                EArgumentType.Points => value.ParsePoints(),
                _ => value != "false"                   // all true, that not false
            };
        }
    }
}
