using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace SpecialTask.Drawing.Shapes.WPF
{
    class WPFText : System.Windows.Shapes.Shape
    {
        private int leftTopX = 0;
        private int leftTopY = 0;
        private int fontSize = 0;
        private string textValue = string.Empty;
        private Brush brush = Brushes.Transparent;

        private readonly Typeface typeface = new("Calibri");
        private readonly CultureInfo cultureInfo = CultureInfo.CurrentCulture;
        private readonly FlowDirection flowDirection = FlowDirection.LeftToRight;
        private const int DIP = 1;		// idk, how to get this value, but 1 works good

        protected override void OnRender(DrawingContext drawingContext)
        {
            System.Windows.Point point = new(leftTopX, leftTopY);
            drawingContext.DrawText(FormText, point);
        }

        public int Left
        {
            get => leftTopX;
            set => leftTopX = value;
        }

        public int Top
        {
            get => leftTopY;
            set => leftTopY = value;
        }

        public int FontSize
        {
            get => fontSize;
            set => fontSize = value;
        }

        public string Text
        {
            get => textValue;
            set => textValue = value;
        }

        public new Brush Stroke
        {
            get => brush;
            set => brush = value;
        }

        protected override Geometry DefiningGeometry => FormText.BuildHighlightGeometry(new(Left, Top));

        private FormattedText FormText => new(Text, cultureInfo, flowDirection, typeface, FontSize, Stroke, DIP);
    }
}
