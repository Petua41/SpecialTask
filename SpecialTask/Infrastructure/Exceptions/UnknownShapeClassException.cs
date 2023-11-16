namespace SpecialTask.Infrastructure.Exceptions
{
    /// <summary>
    /// Raised, when while saving met unknown subclass of Shape
    /// </summary>
    [Serializable]
    public class UnknownShapeClassException : Exception
    {
        private const string _shapeClass = "ShapeClass";    // serialization

        /// <summary>
        /// Don`t use UnknownShapeClassException without passing shape class name or message
        /// </summary>
        public UnknownShapeClassException() { }

        public UnknownShapeClassException(string message) : base(message) { }

        public UnknownShapeClassException(string message, string shapeClass) : base(message)
        {
            ShapeClass = shapeClass;
        }

        public UnknownShapeClassException(string message, Exception inner) : base(message, inner) { }

        public UnknownShapeClassException(string message, Exception inner, string shapeClass) : base(message, inner)
        {
            ShapeClass = shapeClass;
        }

        protected UnknownShapeClassException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
            info.AddValue(_shapeClass, ShapeClass);
        }

        public string? ShapeClass { get; }
    }
}
