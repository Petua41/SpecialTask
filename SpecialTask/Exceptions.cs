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
    class InvalidShapeAttributeException : Exception { }
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
}
