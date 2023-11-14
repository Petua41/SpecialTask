using SpecialTask.Drawing.BrushPrototypes;
using SpecialTask.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SpecialTask.Infrastructure.Extensoins
{
    internal static class StreakTextureExtensions
    {
        private static readonly IBrushPrototype horizontalLinesBrush;
        private static readonly IBrushPrototype verticalLinesBrush;
        private static readonly IBrushPrototype dotsBrush;
        private static readonly IBrushPrototype transpCircles;

        private static readonly IBrushPrototype horizTranspToColorGradientBrush;
        private static readonly IBrushPrototype horizontalRainbow;

        private static readonly IBrushPrototype radTranspToColorGradientBrush;
        private static readonly IBrushPrototype waterSeamlessTexture;

        static StreakTextureExtensions()
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
        public static Brush GetWPFTexture(this StreakTexture texture, InternalColor color)
        {
            Color wpfColor = color.GetWPFColor();
            return texture switch
            {
                StreakTexture.SolidColor => new SolidColorBrush(wpfColor),
                StreakTexture.HorizontalLines => horizontalLinesBrush.Brush(wpfColor),
                StreakTexture.VerticalLines => verticalLinesBrush.Brush(wpfColor),
                StreakTexture.HorizontalTransparentToColorGradient => horizTranspToColorGradientBrush.Brush(wpfColor),
                StreakTexture.HorizontalRainbow => horizontalRainbow.Brush(wpfColor),
                StreakTexture.RadialColorToTransparentGradient => radTranspToColorGradientBrush.Brush(wpfColor),
                StreakTexture.Water => waterSeamlessTexture.Brush(wpfColor),
                StreakTexture.Dots => dotsBrush.Brush(wpfColor),
                StreakTexture.TransparentCircles => transpCircles.Brush(wpfColor),
                _ => Brushes.Transparent
            };
        }
    }
}
