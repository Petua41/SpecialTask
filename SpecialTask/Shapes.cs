using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialTask
{
	public enum EColor 
	{
		None,
		Green,
		Magenta,
		Red,
		White,
		Yellow
	} // TODO

	static class ColorsController
	{
		public static System.Windows.Media.Color GetWPFColor(EColor color)
		{
			return color switch
			{
				EColor.None => System.Windows.Media.Colors.Transparent,
				EColor.Green => System.Windows.Media.Colors.Green,
				EColor.Magenta => System.Windows.Media.Colors.Magenta,
				EColor.Red => System.Windows.Media.Colors.Red,
				EColor.White => System.Windows.Media.Colors.White,
				EColor.Yellow => System.Windows.Media.Colors.Yellow,
				_ => throw new ColorExcepttion(),
			};
		}

		public static EColor GetColorFromString(string colorString)
		{
			colorString = colorString.Trim().ToLower();
			return colorString switch
			{
				"green" => EColor.Green,
				"magenta" => EColor.Magenta,
				"red" => EColor.Red,
				"white" => EColor.White,
				"yellow" => EColor.Yellow,
				_ => EColor.None,
			};
		}
	}

	/// <summary>
	/// Представляет абстрактный класс для всех фигур
	/// </summary>
	abstract class Shape
	{
		private static int firstAvailibleUniqueNumber = 0;
		public string uniqueName;

		public Shape()
		{
			uniqueName = GetNextUniqueName();
		}

		public static string GetNextUniqueName()
		{
			return string.Format("Unknown_Shape_{0}", firstAvailibleUniqueNumber++);
		}

		public abstract object Edit(string attribute, object value);

		public abstract void Redraw();

		public abstract void Destroy();			// А экземпляр вообще может самоуничтожиться?

		public string UniqueName 
		{ 
			get => uniqueName; 
		}

		public abstract (int, int) Center { get; }
	}

	class Circle: Shape
	{
		private int radius;
		private int centerX;
		private int centerY;
		private EColor color;
		private int lineThickness;
		public new string uniqueName;

		private static int firstAvailibleUniqueNumber = 0;

		public Circle(int centerX, int centerY, EColor color, int radius, int lineThickness)
		{
			this.centerX = centerX;
			this.centerY = centerY;
			this.color = color;
			this.radius = radius;
			this.lineThickness = lineThickness;
			uniqueName = GetNextUniqueName();
		}

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

		public override void Redraw()
		{
			// TODO
		}

		public override void Destroy()
		{
			// TODO
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
	}

	class Square: Shape
	{
		private int leftTopX;
		private int leftTopY;
		private int rightBottomX;
		private int rightBottomY;
		private EColor color;
		private int lineThickness;
		public new string uniqueName;

		private static int firstAvailibleUniqueNumber = 0;

		public Square(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY,  EColor color, int lineThickness)
		{
			this.leftTopX = leftTopX;
			this.leftTopY = leftTopY;
			this.rightBottomX = rightBottomX;
			this.rightBottomY = rightBottomY;
			this.color = color;
			this.lineThickness = lineThickness;
			uniqueName = GetNextUniqueName();
		}

		public static new string GetNextUniqueName()
		{
			return string.Format("Square_{0}", firstAvailibleUniqueNumber++);
		}

        public override (int, int) Center
		{
			get => ((leftTopX + rightBottomX) / 2, (leftTopY + rightBottomY) / 2);
		}

        public override void Redraw()
        {
            // TODO
        }

        public override void Destroy()
        {
            // TODO
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

		private int RightBottomX
		{
			get => rightBottomX;
			set
			{
				rightBottomX = value;
				Redraw();
			}
		}

		private int RightBottomY
		{
			get => rightBottomY;
			set
			{
				rightBottomY = value;
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
    }

	class Line: Shape
	{
		private int firstX;
		private int firstY;
		private int secondX;
		private int secondY;
		private EColor color;
		private int lineThickness;
		public new string uniqueName;

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
        }

        public static new string GetNextUniqueName()
        {
            return string.Format("Line_{0}", firstAvailibleUniqueNumber++);
        }

        public override (int, int) Center
        {
            get => ((firstX + secondX) / 2, (firstY + secondY) / 2);
        }

        public override void Redraw()
        {
            // TODO
        }

        public override void Destroy()
        {
            // TODO
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
    }
}
