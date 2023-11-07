using System.Windows.Media;

namespace SpecialTask.Drawing.BrushPrototypes
{
    /// <summary>
    /// Texture brush for linear gradients. Last color must be passed to Brush()
    /// </summary>
    internal class LinearGradientTexture : GradientTexture
    {
        private System.Windows.Point startPoint;
        private System.Windows.Point endPoint;

        public LinearGradientTexture(System.Windows.Point startPoint, System.Windows.Point endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }

        protected override GradientBrush GetGradBrush(GradientStopCollection grStops)
        {
            return new LinearGradientBrush(grStops, startPoint, endPoint);
        }
    }
}
