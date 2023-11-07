using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.CommandHelpers;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to paste selected shapes
    /// </summary>
    internal class PasteCommand : ICommand
    {
        private readonly int leftTopX;
        private readonly int leftTopY;

        private List<Shape> pastedShapes = new();

        public PasteCommand(object[] args)
        {
            leftTopX = (int)args[0];
            leftTopY = (int)args[1];
        }

        public void Execute()
        {
            pastedShapes = SelectionMemento.PasteArea(leftTopX, leftTopY);
        }

        public void Unexecute()
        {
            foreach (Shape shape in pastedShapes)
            {
                shape.Destroy();
            }
        }
    }
}
