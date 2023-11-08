namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised when entered argument cannot be recognized
    /// </summary>
    [Serializable]
    public class ArgumentParsingError : Exception
    {
        private const string _argName = "ArgumentName";    // serialization

        [Obsolete("Don`t use ArgumentParsingError without passing argument name or message")]
        public ArgumentParsingError() { }

        public ArgumentParsingError(string message) : base(message) { }

        public ArgumentParsingError(string message, string argumentName) : base(message)
        {
            ArgumentName = argumentName;
        }

        public ArgumentParsingError(string message, Exception inner) : base(message, inner) { }

        public ArgumentParsingError(string message, Exception inner, string argumentName) : base(message, inner)
        {
            ArgumentName = argumentName;
        }

        protected ArgumentParsingError(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_argName, ArgumentName);
        }

        public string? ArgumentName { get; }
    }
}
