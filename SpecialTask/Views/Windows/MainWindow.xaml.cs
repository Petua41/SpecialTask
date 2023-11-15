using SpecialTask.Presenters;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace SpecialTask
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

        public void DisplayInputString(string text)
        {
            presenter.DisplayInputString(text);
        }

        private void OnConsoleClosed(object sender, EventArgs e)        // maybe presenter should subscribe to Closed, GotFocus and KeyDown?
        {
            presenter.OnConsoleClosed(sender, e);
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
