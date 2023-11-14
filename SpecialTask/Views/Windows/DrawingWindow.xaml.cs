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
    }
}
