using SpecialTask.Presenters;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace SpecialTask
{
    internal enum SpecialKeyCombinations { None, Enter, Backspace, CtrlC }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private readonly MainWindowPresenter presenter;

        public MainWindow()
        {
            InitializeComponent();

            presenter = new(this);
        }

        public void Display(string message, Color color)
        {
            presenter.Display(message, color);
        }

        public void Display(string message)
        {
            presenter.Display(message);
        }

        public void Display(string message, Brush brush)
        {
            presenter.Display(message, brush);
        }

        private void OnConsoleClosed(object sender, EventArgs e)
        {
            presenter.OnConsoleClosed(sender, e);
        }

        public void DisplayInputString(string text)
        {
            presenter.DisplayInputString(text);
        }
        private void ConsoleWindowGotFocus(object sender, RoutedEventArgs e)
        {
            presenter.ConsoleWindowGotFocus(sender, e);
        }

        private void ConsoleEntryKeyDown(object sender, KeyEventArgs e)
        {
            presenter.ConsoleEntryKeyDown(sender, e);
        }
    }
}
