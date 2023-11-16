namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when shape with specified unique name doesn`t exist on current window
    /// </summary>
    [Serializable]
    public class ShapeNotFoundException : Exception
    {
        private const string _shapeClass = "ShapeClass";    // serialization

        /// <summary>
        /// Don`t use ShapeNotFoundException without passing shape name or message
        /// </summary>
        public ShapeNotFoundException() { }

        public ShapeNotFoundException(string message) : base(message) { }

        public ShapeNotFoundException(string message, string shapeClass) : base(message)
        {
            ShapeClass = shapeClass;
        }

        public ShapeNotFoundException(string message, Exception inner) : base(message, inner) { }

        public ShapeNotFoundException(string message, Exception inner, string shapeClass) : base(message, inner)
        {
            ShapeClass = shapeClass;
        }

        protected ShapeNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_shapeClass, ShapeClass);
        }

        public string? ShapeClass { get; }
    }
}
