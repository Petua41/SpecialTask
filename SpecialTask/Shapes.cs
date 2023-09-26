using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialTask
{
	public enum EColor 
	{
		None
	} // TODO

	static class ColorsController
	{
		public static System.Windows.Media.Color GetWPFColor(EColor color)
		{
            return color switch
            {
                EColor.None => System.Windows.Media.Colors.Transparent,
                _ => throw new ColorExcepttion(),
            };
        }

		public static EColor GetColorFromString(string colorString)
		{
			colorString = colorString.Trim().ToLower();
			return colorString switch
			{
				_ => EColor.None,
			};
		}
	}

	abstract class Shape
	{
		private static int firstAvailibleUniqueNumber = 0;
		private string uniqueName;

		public Shape()
		{
			uniqueName = GetNextUniqueName();
		}

		public static string GetNextUniqueName()
		{
			return string.Format("Unknown_Shape_{0}", firstAvailibleUniqueNumber++);
        }

		public abstract void Edit(string attribute, object value);

		public abstract void Redraw();

        public string UniqueName 
		{ 
			get => uniqueName; 
		}
    }

	class Circle: Shape
	{
		private int radius;
		private int centerX;
		private int centerY;
		private EColor color;
		private int lineThickness;
		private string uniqueName;

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

		public override void Edit(string attribute, object value)
		{
			attribute = attribute.ToLower();
			try
			{
				switch (attribute)
				{
					case "centerx":
						CenterX = (int)value; break;
					case "centery":
						CenterY = (int)value; break;
					case "color":
						color = (EColor)value; break;
					case "linethickness":
						lineThickness = (int)value; break;
					default:
						throw new InvalidShapeAttributeException();
				}
			}
			catch (InvalidCastException) { throw new ShapeAttributeCastException(); }
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
		private string uniqueName;

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
    }
}
