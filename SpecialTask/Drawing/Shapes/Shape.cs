using SpecialTask.Infrastructure;

namespace SpecialTask.Drawing
{
    abstract class Shape
    {
        private static int firstAvailibleUniqueNumber = 0;
        protected System.Windows.Shapes.Shape? wpfShape;
        protected string uniqueName = string.Empty;
        protected MyMap<string, string> ATTRS_TO_EDIT = new();

        public static string GetNextUniqueName()
        {
            return $"Unknown_Shape_{firstAvailibleUniqueNumber++}";
        }

        public abstract object Edit(string attribute, string value);

        public virtual void Display()
        {
            CurrentWindow.AddShape(this);
        }

        // It`s kinda template method
        public virtual void Redraw()
        {
            Destroy();
            NullifyWPFShape();
            Display();
        }

        public virtual void Destroy()
        {
            CurrentWindow.RemoveShape(this);
        }

        public abstract Dictionary<string, object> Accept();

        public abstract void MoveXBy(int offset);

        public abstract void MoveYBy(int offset);

        public abstract Shape Clone();

        public virtual MyMap<string, string> AttributesToEditWithNames => ATTRS_TO_EDIT;

        public virtual string UniqueName => uniqueName;

        /// <summary>
        /// Windows.Shapes.Shape instance that can be added to Canvas
        /// </summary>
        public abstract System.Windows.Shapes.Shape WPFShape { get; }

        public abstract Point Center { get; }

        protected void NullifyWPFShape()
        {
            wpfShape = null;
        }
    }
}
