using SpecialTask.Exceptions;
using SpecialTask.Helpers;
using System;
using System.Collections.Generic;

namespace SpecialTask.Drawing.Shapes.Decorators
{
    class StreakDecorator : Shape
    {
        private EColor streakColor;
        private EStreakTexture streakTexture;
        private readonly Shape? decoratedShape;

        public StreakDecorator(Shape? decoratedShape, EColor streakColor, EStreakTexture streakTexture)
        {
            this.decoratedShape = decoratedShape;
            this.streakColor = streakColor;
            this.streakTexture = streakTexture;

            ATTRS_TO_EDIT = new() { { "streakColor", "Streak color" }, { "streakTexture", "Streak texture" } };
        }

        public StreakDecorator(StreakDecorator old) : this(old.DecoratedShape, old.StreakColor, old.StreakTexture) { }

        public override object Edit(string attribute, string value)
        {
            object oldValue;

            if (decoratedShape == null)
            {
                Logger.Instance.Warning("Trying to edit hanging decorator");
                return new();
            }

            try
            {
                switch (attribute)
                {
                    case "streakColor":
                        oldValue = StreakColor;
                        StreakColor = ColorsController.Parse(value);
                        break;
                    case "streakTexture":
                        oldValue = StreakTexture;
                        StreakTexture = TextureController.Parse(value);
                        break;
                    default:
                        oldValue = decoratedShape.Edit(attribute, value);
                        break;
                }
            }
            catch (FormatException) { throw new ShapeAttributeCastException(); }

            return oldValue;
        }

        public override void Destroy()
        {
            decoratedShape?.Destroy();
            CurrentWindow.RemoveShape(this);
        }

        public override Dictionary<string, object> Accept()
        {
            return new() { { "streakColor", StreakColor }, { "streakTexture", StreakTexture }, { "decoratedShape", DecoratedShape } };
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

        public override Point Center
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
                    throw new InvalidOperationException("Hanging decorator!");
                }

                System.Windows.Shapes.Shape shape = decoratedShape.WPFShape;
                shape.Fill = streakTexture.GetWPFTexture(streakColor);

                wpfShape = shape;
                return shape;
            }
        }

        private Shape DecoratedShape => decoratedShape ?? throw new InvalidOperationException("Hanging decorator!");

        private EStreakTexture StreakTexture
        {
            get => streakTexture;
            set
            {
                streakTexture = value;
                base.Redraw();
            }
        }

        private EColor StreakColor
        {
            get => streakColor;
            set
            {
                streakColor = value;
                base.Redraw();
            }
        }

        public override MyMap<string, string> AttributesToEditWithNames
        {
            get
            {
                if (decoratedShape == null) return new();

                return decoratedShape.AttributesToEditWithNames + ATTRS_TO_EDIT;
            }
        }

        public override string UniqueName
        {
            get
            {
                if (decoratedShape == null) return "";
                return $"Filled_{decoratedShape.UniqueName}";
            }
        }
    }
}
