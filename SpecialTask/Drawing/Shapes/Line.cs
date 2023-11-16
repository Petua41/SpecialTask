using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Exceptions;
using System.Windows.Controls;
using System.Windows.Media;
using static SpecialTask.Infrastructure.Extensoins.InternalColorExtensions;
using static SpecialTask.Infrastructure.Extensoins.KeyValuePairListExtension;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTask.Drawing.Shapes
{
    internal class Line : Shape
    {
        private int firstX;
        private int firstY;
        private int secondX;
        private int secondY;
        private InternalColor color;
        private int lineThickness;

        private static int firstAvailibleUniqueNumber = 0;

        public Line(int firstX, int firstY, int secondX, int secondY, InternalColor color, int lineThickness)
        {
            this.firstX = firstX;
            this.firstY = firstY;
            this.secondX = secondX;
            this.secondY = secondY;
            this.color = color;
            this.lineThickness = lineThickness;
            UniqueName = GetNextUniqueName();

            ATTRS_TO_EDIT = new() { { "firstX", "First X"}, { "firstY", "First Y" },
            { "secondX", "Second X" }, { "secondY", "Second Y" }, { "lineThickness", "Line thickness" },
            { "color", "Line color" } };
        }

        public Line(Line old) : this(old.FirstX, old.FirstY, old.SecondX, old.SecondY, old.Color, old.LineThickness) { }

        public static new string GetNextUniqueName()
        {
            return $"Line_{firstAvailibleUniqueNumber++}";
        }

        public override string Edit(string attribute, string value)
        {
            attribute = attribute.ToLower();
            string oldValue;

            try
            {
                switch (attribute)
                {
                    case "firstx":
                        oldValue = FirstX.ToString();
                        FirstX = int.Parse(value);
                        break;
                    case "firsty":
                        oldValue = FirstY.ToString();
                        FirstY = int.Parse(value);
                        break;
                    case "secondx":
                        oldValue = SecondX.ToString();
                        SecondX = int.Parse(value);
                        break;
                    case "secondy":
                        oldValue = SecondY.ToString();
                        SecondY = int.Parse(value);
                        break;
                    case "color":
                        oldValue = Color.ToString();
                        Color = value.ParseColor();
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

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (base.wpfShape is not null)
                {
                    return base.wpfShape;
                }

                System.Windows.Shapes.Shape wpfShape = new System.Windows.Shapes.Line
                {
                    Stroke = new SolidColorBrush(color.GetWPFColor()),      // we call it outline. They call it stroke
                    StrokeThickness = LineThickness,
                    X1 = FirstX,
                    Y1 = FirstY,
                    X2 = SecondX,
                    Y2 = SecondY
                };
                Canvas.SetTop(wpfShape, 0);
                Canvas.SetLeft(wpfShape, 0);

                base.wpfShape = wpfShape;
                return wpfShape;
            }
        }

        public override Dictionary<string, object> Accept()
        {
            return new()
            {
                { "firstX", FirstX }, { "firstY", FirstY }, { "secondX", SecondX },
                { "secondY", SecondY }, { "color", Color }, { "lineThickness", LineThickness }
            };
        }

        public override void MoveXBy(int offset)
        {
            FirstX += offset;
            SecondX += offset;
        }

        public override void MoveYBy(int offset)
        {
            FirstY += offset;
            SecondY += offset;
        }

        public override Shape Clone()
        {
            return new Line(this);
        }

        public override Point Center => ((FirstX + SecondX) / 2, (FirstY + SecondY) / 2);

        private int FirstX
        {
            get => firstX;
            set
            {
                firstX = value;
                base.Destroy(); base.Display(); ;
            }
        }

        private int FirstY
        {
            get => firstY;
            set
            {
                firstY = value;
                base.Destroy(); base.Display(); ;
            }
        }

        private int SecondX
        {
            get => secondX;
            set
            {
                secondX = value;
                base.Destroy(); base.Display(); ;
            }
        }

        private int SecondY
        {
            get => secondY;
            set
            {
                secondY = value;
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

        private int LineThickness
        {
            get => lineThickness;
            set
            {
                lineThickness = value;
                base.Destroy(); base.Display(); ;
            }
        }
    }
}
