﻿using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to add rectangle to the screen
    /// </summary>
    internal class CreateSquareCommand : ICommand
    {
        private Shape? receiver;

        private readonly int leftTopX;
        private readonly int leftTopY;
        private readonly int rightBottomX;
        private readonly int rightBottomY;
        private readonly STColor color;
        private readonly int lineThickness;
        private readonly bool streak;
        private readonly STColor streakColor;
        private readonly StreakTexture streakTexture;

        public CreateSquareCommand(object[] args)
        {
            leftTopX = (int)args[0];
            leftTopY = (int)args[1];
            rightBottomX = (int)args[2];
            rightBottomY = (int)args[3];
            color = (STColor)args[4];
            lineThickness = (int)args[5];
            streak = (bool)args[6];
            streakColor = (STColor)args[7];
            streakTexture = (StreakTexture)args[8];
        }

        public void Execute()
        {
            receiver = new Square(leftTopX, leftTopY, rightBottomX, rightBottomY, color, lineThickness);

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
