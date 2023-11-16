namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised when resource file contains invalid values
    /// </summary>
    [Serializable]
    public class InvalidResourceFileException : Exception
    {
        public InvalidResourceFileException() { }

        public InvalidResourceFileException(string message) : base(message) { }

        public InvalidResourceFileException(string message, Exception inner) : base(message, inner) { }

        protected InvalidResourceFileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
