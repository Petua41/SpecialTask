using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpecialTask
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

            if (Directory.Exists(@"Resources")) waterSeamlessTexture = new SeamlessTexture(@"Resources/water_texture.jpg");
            else waterSeamlessTexture = new SeamlessTexture(@"../../../Resources/water_texture.jpg");
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

    /// <summary>
    /// Interface for Texture Brushes
    /// </summary>
    interface IBrushPrototype
    {
        /// <summary>
        /// Creates WPF Brush. All Brushes are freezed as possible
        /// </summary>
        /// <param name="wpfColor"></param>
        /// <returns></returns>
        Brush Brush(Color wpfColor);
    }

    /// <summary>
    /// Texture Brush, that tiles figure with specified Geometry
    /// </summary>
    class GeometryTileTexture : IBrushPrototype
    {
        private readonly Geometry geom;

        private readonly Rect sizeOfTile = new(0, 0, 10, 10);         // first and second -- shift. Third and fourth -- size
        private const double lineThickness = 1.5;

        public GeometryTileTexture(Geometry geom)
        {
            this.geom = geom;
        }

        public Brush Brush(Color wpfColor)
        {
            Brush internalBrush = new SolidColorBrush(wpfColor);
            GeometryDrawing geomDr = new(internalBrush, new(internalBrush, lineThickness), geom);

            DrawingBrush externalBrush = new(geomDr)
            {
                Viewport = sizeOfTile,
                ViewportUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.None,
                TileMode = TileMode.Tile
            };

            if (externalBrush.CanFreeze) externalBrush.Freeze();

            return externalBrush;
        }
    }

    /// <summary>
    /// Texture brush for linear gradients. Last color must be passed to Brush()
    /// </summary>
    class LinearGradientTexture : IBrushPrototype, IEnumerable<GradientStop>
    {
        private readonly List<GradientStop> gradientStops = new();
        private System.Windows.Point startPoint;
        private System.Windows.Point endPoint;

        public LinearGradientTexture(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }

        public void Add(Color color, double offset)
        {
            gradientStops.Add(new(color, offset));
        }

        public Brush Brush(Color color)
        {
            Add(color, 1);
            GradientStopCollection grStCollection = new(gradientStops);
            Brush brush = new LinearGradientBrush(grStCollection, startPoint, endPoint);
            if (brush.CanFreeze) brush.Freeze();
            return brush;
        }

        public IEnumerator<GradientStop> GetEnumerator()
        {
            return gradientStops.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Texture brush for radial gradients. Last color must be passed to Brush()
    /// </summary>
    class RadialGradientTexture : IBrushPrototype, IEnumerable<GradientStop>
    {
        private readonly List<GradientStop> gradientStops = new();

        public void Add(Color color, double offset)
        {
            gradientStops.Add(new(color, offset));
        }

        public Brush Brush(Color color)
        {
            Add(color, 1);
            GradientStopCollection grStCollection = new(gradientStops);
            Brush brush = new RadialGradientBrush(grStCollection);
            if (brush.CanFreeze) brush.Freeze();
            return brush;
        }

        public IEnumerator<GradientStop> GetEnumerator()
        {
            return gradientStops.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Fills figure with seamless texture. Color is ignored
    /// </summary>
    class SeamlessTexture : IBrushPrototype
    {
        private readonly BitmapImage seamlessTexture;

        public SeamlessTexture(string filename)
        {
            Uri uri = new(filename, UriKind.Relative);
            seamlessTexture = new(uri);
        }

        public Brush Brush(Color color)
        {
            Brush brush = new ImageBrush(seamlessTexture);
            if (brush.CanFreeze) brush.Freeze();
            return brush;
        }
    }
}
