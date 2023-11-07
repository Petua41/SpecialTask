using SpecialTask.Infrastructure;
using SpecialTask.Infrastructure.Enums;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpecialTask.Drawing.Shapes
{
    internal class Circle : Shape
    {
        private int radius;
        private int centerX;
        private int centerY;
        private EColor color;
        private int lineThickness;

        private static int firstAvailibleUniqueNumber = 0;

        public Circle(int centerX, int centerY, EColor color, int radius, int lineThickness)
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

        public override object Edit(string attribute, string value)
        {
            attribute = attribute.ToLower();
            object oldValue;

            try
            {
                switch (attribute)
                {
                    case "centerx":
                        oldValue = CenterX;
                        CenterX = int.Parse(value);
                        break;
                    case "centery":
                        oldValue = CenterY;
                        CenterY = int.Parse(value);
                        break;
                    case "color":
                        oldValue = Color;
                        Color = ColorsController.Parse(value);
                        break;
                    case "radius":
                        oldValue = Radius;
                        Radius = int.Parse(value);
                        break;
                    case "linethickness":
                        oldValue = LineThickness;
                        LineThickness = int.Parse(value);
                        break;
                    default:
                        throw new ArgumentException($"Unknown attribute: {attribute}");
                }
            }
            catch (FormatException) { throw new ShapeAttributeCastException(); }

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

        private EColor Color
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
