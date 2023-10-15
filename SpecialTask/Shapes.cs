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
	public enum EColor				// it`s standard ANSI colors (values are similar to ones in xterm)
	{
		None, Purple,				// with this line added
		
		Black, Red, Green, Yellow, Blue,
		Magenta, Cyan, White, Gray, BrightRed,
		BrightGreen, BrightYellow, BrightBlue,
		BrightMagenta, BrightCyan, BrightWhite
	}

	static class ColorsController
	{
		private static readonly Dictionary<string, EColor> colorNames = new()
		{
			{ "purple", EColor.Purple }, { "black", EColor.Black }, { "red", EColor.Red }, { "green", EColor.Green }, { "yellow", EColor.Yellow },
			{ "blue", EColor.Blue }, { "magenta", EColor.Magenta }, { "cyan", EColor.Cyan }, { "white", EColor.White }, { "gray", EColor.Gray },
			{ "brightred", EColor.BrightRed }, { "brightgreen", EColor.BrightGreen }, { "brightyellow", EColor.BrightYellow },
			{ "brightblue", EColor.BrightBlue }, { "brightmagenta", EColor.BrightMagenta }, { "brightcyan", EColor.BrightCyan },
			{ "brightwhite", EColor.BrightWhite }
		};

		private static readonly Dictionary<EColor, Color> wpfColors = new()
		{
			{ EColor.Purple, Color.FromRgb(128, 0, 128) }, { EColor.Black, Colors.Black }, { EColor.Red, Color.FromRgb(205, 0, 0) },
			{ EColor.Green, Color.FromRgb(0, 205, 0) }, { EColor.Yellow, Color.FromRgb(205, 205, 0) }, { EColor.Blue, Color.FromRgb(0, 0, 238) },
			{ EColor.Magenta, Color.FromRgb(205, 0, 205) }, { EColor.Cyan, Color.FromRgb(0, 205, 205) }, { EColor.White, Color.FromRgb(229, 229, 229) },
			{ EColor.Gray, Color.FromRgb(127, 127, 127) }, { EColor.BrightRed, Colors.Red }, {EColor.BrightGreen, Color.FromRgb(0, 255, 0) },
			{ EColor.BrightYellow, Colors.Yellow }, { EColor.BrightBlue, Color.FromRgb(92, 92, 255) }, { EColor.BrightMagenta, Colors.Magenta },
			{ EColor.BrightCyan, Colors.Cyan }, { EColor.BrightWhite, Colors.White }
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
		protected System.Windows.Shapes.Shape? wpfShape;
		protected string uniqueName = "";
		protected MyMap<string, string> ATTRS_TO_EDIT = new();

		public static string GetNextUniqueName()
		{
			return $"Unknown_Shape_{firstAvailibleUniqueNumber++}";
		}

		public abstract object Edit(string attribute, string value);

		public virtual string UniqueName => uniqueName;

		/// <summary>
		/// Windows.Shapes.Shape instance that can be added to Canvas
		/// </summary>
		public abstract System.Windows.Shapes.Shape WPFShape { get; }

		public abstract (int, int) Center { get; }

		// It`s kinda template method
        public virtual void Redraw()
		{
			Destroy();
			NullifyWPFShape();
			WindowManager.Instance.DisplayOnCurrentWindow(this);
		}

		public virtual void Destroy()
		{
			WindowManager.Instance.RemoveFromCurrentWindow(this);
		}

		protected virtual void NullifyWPFShape()
		{
			wpfShape = null;
		}

		public abstract Dictionary<string, object> Accept();

		public abstract void MoveXBy(int offset);

		public abstract void MoveYBy(int offset);

		public abstract Shape Clone();

		public virtual MyMap<string, string> AttributesToEditWithNames => ATTRS_TO_EDIT;
	}

	class Circle : Shape
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

            WindowManager.Instance.DisplayOnCurrentWindow(this);
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
						throw new InvalidShapeAttributeException();
				}
			}
			catch (FormatException) { throw new ShapeAttributeCastException(); }

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
				if (base.wpfShape != null) return base.wpfShape;

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
                if (value < 0) throw new ShapeValueException();
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

            WindowManager.Instance.DisplayOnCurrentWindow(this);
		}

		public Square(Square old) : this(old.LeftTopX, old.LeftTopY, old.RightBottomX, old.RightBottomY, old.Color, old.LineThickness) { }

		public static new string GetNextUniqueName()
		{
			return $"Rectangle_{firstAvailibleUniqueNumber++}";
		}

		public override (int, int) Center
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
						throw new InvalidShapeAttributeException();
				}
			}
			catch (FormatException) { throw new ShapeAttributeCastException(); }

			return oldValue;
		}

		public override System.Windows.Shapes.Shape WPFShape
		{
			get
			{
				if (base.wpfShape != null) return base.wpfShape;

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

	class Line : Shape
	{
		private int firstX;
		private int firstY;
		private int secondX;
		private int secondY;
		private EColor color;
		private int lineThickness;

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

            ATTRS_TO_EDIT = new() { { "firstX", "First X"}, { "firstY", "First Y" },
            { "secondX", "Second X" }, { "secondY", "Second Y" }, { "lineThickness", "Line thickness" },
            { "color", "Line color" } };

            WindowManager.Instance.DisplayOnCurrentWindow(this);
		}

		public Line(Line old) : this(old.FirstX, old.FirstY, old.SecondX, old.SecondY, old.Color, old.LineThickness) { }

		public static new string GetNextUniqueName()
		{
			return $"Line_{firstAvailibleUniqueNumber++}";
		}

		public override object Edit(string attribute, string value)
		{
			attribute = attribute.ToLower();
			object oldValue;

			try
			{
				switch (attribute)
				{
					case "firstx":
						oldValue = FirstX;
						FirstX = int.Parse(value);
						break;
					case "firsty":
						oldValue = FirstY;
						FirstY = int.Parse(value);
						break;
					case "secondx":
						oldValue = SecondX;
						SecondX = int.Parse(value);
						break;
					case "secondy":
						oldValue = SecondY;
						SecondY = int.Parse(value);
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
						throw new InvalidShapeAttributeException();
				}
			}
			catch (FormatException) { throw new ShapeAttributeCastException(); }

			return oldValue;
		}

		public override System.Windows.Shapes.Shape WPFShape
		{
			get
			{
				if (base.wpfShape != null) return base.wpfShape;

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

        public override (int, int) Center
        {
            get => ((FirstX + SecondX) / 2, (FirstY + SecondY) / 2);
        }

        private int FirstX
		{
			get => firstX;
			set
			{
				firstX = value;
				base.Redraw();
			}
		}

		private int FirstY
		{
			get => firstY;
			set
			{
				firstY = value;
				base.Redraw();
			}
		}

		private int SecondX
		{
			get => secondX;
			set
			{
				secondX = value;
				base.Redraw();
			}
		}

		private int SecondY
		{
			get => secondY;
			set
			{
				secondY = value;
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

		public override Shape Clone()
		{
			return new Line(this);
        }
    }

	class SelectionMarker : Shape
	{
		private readonly Brush brush = new GeometryTileTexture(new EllipseGeometry(new(5, 5), 5, 5)).Brush(Colors.Black);
		private readonly Square square;

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

        public override object Edit(string attribute, string value)
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
				if (base.wpfShape != null) return base.wpfShape;

				System.Windows.Shapes.Shape wpfShape = square.WPFShape;
				wpfShape.Stroke = brush;

				square.Destroy();

				base.wpfShape = wpfShape;
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

        public override MyMap<string, string> AttributesToEditWithNames => new();
    }

	class Text: Shape
	{
        private int leftTopX;
        private int leftTopY;
        private EColor color;
        private int fontSize;
		private string textValue;

        private static int firstAvailibleUniqueNumber = 0;

        public Text(int leftTopX, int leftTopY, int fontSize, string textValue, EColor color)
        {
            this.leftTopX = leftTopX;
            this.leftTopY = leftTopY;
            this.color = color;
			this.textValue = textValue;
            this.fontSize = fontSize;
            uniqueName = GetNextUniqueName();

            ATTRS_TO_EDIT = new() { { "leftTopX", "Left-top X"}, { "leftTopY", "Left-top Y" }, { "fontSize", "Font size" }, 
				{ "text", "Text" }, { "color", "Text color" } };

            WindowManager.Instance.DisplayOnCurrentWindow(this);
        }

        public Text(Text old) : this(old.LeftTopX, old.LeftTopY, old.fontSize, old.textValue, old.Color) { }

        public static new string GetNextUniqueName()
        {
			return $"Text_{firstAvailibleUniqueNumber++}";
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
                    case "fontSize":
                        oldValue = FontSize;
                        FontSize = int.Parse(value);
                        break;
                    case "text":
                        oldValue = TextValue;
                        TextValue = value;
                        break;
                    case "color":
                        oldValue = Color;
                        Color = ColorsController.Parse(value);
                        break;
                    default:
                        throw new InvalidShapeAttributeException();
                }
            }
            catch (FormatException) { throw new ShapeAttributeCastException(); }

            return oldValue;
        }

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (base.wpfShape != null) return base.wpfShape;

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

        public override (int, int) Center
        {
            get
            {
                if (wpfShape == null) return (0, 0);
                return (LeftTopX + (int)(wpfShape.Width / 2), LeftTopY + (int)(wpfShape.Height / 2));
            }
        }

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
            return new Text(this);
        }
    }

	class Polygon: Shape
	{
		List<(int, int)> points;
        private EColor color;
        private int lineThickness;

        private static int firstAvailibleUniqueNumber = 0;

        public Polygon(List<(int, int)> points, int lineThickness, EColor color)
        {
			this.points = points;
            this.color = color;
            this.lineThickness = lineThickness;
            uniqueName = GetNextUniqueName();

            ATTRS_TO_EDIT = new() { { "points", "Points" }, { "lineThickness", "Outline thickness" }, { "color", "Outline color" } };

            WindowManager.Instance.DisplayOnCurrentWindow(this);
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
                        Points = (List<(int, int)>)EArgumentType.Points.ParseValue(value);
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
                        throw new InvalidShapeAttributeException();
                }
            }
            catch (FormatException) { throw new ShapeAttributeCastException(); }

            return oldValue;
        }

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (base.wpfShape != null) return base.wpfShape;

				System.Windows.Shapes.Shape wpfShape = new System.Windows.Shapes.Polygon
                {
                    Points = new(from p in Points select new Point(p.Item1, p.Item2)),
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
            Points = (from p in Points select (p.Item1 + offset, p.Item2)).ToList();
        }

        public override void MoveYBy(int offset)
        {
            Points = (from p in Points select (p.Item1, p.Item2 + offset)).ToList();
        }

        public override (int, int) Center
        {
            get
            {
                int x = (int)(from p in Points select p.Item1).Average();
                int y = (int)(from p in Points select p.Item2).Average();
                return (x, y);
            }
        }

        private List<(int, int)> Points
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
