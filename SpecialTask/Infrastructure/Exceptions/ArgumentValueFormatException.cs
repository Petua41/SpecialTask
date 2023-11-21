namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when necessary argument is not entered by user
    /// </summary>
    [Serializable]
    public class ArgumentValueFormatException : Exception
    {
        private const string _longArg = "LongArgument";    // serialization
        private const string _value = "Value";

        /// <summary>
        /// Don`t use ArgumentValueFormatException without passing long argument or message
        /// </summary>
        public ArgumentValueFormatException() { }

        public ArgumentValueFormatException(string message) : base(message) { }

        public ArgumentValueFormatException(string message, string longArgument) : base(message)
        {
            LongArgument = longArgument;
        }

        public ArgumentValueFormatException(string message, string longArgument, string value) : base(message)
        {
            LongArgument = longArgument;
            Value = value;
        }

        public ArgumentValueFormatException(string message, Exception inner) : base(message, inner) { }

        public ArgumentValueFormatException(string message, Exception inner, string longArgument) : base(message, inner)
        {
            LongArgument = longArgument;
        }

        public ArgumentValueFormatException(string message, Exception inner, string longArgument, string value) : base(message, inner)
        {
            LongArgument = longArgument;
            Value = value;
        }

        protected ArgumentValueFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_longArg, LongArgument);
            info.AddValue(_value, Value);
        }

        public string? LongArgument { get; }

        public string? Value { get; }
    }
}
