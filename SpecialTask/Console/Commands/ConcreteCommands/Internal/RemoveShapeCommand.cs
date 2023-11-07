using SpecialTask.Drawing.Shapes;

namespace SpecialTask.Console.Commands.ConcreteCommands.Internal
{
    /// <summary>
    /// Command to remove shape
    /// </summary>
    internal class RemoveShapeCommand : ICommand
    {
        private readonly Shape receiver;

        public RemoveShapeCommand(Shape shape)
        {
            receiver = shape;
        }

        public void Execute()
        {
            receiver.Destroy();
        }

        public void Unexecute()
        {
            receiver.Redraw();
        }
    }
}
