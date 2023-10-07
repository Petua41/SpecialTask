using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpecialTask
{
    public enum EColor
    {
        None,
        Green,
        Magenta,
        Red,
        White,
        Yellow,
        Purple,
        Black
    } // TODO

    static class ColorsController
    {
        public static System.Windows.Media.Color GetWPFColor(this EColor color)
        {
            return color switch
            {
                EColor.None => Colors.Transparent,
                EColor.Green => Colors.LimeGreen,
                EColor.Magenta => Colors.Magenta,
                EColor.Red => Colors.Red,
                EColor.White => Colors.White,
                EColor.Yellow => Colors.Yellow,
                EColor.Purple => System.Windows.Media.Color.FromRgb(128, 0, 128),
                EColor.Black => Colors.Black,
                _ => throw new ColorExcepttion(),
            };
        }

        public static EColor Parse(string colorString)
        {
            colorString = colorString.Trim().ToLower();
            return colorString switch
            {
                "green" => EColor.Green,
                "magenta" => EColor.Magenta,
                "red" => EColor.Red,
                "white" => EColor.White,
                "yellow" => EColor.Yellow,
                "purple" => EColor.Purple,
                "black" => EColor.Black,
                _ => EColor.None,
            };
        }

        public static List<string> GetColorsList()
        {
            return new() { "green", "magenta", "red", "white", "yellow", "purple", "black" };
        }
    }

    /// <summary>
    /// Представляет абстрактный класс для всех фигур
    /// </summary>
    abstract class Shape
    {
        private static int firstAvailibleUniqueNumber = 0;
        public string uniqueName;
        private System.Windows.Shapes.Shape? wpfShape;

        public Shape()
        {
            uniqueName = GetNextUniqueName();
        }

        public static string GetNextUniqueName()
        {
            return string.Format("Unknown_Shape_{0}", firstAvailibleUniqueNumber++);
        }

        public abstract object Edit(string attribute, object value);

        public string UniqueName
        {
            get => uniqueName;
        }

        /// <summary>
        /// Windows.Shapes.Shape instance that can be added to Canvas
        /// </summary>
        public abstract System.Windows.Shapes.Shape WPFShape { get; }

        public abstract (int, int) Center { get; }

        public virtual void Redraw()		// It`s template method
        {
            Destroy();
            NullifyWPFShape();
            WindowManager.Instance.DisplayOnCurrentWindow(this);
        }
        public virtual void Destroy()
        {
            WindowManager.Instance.RemoveFromCurrentWindow(this);
        }

        public virtual void NullifyWPFShape()
        {
            wpfShape = null;
        }

        public abstract Dictionary<string, object> Accept();

        public abstract void MoveXBy(int offset);

        public abstract void MoveYBy(int offset);

        public abstract Shape Clone();
    }

    class Circle : Shape
    {
        private int radius;
        private int centerX;
        private int centerY;
        private EColor color;
        private int lineThickness;
        public new string uniqueName;
        private System.Windows.Shapes.Shape? wpfShape = null;

        private static int firstAvailibleUniqueNumber = 0;

        public Circle(int centerX, int centerY, EColor color, int radius, int lineThickness)
        {
            this.centerX = centerX;
            this.centerY = centerY;
            this.color = color;
            this.radius = radius;
            this.lineThickness = lineThickness;
            uniqueName = GetNextUniqueName();

            WindowManager.Instance.DisplayOnCurrentWindow(this);
        }

        public Circle(Circle old) : this(old.CenterX, old.CenterY, old.Color, old.Radius, old.LineThickness) { }

        public static new string GetNextUniqueName()
        {
            return string.Format("Circle_{0}", firstAvailibleUniqueNumber++);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="InvalidShapeAttributeException">Редактирование несуществующего атрибута</exception>
        /// <exception cref="ShapeAttributeCastException">Невозможно привести атрибут к нужному типу</exception>
        public override object Edit(string attribute, object value)
        {
            attribute = attribute.ToLower();
            object oldValue;

            try
            {
                switch (attribute)
                {
                    case "centerx":
                        oldValue = CenterX;
                        CenterX = (int)value;
                        break;
                    case "centery":
                        oldValue = CenterY;
                        CenterY = (int)value;
                        break;
                    case "color":
                        oldValue = Color;
                        Color = (EColor)value;
                        break;
                    case "radius":
                        oldValue = Radius;
                        Radius = (int)value;
                        break;
                    case "linethickness":
                        oldValue = LineThickness;
                        LineThickness = (int)value;
                        break;
                    default:
                        throw new InvalidShapeAttributeException();
                }
            }
            catch (InvalidCastException) { throw new ShapeAttributeCastException(); }

            return oldValue;
        }

        public override (int, int) Center
        {
            get => (CenterX, CenterY);
        }

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (this.wpfShape != null) return this.wpfShape;

                System.Windows.Shapes.Shape wpfShape = new System.Windows.Shapes.Ellipse
                {
                    Stroke = new SolidColorBrush(color.GetWPFColor()),      // we call it outline. They call it stroke
                    StrokeThickness = LineThickness,
                    Width = Radius * 2,
                    Height = Radius * 2
                };
                Canvas.SetTop(wpfShape, Top);
                Canvas.SetLeft(wpfShape, Left);

                this.wpfShape = wpfShape;               // memoize it, so that WindowToDraw can find it on Canvas
                return wpfShape;
            }
        }

        public override Dictionary<string, object> Accept()
        {
            return new()
            {
                { "radius", radius }, { "centerX", centerX }, { "centerY", centerY }, { "color", color }, { "lineThickness", lineThickness }
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

        private int Radius
        {
            get => radius;
            set
            {
                if (value < 0) throw new ShapeValueException();
                radius = value;
                Redraw();
            }
        }

        private int Top
        {
            get => CenterY - Radius;
        }

        private int Left
        {
            get => CenterX - Radius;
        }

        private int CenterX
        {
            get => centerX;
            set
            {
                centerX = value;
                Redraw();
            }
        }
        private int CenterY
        {
            get => centerY;
            set
            {
                centerY = value;
                Redraw();
            }
        }
        private EColor Color
        {
            get => color;
            set
            {
                color = value;
                Redraw();
            }
        }

        private int LineThickness
        {
            get => lineThickness;
            set
            {
                lineThickness = value;
                Redraw();
            }
        }

        public override void NullifyWPFShape()
        {
            wpfShape = null;
        }

        public override Shape Clone()
        {
            return new Circle(this);
        }
    }

    class Square : Shape
    {
        private int leftTopX;
        private int leftTopY;
        private int rightBottomX;
        private int rightBottomY;
        private EColor color;
        private int lineThickness;
        public new string uniqueName;
        private System.Windows.Shapes.Shape? wpfShape = null;

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

            WindowManager.Instance.DisplayOnCurrentWindow(this);
        }

        public Square(Square old) : this(old.LeftTopX, old.LeftTopY, old.RightBottomX, old.RightBottomY, old.Color, old.LineThickness) { }

        public static new string GetNextUniqueName()
        {
            return string.Format("Square_{0}", firstAvailibleUniqueNumber++);
        }

        public override (int, int) Center
        {
            get => ((leftTopX + rightBottomX) / 2, (leftTopY + rightBottomY) / 2);
        }

        public override object Edit(string attribute, object value)
        {
            attribute = attribute.ToLower();
            object oldValue;

            try
            {
                switch (attribute)
                {
                    case "lefttopx":
                        oldValue = LeftTopX;
                        LeftTopX = (int)value;
                        break;
                    case "lefttopy":
                        oldValue = LeftTopY;
                        LeftTopY = (int)value;
                        break;
                    case "rightbottomx":
                        oldValue = RightBottomX;
                        RightBottomX = (int)value;
                        break;
                    case "rightbottomy":
                        oldValue = RightBottomY;
                        RightBottomY = (int)value;
                        break;
                    case "color":
                        oldValue = Color;
                        Color = (EColor)value;
                        break;
                    case "linethickness":
                        oldValue = LineThickness;
                        LineThickness = (int)value;
                        break;
                    default:
                        throw new InvalidShapeAttributeException();
                }
            }
            catch (InvalidCastException) { throw new ShapeAttributeCastException(); }

            return oldValue;
        }

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (this.wpfShape != null) return this.wpfShape;

                System.Windows.Shapes.Shape wpfShape = new System.Windows.Shapes.Rectangle
                {
                    Stroke = new SolidColorBrush(color.GetWPFColor()),      // we call it outline. They call it stroke
                    StrokeThickness = LineThickness,
                    Width = Width,
                    Height = Height
                };
                Canvas.SetTop(wpfShape, LeftTopY);
                Canvas.SetLeft(wpfShape, LeftTopX);

                this.wpfShape = wpfShape;
                return wpfShape;
            }
        }

        public override Dictionary<string, object> Accept()
        {
            return new()
            {
                { "leftTopX", leftTopX }, { "leftTopY", leftTopY }, { "rightBottomX", rightBottomX },
                { "rightBottomY", rightBottomY }, { "color", color }, { "lineThickness", lineThickness }
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
                Redraw();
            }
        }

        protected int LeftTopY
        {
            get => leftTopY;
            set
            {
                leftTopY = value;
                Redraw();
            }
        }

        protected int RightBottomX
        {
            get => rightBottomX;
            set
            {
                rightBottomX = value;
                Redraw();
            }
        }

        protected int RightBottomY
        {
            get => rightBottomY;
            set
            {
                rightBottomY = value;
                Redraw();
            }
        }

        protected EColor Color
        {
            get => color;
            set
            {
                color = value;
                Redraw();
            }
        }

        protected int LineThickness
        {
            get => lineThickness;
            set
            {
                lineThickness = value;
                Redraw();
            }
        }

        public override void NullifyWPFShape()
        {
            wpfShape = null;
        }

        public override Shape Clone()
        {
            return new Square(this);
        }
    }

    class Line : Shape
    {
        private int firstX;
        private int firstY;
        private int secondX;
        private int secondY;
        private EColor color;
        private int lineThickness;
        public new string uniqueName;
        private System.Windows.Shapes.Shape? wpfShape = null;

        private static int firstAvailibleUniqueNumber = 0;

        public Line(int firstX, int firstY, int secondX, int secondY, EColor color, int lineThickness)
        {
            this.firstX = firstX;
            this.firstY = firstY;
            this.secondX = secondX;
            this.secondY = secondY;
            this.color = color;
            this.lineThickness = lineThickness;
            uniqueName = GetNextUniqueName();

            WindowManager.Instance.DisplayOnCurrentWindow(this);
        }

        public Line(Line old) : this(old.FirstX, old.FirstY, old.SecondX, old.SecondY, old.Color, old.LineThickness) { }

        public static new string GetNextUniqueName()
        {
            return string.Format("Line_{0}", firstAvailibleUniqueNumber++);
        }

        public override (int, int) Center
        {
            get => ((firstX + secondX) / 2, (firstY + secondY) / 2);
        }

        public override object Edit(string attribute, object value)
        {
            attribute = attribute.ToLower();
            object oldValue;

            try
            {
                switch (attribute)
                {
                    case "firstx":
                        oldValue = FirstX;
                        FirstX = (int)value;
                        break;
                    case "firsty":
                        oldValue = FirstY;
                        FirstY = (int)value;
                        break;
                    case "secondx":
                        oldValue = SecondX;
                        SecondX = (int)value;
                        break;
                    case "secondy":
                        oldValue = SecondY;
                        SecondY = (int)value;
                        break;
                    case "color":
                        oldValue = Color;
                        Color = (EColor)value;
                        break;
                    case "linethickness":
                        oldValue = LineThickness;
                        LineThickness = (int)value;
                        break;
                    default:
                        throw new InvalidShapeAttributeException();
                }
            }
            catch (InvalidCastException) { throw new ShapeAttributeCastException(); }

            return oldValue;
        }

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (this.wpfShape != null) return this.wpfShape;

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

                this.wpfShape = wpfShape;
                return wpfShape;
            }
        }

        public override Dictionary<string, object> Accept()
        {
            return new()
            {
                { "firstX", firstX }, { "firstY", firstY }, { "secondX", secondX },
                { "secondY", secondY }, { "color", color }, { "lineThickness", lineThickness }
            };
        }

        public override void MoveXBy(int offset)
        {
            FirstX += offset;
            SecondX += offset;         // FIXME
        }

        public override void MoveYBy(int offset)
        {
            FirstY += offset;
            SecondY += offset;      // FIXME
        }

        private int FirstX
        {
            get => firstX;
            set
            {
                firstX = value;
                Redraw();
            }
        }

        private int FirstY
        {
            get => firstY;
            set
            {
                firstY = value;
                Redraw();
            }
        }

        private int SecondX
        {
            get => secondX;
            set
            {
                secondX = value;
                Redraw();
            }
        }

        private int SecondY
        {
            get => secondY;
            set
            {
                secondY = value;
                Redraw();
            }
        }

        private EColor Color
        {
            get => color;
            set
            {
                color = value;
                Redraw();
            }
        }

        private int LineThickness
        {
            get => lineThickness;
            set
            {
                lineThickness = value;
                Redraw();
            }
        }

        public override void NullifyWPFShape()
        {
            wpfShape = null;
        }

        public override Shape Clone()
        {
            return new Line(this);
        }
    }

    class SelectionMarker : Shape
    {
        static int firstAvailibleUniqueNumber = 0;
        private System.Windows.Shapes.Shape? wpfShape;
        private readonly Brush brush = new GeometryTileTexture(new EllipseGeometry(new(5, 5), 5, 5)).Brush(Colors.Black);
        private readonly Square square;

        public SelectionMarker(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY)
        {
            square = new(leftTopX, leftTopY, rightBottomX, rightBottomY, EColor.Black, 1);

            WindowManager.Instance.DisplayOnCurrentWindow(this);

            DestroyAfterDelay(5000);
        }

        public SelectionMarker(SelectionMarker old)
        {
            square = new(old.square);

            WindowManager.Instance.DisplayOnCurrentWindow(this);

            Task task = Task.Delay(5_000);
            task.Start();
        }

        public static new string GetNextUniqueName()
        {
            return string.Format("SelectionMarker_{0}", firstAvailibleUniqueNumber++);
        }

        public override object Edit(string attribute, object value)
        {
            throw new SelectionMarkerException();
        }

        public override Dictionary<string, object> Accept()
        {
            throw new SelectionMarkerException();
        }

        public override void MoveXBy(int offset)
        {
            throw new SelectionMarkerException();
        }

        public override void MoveYBy(int offset)
        {
            throw new SelectionMarkerException();
        }

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (this.wpfShape != null) return this.wpfShape;

                System.Windows.Shapes.Shape wpfShape = square.WPFShape;
                wpfShape.Stroke = brush;

                square.Destroy();

                this.wpfShape = wpfShape;
                return wpfShape;
            }
        }

        public override (int, int) Center => square.Center;

        private async void DestroyAfterDelay(int delay)
        {
            await Task.Delay(delay);

            Destroy();
        }

        public override void Destroy()
        {
            square.Destroy();
            base.Destroy();
        }

        public override Shape Clone()
        {
            throw new SelectionMarkerException();
        }
    }
}
