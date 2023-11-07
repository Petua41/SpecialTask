using System.Collections;
using System.Windows.Media;

namespace SpecialTask.Drawing.BrushPrototypes
{
    /// <summary>
    /// Template class for gradient brushes
    /// </summary>
    internal abstract class GradientTexture : IBrushPrototype, IEnumerable<GradientStop>
    {
        protected GradientStopCollection gradientStops = new();

        public void Add(Color color, double offset)
        {
            gradientStops.Add(new(color, offset));
        }

        public IEnumerator<GradientStop> GetEnumerator()
        {
            return gradientStops.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Brush Brush(Color color)
        {
            Add(color, 1);
            Brush brush = GetGradBrush(gradientStops);
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }

            return brush;
        }

        protected abstract GradientBrush GetGradBrush(GradientStopCollection grStops);
    }
}
