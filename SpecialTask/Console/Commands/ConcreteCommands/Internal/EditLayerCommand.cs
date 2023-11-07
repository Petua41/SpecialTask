using SpecialTask.Infrastructure;
using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Console.Commands.ConcreteCommands.Internal
{
    internal enum ELayerDirection { None, Forward, Backward, Front, Back }

    /// <summary>
    /// Command to move shapes up and down
    /// </summary>
    internal class EditLayerCommand : ICommand
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
                Logger.Error($"Shape {uniqueName} not found, while changing layer");
                throw;
            }
        }

        public void Unexecute()
        {
            if (!layerChanged)
            {
                return;
            }

            try
            {
                switch (direction)
                {
                    case ELayerDirection.Forward:
                        try { _ = CurrentWindow.SendBackward(uniqueName); }
                        catch (InvalidOperationException)
                        {
                            Logger.Error($"[undo] Cannot send {uniqueName} backward: already on back");
                            HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on back");
                        }
                        break;
                    case ELayerDirection.Backward:
                        try { _ = CurrentWindow.BringForward(uniqueName); }
                        catch (InvalidOperationException)
                        {
                            Logger.Error($"[undo] Cannot bring {uniqueName} forward: already on top");
                            HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on top");
                        }
                        break;
                    case ELayerDirection.Front:
                        try { CurrentWindow.MoveToLayer(uniqueName, oldLayer); }
                        catch (ArgumentException) { _ = CurrentWindow.SendToBack(uniqueName); }
                        break;
                    case ELayerDirection.Back:
                        try { CurrentWindow.MoveToLayer(uniqueName, oldLayer); }
                        catch (ArgumentException) { _ = CurrentWindow.BringToFront(uniqueName); }
                        break;
                }
            }
            catch (ShapeNotFoundException)
            {
                Logger.Error($"Shape {uniqueName} not found, while changing layer");
                throw;
            }
        }
    }
}
