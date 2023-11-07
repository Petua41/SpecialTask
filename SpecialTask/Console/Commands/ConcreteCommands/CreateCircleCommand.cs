using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to add circle to the screen
    /// </summary>
    internal class CreateCircleCommand : ICommand
    {
        private Shape? receiver;        // Needed for Unexecution

        private readonly int centerX;
        private readonly int centerY;
        private readonly EColor color;
        private readonly int radius;
        private readonly int lineThickness;
        private readonly bool streak;
        private readonly EColor streakColor;
        private readonly EStreakTexture streakTexture;

        public CreateCircleCommand(object[] args)
        {
            centerX = (int)args[0];
            centerY = (int)args[1];
            color = (EColor)args[2];
            radius = (int)args[3];
            lineThickness = (int)args[4];
            streak = (bool)args[5];
            streakColor = (EColor)args[6];
            streakTexture = (EStreakTexture)args[7];
        }


        public void Execute()
        {
            receiver = new Circle(centerX, centerY, color, radius, lineThickness);

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
