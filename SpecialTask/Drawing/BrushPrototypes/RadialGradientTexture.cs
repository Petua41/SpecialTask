using System.Collections;
using System.Windows.Media;

namespace SpecialTask.Drawing.BrushPrototypes
{
    /// <summary>
    /// Texture brush for radial gradients. Last color must be passed to Brush()
    /// </summary>
    class RadialGradientTexture : IBrushPrototype, IEnumerable<GradientStop>
    {
        private readonly List<GradientStop> gradientStops = new();

        public void Add(Color color, double offset)
        {
            gradientStops.Add(new(color, offset));
        }

        public Brush Brush(Color color)
        {
            Add(color, 1);
            GradientStopCollection grStCollection = new(gradientStops);
            Brush brush = new RadialGradientBrush(grStCollection);
            if (brush.CanFreeze) brush.Freeze();
            return brush;
        }

        public IEnumerator<GradientStop> GetEnumerator()
        {
            return gradientStops.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
