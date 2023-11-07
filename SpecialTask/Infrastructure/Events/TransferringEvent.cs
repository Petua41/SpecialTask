namespace SpecialTask.Infrastructure.Events
{
    public class TransferringEventArgs : EventArgs
    {
        public TransferringEventArgs(string input)
        {
            Input = input;
        }

        public string Input { get; set; }
    }

    public delegate void TransferringEventHandler(object sender, TransferringEventArgs args);
}
