using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.CommandHelpers;

namespace SpecialTask.Console.Commands.ConcreteCommands.Internal
{
    /// <summary>
    /// Command to remove shape
    /// </summary>
    internal class RemoveShapeCommand : ICommand
    {
        private readonly Shape receiver;

        private DeletedShapeMemento? dsMemento;

        public RemoveShapeCommand(Shape shape)
        {
            receiver = shape;
        }

        public void Execute()
        {
            dsMemento = new(receiver);
            receiver.Destroy();
        }

        public void Unexecute()
        {
            receiver.Display();
            dsMemento?.Restore(receiver);
        }
    }
}
