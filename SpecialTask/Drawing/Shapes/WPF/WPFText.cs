using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SpecialTask.Drawing.Shapes.WPF
{
    internal class WPFText : System.Windows.Shapes.Shape
    {
        private readonly Typeface typeface = new("Calibri");    // We could ask user what type they want, but command will be too complicated for graphics editor
        private readonly CultureInfo cultureInfo = CultureInfo.CurrentCulture;
        private readonly FlowDirection flowDirection = FlowDirection.LeftToRight;
        private const int DIP = 1;                              // idk how to get this value, but 1 works good

        protected override void OnRender(DrawingContext drawingContext)
        {
            System.Windows.Point point = new(Left, Top);
            drawingContext.DrawText(FormText, point);
        }

        public int Left { get; set; } = 0;

        public int Top { get; set; } = 0;

        public int FontSize { get; set; } = 0;

        public string Text { get; set; } = string.Empty;

        public new Brush Stroke { get; set; } = Brushes.Transparent;

        protected override Geometry DefiningGeometry => FormText.BuildHighlightGeometry(new(Left, Top));

        private FormattedText FormText => new(Text, cultureInfo, flowDirection, typeface, FontSize, Stroke, DIP);
    }
}
