namespace SpecialTaskConverter.Exceptions
{

    [Serializable]
    public class STDParsingException : Exception
    {
        public STDParsingException() { }
        public STDParsingException(string message) : base(message) { }
        public STDParsingException(string message, Exception inner) : base(message, inner) { }
        protected STDParsingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
