using System.Collections.Generic;

namespace SpecialTask
{
    /// <summary>
    /// Контролирует вызов всех команд, их отмену и повтор
    /// </summary>
    static class CommandsFacade
    {
        private static PseudoDeque<ICommand> stack = new();
        private static Stack<ICommand> undoneStack = new();
        private static int undoStackDepth;

        public static void RegisterAndExecute(ICommand command)
        {
            Push(command);
            command.Execute();
        }

        public static void ExecuteButDontRegister(ICommand command)
        {
            command.Execute();
        }

        public static void UndoCommands(int numberOfCommands = 1)
        {
            for (int i = 0; i < numberOfCommands; i++) Undo();
        }

        public static void RedoCommands(int numberOfCommands = 1)
        {
            if (numberOfCommands < undoneStack.Count) throw new InvalidRedoNumber();
            for (int i = 0; i < numberOfCommands; i++) Redo();
        }

        public static Stack<ICommand> Stack
        {
            get => new(stack);
        }

        public static Stack<ICommand> UndoneStack
        {
            get => new(undoneStack);
        }

        public static void ChangeUndoStackDepth(int depth)
        {
            undoStackDepth = depth;
        }

        private static void Undo()
        {
            try
            {
                ICommand command = stack.Pop();
                command.Unexecute();
                undoneStack.Push(command);
            }
            catch (UnderflowException)
            {
                Logger.Instance.Error("Noting to undo!");
                throw;
            }
        }

        private static void Redo()
        {
            ICommand command = undoneStack.Pop();
            RegisterAndExecute(command);
        }

        private static void Push(ICommand command)
        {
            if (stack.Count >= undoStackDepth) stack.PopBottom();
            stack.Push(command);
        }
    }

    interface ICommand
    {
        void Execute();
        void Unexecute();
    }

    /// <summary>
    /// Представляет команду для редактирования любых атрибутов фигур
    /// </summary>
    class EditShapeAttributesCommand : ICommand
    {
        private Shape receiver;
        private string attribute;
        private object newValue;
        private object? oldValue;

        public EditShapeAttributesCommand(string attribute, Shape receiver, object newValue)
        {
            this.attribute = attribute;
            this.receiver = receiver;
            this.newValue = newValue;
        }

        public void Execute()
        {
            oldValue = receiver.Edit(attribute, newValue);
        }

        public void Unexecute()
        {
            if (oldValue == null) throw new CommandUnexecuteBeforeExecuteException();
            receiver.Edit(attribute, newValue);
        }
    }

    /// <summary>
    /// Команда для добавления круга на экран
    /// </summary>
    class CreateCircleCommand : ICommand
    {
        private Shape? receiver;        // Нужен для отмены
        int centerX;
        int centerY;
        EColor color;
        int radius;
        int lineThickness;
        bool streak;
        EColor streakColor;
        EStreakTexture streakTexture;

        public CreateCircleCommand(int centerX, int centerY, EColor color, int radius, int lineThickness)
        {
            this.centerX = centerX;
            this.centerY = centerY;
            this.color = color;
            this.radius = radius;
            this.lineThickness = lineThickness;
        }

        public CreateCircleCommand(int centerX, int centerY, EColor color, int radius, int lineThickness, bool streak, EColor streakColor, EStreakTexture streakTexture)
            : this(centerX, centerY, color, radius, lineThickness)
        {
            this.streak = streak;
            this.streakColor = streakColor;
            this.streakTexture = streakTexture;
        }


        public void Execute()
        {
            receiver = new Circle(centerX, centerY, color, radius, lineThickness);
            if (streak) receiver = new StreakDecorator(receiver, streakColor, streakTexture);
        }

        public void Unexecute()
        {
            if (receiver == null) throw new CommandUnexecuteBeforeExecuteException();
            receiver.Destroy();
        }
    }

    /// <summary>
    /// Команда для добавления прямоугольника на экран
    /// </summary>
    class CreateSquareCommand : ICommand
    {
        private Shape? receiver;
        int leftTopX;
        int leftTopY;
        int rightBottomX;
        int rightBottomY;
        EColor color;
        int lineThickness;

        public CreateSquareCommand(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY, EColor color, int lineThickness)
        {
            this.leftTopX = leftTopX;
            this.leftTopY = leftTopY;
            this.rightBottomX = rightBottomX;
            this.rightBottomY = rightBottomY;
            this.color = color;
            this.lineThickness = lineThickness;
        }

        public void Execute()
        {
            receiver = new Square(leftTopX, leftTopY, rightBottomX, rightBottomY, color, lineThickness);
        }

        public void Unexecute()
        {
            if (receiver == null) throw new CommandUnexecuteBeforeExecuteException();
            receiver.Destroy();
        }
    }

    /// <summary>
    /// Команда для добавления линии на экран
    /// </summary>
    class CreateLineCommand : ICommand
    {
        private Shape? receiver;
        int firstX;
        int firstY;
        int secondX;
        int secondY;
        EColor color;
        int lineThickness;

        public CreateLineCommand(int firstX, int firstY, int secondX, int secondY, EColor color, int lineThickness)
        {
            this.firstX = firstX;
            this.firstY = firstY;
            this.secondX = secondX;
            this.secondY = secondY;
            this.color = color;
            this.lineThickness = lineThickness;
        }

