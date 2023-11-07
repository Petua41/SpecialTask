using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure;

namespace SpecialTask.Console.Commands.ConcreteCommands.Internal
{
    /// <summary>
    /// Command to edit shape attributes
    /// </summary>
    internal class EditShapeAttributesCommand : ICommand
    {
        private readonly Shape receiver;

        private readonly string attribute;
        private readonly string newValue;

        private object? oldValue;

        public EditShapeAttributesCommand(string attribute, Shape shape, string newValue)
        {
            this.attribute = attribute;
            receiver = shape;
            this.newValue = newValue;
        }

        public void Execute()
        {
            try { oldValue = receiver.Edit(attribute, newValue); }
            catch (ArgumentException)
            {
                Logger.Error($"Cannot change {receiver.UniqueName}`s attribute {attribute}: invalid attribute");
                HighConsole.DisplayError($"{receiver.UniqueName} has no attribute {attribute}");
            }
            catch (ShapeAttributeCastException)
            {
                Logger.Error($"Cannot change {receiver.UniqueName}`s attribute {attribute}: invalid cast");
                HighConsole.DisplayError($"Invalid value for {attribute}: {newValue}");
            }
        }

        public void Unexecute()
        {
            if (oldValue is null)
            {
                Logger.Warning("EditShapeAttributesCommand unexecute before execute. Maybe execute exitted with error.");
            }
            else
            {
                _ = receiver.Edit(attribute, newValue);
            }
        }
    }
}
