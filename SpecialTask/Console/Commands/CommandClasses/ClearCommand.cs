using SpecialTask.Drawing;
using SpecialTask.Helpers;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
    /// Command to clear the screen
    /// </summary>
    class ClearCommand : ICommand
    {
        private List<Shape> destroyedShapes = new();

        public void Execute()
        {
            destroyedShapes = new(CurrentWindow.Shapes);

            foreach (Shape shape in destroyedShapes) shape.Destroy();
        }

        public void Unexecute()
        {
            foreach (Shape shape in destroyedShapes) shape.Redraw();

            destroyedShapes.Clear();
        }
    }
}
