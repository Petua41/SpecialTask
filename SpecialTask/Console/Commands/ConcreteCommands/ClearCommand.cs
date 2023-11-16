using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.CommandHelpers;
using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to clear the screen
    /// </summary>
    internal class ClearCommand : ICommand
    {
        private List<Shape> destroyedShapes = new();
        private readonly Dictionary<string, DeletedShapeMemento> dsMementos = new();

        public void Execute()
        {
            destroyedShapes = new(CurrentWindow.Shapes);
            dsMementos.Clear();

            foreach (Shape shape in destroyedShapes)
            {
                dsMementos.Add(shape.UniqueName, new(shape));
                shape.Destroy();
            }
        }

        public void Unexecute()
        {
            foreach (Shape shape in destroyedShapes)
            {
                shape.Display();
                if (dsMementos.TryGetValue(shape.UniqueName, out DeletedShapeMemento? dsMemento)) dsMemento.Restore(shape);
            }

            destroyedShapes.Clear();
        }
    }
}
