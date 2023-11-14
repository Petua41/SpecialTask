namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when necessary argument is not entered by user
    /// </summary>
    [Serializable]
    public class NecessaryArgumentNotPresentedException : Exception
    {
        private const string _longArg = "LongArgument";    // serialization

        [Obsolete("Don`t use NecessaryArgumentNotPresentedException without passing long argument or message")]
        public NecessaryArgumentNotPresentedException() { }

        public NecessaryArgumentNotPresentedException(string message) : base(message) { }

        public NecessaryArgumentNotPresentedException(string message, string longArgument) : base(message)
        {
            LongArgument = longArgument;
        }

        public NecessaryArgumentNotPresentedException(string message, Exception inner) : base(message, inner) { }

        public NecessaryArgumentNotPresentedException(string message, Exception inner, string longArgument) : base(message, inner)
        {
            LongArgument = longArgument;
        }

        protected NecessaryArgumentNotPresentedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_longArg, LongArgument);
        }

        public string? LongArgument { get; }
    }
}
