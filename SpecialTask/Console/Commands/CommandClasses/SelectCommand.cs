using SpecialTask.Drawing.Shapes;
using SpecialTask.Helpers;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
	/// Command for selecting shapes on specified area
	/// </summary>
	class SelectCommand : ICommand
    {
        private readonly int leftTopX;
        private readonly int leftTopY;
        private readonly int rightBottomX;
        private readonly int rightBottomY;

        public SelectCommand(object[] args)
        {
            leftTopX = (int)args[0];
            leftTopY = (int)args[1];
            rightBottomX = (int)args[2];
            rightBottomY = (int)args[3];
        }

        public void Execute()
        {
            SelectionMarker marker = new(leftTopX, leftTopY, rightBottomX, rightBottomY);
            marker.Display();

            SelectionMemento.SaveArea(leftTopX, leftTopY, rightBottomX, rightBottomY);
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of select command");
        }
    }
}
