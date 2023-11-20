namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when necessary argument is not entered by user
    /// </summary>
    [Serializable]
    public class ArgumentMissingValueException : Exception
    {
        private const string _longArg = "LongArgument";    // serialization

        /// <summary>
        /// Don`t use NecessaryArgumentNotPresentedException without passing long argument or message
        /// </summary>
        public ArgumentMissingValueException() { }

        public ArgumentMissingValueException(string message) : base(message) { }

        public ArgumentMissingValueException(string message, string longArgument) : base(message)
        {
            LongArgument = longArgument;
        }

        public ArgumentMissingValueException(string message, Exception inner) : base(message, inner) { }

        public ArgumentMissingValueException(string message, Exception inner, string longArgument) : base(message, inner)
        {
            LongArgument = longArgument;
        }

        protected ArgumentMissingValueException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_longArg, LongArgument);
        }

        public string? LongArgument { get; }
    }
}
