namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when edit command meets invalid input
    /// </summary>
    [Serializable]
    public class InvalidInputException : Exception
    {
        private const string _input = "Input";    // serialization

        [Obsolete("Don`t use InvalidInputException without passing wrong input or message")]
        public InvalidInputException() { }

        public InvalidInputException(string message) : base(message) { }

        public InvalidInputException(string message, string input) : base(message)
        {
            Input = input;
        }

        public InvalidInputException(string message, Exception inner) : base(message, inner) { }

        public InvalidInputException(string message, Exception inner, string input) : base(message, inner)
        {
            Input = input;
        }

        protected InvalidInputException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_input, Input);
        }

        public string? Input { get; }
    }
}
