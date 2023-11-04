using SpecialTask.Exceptions;
using SpecialTask.Helpers;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
    /// Command to move shapes up and down
    /// </summary>
    class EditLayerCommand : ICommand
    {
        private readonly string uniqueName;
        private readonly ELayerDirection direction;

        private int oldLayer = -1;
        private bool layerChanged = false;

        public EditLayerCommand(string uniqueName, ELayerDirection direction)
        {
            this.uniqueName = uniqueName;
            this.direction = direction;
        }

        public void Execute()
        {
            try
            {
                switch (direction)
                {
                    case ELayerDirection.Forward:
                        oldLayer = CurrentWindow.BringForward(uniqueName);
                        layerChanged = true;
                        break;
                    case ELayerDirection.Backward:
                        oldLayer = CurrentWindow.SendBackward(uniqueName);
                        layerChanged = true;
                        break;
                    case ELayerDirection.Front:
                        oldLayer = CurrentWindow.BringToFront(uniqueName);
                        layerChanged = true;
                        break;
                    case ELayerDirection.Back:
                        oldLayer = CurrentWindow.SendToBack(uniqueName);
                        layerChanged = true;
                        break;
                }
            }
            catch (ShapeNotFoundException)
            {
                Logger.Instance.Error($"Shape {uniqueName} not found, while changing layer");
                throw;
            }
        }

        public void Unexecute()
        {
            if (!layerChanged) return;
            try
            {
                switch (direction)
                {
                    case ELayerDirection.Forward:
                        try { CurrentWindow.SendBackward(uniqueName); }
                        catch (InvalidOperationException)
                        {
                            Logger.Instance.Error($"[undo] Cannot send {uniqueName} backward: already on back");
                            HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on back");
                        }
                        break;
                    case ELayerDirection.Backward:
                        try { CurrentWindow.BringForward(uniqueName); }
                        catch (InvalidOperationException)
                        {
                            Logger.Instance.Error($"[undo] Cannot bring {uniqueName} forward: already on top");
                            HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on top");
                        }
                        break;
                    case ELayerDirection.Front:
                        try { CurrentWindow.MoveToLayer(uniqueName, oldLayer); }
                        catch (ArgumentException) { CurrentWindow.SendToBack(uniqueName); }
                        break;
                    case ELayerDirection.Back:
                        try { CurrentWindow.MoveToLayer(uniqueName, oldLayer); }
                        catch (ArgumentException) { CurrentWindow.BringToFront(uniqueName); }
                        break;
                }
            }
            catch (ShapeNotFoundException)
            {
                Logger.Instance.Error($"Shape {uniqueName} not found, while changing layer");
                throw;
            }
        }
    }
}
