using System.Windows.Media;

namespace SpecialTask.Drawing.BrushPrototypes
{
    /// <summary>
    /// Interface for Texture Brushes
    /// </summary>
    internal interface IBrushPrototype
    {
        /// <summary>
        /// Creates WPF Brush. All Brushes are freezed as possible
        /// </summary>
        /// <param name="wpfColor"></param>
        /// <returns></returns>
        Brush Brush(Color wpfColor);
    }
}
