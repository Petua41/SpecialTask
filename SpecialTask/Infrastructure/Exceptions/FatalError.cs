namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when application must be closed
    /// </summary>
    [Serializable]
    public class FatalError : Exception
    {
        public FatalError() { }

        public FatalError(string message) : base(message) { }

        public FatalError(string message, Exception inner) : base(message, inner) { }

        protected FatalError(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
