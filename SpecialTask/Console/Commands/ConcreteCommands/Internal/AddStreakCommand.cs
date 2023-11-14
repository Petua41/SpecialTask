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

        private readonly InternalColor streakColor;
        private readonly StreakTexture streakTexture;

        private StreakDecorator? decorator;

        public AddStreakCommand(Shape shape, InternalColor streakColor, StreakTexture streakTexture)
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
            decorator?.Destroy();       // if there wasn`t execution, there won`t be unexecution. It`s not bad
            receiver?.Redraw();
        }
    }
}