        public void Execute()
        {
            receiver = new Square(firstX, firstY, secondX, secondY, color, lineThickness);
        }

        public void Unexecute()
        {
            if (receiver == null) throw new CommandUnexecuteBeforeExecuteException();
            receiver.Destroy();
        }
    }

    /// <summary>
    /// Команда для вывода глобальной справки
    /// </summary>
    class HelpCommand : ICommand
    {
        private STConsole receiver;
        private string helpText;

        public HelpCommand(string helpText)
        {
            receiver = STConsole.Instance;
            this.helpText = helpText;
        }

        public void Execute()
        {
            receiver.Display(helpText);
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of help command");
        }
    }

    /// <summary>
    /// Команда для создания нового окна
    /// </summary>
    class CreateWindowCommand : ICommand
    {
        private WindowManager receiver;

        public CreateWindowCommand()
        {
            receiver = WindowManager.Instance;
        }

        public void Execute()
        {
            receiver.CreateWindow();
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of window command");
        }
    }

    /// <summary>
    /// Команда для переключения на заданное окно
    /// </summary>
    class SwitchWindowCommand : ICommand
    {
        private WindowManager receiver;
        private int numberOfWindow;

        public SwitchWindowCommand(int number)
        {
            receiver = WindowManager.Instance;
            numberOfWindow = number;
        }

        public void Execute()
        {
            try { receiver.SwitchToWindow(numberOfWindow); }
            catch (WindowDoesntExistException)
            {
                Logger.Instance.Error(string.Format("Trying to switch to window {0}, but window {0} doesn`t exist", numberOfWindow));
                STConsole.Instance.Display(string.Format("[color:red]Window {0} doesn`t exist![color]", numberOfWindow));
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of window command");
        }
    }

    /// <summary>
    /// Команда для переключения на заданное окно
    /// </summary>
    class DeleteWindowCommand : ICommand
    {
        private WindowManager receiver;
        private int numberOfWindow;

        public DeleteWindowCommand(int number)
        {
            receiver = WindowManager.Instance;
            numberOfWindow = number;
        }

        public void Execute()
        {
            try { receiver.DestroyWindow(numberOfWindow); }
            catch (WindowDoesntExistException)
            {
                Logger.Instance.Error(string.Format("Trying to delete window {0}, but window {0} doesn`t exist", numberOfWindow));
                STConsole.Instance.Display(string.Format("[color:red]Window {0} doesn`t exist![color]", numberOfWindow));
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of window command");
        }
    }

    /// <summary>
    /// Команда для переключения на заданное окно
    /// </summary>
    class SelectCommand : ICommand
    {
        // TODO

        public SelectCommand()
        {
            // TODO
        }

        public void Execute()
        {
            // TODO
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of select command");
        }
    }

    /// <summary>
    /// Команда для отмены команд
    /// </summary>
    class UndoCommand : ICommand
    {
        // У неё нет receiver, потому что CommandsFacade статический
        int number;

        public UndoCommand(int number = 1)
        {
            this.number = number;
        }

        public void Execute()
        {
            try { CommandsFacade.UndoCommands(number); }
            catch (UnderflowException)
            {
                // Если отменять нечего, это не катастрофа. CommandsFacade уже записывает этот факт в лог
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of undo command");
        }
    }

    /// <summary>
    /// Команда для повтора отменённых команд
    /// </summary>
    class RedoCommand : ICommand
    {
        // У неё нет receiver, потому что CommandsFacade статический
        private int number;

        public RedoCommand(int number = 1)
        {
            this.number = number;
        }

        public void Execute()
        {
            try { CommandsFacade.RedoCommands(number); }
            catch (InvalidRedoNumber)
            {
                Logger.Instance.Error("Nothing to redo");
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of redo command");
        }
    }

    /// <summary>
    /// Команда "сохранить"
    /// </summary>
    class SaveCommand : ICommand
    {
        // TODO: я не знаю, кто здесь будет receiver

        public void Execute()
        {
            // TODO
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of save command");
        }
    }

    /// <summary>
    /// Команда "сохранить как"
    /// </summary>
    class SaveAsCommand : ICommand
    {
        // TODO: я не знаю, кто здесь будет receiver
        string filename;

        public SaveAsCommand(string filename)
        {
            // TODO: receiver
            this.filename = filename;
        }

        public void Execute()
        {
            // TODO
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of save_as command");
        }
    }

    /// <summary>
    /// Команда "загрузить"
    /// </summary>
    class LoadCommand : ICommand
    {
        // TODO: я не знаю, кто здесь будет receiver
        string filename;

        public LoadCommand(string filename)
        {
            // TODO: receiver
            this.filename = filename;
        }

        public void Execute()
        {
            // TODO
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of load command");
        }
    }

    /// <summary>
    /// Команда, чтобы закрыть приложение
    /// </summary>
    class ExitCommand : ICommand
    {
        private readonly System.Windows.Application receiver;

        public ExitCommand()
        {
            receiver = System.Windows.Application.Current;
        }

        public void Execute()
        {
            receiver.Shutdown();    // Выход из приложения должен перехватываться MainWindow.ConsoleClosed. Там же вся очитска
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of exit command.");
            Logger.Instance.Error("Exit command hasn`t closed application");
        }
    }
}
