using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
		Black,
		Blue
	} // TODO

	static class ColorsController
	{
		private static readonly Dictionary<string, EColor> colorNames = new()
		{ 
			{ "green", EColor.Green }, { "magenta", EColor.Magenta }, { "red", EColor.Red }, { "white", EColor.White }, { "yellow", EColor.Yellow },
			{ "purple", EColor.Purple }, { "black", EColor.Black }, { "blue", EColor.Blue }
		};

		private static readonly Dictionary<EColor, Color> wpfColors = new()
		{
			{ EColor.Green, Colors.LimeGreen }, { EColor.Magenta, Colors.Magenta }, { EColor.Red, Colors.Red }, { EColor.White, Colors.White },
			{ EColor.Yellow, Colors.Yellow }, { EColor.Purple, Color.FromRgb(128, 0, 128) }, { EColor.Black, Colors.Black }, { EColor.Blue, Colors.Blue }
		};

		public static Color GetWPFColor(this EColor color)
		{
			try { return wpfColors[color]; }
			catch (KeyNotFoundException) { return Colors.Transparent; }
		}

		public static EColor Parse(string colorString)
		{
			colorString = colorString.Trim().ToLower();
			try { return colorNames[colorString]; }
			catch (KeyNotFoundException) { return EColor.None; }
		}

		public static List<string> GetColorsList()
		{
			return colorNames.Keys.ToList();
		}
	}

	abstract class Shape
	{
		private static int firstAvailibleUniqueNumber = 0;
		private System.Windows.Shapes.Shape? wpfShape;

		public static string GetNextUniqueName()
		{
			return string.Format("Unknown_Shape_{0}", firstAvailibleUniqueNumber++);
		}

		public abstract object Edit(string attribute, object value);

		public abstract string UniqueName { get; }

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

		public abstract MyMap<string, string> AttributesToEditWithNames { get; }
	}

	class Circle : Shape
	{
		private int radius;
		private int centerX;
		private int centerY;
		private EColor color;
		private int lineThickness;
		private readonly string uniqueName;
		private System.Windows.Shapes.Shape? wpfShape = null;

		private readonly MyMap<string, string> ATTRS_TO_EDIT = new() { { "centerX", "Center X"}, { "centerY", "Center Y" },
			{ "radius", "Radius" }, { "lineThickness", "Outline thickness" }, { "color", "Outline color" } };

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
			return $"Circle_{firstAvailibleUniqueNumber++}";
		}

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

		public override string UniqueName => uniqueName;

		public override MyMap<string, string> AttributesToEditWithNames => ATTRS_TO_EDIT;
    }

	class Square : Shape
	{
		private int leftTopX;
		private int leftTopY;
		private int rightBottomX;
		private int rightBottomY;
		private EColor color;
		private int lineThickness;
		private readonly string uniqueName;
		private System.Windows.Shapes.Shape? wpfShape = null;

		private static int firstAvailibleUniqueNumber = 0;

        private readonly MyMap<string, string> ATTRS_TO_EDIT = new() { { "leftTopX", "Left-top X"}, { "leftTopY", "Left-top Y" },
            { "rightBottomX", "Right-bottom X" }, { "rightBottomY", "Right-bottom Y" }, { "lineThickness", "Outline thickness" }, 
			{ "color", "Outline color" } };

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
			return $"Square_{firstAvailibleUniqueNumber++}";
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

        public override string UniqueName => uniqueName;

        public override MyMap<string, string> AttributesToEditWithNames => ATTRS_TO_EDIT;
    }

	class Line : Shape
	{
		private int firstX;
		private int firstY;
		private int secondX;
		private int secondY;
		private EColor color;
		private int lineThickness;
		private readonly string uniqueName;
		private System.Windows.Shapes.Shape? wpfShape = null;

		private static int firstAvailibleUniqueNumber = 0;

        private readonly MyMap<string, string> ATTRS_TO_EDIT = new() { { "firstX", "First X"}, { "firstY", "First Y" },
            { "secondX", "Second X" }, { "secondY", "Second Y" }, { "lineThickness", "Line thickness" },
            { "color", "Line color" } };

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
			return $"Line_{firstAvailibleUniqueNumber++}";
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

        public override string UniqueName => uniqueName;

        public override MyMap<string, string> AttributesToEditWithNames => ATTRS_TO_EDIT;
    }

	class SelectionMarker : Shape
	{
		private System.Windows.Shapes.Shape? wpfShape;
		private readonly Brush brush = new GeometryTileTexture(new EllipseGeometry(new(5, 5), 5, 5)).Brush(Colors.Black);
		private readonly Square square;
		private readonly string uniqueName;

        private static int firstAvailibleUniqueNumber = 0;

        public SelectionMarker(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY)
		{
			square = new(leftTopX, leftTopY, rightBottomX, rightBottomY, EColor.Black, 1);
			uniqueName = GetNextUniqueName();

			WindowManager.Instance.DisplayOnCurrentWindow(this);

			DestroyAfterDelay(5000);
		}

		public static new string GetNextUniqueName()
		{
			return $"SelectionMarker_{firstAvailibleUniqueNumber++}";
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

		public override string UniqueName => uniqueName;

        public override MyMap<string, string> AttributesToEditWithNames => new();
    }

	class Text: Shape
	{
        private int leftTopX;
        private int leftTopY;
        private EColor color;
        private int fontSize;
		private string textValue;
        private readonly string uniqueName;
        private System.Windows.Shapes.Shape? wpfShape = null;

        private static int firstAvailibleUniqueNumber = 0;

        private readonly MyMap<string, string> ATTRS_TO_EDIT = new() { { "leftTopX", "Left-top X"}, { "leftTopY", "Left-top Y" },
            { "fontSize", "Font size" }, { "text", "Text" }, { "color", "Line color" } };

        public Text(int leftTopX, int leftTopY, int fontSize, string textValue, EColor color)
        {
            this.leftTopX = leftTopX;
            this.leftTopY = leftTopY;
            this.color = color;
			this.textValue = textValue;
            this.fontSize = fontSize;
            uniqueName = GetNextUniqueName();

            WindowManager.Instance.DisplayOnCurrentWindow(this);
        }

        public Text(Text old) : this(old.LeftTopX, old.LeftTopY, old.fontSize, old.textValue, old.Color) { }

        public static new string GetNextUniqueName()
        {
			return $"Text_{firstAvailibleUniqueNumber++}";
		}

        public override (int, int) Center
        {
            get
			{
				if (wpfShape == null) return (0, 0);
				return (leftTopX + (int)(wpfShape.Width / 2), leftTopY + (int)(wpfShape.Height / 2));
			}
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
                    case "fontSize":
                        oldValue = FontSize;
                        FontSize = (int)value;
                        break;
                    case "text":
                        oldValue = TextValue;
                        TextValue = (string)value;
                        break;
                    case "color":
                        oldValue = Color;
                        Color = (EColor)value;
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

                this.wpfShape = wpfShape;
                return wpfShape;
            }
        }

        public override Dictionary<string, object> Accept()
        {
            return new()
            {
                { "leftTopX", leftTopX }, { "leftTopY", leftTopY }, { "fontSize", fontSize },
                { "textValue", textValue }, { "color", color }
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

        private int LeftTopX
        {
            get => leftTopX;
            set
            {
                leftTopX = value;
                Redraw();
            }
        }

        private int LeftTopY
        {
            get => leftTopY;
            set
            {
                leftTopY = value;
                Redraw();
            }
        }

        private int FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;
                Redraw();
            }
        }

        private string TextValue
        {
            get => textValue;
            set
            {
                textValue = value;
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

        public override void NullifyWPFShape()
        {
            wpfShape = null;
        }

        public override Shape Clone()
        {
            return new Text(this);
        }

        public override string UniqueName => uniqueName;

        public override MyMap<string, string> AttributesToEditWithNames => ATTRS_TO_EDIT;
    }

	class WPFText: System.Windows.Shapes.Shape
	{
		private int leftTopX = 0;
		private int leftTopY = 0;
		private int fontSize = 0;
		private string textValue = "";
		private Brush brush = Brushes.Transparent;

		private readonly Typeface typeface = new("Calibri");
        private readonly CultureInfo cultureInfo = CultureInfo.CurrentCulture;
        private readonly FlowDirection flowDirection = FlowDirection.LeftToRight;
		private const int DIP = 1;

        protected override void OnRender(DrawingContext drawingContext)
        {
			Point point = new(leftTopX, leftTopY);
			drawingContext.DrawText(FormText, point);
        }

		public int Left
		{
			get => leftTopX;
			set => leftTopX = value;
		}

		public int Top
		{
			get => leftTopY;
			set => leftTopY = value;
		}

		public int FontSize
		{
			get => fontSize;
			set => fontSize = value;
		}

		public string Text
		{
			get => textValue;
			set => textValue = value;
		}

		public new Brush Stroke
		{
			get => brush;
			set => brush = value;
		}

		public new int Width => (int)DefiningGeometry.Bounds.Width;

		public new int Height => (int)DefiningGeometry.Bounds.Height;

        protected override Geometry DefiningGeometry => FormText.BuildHighlightGeometry(new(Left, Top));

		private FormattedText FormText
		{
			get
			{
                return new(Text, cultureInfo, flowDirection, typeface, FontSize, Stroke, DIP);
            }
        }

    }
}
