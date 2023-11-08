namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised when cannot cast parameter (while editing shape)
    /// </summary>
    [Serializable]
    public class ShapeAttributeCastException : Exception
    {
        private const string _attrName = "AttributeName";    // serialization
        private const string _value = "Value";

        [Obsolete("Don`t use ShapeAttributeCastException without passing attribute name or value or message")]
        public ShapeAttributeCastException() { }

        public ShapeAttributeCastException(string message) : base(message) { }

        public ShapeAttributeCastException(string message, string attributeName) : base(message)
        {
            AttributeName = attributeName;
        }

        public ShapeAttributeCastException(string message, string attributeName, string value) : base(message)
        {
            AttributeName = attributeName;
            Value = value;
        }

        public ShapeAttributeCastException(string message, Exception inner) : base(message, inner) { }

        public ShapeAttributeCastException(string message, Exception inner, string attributeName) : base(message, inner)
        {
            AttributeName = attributeName;
        }

        public ShapeAttributeCastException(string message, Exception inner, string attributeName, string value) : base(message, inner)
        {
            AttributeName = attributeName;
            Value = value;
        }

        protected ShapeAttributeCastException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_attrName, AttributeName);
            info.AddValue(_value, Value);
        }

        public string? AttributeName { get; }

        public string? Value { get; }
    }
}
