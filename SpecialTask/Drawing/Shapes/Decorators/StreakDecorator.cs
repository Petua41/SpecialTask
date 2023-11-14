using SpecialTask.Infrastructure.Collections;
using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Exceptions;
using SpecialTask.Infrastructure.WindowSystem;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;
using static SpecialTask.Infrastructure.Extensoins.StreakTextureExtensions;

namespace SpecialTask.Drawing.Shapes.Decorators
{
    internal class StreakDecorator : Shape
    {
        private InternalColor streakColor;
        private StreakTexture streakTexture;
        private readonly Shape decoratedShape;

        public StreakDecorator(Shape decoratedShape, InternalColor streakColor, StreakTexture streakTexture)
        {
            this.decoratedShape = decoratedShape;
            this.streakColor = streakColor;
            this.streakTexture = streakTexture;

            ATTRS_TO_EDIT = new() { { "streakColor", "Streak color" }, { "streakTexture", "Streak texture" } };
        }

        public StreakDecorator(StreakDecorator old) : this(old.DecoratedShape, old.StreakColor, old.StreakTexture) { }

        public override string Edit(string attribute, string value)
        {
            string oldValue;

            try
            {
                switch (attribute)
                {
                    case "streakColor":
                        oldValue = StreakColor.ToString();
                        StreakColor = value.ParseColor();
                        break;
                    case "streakTexture":
                        oldValue = StreakTexture.ToString();
                        StreakTexture = value.ParseStreakTexture();
                        break;
                    default:
                        oldValue = decoratedShape.Edit(attribute, value);
                        break;
                }
            }
            catch (FormatException e) { throw new ShapeAttributeCastException($"Cannot cast {value} to value of {attribute}", e, attribute, value); }

            return oldValue;
        }

        public override void Destroy()
        {
            decoratedShape.Destroy();
            CurrentWindow.RemoveShape(this);
        }

        public override Dictionary<string, object> Accept()
        {
            return new() { { "streakColor", StreakColor }, { "streakTexture", StreakTexture }, { "decoratedShape", DecoratedShape } };
        }

        public override void MoveXBy(int offset)
        {
            decoratedShape.MoveXBy(offset);
            Redraw();
        }

        public override void MoveYBy(int offset)
        {
            decoratedShape.MoveYBy(offset);
            Redraw();
        }

        public override Shape Clone()
        {
            return new StreakDecorator(this);
        }

        public override Point Center => decoratedShape.Center;

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (wpfShape is not null)
                {
                    return wpfShape;
                }

                System.Windows.Shapes.Shape shape = decoratedShape.WPFShape;
                shape.Fill = streakTexture.GetWPFTexture(streakColor);

                wpfShape = shape;
                return shape;
            }
        }

        private Shape DecoratedShape => decoratedShape;

        private StreakTexture StreakTexture
        {
            get => streakTexture;
            set
            {
                streakTexture = value;
                base.Redraw();
            }
        }

        private InternalColor StreakColor
        {
            get => streakColor;
            set
            {
                streakColor = value;
                base.Redraw();
            }
        }

        public override Pairs<string, string> AttributesToEditWithNames => decoratedShape.AttributesToEditWithNames + ATTRS_TO_EDIT;

        public override string UniqueName => $"Filled_{decoratedShape.UniqueName}";
    }
}
