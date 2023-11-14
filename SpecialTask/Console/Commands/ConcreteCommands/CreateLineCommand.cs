using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to add line to the screen
    /// </summary>
    internal class CreateLineCommand : ICommand
    {
        private Shape? receiver;

        private readonly int firstX;
        private readonly int firstY;
        private readonly int secondX;
        private readonly int secondY;
        private readonly InternalColor color;
        private readonly int lineThickness;
        private readonly bool streak;
        private readonly InternalColor streakColor;
        private readonly StreakTexture streakTexture;

        public CreateLineCommand(object[] args)
        {
            firstX = (int)args[0];
            firstY = (int)args[1];
            secondX = (int)args[2];
            secondY = (int)args[3];
            color = (InternalColor)args[4];
            lineThickness = (int)args[5];
            streak = (bool)args[6];
            streakColor = (InternalColor)args[7];
            streakTexture = (StreakTexture)args[8];
        }

        public void Execute()
        {
            receiver = new Line(firstX, firstY, secondX, secondY, color, lineThickness);

            if (streak)
            {
                receiver = new StreakDecorator(receiver, streakColor, streakTexture);
            }

            receiver.Display();
        }

        public void Unexecute()
        {
            if (receiver is null)
            {
                throw new InvalidOperationException();
            }

            receiver.Destroy();
        }
    }
}
