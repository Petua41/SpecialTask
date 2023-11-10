using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
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
        private readonly STColor color;
        private readonly bool streak;
        private readonly STColor streakColor;
        private readonly StreakTexture streakTexture;

        public CreatePolygonCommand(object[] args)
        {
            points = (List<Point>)args[0];
            lineThickness = (int)args[1];
            color = (STColor)args[2];
            streak = (bool)args[3];
            streakColor = (STColor)args[4];
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
