using System;

namespace SpecialTask
{
	enum EStreakTexture { None } // TODO

	abstract class ShapeDecorator : Shape
	{
		private Shape? decoratedShape;

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
		private Shape? decoratedShape;
		private EColor streakColor;
		private EStreakTexture streakTexture;

		public StreakDecorator(Shape decoratedShape, EColor streakColor, EStreakTexture streakTexture)
		{
			this.decoratedShape = decoratedShape;
			this.streakColor = streakColor;
			this.streakTexture = streakTexture;
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

		public override void Redraw()
		{
			if (decoratedShape == null) throw new HangingDecoratorException();
			decoratedShape.Redraw();
			RedrawStreak();
		}

        public override (int, int) Center
		{
			get
			{
				if (decoratedShape == null) throw new HangingDecoratorException();
				return decoratedShape.Center;
			}
		}

        public override void Destroy()
		{
			// TODO
		}

		private void RedrawStreak()
		{
			// TODO
		}
	}
}
