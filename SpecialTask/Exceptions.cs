using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialTask
{
    /// <summary>
    /// Возникает при попытке обратиться к одиночке напрямую, а не через Instance()
    /// </summary>
    class SingletonError : Exception { }
    /// <summary>
    /// Возникает при попытке обратиться к декоратору, который никого не декорирует
    /// </summary>
    class HangingDecoratorException : Exception { }
    /// <summary>
    /// Возникает при попытке изменить несуществующий атрибут в фигуре
    /// </summary>
    class InvalidShapeAttributeException : StringArgumentNameException { }
    /// <summary>
    /// Возникает при попытке присвоить недопустимое значение атрибуту фигуры
    /// </summary>
    class ShapeValueException : Exception { }
    /// <summary>
    /// Возникает, когда невозможно конвертировать одно представление цвета в другое
    /// </summary>
    class ColorExcepttion : Exception { }
    /// <summary>
    /// Возникает, когда при редактировании фигуры невозможно привести переданное значение к нужному типу
    /// </summary>
    class ShapeAttributeCastException: InvalidCastException { }
    /// <summary>
    /// Возникает, когда команде передано неправильное количество параметров
    /// </summary>
    class InvalidNumberOfCommandParametersException: ArgumentException { }
    /// <summary>
    /// Возникает при попытке отменить команду, которая ещё не была вызвана (или не закончила выполнение)
    /// </summary>
    class CommandUnexecuteBeforeExecuteException: Exception { }
    /// <summary>
    /// Возникает при попытке передать в качетсве аргумента недопустимое строковое значение
    /// </summary>
    class StringArgumentNameException: ArgumentException { }
    /// <summary>
    /// Возникае при попытке сравнить два объекта, когда один из них null
    /// </summary>
    class NullComparisonException : NullReferenceException { }
    /// <summary>
    /// Альтернатива abstract для статических методов
    /// </summary>
    class NotOverridenException : NotImplementedException { }
    /// <summary>
    /// Возникает, если при переключении окна указан неверный номер окна
    /// </summary>
    class WindowDoesntExistException: ArgumentException { }
    /// <summary>
    /// Возникает при попытке повторить больше команд, чем было отменено
    /// </summary>
    class InvalidRedoNumber: ArgumentException { }
    /// <summary>
    /// Возникает, когда файл ресурсов не найден или содержит недопсутимые значения
    /// </summary>
    class InvalidResourceFileException: Exception { }
    /// <summary>
    /// Возникает, если название цвета в escape-последовательности не распознано
    /// </summary>
    class EscapeSequenceParsingError: MessageDisplayingError { }
    /// <summary>
    /// Возникает, когда по какой-то причине невозможно отобразить сообщение в консоли
    /// </summary>
    class  MessageDisplayingError: Exception { }
    /// <summary>
    /// Возникает, если при передаче запроса по цепочке обязанностей кто-то передал запрос не туда
    /// </summary>
    class  ChainOfResponsibilityException: Exception {  }
    /// <summary>
    /// Возникает при попытке снять элемент с пустого PseudoDeque
    /// </summary>
    public class UnderflowException: Exception { }
    /// <summary>
    /// Возникает, когда файл ресурсов не найден
    /// </summary>
    class CannotFindResourceFileException: InvalidResourceFileException { }
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
    /// Raised, when user enters something like "new line -x100 -x50"
    /// </summary>
    class DuplicatedConsoleArgument: Exception { }
}
