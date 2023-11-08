namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Keyboard interrupt
    /// </summary>
    [Serializable]
    public class KeyboardInterruptException : Exception
    {
        public KeyboardInterruptException() { }

        public KeyboardInterruptException(string message) : base(message) { }

        public KeyboardInterruptException(string message, Exception inner) : base(message, inner) { }
        
        protected KeyboardInterruptException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
