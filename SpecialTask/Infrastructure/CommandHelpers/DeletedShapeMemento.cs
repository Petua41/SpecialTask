using SpecialTask.Drawing.Shapes;
using System.Windows.Controls;

namespace SpecialTask.Infrastructure.CommandHelpers
{
    internal class DeletedShapeMemento
    {
        private readonly int zIndex;

        public DeletedShapeMemento(Shape shape)
        {
            zIndex = Panel.GetZIndex(shape.WPFShape);
        }

        public void Restore(Shape shape)
        {
            Panel.SetZIndex(shape.WPFShape, zIndex);
        }
    }
}
