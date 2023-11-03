using SpecialTask.Exceptions;
using SpecialTask.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using static SpecialTask.Helpers.Extensoins.PointListExtensions;

namespace SpecialTask.Drawing.Shapes
{
    class Polygon : Shape
    {
        List<Point> points;
        private EColor color;
        private int lineThickness;

        private static int firstAvailibleUniqueNumber = 0;

        public Polygon(List<Point> points, int lineThickness, EColor color)
        {
            this.points = points;
            this.color = color;
            this.lineThickness = lineThickness;
            uniqueName = GetNextUniqueName();

            ATTRS_TO_EDIT = new() { { "points", "Points" }, { "lineThickness", "Outline thickness" }, { "color", "Outline color" } };
        }

        public Polygon(Polygon old) : this(old.points, old.lineThickness, old.color) { }

        public static new string GetNextUniqueName()
        {
            return $"Polygon_{firstAvailibleUniqueNumber++}";
        }

        public override object Edit(string attribute, string value)
        {
            attribute = attribute.ToLower();
            object oldValue;

            try
            {
                switch (attribute)
                {
                    case "points":
                        oldValue = Points;
                        Points = value.ParsePoints();
                        break;
                    case "lineThickness":
                        oldValue = LineThickness;
                        LineThickness = int.Parse(value);
                        break;
                    case "color":
                        oldValue = Color;
                        Color = ColorsController.Parse(value);
                        break;
                    default:
                        throw new ArgumentException($"Unknown attribute: {attribute}");
                }
            }
            catch (FormatException) { throw new ShapeAttributeCastException(); }

            return oldValue;
        }

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (base.wpfShape is not null) return base.wpfShape;

                System.Windows.Shapes.Shape wpfShape = new System.Windows.Shapes.Polygon
                {
                    Points = new(from p in points select (System.Windows.Point)p),
                    StrokeThickness = LineThickness,
                    Stroke = new SolidColorBrush(Color.GetWPFColor())
                };
                Canvas.SetTop(wpfShape, 0);
                Canvas.SetLeft(wpfShape, 0);

                base.wpfShape = wpfShape;
                return wpfShape;
            }
        }

        public override Dictionary<string, object> Accept()
        {
            return new() { { "points", Points }, { "lineThickness", LineThickness }, { "color", color } };
        }

        public override void MoveXBy(int offset)
        {
            Points = (from p in Points select p + new Point(offset, 0)).ToList();
        }

        public override void MoveYBy(int offset)
        {
            Points = (from p in Points select p + new Point(0, offset)).ToList();
        }

        public override Point Center
        {
            get
            {
                return Points.Center();
            }
        }

        private List<Point> Points
        {
            get => points;
            set
            {
                points = value;
                base.Redraw();
            }
        }

        private int LineThickness
        {
            get => lineThickness;
            set
            {
                lineThickness = value;
                base.Redraw();
            }
        }

        private EColor Color
        {
            get => color;
            set
            {
                color = value;
                base.Redraw();
            }
        }

        public override Shape Clone()
        {
            return new Polygon(this);
        }
    }
}
