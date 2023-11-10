using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpecialTask.Drawing.BrushPrototypes
{
    /// <summary>
    /// Fills figure with seamless texture. Color is ignored
    /// </summary>
    internal class SeamlessTexture : IBrushPrototype
    {
        private readonly ImageSource seamlessTexture;

        public SeamlessTexture(string filename)
        {
            Uri uri = new(filename, UriKind.Relative);
            seamlessTexture = new BitmapImage(uri);
        }

        public SeamlessTexture(System.Drawing.Bitmap image)
        {
            // looks scary (because it works with unmanaged memory), but it works:
            seamlessTexture = Imaging.CreateBitmapSourceFromHBitmap(image.GetHbitmap(), nint.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public Brush Brush(Color color)
        {
            Brush brush = new ImageBrush(seamlessTexture);
            if (brush.CanFreeze)
            {
                brush.Freeze();
            }

            return brush;
        }
    }
}
