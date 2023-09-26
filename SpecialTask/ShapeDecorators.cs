using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override void Edit(string attribute, object value)
        {
			if (decoratedShape == null) throw new HangingDecoratorException();

            switch (attribute)
			{
				case "streakColor":
					try
					{
						streakColor = (EColor)value;
					}
					catch (InvalidCastException)
					{

						throw new ShapeAttributeCastException();
					}
					break;
				case "streakTexture":
					try
					{
						streakTexture = (EStreakTexture)value;
					}
                    catch (InvalidCastException)
                    {

                        throw new ShapeAttributeCastException();
                    }
					break;
				default:
					decoratedShape.Edit(attribute, value);
					break;
            }
        }

		public override void Redraw()
		{
			decoratedShape.Redraw();
			RedrawStreak();

		}

		private void RedrawStreak()
		{
			// TODO
		}
    }
}
