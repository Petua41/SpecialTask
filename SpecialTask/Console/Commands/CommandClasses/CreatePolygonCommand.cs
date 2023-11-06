using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
	/// Command to add polygon
	/// </summary>
    class CreatePolygonCommand : ICommand
    {
        private Shape? receiver;

        readonly List<Point> points;
        readonly int lineThickness;
        readonly EColor color;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

        public CreatePolygonCommand(object[] args)
        {
            points = (List<Point>)args[0];
            lineThickness = (int)args[1];
            color = (EColor)args[2];
            streak = (bool)args[3];
            streakColor = (EColor)args[4];
            streakTexture = (EStreakTexture)args[5];
        }

        public void Execute()
        {
            receiver = new Polygon(points, lineThickness, color);

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
