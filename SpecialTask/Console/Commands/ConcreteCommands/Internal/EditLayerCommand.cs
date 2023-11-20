using SpecialTask.Infrastructure.Exceptions;
using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Console.Commands.ConcreteCommands.Internal
{
    internal enum LayerDirection { None, Forward, Backward, Front, Back }

    /// <summary>
    /// Command to move shapes up and down
    /// </summary>
    internal class EditLayerCommand : ICommand
    {
        private readonly string uniqueName;
        private readonly LayerDirection direction;

        private int oldLayer = -1;
        private bool layerChanged = false;

        public EditLayerCommand(string uniqueName, LayerDirection direction)
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
                    case LayerDirection.Forward:
                        oldLayer = WindowManager.CurrentWindow.BringForward(uniqueName);
                        break;
                    case LayerDirection.Backward:
                        oldLayer = WindowManager.CurrentWindow.SendBackward(uniqueName);
                        break;
                    case LayerDirection.Front:
                        oldLayer = WindowManager.CurrentWindow.BringToFront(uniqueName);
                        break;
                    case LayerDirection.Back:
                        oldLayer = WindowManager.CurrentWindow.SendToBack(uniqueName);
                        break;
                }
                layerChanged = true;
            }
            catch (ShapeNotFoundException)
            {
                Logger.Error($"Shape {uniqueName} not found, while changing layer");
                HighConsole.DisplayError($"Shape {uniqueName} not found, while changing layer. Please contact us");
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
                    case LayerDirection.Forward:
                        try { WindowManager.CurrentWindow.SendBackward(uniqueName); }
                        catch (InvalidOperationException)
                        {
                            Logger.Error($"[undo] Cannot send {uniqueName} backward: already on back");
                            HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on back");
                        }
                        break;
                    case LayerDirection.Backward:
                        try { WindowManager.CurrentWindow.BringForward(uniqueName); }
                        catch (InvalidOperationException)
                        {
                            Logger.Error($"[undo] Cannot bring {uniqueName} forward: already on top");
                            HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on top");
                        }
                        break;
                    case LayerDirection.Front:
                        try { WindowManager.CurrentWindow.MoveToLayer(uniqueName, oldLayer); }
                        catch (ArgumentException) { WindowManager.CurrentWindow.SendToBack(uniqueName); }
                        break;
                    case LayerDirection.Back:
                        try { WindowManager.CurrentWindow.MoveToLayer(uniqueName, oldLayer); }
                        catch (ArgumentException) { WindowManager.CurrentWindow.BringToFront(uniqueName); }
                        break;
                }
            }
            catch (ShapeNotFoundException)
            {
                Logger.Error($"Shape {uniqueName} not found, while changing layer");
                HighConsole.DisplayError($"Shape {uniqueName} not found, while changing layer. Please contact us");
            }
        }
    }
}
