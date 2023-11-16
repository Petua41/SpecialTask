namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when while loading met element with unknown tag
    /// </summary>
    [Serializable]
    public class UnknownShapeTagException : Exception
    {
        private const string _tag = "Tag";    // serialization

        /// <summary>
        /// Don`t use UnknownShapeTagException without passing unknown tag or message
        /// </summary>
        public UnknownShapeTagException() { }

        public UnknownShapeTagException(string message) : base(message) { }

        public UnknownShapeTagException(string message, string tag) : base(message)
        {
            Tag = tag;
        }

        public UnknownShapeTagException(string message, Exception inner) : base(message, inner) { }

        public UnknownShapeTagException(string message, Exception inner, string tag) : base(message, inner)
        {
            Tag = tag;
        }

        protected UnknownShapeTagException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_tag, Tag);
        }

        public string? Tag { get; }
    }
}
