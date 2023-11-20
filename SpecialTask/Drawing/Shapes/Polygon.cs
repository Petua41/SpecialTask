using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Exceptions;
using System.Windows.Controls;
using System.Windows.Media;
using static SpecialTask.Infrastructure.Extensoins.InternalColorExtensions;
using static SpecialTask.Infrastructure.Extensoins.KeyValuePairListExtensions;
using static SpecialTask.Infrastructure.Extensoins.PointListExtensions;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTask.Drawing.Shapes
{
    internal class Polygon : Shape
    {
        private List<Point> points;
        private InternalColor color;
        private int lineThickness;

        private static int firstAvailibleUniqueNumber = 0;

        public Polygon(List<Point> points, int lineThickness, InternalColor color)
        {
            this.points = points;
            this.color = color;
            this.lineThickness = lineThickness;
            UniqueName = GetNextUniqueName();

            ATTRS_TO_EDIT = new() { { "points", "Points" }, { "lineThickness", "Outline thickness" }, { "color", "Outline color" } };
        }

        public Polygon(Polygon old) : this(old.points, old.lineThickness, old.color) { }

        public static new string GetNextUniqueName()
        {
            return $"Polygon_{firstAvailibleUniqueNumber++}";
        }

        public override string Edit(string attribute, string value)
        {
            attribute = attribute.ToLower();
            string oldValue;

            try
            {
                switch (attribute)
                {
                    case "points":
                        oldValue = Points.PointsToString();
                        Points = value.ParsePoints();
                        break;
                    case "lineThickness":
                        oldValue = LineThickness.ToString();
                        LineThickness = int.Parse(value);
                        break;
                    case "color":
                        oldValue = Color.ToString();
                        Color = value.ParseColor();
                        break;
                    default:
                        throw new ArgumentException($"Unknown attribute: {attribute}");
                }
            }
            catch (FormatException e) { throw new ShapeAttributeCastException($"Cannot cast {value} to value of {attribute}", e, attribute, value); }

            return oldValue;
        }

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (base.wpfShape is not null)
                {
                    return base.wpfShape;
                }

                System.Windows.Shapes.Shape wpfShape = new System.Windows.Shapes.Polygon
                {
                    Points = new(points.Select(p => (System.Windows.Point)p)),
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
            Points = Points.Select(p => p + (offset, 0)).ToList();
        }

        public override void MoveYBy(int offset)
        {
            Points = Points.Select(p => p + (0, offset)).ToList();
        }

        public override Point Center => Points.Center();

        private List<Point> Points
        {
            get => points;
            set
            {
                points = value;
                base.Destroy(); base.Display(); ;
            }
        }

        private int LineThickness
        {
            get => lineThickness;
            set
            {
                lineThickness = value;
                base.Destroy(); base.Display(); ;
            }
        }

        private InternalColor Color
        {
            get => color;
            set
            {
                color = value;
                base.Destroy(); base.Display(); ;
            }
        }

        public override Shape Clone()
        {
            return new Polygon(this);
        }
    }
}
