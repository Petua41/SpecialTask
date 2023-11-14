using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to add text
    /// </summary>
    internal class CreateTextCommand : ICommand
    {
        private Shape? receiver;

        private readonly int leftTopX;
        private readonly int leftTopY;
        private readonly int fontSize;
        private readonly string textValue;
        private readonly InternalColor color;
        private readonly bool streak;
        private readonly InternalColor streakColor;
        private readonly StreakTexture streakTexture;

        public CreateTextCommand(object[] args)
        {
            leftTopX = (int)args[0];
            leftTopY = (int)args[1];
            fontSize = (int)args[2];
            textValue = (string)args[3];
            color = (InternalColor)args[4];
            streak = (bool)args[5];
            streakColor = (InternalColor)args[6];
            streakTexture = (StreakTexture)args[7];
        }

        public void Execute()
        {
            receiver = new Text(leftTopX, leftTopY, fontSize, textValue, color);

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
