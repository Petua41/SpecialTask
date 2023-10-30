using System.Collections.Generic;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
    /// Command to clear the screen
    /// </summary>
    class ClearCommand : ICommand
    {
        private readonly WindowManager receiver;

        private List<Shape> destroyedShapes = new();

        public ClearCommand()
        {
            receiver = WindowManager.Instance;
        }

        public void Execute()
        {
            destroyedShapes = new(receiver.ShapesOnCurrentWindow);

            foreach (Shape shape in destroyedShapes) shape.Destroy();
        }

        public void Unexecute()
        {
            foreach (Shape shape in destroyedShapes) shape.Redraw();

            destroyedShapes.Clear();
        }
    }
}
