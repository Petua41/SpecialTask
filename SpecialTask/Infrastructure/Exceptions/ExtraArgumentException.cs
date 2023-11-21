namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when necessary argument is not entered by user
    /// </summary>
    [Serializable]
    public class ExtraArgumentException : Exception
    {
        private const string _longArg = "LongArgument";    // serialization

        /// <summary>
        /// Don`t use ExtraArgumentException without passing long argument or message
        /// </summary>
        public ExtraArgumentException() { }

        public ExtraArgumentException(string message) : base(message) { }

        public ExtraArgumentException(string message, string longArgument) : base(message)
        {
            LongArgument = longArgument;
        }

        public ExtraArgumentException(string message, Exception inner) : base(message, inner) { }

        public ExtraArgumentException(string message, Exception inner, string longArgument) : base(message, inner)
        {
            LongArgument = longArgument;
        }

        protected ExtraArgumentException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_longArg, LongArgument);
        }

        public string? LongArgument { get; }
    }
}
