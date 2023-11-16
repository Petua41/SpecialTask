using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure.CommandHelpers;
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
        private readonly InternalColor color;
        private readonly int lineThickness;
        private readonly bool streak;
        private readonly InternalColor streakColor;
        private readonly StreakTexture streakTexture;

        private DeletedShapeMemento? dsMemento;

        public CreateSquareCommand(object[] args)
        {
            leftTopX = (int)args[0];
            leftTopY = (int)args[1];
            rightBottomX = (int)args[2];
            rightBottomY = (int)args[3];
            color = (InternalColor)args[4];
            lineThickness = (int)args[5];
            streak = (bool)args[6];
            streakColor = (InternalColor)args[7];
            streakTexture = (StreakTexture)args[8];
        }

        public void Execute()
        {
            receiver = new Square(leftTopX, leftTopY, rightBottomX, rightBottomY, color, lineThickness);

            if (streak)
            {
                receiver = new StreakDecorator(receiver, streakColor, streakTexture);
            }

            dsMemento?.Restore(receiver);   // if it`s redo, restore Z index
            receiver.Display();
        }

        public void Unexecute()
        {
            if (receiver is null)
            {
                throw new InvalidOperationException();
            }

            dsMemento = new(receiver);
            receiver.Destroy();
        }
    }
}
