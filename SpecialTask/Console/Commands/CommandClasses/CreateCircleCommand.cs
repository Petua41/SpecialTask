using System;
using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Helpers;
using SpecialTask.Drawing.Shapes.Decorators;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
    /// Command to add circle to the screen
    /// </summary>
    class CreateCircleCommand : ICommand
    {
        private Shape? receiver;        // Needed for Unexecution

        readonly int centerX;
        readonly int centerY;
        readonly EColor color;
        readonly int radius;
        readonly int lineThickness;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

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
