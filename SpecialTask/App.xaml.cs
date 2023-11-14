using SpecialTask.Console.CommandsParser;
using SpecialTask.Infrastructure;
using SpecialTask.Infrastructure.Exceptions;
using SpecialTask.Infrastructure.Loggers;
using SpecialTask.Infrastructure.WindowSystem;
using System.Windows;

namespace SpecialTask
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow mainWindow = new()
            {
                DataContext = null
            };

            mainWindow.Show();

            PathsController.InitPaths();                    // we must call it before any other calls. It`s not good
            InitializeLogger(ConcreteLoggers.SimpleLogger); // so that it gets right creation time
            try
            {
                ConsoleCommandsParser.InitializeCommands();
            }
            catch (FatalError)
            {
                Logger.Fatal($"Invalid XML file with commands!{Environment.NewLine}Please, contact us");
                Current.Shutdown();
            }

            _ = LowConsole;                 // So that it will be initialized

            _ = WindowManager.Instance;     // So that it will be initialized

            ParseCommandLineArguments(e.Args);
        }

        private static void ParseCommandLineArguments(string[] command_line_args)
        {
            int undo_stack_depth = 15;

            Mono.Options.OptionSet optionSet = new()
            {
                { "d|undo_stack_depth", (int d) => undo_stack_depth = d }       // we don`t add descriptions to options, because we cannot show help
            };

            optionSet.Parse(command_line_args);

            if (undo_stack_depth > 0) LowConsole.ChangeUndoStackDepth(undo_stack_depth);
        }
    }
}
