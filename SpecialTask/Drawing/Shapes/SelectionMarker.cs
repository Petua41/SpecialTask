using SpecialTask.Drawing.BrushPrototypes;
using SpecialTask.Infrastructure.Collections;
using SpecialTask.Infrastructure.Enums;
using System.Windows.Media;

namespace SpecialTask.Drawing.Shapes
{
    internal class SelectionMarker : Shape
    {
        private readonly Brush brush = new GeometryTileTexture(new EllipseGeometry(new(5, 5), 5, 5)).Brush(Colors.Black);
        private readonly Square square;

        private static int firstAvailibleUniqueNumber = 0;

        public SelectionMarker(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY)
        {
            square = new(leftTopX, leftTopY, rightBottomX, rightBottomY, EColor.Black, 1);
            uniqueName = GetNextUniqueName();
        }

        public static new string GetNextUniqueName()
        {
            return $"SelectionMarker_{firstAvailibleUniqueNumber++}";
        }

        public override void Display()
        {
            square.Display();

            DestroyAfterDelay(5000);
        }

        public override object Edit(string attribute, string value) { throw new InvalidOperationException(); }

        public override Dictionary<string, object> Accept()
        {
            throw new InvalidOperationException();
        }

        public override void MoveXBy(int offset)
        {
            throw new InvalidOperationException();
        }

        public override void MoveYBy(int offset)
        {
            throw new InvalidOperationException();
        }

        public override System.Windows.Shapes.Shape WPFShape
        {
            get
            {
                if (base.wpfShape is not null)
                {
                    return base.wpfShape;
                }

                System.Windows.Shapes.Shape wpfShape = square.WPFShape;
                wpfShape.Stroke = brush;

                square.Destroy();

                base.wpfShape = wpfShape;
                return wpfShape;
            }
        }

        public override Point Center => square.Center;

        private async void DestroyAfterDelay(int delay)
        {
            await Task.Delay(delay);

            Destroy();
        }

        public override void Destroy()
        {
            square.Destroy();
            base.Destroy();
        }

        public override Shape Clone()
        {
            throw new InvalidOperationException();
        }

        public override Pairs<string, string> AttributesToEditWithNames => new();
    }
}
