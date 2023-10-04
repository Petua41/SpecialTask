using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SpecialTask
{
    enum EStreakTexture { None, SolidColor } // TODO

    static class TextureController
    {
        public static EStreakTexture GetTextureFromString(string textureName)
        {
            // TODO
            return textureName.ToLower() switch
            {
                "solid" or "solidcolor" or "color" => EStreakTexture.SolidColor,
                _ => EStreakTexture.None
            };
        }

        public static Brush GetWPFTexture(EStreakTexture texture, EColor color)
        {
            Color wpfColor = ColorsController.GetWPFColor(color);
            // TODO
            return texture switch
            {
                EStreakTexture.SolidColor => new SolidColorBrush(wpfColor),
                _ => Brushes.Transparent
            };
        }

        public static Dictionary<string, string> GetTexturesWithDescriptions()
        {
            return new()
            {
                { "solid", "Solid color" }
            };
        }
    }

    interface IBrushPrototype
    {
        Brush Brush(EColor color);
    }
}
