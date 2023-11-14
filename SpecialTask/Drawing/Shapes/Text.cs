using SpecialTask.Drawing.Shapes.WPF;
using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Exceptions;
using System.Windows.Controls;
using System.Windows.Media;
using static SpecialTask.Infrastructure.Extensoins.InternalColorExtensions;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTask.Drawing.Shapes
{
    internal class Text : Shape
    {
        private int leftTopX;
        private int leftTopY;
        private InternalColor color;
        private int fontSize;
        private string textValue;

        private static int firstAvailibleUniqueNumber = 0;

        public Text(int leftTopX, int leftTopY, int fontSize, string textValue, InternalColor color)
        {
            this.leftTopX = leftTopX;
            this.leftTopY = leftTopY;
            this.color = color;
            this.textValue = textValue;
            this.fontSize = fontSize;
            uniqueName = GetNextUniqueName();

            ATTRS_TO_EDIT = new() { { "leftTopX", "Left-top X"}, { "leftTopY", "Left-top Y" }, { "fontSize", "Font size" },
                { "text", "Text" }, { "color", "Text color" } };
        }

        public Text(Text old) : this(old.LeftTopX, old.LeftTopY, old.fontSize, old.textValue, old.Color) { }

        public static new string GetNextUniqueName()
        {
            return $"Text_{firstAvailibleUniqueNumber++}";
        }

        public override string Edit(string attribute, string value)
        {
            attribute = attribute.ToLower();
            string oldValue;

            try
            {
                switch (attribute)
                {
                    case "lefttopx":
                        oldValue = LeftTopX.ToString();
                        LeftTopX = int.Parse(value);
                        break;
                    case "lefttopy":
                        oldValue = LeftTopY.ToString();
                        LeftTopY = int.Parse(value);
                        break;
                    case "fontSize":
                        oldValue = FontSize.ToString();
                        FontSize = int.Parse(value);
                        break;
                    case "text":
                        oldValue = TextValue.ToString();
                        TextValue = value;
                        break;
                    case "color":
                        oldValue = Color.ToString();
                        Color = value.ParseColor();
                        break;
                    default:
                        throw new ArgumentException($"Unknown attribute: {attribute}");
                }
            }
            catch (FormatException) { throw new ShapeAttributeCastException($"Cannot cast {value} to value of {attribute}", attribute, value); }

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

                System.Windows.Shapes.Shape wpfShape = new WPFText
                {
                    Left = LeftTopX,
                    Top = LeftTopY,
                    FontSize = FontSize,
                    Text = TextValue,
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
            return new()
            {
                { "leftTopX", LeftTopX }, { "leftTopY", LeftTopY }, { "fontSize", FontSize },
                { "textValue", TextValue }, { "color", Color }
            };
        }

        public override void MoveXBy(int offset)
        {
            LeftTopX += offset;
        }

        public override void MoveYBy(int offset)
        {
            LeftTopY += offset;
        }

        public override Point Center => wpfShape is null ? (Point)(0, 0) : (Point)(LeftTopX + (int)(wpfShape.Width / 2), LeftTopY + (int)(wpfShape.Height / 2));

        private int LeftTopX
        {
            get => leftTopX;
            set
            {
                leftTopX = value;
                base.Redraw();
            }
        }

        private int LeftTopY
        {
            get => leftTopY;
            set
            {
                leftTopY = value;
                base.Redraw();
            }
        }

        private int FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;
                base.Redraw();
            }
        }

        private string TextValue
        {
            get => textValue;
            set
            {
                textValue = value;
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

        public override Shape Clone()
        {
            return new Text(this);
        }
    }
}
