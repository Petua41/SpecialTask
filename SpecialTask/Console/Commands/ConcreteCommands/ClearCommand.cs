using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to clear the screen
    /// </summary>
    internal class ClearCommand : ICommand
    {
        private List<Shape> destroyedShapes = new();

        public void Execute()
        {
            destroyedShapes = new(CurrentWindow.Shapes);

            foreach (Shape shape in destroyedShapes)
            {
                shape.Destroy();
            }
        }

        public void Unexecute()
        {
            foreach (Shape shape in destroyedShapes)
            {
                shape.Redraw();
            }

            destroyedShapes.Clear();
        }
    }
}
