using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Console.Commands.ConcreteCommands.Internal
{
    /// <summary>
    /// Command to decorate shape with StreakDecorator
    /// </summary>
    internal class AddStreakCommand : ICommand
    {
        private readonly Shape? receiver;

        private readonly EColor streakColor;
        private readonly EStreakTexture streakTexture;

        private StreakDecorator? decorator;

        public AddStreakCommand(Shape shape, EColor streakColor, EStreakTexture streakTexture)
        {
            receiver = shape;
            this.streakColor = streakColor;
            this.streakTexture = streakTexture;
        }

        public void Execute()
        {
            decorator = new(receiver, streakColor, streakTexture);
        }

        public void Unexecute()
        {
            decorator?.Destroy();
            receiver?.Redraw();
        }
    }
}
