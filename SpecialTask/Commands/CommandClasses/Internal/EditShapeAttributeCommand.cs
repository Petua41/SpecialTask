using System;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// Command to edit shape attributes
	/// </summary>
	class EditShapeAttributesCommand : ICommand
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
                Logger.Instance.Error($"Cannot change {receiver.UniqueName}`s attribute {attribute}: invalid attribute");
                MiddleConsole.HighConsole.DisplayError($"{receiver.UniqueName} has no attribute {attribute}");
            }
            catch (ShapeAttributeCastException)
            {
                Logger.Instance.Error($"Cannot change {receiver.UniqueName}`s attribute {attribute}: invalid cast");
                MiddleConsole.HighConsole.DisplayError($"Invalid value for {attribute}: {newValue}");
            }
        }

        public void Unexecute()
        {
            if (oldValue == null) Logger.Instance.Warning("EditShapeAttributesCommand unexecute before execute. Maybe execute exitted with error.");
            else receiver.Edit(attribute, newValue);
        }
    }
}
