using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using SpecialTask.Drawing.BrushPrototypes;

namespace SpecialTask.Helpers
{
    enum EStreakTexture
    {
        None,
        SolidColor,
        HorizontalLines, VerticalLines, Dots, TransparentCircles,               // We can add lines (diagonal, vawes, etc.) endlessly
        HorizontalTransparentToColorGradient, HorizontalRainbow,                // We can add linear gradients endlessly
        RadialColorToTransparentGradient,                                       // We can add radial gradients endlessly
        Water                                                                   // We can add seamless textures really endlessly
    }

    static class TextureController
    {
        private static readonly Dictionary<string, string> textureDescriptions = new()
        {
            { "solid", "Solid color" }, { "horizontallines", "Horizontal lines" }, { "verticallines", "Vertical lines" },
            { "horizontaltransparencygradient", "Horizontal gradient with transparent on the left and color on the right" },
            { "rainbow", "Horizontal rainbow gradient. Color is ignored" },
            { "radialtransparencygradient", "Radial gradient with color in center and transparency on the edge" },
            { "water", "Water texture. Color is ignored" }, { "dots", "Dots" }, { "holes", "Solid color with transparent round \"holes\""}
        };

        private static readonly IBrushPrototype horizontalLinesBrush;
        private static readonly IBrushPrototype verticalLinesBrush;
        private static readonly IBrushPrototype dotsBrush;
        private static readonly IBrushPrototype transpCircles;

        private static readonly IBrushPrototype horizTranspToColorGradientBrush;
        private static readonly IBrushPrototype horizontalRainbow;

        private static readonly IBrushPrototype radTranspToColorGradientBrush;
        private static readonly IBrushPrototype waterSeamlessTexture;

        static TextureController()
        {
            horizontalLinesBrush = new GeometryTileTexture(new LineGeometry(new(0, 0), new(10, 0)));
            verticalLinesBrush = new GeometryTileTexture(new LineGeometry(new(0, 0), new(0, 10)));
            dotsBrush = new GeometryTileTexture(new EllipseGeometry(new(5, 5), 0.2, 0.2));
            transpCircles = new GeometryTileTexture(new CombinedGeometry(GeometryCombineMode.Exclude,
                    new RectangleGeometry(new Rect(0, 0, 10, 10)), new EllipseGeometry(new(5, 5), 3.5, 3.5)));

            horizTranspToColorGradientBrush = new LinearGradientTexture(new(0, 0.5), new(1, 0.5)) { { Colors.Transparent, 0 } };
            horizontalRainbow = new LinearGradientTexture(new(0.5, 1), new(0.5, 0))
            {
                { Colors.Red, 0 }, { Colors.Orange, 0.16 }, { Colors.Yellow, 0.32 }, {Colors.Green, 0.48 }, { Colors.LightBlue, 0.64 }, { Colors.Blue, 0.8 },
                { Color.FromRgb(36, 9, 53), 1 }
            };
            radTranspToColorGradientBrush = new RadialGradientTexture() { { Colors.Transparent, 0 } };

            waterSeamlessTexture = new SeamlessTexture(Properties.Resources.water_texture);
        }

        public static EStreakTexture Parse(string textureName)
        {
            return textureName.ToLower() switch
            {
                "solid" or "solidcolor" or "color" or "sc" => EStreakTexture.SolidColor,
                "horizontallines" or "hl" => EStreakTexture.HorizontalLines,
                "verticallines" or "vl" => EStreakTexture.VerticalLines,
                "horizontaltransparencygradient" or "htg" or "horizontaltransparenttocolorgradient" => EStreakTexture.HorizontalTransparentToColorGradient,
                "horizontalrainbow" or "rainbow" or "hrb" => EStreakTexture.HorizontalRainbow,
                "radialtarnsparencygradient" or "rtg" or "radialcolortotransparentgradient" => EStreakTexture.RadialColorToTransparentGradient,
                "watertexture" or "water" or "wt" => EStreakTexture.Water,
                "dots" => EStreakTexture.Dots,
                "holes" or "tc" or "transparentcircles" => EStreakTexture.TransparentCircles,
                _ => EStreakTexture.None
            };
        }

        public static Brush GetWPFTexture(this EStreakTexture texture, EColor color)
        {
            Color wpfColor = color.GetWPFColor();
            return texture switch
            {
                EStreakTexture.SolidColor => new SolidColorBrush(wpfColor),
                EStreakTexture.HorizontalLines => horizontalLinesBrush.Brush(wpfColor),
                EStreakTexture.VerticalLines => verticalLinesBrush.Brush(wpfColor),
                EStreakTexture.HorizontalTransparentToColorGradient => horizTranspToColorGradientBrush.Brush(wpfColor),
                EStreakTexture.HorizontalRainbow => horizontalRainbow.Brush(wpfColor),
                EStreakTexture.RadialColorToTransparentGradient => radTranspToColorGradientBrush.Brush(wpfColor),
                EStreakTexture.Water => waterSeamlessTexture.Brush(wpfColor),
                EStreakTexture.Dots => dotsBrush.Brush(wpfColor),
                EStreakTexture.TransparentCircles => transpCircles.Brush(wpfColor),
                _ => Brushes.Transparent
            };
        }

        public static Dictionary<string, string> TexturesWithDescriptions => textureDescriptions;
    }
}
