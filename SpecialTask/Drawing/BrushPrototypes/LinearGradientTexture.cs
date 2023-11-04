using System.Collections;
using System.Windows;
using System.Windows.Media;

namespace SpecialTask.Drawing.BrushPrototypes
{
    /// <summary>
    /// Texture brush for linear gradients. Last color must be passed to Brush()
    /// </summary>
    class LinearGradientTexture : IBrushPrototype, IEnumerable<GradientStop>
    {
        private readonly List<GradientStop> gradientStops = new();
        private Point startPoint;
        private Point endPoint;

        public LinearGradientTexture(Point startPoint, Point endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }

        public void Add(Color color, double offset)
        {
            gradientStops.Add(new(color, offset));
        }

        public Brush Brush(Color color)
        {
            Add(color, 1);
            GradientStopCollection grStCollection = new(gradientStops);
            Brush brush = new LinearGradientBrush(grStCollection, startPoint, endPoint);
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
