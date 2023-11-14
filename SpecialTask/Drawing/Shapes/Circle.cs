using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Exceptions;
using System.Windows.Controls;
using System.Windows.Media;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;
using static SpecialTask.Infrastructure.Extensoins.InternalColorExtensions;

namespace SpecialTask.Drawing.Shapes
{
    internal class Circle : Shape
    {
        private int radius;
        private int centerX;
        private int centerY;
        private InternalColor color;
        private int lineThickness;

        private static int firstAvailibleUniqueNumber = 0;

        public Circle(int centerX, int centerY, InternalColor color, int radius, int lineThickness)
        {
            this.centerX = centerX;
            this.centerY = centerY;
            this.color = color;
            this.radius = radius;
            this.lineThickness = lineThickness;
            uniqueName = GetNextUniqueName();

            ATTRS_TO_EDIT = new() { { "centerX", "Center X"}, { "centerY", "Center Y" },
            { "radius", "Radius" }, { "lineThickness", "Outline thickness" }, { "color", "Outline color" } };
        }

        public Circle(Circle old) : this(old.CenterX, old.CenterY, old.Color, old.Radius, old.LineThickness) { }

        public static new string GetNextUniqueName()
        {
            return $"Circle_{firstAvailibleUniqueNumber++}";
        }

        public override string Edit(string attribute, string value)
        {
            attribute = attribute.ToLower();
            string oldValue;

            try
            {
                switch (attribute)
                {
                    case "centerx":
                        oldValue = CenterX.ToString();
                        CenterX = int.Parse(value);
                        break;
                    case "centery":
                        oldValue = CenterY.ToString();
                        CenterY = int.Parse(value);
                        break;
                    case "color":
                        oldValue = Color.ToString();
                        Color = value.ParseColor();
                        break;
                    case "radius":
                        oldValue = Radius.ToString();
                        Radius = int.Parse(value);
                        break;
                    case "linethickness":
                        oldValue = LineThickness.ToString();
                        LineThickness = int.Parse(value);
                        break;
                    default:
                        throw new ArgumentException($"Unknown attribute: {attribute}");
                }
            }
            catch (FormatException e) { throw new ShapeAttributeCastException($"Cannot cast {value} to value of {attribute}", e, attribute, value); }

            return oldValue;
        }

        public override Point Center => (CenterX, CenterY);

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (base.wpfShape is not null)
                {
                    return base.wpfShape;
                }

                System.Windows.Shapes.Shape wpfShape = new System.Windows.Shapes.Ellipse
                {
                    Stroke = new SolidColorBrush(color.GetWPFColor()),      // we call it outline. They call it stroke
                    StrokeThickness = LineThickness,
                    Width = Radius * 2,
                    Height = Radius * 2
                };
                Canvas.SetTop(wpfShape, Top);
                Canvas.SetLeft(wpfShape, Left);

                base.wpfShape = wpfShape;               // memoize it, so that WindowToDraw can find it on Canvas
                return wpfShape;
            }
        }

        public override Dictionary<string, object> Accept()
        {
            return new()
            {
                { "radius", Radius }, { "centerX", CenterX }, { "centerY", CenterY }, { "color", Color }, { "lineThickness", LineThickness }
            };
        }

        public override void MoveXBy(int offset)
        {
            CenterX += offset;
        }

        public override void MoveYBy(int offset)
        {
            CenterY += offset;
        }

        public override Shape Clone()
        {
            return new Circle(this);
        }

        private int Top => CenterY - Radius;

        private int Left => CenterX - Radius;

        private int CenterX
        {
            get => centerX;
            set
            {
                centerX = value;
                base.Redraw();
            }
        }

        private int CenterY
        {
            get => centerY;
            set
            {
                centerY = value;
                base.Redraw();
            }
        }

        private int Radius
        {
            get => radius;
            set
            {
                radius = value;
                base.Redraw();
            }
        }

        private InternalColor Color
        {
            get => color;
            set
            {
                color = value;
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
    }
}
