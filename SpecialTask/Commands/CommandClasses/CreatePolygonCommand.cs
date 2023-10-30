using System;
using System.Collections.Generic;

namespace SpecialTask.Commands.CommandClasses
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
        }

        public void Unexecute()
        {
            if (receiver == null) throw new InvalidOperationException();
            receiver.Destroy();
        }
    }
}
