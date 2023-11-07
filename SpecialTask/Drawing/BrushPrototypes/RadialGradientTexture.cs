using System.Windows.Media;

namespace SpecialTask.Drawing.BrushPrototypes
{
    /// <summary>
    /// Texture brush for radial gradients. Last color must be passed to Brush()
    /// </summary>
    internal class RadialGradientTexture : GradientTexture
    {
        protected override GradientBrush GetGradBrush(GradientStopCollection grStops)
        {
            return new RadialGradientBrush(grStops);
        }
    }
}
