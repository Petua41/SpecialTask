namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when cannot parse XML (while loading)
    /// </summary>
    [Serializable]
    public class LoadXMLException : Exception
    {
        public LoadXMLException() { }

        public LoadXMLException(string message) : base(message) { }

        public LoadXMLException(string message, Exception inner) : base(message, inner) { }

        protected LoadXMLException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
