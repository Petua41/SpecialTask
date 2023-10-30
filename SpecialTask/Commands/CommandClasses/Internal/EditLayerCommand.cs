using System;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
    /// Command to move shapes up and down
    /// </summary>
    class EditLayerCommand : ICommand
    {
        private readonly WindowManager receiver;

        private readonly string uniqueName;
        private readonly ELayerDirection direction;

        private int oldLayer = -1;
        private bool layerChanged = false;

        public EditLayerCommand(string uniqueName, ELayerDirection direction)
        {
            receiver = WindowManager.Instance;
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
                        oldLayer = receiver.BringForward(uniqueName);
                        layerChanged = true;
                        break;
                    case ELayerDirection.Backward:
                        oldLayer = receiver.SendBackward(uniqueName);
                        layerChanged = true;
                        break;
                    case ELayerDirection.Front:
                        oldLayer = receiver.BringToFront(uniqueName);
                        layerChanged = true;
                        break;
                    case ELayerDirection.Back:
                        oldLayer = receiver.SendToBack(uniqueName);
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
                        try { receiver.SendBackward(uniqueName); }
                        catch (InvalidOperationException)
                        {
                            Logger.Instance.Error($"[undo] Cannot send {uniqueName} backward: already on back");
                            MiddleConsole.HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on back");
                        }
                        break;
                    case ELayerDirection.Backward:
                        try { receiver.BringForward(uniqueName); }
                        catch (InvalidOperationException)
                        {
                            Logger.Instance.Error($"[undo] Cannot bring {uniqueName} forward: already on top");
                            MiddleConsole.HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on top");
                        }
                        break;
                    case ELayerDirection.Front:
                        try { receiver.MoveToLayer(uniqueName, oldLayer); }
                        catch (ArgumentException) { receiver.SendToBack(uniqueName); }
                        break;
                    case ELayerDirection.Back:
                        try { receiver.MoveToLayer(uniqueName, oldLayer); }
                        catch (ArgumentException) { receiver.BringToFront(uniqueName); }
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
