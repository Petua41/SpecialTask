namespace SpecialTask.Infrastructure
{
    /// <summary>
    /// Raised when cannot cast parameter (while editing shape)
    /// </summary>
    internal class ShapeAttributeCastException : InvalidCastException { }

    /// <summary>
    /// Raised when resource file contains invalid values
    /// </summary>
    internal class InvalidResourceFileException : Exception { }

    /// <summary>
    /// Raised when entered argument cannot be recognized
    /// </summary>
    internal class ArgumentParsingError : ArgumentException { }

    /// <summary>
    /// Raised, when Accept() returns wrong data
    /// </summary>
    internal class VisitorInvalidAcceptError : Exception { }

    /// <summary>
    /// Raised, when while loading/saving met element with unknown tag or subclass of Shape
    /// </summary>
    internal class UnknownShapeException : Exception { }

    /// <summary>
    /// Raised, when cannot parse XML (while loading)
    /// </summary>
    internal class LoadXMLError : Exception { }

    /// <summary>
    /// Raised, when shape with specified unique name doesn`t exist on current window
    /// </summary>
    internal class ShapeNotFoundException : ArgumentException { }

    /// <summary>
    /// Keyboard interrupt
    /// </summary>
    internal class KeyboardInterruptException : Exception { }

    /// <summary>
    /// Raised, when edit command meets invalid input
    /// </summary>
    internal class InvalidInputException : Exception { }

    /// <summary>
    /// Raised, when application must be closed
    /// </summary>
    internal class FatalError : Exception { }
}