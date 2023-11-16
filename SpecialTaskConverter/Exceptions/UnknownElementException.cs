namespace SpecialTaskConverter.Exceptions
{
    /// <summary>
    /// Raised when met element with unknown tag while converting .std file
    /// </summary>
    [Serializable]
    internal class UnknownElementException : Exception
    {
        private const string _elemTag = "ElementTag";    // do not rename: binary serialization

        /// <summary>
        /// Don`t use UnknownElementException without specifying element tag or message
        /// </summary>
        public UnknownElementException() { }

        public UnknownElementException(string message) : base(message) { }

        public UnknownElementException(string message, string elementTag) : base(message)
        {
            ElementTag = elementTag;
        }

        public UnknownElementException(string message, Exception inner) : base(message, inner) { }

        public UnknownElementException(string message, Exception inner, string elementTag) : base(message, inner)
        {
            ElementTag = elementTag;
        }

        protected UnknownElementException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_elemTag, ElementTag);
        }

        public string? ElementTag { get; }
    }
}
