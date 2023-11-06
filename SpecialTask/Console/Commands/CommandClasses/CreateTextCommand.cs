using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
    /// Command to add text
    /// </summary>
    class CreateTextCommand : ICommand
    {
        private Shape? receiver;

        readonly int leftTopX;
        readonly int leftTopY;
        readonly int fontSize;
        readonly string textValue;
        readonly EColor color;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

        public CreateTextCommand(object[] args)
        {
            leftTopX = (int)args[0];
            leftTopY = (int)args[1];
            fontSize = (int)args[2];
            textValue = (string)args[3];
            color = (EColor)args[4];
            streak = (bool)args[5];
            streakColor = (EColor)args[6];
            streakTexture = (EStreakTexture)args[7];
        }

        public void Execute()
        {
            receiver = new Text(leftTopX, leftTopY, fontSize, textValue, color);

            if (streak) receiver = new StreakDecorator(receiver, streakColor, streakTexture);

            receiver.Display();
        }

        public void Unexecute()
        {
            if (receiver is null) throw new InvalidOperationException();
            receiver.Destroy();
        }
    }
}
