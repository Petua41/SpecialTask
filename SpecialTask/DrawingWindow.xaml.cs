using System.Windows;

namespace SpecialTask
{
    /// <summary>
    /// Interaction logic for DrawingWindow.xaml
    /// </summary>
    public partial class DrawingWindow : Window
    {
        public DrawingWindow()
        {
            InitializeComponent();
        }

        public void ChangeTitle(string value)
        {
            Title = value;
        }

        private void DrawingWindowClosed(object sender, EventArgs e)
        {
            DrawingWindowClosedEvent?.Invoke(this, e);
        }

        public event EventHandler? DrawingWindowClosedEvent;
    }
}
