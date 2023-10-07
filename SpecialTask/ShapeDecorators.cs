using System;
using System.Collections.Generic;

namespace SpecialTask
{
	abstract class ShapeDecorator : Shape
	{
		private readonly Shape? decoratedShape;

		public new string UniqueName
		{
			get
			{
				if (decoratedShape == null)
				{
					Logger.Instance.Error("Trying to get unique name of hanging decorator");
					return "";
				}
				return decoratedShape.UniqueName;
			}
		}
    }

	class StreakDecorator : ShapeDecorator
	{
		private EColor streakColor;
		private EStreakTexture streakTexture;
        private System.Windows.Shapes.Shape? wpfShape;
        private readonly Shape? decoratedShape;

        public StreakDecorator(Shape? decoratedShape, EColor streakColor, EStreakTexture streakTexture)
		{
			this.decoratedShape = decoratedShape;
			this.streakColor = streakColor;
			this.streakTexture = streakTexture;

            WindowManager.Instance.DisplayOnCurrentWindow(this);
        }

		public StreakDecorator(StreakDecorator old) : this(old.DecoratedShape, old.streakColor, old.streakTexture) { }

		public override object Edit(string attribute, object value)
		{
			object oldValue;

			if (decoratedShape == null)
			{
				Logger.Instance.Error("Trying to edit hanging decorator");
				return new();
			}

			try
			{
				switch (attribute)
				{
					case "streakColor":
						oldValue = streakColor;
						streakColor = (EColor)value;
						break;
					case "streakTexture":
						oldValue = streakTexture;
						streakTexture = (EStreakTexture)value;
						break;
					default:
						oldValue = decoratedShape.Edit(attribute, value);
						break;
				}
			}
			catch (InvalidCastException) { throw new ShapeAttributeCastException(); }

			return oldValue;
		}

        public override (int, int) Center
		{
			get
			{
				if (decoratedShape == null)
				{
					Logger.Instance.Error("Trying to get center of hanging decorator");
					return (0, 0);
				}
				return decoratedShape.Center;
			}
		}

        public override System.Windows.Shapes.Shape WPFShape
		{
			get
			{
				if (wpfShape != null) return wpfShape;

				if (decoratedShape == null)
				{
					Logger.Instance.Error("Trying to get WPFShape of hanging decorator");
					throw new HangingDecoratorException();
				}

				System.Windows.Shapes.Shape shape = decoratedShape.WPFShape;
				shape.Fill = streakTexture.GetWPFTexture(streakColor);
				decoratedShape.Destroy();               // Shape displays itself in constructor, so we should destroy it

				wpfShape = shape;
				return shape;
            }
        }

        public override void Destroy()
        {
            decoratedShape?.Destroy();
            WindowManager.Instance.RemoveFromCurrentWindow(this);
        }

        public override void NullifyWPFShape()
        {
            wpfShape = null;
            decoratedShape?.NullifyWPFShape();
        }

		public Shape? DecoratedShape => decoratedShape;

        public override Dictionary<string, object> Accept()
        {
			return new() { { "streakColor", streakColor }, { "streakTexture", streakTexture } };
        }

        public override void MoveXBy(int offset)
        {
			decoratedShape?.MoveXBy(offset);
			Redraw();
        }

        public override void MoveYBy(int offset)
        {
            decoratedShape?.MoveYBy(offset);
            Redraw();
        }

        public override Shape Clone()
        {
			return new StreakDecorator(this);
        }
    }
}
