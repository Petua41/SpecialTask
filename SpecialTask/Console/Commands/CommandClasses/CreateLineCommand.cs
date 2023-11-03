using System;
using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Helpers;
using SpecialTask.Drawing.Shapes.Decorators;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
	/// Command to add line to the screen
	/// </summary>
	class CreateLineCommand : ICommand
    {
        private Shape? receiver;

        readonly int firstX;
        readonly int firstY;
        readonly int secondX;
        readonly int secondY;
        readonly EColor color;
        readonly int lineThickness;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

        public CreateLineCommand(object[] args)
        {
            firstX = (int)args[0];
            firstY = (int)args[1];
            secondX = (int)args[2];
            secondY = (int)args[3];
            color = (EColor)args[4];
            lineThickness = (int)args[5];
            streak = (bool)args[6];
            streakColor = (EColor)args[7];
            streakTexture = (EStreakTexture)args[8];
        }

        public void Execute()
        {
            receiver = new Line(firstX, firstY, secondX, secondY, color, lineThickness);

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
