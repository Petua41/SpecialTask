﻿using System;
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
    /// Возникает при попытке отменить фигуру, которая ещё не была вызвана (или не закончила выполнение)
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
    /// Альтернатива abstract для статическиз методов
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
    class  InvalidResourceFileException: Exception { }
    /// <summary>
    /// Возникает, если название цвета в escape-последовательности не распознано
    /// </summary>
    class EscapeSequenceParsingError: ColorExcepttion { }
}
