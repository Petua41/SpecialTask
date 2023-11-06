namespace SpecialTask.Exceptions
{
    /// <summary>
    /// Возникает, когда при редактировании фигуры невозможно привести переданное значение к нужному типу
    /// </summary>
    class ShapeAttributeCastException : InvalidCastException { }
    /// <summary>
    /// Возникает, когда файл ресурсов не найден или содержит недопсутимые значения
    /// </summary>
    class InvalidResourceFileException : Exception { }
    /// <summary>
    /// Возникает при ошибке распознавания аргумента, введённого в консоль
    /// </summary>
    class ArgumentParsingError : ArgumentException { }
    /// <summary>
    /// Возникает, если класс ConcreteCommand не реализует ICommand, в том числе его необъявленную часть (не содержит нужного конструктора)
    /// </summary>
    class InvalidCommandClassException : Exception { }
    /// <summary>
    /// Raised, when Accept() returns wrong data
    /// </summary>
    class VisitorInvalidAcceptError : Exception { }
    /// <summary>
    /// Raised, when while loading/saving met element with unknown tag or subclass of Shape
    /// </summary>
    class UnknownShapeException : Exception { }
    /// <summary>
    /// Raised, when cannot parse XML (while loading)
    /// </summary>
    class LoadXMLError : Exception { }
    /// <summary>
    /// Raised, when shape with specified unique name doesn`t exist on current window
    /// </summary>
    class ShapeNotFoundException : ArgumentException { }
    /// <summary>
    /// Keyboard interrupt
    /// </summary>
    class KeyboardInterruptException : Exception { }
    /// <summary>
    /// Raised, when edit command meets invalid input
    /// </summary>
    class InvalidInputException : Exception { }
}