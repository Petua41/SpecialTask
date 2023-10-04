using System;

namespace SpecialTask
{
	abstract class ShapeDecorator : Shape
	{
		private Shape? decoratedShape;
		private System.Windows.Shapes.Shape? wpfShape;

		public new string UniqueName
		{
			get
			{
				if (decoratedShape == null) throw new HangingDecoratorException();
				return decoratedShape.UniqueName;
			}
		}
    }

	class StreakDecorator : ShapeDecorator
	{
		private EColor streakColor;
		private EStreakTexture streakTexture;
        private System.Windows.Shapes.Shape? wpfShape;
        private Shape? decoratedShape;

        public StreakDecorator(Shape decoratedShape, EColor streakColor, EStreakTexture streakTexture)
		{
			this.decoratedShape = decoratedShape;
			this.streakColor = streakColor;
			this.streakTexture = streakTexture;

            WindowManager.Instance.DisplayOnCurrentWindow(this);
        }

		public override object Edit(string attribute, object value)
		{
			object oldValue;
			if (decoratedShape == null) throw new HangingDecoratorException();

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
				if (decoratedShape == null) throw new HangingDecoratorException();
				return decoratedShape.Center;
			}
		}

        public override System.Windows.Shapes.Shape WPFShape
		{
			get
			{
				if (wpfShape != null) return wpfShape;

				if (decoratedShape == null) throw new HangingDecoratorException();

				System.Windows.Shapes.Shape shape = decoratedShape.WPFShape;
				shape.Fill = TextureController.GetWPFTexture(streakTexture, streakColor);
				decoratedShape.Destroy();               // Shape displays itself in constructor, so we should destroy it

				wpfShape = shape;
				return shape;
            }
        }

        public override void Destroy()
        {
            if (decoratedShape == null)
            {
                Logger.Instance.Error("Attempt to destroy hanging decorator");
                throw new HangingDecoratorException();
            }
            decoratedShape.Destroy();
            WindowManager.Instance.RemoveFromCurrentWindow(this);
        }

        public override void NullifyWPFShape()
        {
            if (decoratedShape == null)
            {
                Logger.Instance.Error("Attempt to nullify hanging decorator`s wpfShape");
                throw new HangingDecoratorException();
            }
            wpfShape = null;
            decoratedShape.NullifyWPFShape();
        }
    }
}
