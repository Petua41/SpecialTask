namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// Command to remove shape
	/// </summary>
	class RemoveShapeCommand : ICommand
    {
        private readonly Shape receiver;

        public RemoveShapeCommand(Shape shape)
        {
            receiver = shape;
        }

        public void Execute()
        {
            receiver.Destroy();
        }

        public void Unexecute()
        {
            receiver.Redraw();
        }
    }
}
