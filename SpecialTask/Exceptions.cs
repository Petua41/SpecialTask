using System;

namespace SpecialTask
{
    /// <summary>
    /// Возникает при попытке обратиться к декоратору, который никого не декорирует
    /// </summary>
    class HangingDecoratorException : Exception { }
    /// <summary>
    /// Возникает при попытке изменить несуществующий атрибут в фигуре
    /// </summary>
    class InvalidShapeAttributeException : ArgumentException { }
    /// <summary>
    /// Возникает, когда при редактировании фигуры невозможно привести переданное значение к нужному типу
    /// </summary>
    class ShapeAttributeCastException: InvalidCastException { }
    /// <summary>
    /// Возникает при попытке отменить команду, которая ещё не была вызвана (или не закончила выполнение)
    /// </summary>
    class CommandUnexecuteBeforeExecuteException: Exception { }
    /// <summary>
    /// Возникает, если при переключении окна указан неверный номер окна
    /// </summary>
    class WindowDoesntExistException: ArgumentException { }
    /// <summary>
    /// Возникает, когда файл ресурсов не найден или содержит недопсутимые значения
    /// </summary>
    class InvalidResourceFileException: Exception { }
    /// <summary>
    /// Возникает при попытке снять элемент с пустого LimitedStack
    /// </summary>
    public class UnderflowException: Exception { }
    /// <summary>
    /// Возникает при ошибке распознавания аргумента, введённого в консоль
    /// </summary>
    class ArgumentParsingError: Exception { }
    /// <summary>
    /// Возникает, если класс ConcreteCommand не реализует ICommand, в том числе его необъявленную часть (не содержит нужного конструктора)
    /// </summary>
    class InvalidCommandClassException: Exception { }
    /// <summary>
    /// Возникает при вызове "fictional" команды (они используются только с --help)
    /// </summary>
    class CallOfFictionalCommandException: Exception { }
    /// <summary>
    /// Raised, when Accept() returns wrong data
    /// </summary>
    class VisitorInvalidAcceptError : Exception { }
    /// <summary>
    /// Raised, when save or save_as invoked, but there`s no point in saving
    /// </summary>
    class NothingToSaveException: Exception { }
    /// <summary>
    /// Raised, when while loading/saving met element with unknown tag or subclass of Shape
    /// </summary>
    class UnknownShapeException: Exception { }
    /// <summary>
    /// Raised, when cannot parse XML (while loading)
    /// </summary>
    class LoadXMLError: Exception { }
    /// <summary>
    /// Raised, when called SlectionMarker.Edit or Accept
    /// </summary>
    class SelectionMarkerException: Exception { }
    /// <summary>
    /// Raised, when shape sent backwards (brought forwards), but it`s already on the back (front)
    /// </summary>
    class CannotChangeShapeLayerException : Exception { }
    /// <summary>
    /// Raised, when shape with specified unique name doesn`t exist on current window
    /// </summary>
    class ShapeNotFoundException: Exception { }
    /// <summary>
    /// Keyboard interrupt
    /// </summary>
    class KeyboardInterruptException: Exception { }
    /// <summary>
    /// Raised, when edit command meets invalid input
    /// </summary>
    class InvalidInputException: Exception { }
}