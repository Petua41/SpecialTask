﻿using System.Windows;
using System.Windows.Media;

namespace SpecialTask.Drawing.BrushPrototypes
{
    /// <summary>
    /// Texture Brush, that tiles figure with specified Geometry
    /// </summary>
    internal class GeometryTileTexture : IBrushPrototype
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

            if (externalBrush.CanFreeze)
            {
                externalBrush.Freeze();
            }

            return externalBrush;
        }
    }
}
