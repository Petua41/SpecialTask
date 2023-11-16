namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when Shape.Accept() returns wrong data
    /// </summary>
    [Serializable]
    public class VisitorInvalidAcceptError : Exception
    {
        public VisitorInvalidAcceptError() { }

        public VisitorInvalidAcceptError(string message) : base(message) { }

        public VisitorInvalidAcceptError(string message, Exception inner) : base(message, inner) { }

        protected VisitorInvalidAcceptError(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
