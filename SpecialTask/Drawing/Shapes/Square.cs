using SpecialTask.Exceptions;
using SpecialTask.Infrastructure;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpecialTask.Drawing.Shapes
{
    class Square : Shape
    {
        private int leftTopX;
        private int leftTopY;
        private int rightBottomX;
        private int rightBottomY;
        private EColor color;
        private int lineThickness;

        private static int firstAvailibleUniqueNumber = 0;

        public Square(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY, EColor color, int lineThickness)
        {
            this.leftTopX = leftTopX;
            this.leftTopY = leftTopY;
            this.rightBottomX = rightBottomX;
            this.rightBottomY = rightBottomY;
            this.color = color;
            this.lineThickness = lineThickness;
            uniqueName = GetNextUniqueName();

            ATTRS_TO_EDIT = new() { { "leftTopX", "Left-top X"}, { "leftTopY", "Left-top Y" },
            { "rightBottomX", "Right-bottom X" }, { "rightBottomY", "Right-bottom Y" }, { "lineThickness", "Outline thickness" },
            { "color", "Outline color" } };
        }

        public Square(Square old) : this(old.LeftTopX, old.LeftTopY, old.RightBottomX, old.RightBottomY, old.Color, old.LineThickness) { }

        public static new string GetNextUniqueName()
        {
            return $"Rectangle_{firstAvailibleUniqueNumber++}";
        }

        public override Point Center
        {
            get => ((leftTopX + rightBottomX) / 2, (leftTopY + rightBottomY) / 2);
        }

        public override object Edit(string attribute, string value)
        {
            attribute = attribute.ToLower();
            object oldValue;

            try
            {
                switch (attribute)
                {
                    case "lefttopx":
                        oldValue = LeftTopX;
                        LeftTopX = int.Parse(value);
                        break;
                    case "lefttopy":
                        oldValue = LeftTopY;
                        LeftTopY = int.Parse(value);
                        break;
                    case "rightbottomx":
                        oldValue = RightBottomX;
                        RightBottomX = int.Parse(value);
                        break;
                    case "rightbottomy":
                        oldValue = RightBottomY;
                        RightBottomY = int.Parse(value);
                        break;
                    case "color":
                        oldValue = Color;
                        Color = ColorsController.Parse(value);
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

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (base.wpfShape is not null) return base.wpfShape;

                System.Windows.Shapes.Shape wpfShape = new System.Windows.Shapes.Rectangle
                {
                    Stroke = new SolidColorBrush(color.GetWPFColor()),      // we call it outline. They call it stroke
                    StrokeThickness = LineThickness,
                    Width = Width,
                    Height = Height
                };
                Canvas.SetTop(wpfShape, LeftTopY);
                Canvas.SetLeft(wpfShape, LeftTopX);

                base.wpfShape = wpfShape;
                return wpfShape;
            }
        }

        public override Dictionary<string, object> Accept()
        {
            return new()
            {
                { "leftTopX", LeftTopX }, { "leftTopY", LeftTopY }, { "rightBottomX", RightBottomX },
                { "rightBottomY", RightBottomY }, { "color", Color }, { "lineThickness", LineThickness }
            };
        }

        public override void MoveXBy(int offset)
        {
            LeftTopX += offset;
            RightBottomX += offset;
        }

        public override void MoveYBy(int offset)
        {
            LeftTopY += offset;
            RightBottomY += offset;
        }

        protected int Width
        {
            get => Math.Abs(RightBottomX - LeftTopX);
        }

        protected int Height
        {
            get => Math.Abs(RightBottomY - LeftTopY);
        }

        protected int LeftTopX
        {
            get => leftTopX;
            set
            {
                leftTopX = value;
                base.Redraw();
            }
        }

        protected int LeftTopY
        {
            get => leftTopY;
            set
            {
                leftTopY = value;
                base.Redraw();
            }
        }

        protected int RightBottomX
        {
            get => rightBottomX;
            set
            {
                rightBottomX = value;
                base.Redraw();
            }
        }

        protected int RightBottomY
        {
            get => rightBottomY;
            set
            {
                rightBottomY = value;
                base.Redraw();
            }
        }

        protected EColor Color
        {
            get => color;
            set
            {
                color = value;
                base.Redraw();
            }
        }

        protected int LineThickness
        {
            get => lineThickness;
            set
            {
                lineThickness = value;
                base.Redraw();
            }
        }

        public override Shape Clone()
        {
            return new Square(this);
        }
    }
}
