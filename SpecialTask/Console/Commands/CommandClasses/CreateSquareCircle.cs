using System;
using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Helpers;
using SpecialTask.Drawing.Shapes.Decorators;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
	/// Command to add rectangle to the screen
	/// </summary>
	class CreateSquareCommand : ICommand
    {
        private Shape? receiver;

        readonly int leftTopX;
        readonly int leftTopY;
        readonly int rightBottomX;
        readonly int rightBottomY;
        readonly EColor color;
        readonly int lineThickness;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

        public CreateSquareCommand(object[] args)
        {
            leftTopX = (int)args[0];
            leftTopY = (int)args[1];
            rightBottomX = (int)args[2];
            rightBottomY = (int)args[3];
            color = (EColor)args[4];
            lineThickness = (int)args[5];
            streak = (bool)args[6];
            streakColor = (EColor)args[7];
            streakTexture = (EStreakTexture)args[8];
        }

        public void Execute()
        {
            receiver = new Square(leftTopX, leftTopY, rightBottomX, rightBottomY, color, lineThickness);

            if (streak) receiver = new StreakDecorator(receiver, streakColor, streakTexture);

            receiver.Display();
        }

        public void Unexecute()
        {
            if (receiver == null) throw new InvalidOperationException();
            receiver.Destroy();
        }
    }
}
