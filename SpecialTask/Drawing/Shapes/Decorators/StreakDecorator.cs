using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Exceptions;
using SpecialTask.Infrastructure.WindowSystem;
using static SpecialTask.Infrastructure.Extensoins.KeyValuePairListExtension;
using static SpecialTask.Infrastructure.Extensoins.StreakTextureExtensions;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTask.Drawing.Shapes.Decorators
{
    internal class StreakDecorator : Shape
    {
        private InternalColor streakColor;
        private StreakTexture streakTexture;

        public StreakDecorator(Shape decoratedShape, InternalColor streakColor, StreakTexture streakTexture)
        {
            DecoratedShape = decoratedShape;
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
                        oldValue = DecoratedShape.Edit(attribute, value);
                        break;
                }
            }
            catch (FormatException e) { throw new ShapeAttributeCastException($"Cannot cast {value} to value of {attribute}", e, attribute, value); }

            return oldValue;
        }

        public override void Destroy()
        {
            DecoratedShape.Destroy();
            WindowManager.CurrentWindow.RemoveShape(this);
        }

        public override Dictionary<string, object> Accept()
        {
            return new() { { "streakColor", StreakColor }, { "streakTexture", StreakTexture }, { "decoratedShape", DecoratedShape } };
        }

        public override void MoveXBy(int offset)
        {
            DecoratedShape.MoveXBy(offset);
            Destroy();
            Display();
        }

        public override void MoveYBy(int offset)
        {
            DecoratedShape.MoveYBy(offset);
            Destroy();
            Display();
        }

        public override Shape Clone()
        {
            return new StreakDecorator(this);
        }

        public override Point Center => DecoratedShape.Center;

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (wpfShape is not null)
                {
                    return wpfShape;
                }

                System.Windows.Shapes.Shape shape = DecoratedShape.WPFShape;
                shape.Fill = streakTexture.GetWPFTexture(streakColor);

                wpfShape = shape;
                return shape;
            }
        }

        private Shape DecoratedShape { get; }

        private StreakTexture StreakTexture
        {
            get => streakTexture;
            set
            {
                streakTexture = value;
                base.Destroy(); base.Display(); ;
            }
        }

        private InternalColor StreakColor
        {
            get => streakColor;
            set
            {
                streakColor = value;
                base.Destroy(); base.Display(); ;
            }
        }

        public override List<KeyValuePair<string, string>> AttributesToEditWithNames => DecoratedShape.AttributesToEditWithNames.Concatenate(ATTRS_TO_EDIT);

        public override string UniqueName => $"Filled_{DecoratedShape.UniqueName}";
    }
}
