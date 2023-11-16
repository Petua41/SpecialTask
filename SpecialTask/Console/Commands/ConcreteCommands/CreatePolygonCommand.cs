using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure.CommandHelpers;
using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to add polygon
    /// </summary>
    internal class CreatePolygonCommand : ICommand
    {
        private Shape? receiver;

        private readonly List<Point> points;
        private readonly int lineThickness;
        private readonly InternalColor color;
        private readonly bool streak;
        private readonly InternalColor streakColor;
        private readonly StreakTexture streakTexture;

        private DeletedShapeMemento? dsMemento;

        public CreatePolygonCommand(object[] args)
        {
            points = (List<Point>)args[0];
            lineThickness = (int)args[1];
            color = (InternalColor)args[2];
            streak = (bool)args[3];
            streakColor = (InternalColor)args[4];
            streakTexture = (StreakTexture)args[5];
        }

        public void Execute()
        {
            receiver = new Polygon(points, lineThickness, color);

            if (streak)
            {
                receiver = new StreakDecorator(receiver, streakColor, streakTexture);
            }

            receiver.Display();
            dsMemento?.Restore(receiver);   // if it`s redo, restore Z index
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
