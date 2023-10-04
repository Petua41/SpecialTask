﻿using System;
using System.Collections.Generic;

namespace SpecialTask
{
    /// <summary>
    /// Контролирует вызов всех команд, их отмену и повтор
    /// </summary>
    static class CommandsFacade
    {
        private static readonly PseudoDeque<ICommand> stack = new();
        private static readonly Stack<ICommand> undoneStack = new();
        private static int undoStackDepth = 15;

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
            if (numberOfCommands > undoneStack.Count) throw new InvalidRedoNumber();
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
                STConsole.Instance.DisplayWarning("Nothung to undo!");
                throw;
            }
        }

        private static void Redo()
        {
            if (undoneStack.Count > 0)
            {
                ICommand command = undoneStack.Pop();
                RegisterAndExecute(command);
            }
            else throw new InvalidRedoNumber();
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
        private readonly Shape receiver;
        private readonly string attribute;
        private readonly object newValue;
        private object? oldValue;

        public EditShapeAttributesCommand(Dictionary<string, object> arguments)
        {
            try
            {
                attribute = (string)arguments["attribute"];
                receiver = (Shape)arguments["receiver"];
                newValue = arguments["newValue"];
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of EditShapeCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of EditShapeCommand");
                throw;
            }
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
        readonly int centerX;
        readonly int centerY;
        readonly EColor color;
        readonly int radius;
        readonly int lineThickness;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

        public CreateCircleCommand(Dictionary<string, object> arguments)
        {
            try
            {
                centerX = (int)arguments["centerX"];
                centerY = (int)arguments["centerY"];
                color = (EColor)arguments["color"];
                radius = (int)arguments["radius"];
                lineThickness = (int)arguments["lineThickness"];

                // unnecessary, but if streak is present, other should be too
                if (arguments.ContainsKey("streak"))
                {
                    streak = (bool)arguments["streak"];
                    streakTexture = (EStreakTexture)arguments["streakTexture"];
                    streakColor = (EColor)arguments["streakColor"];
                }
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of CreateCircleCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of CreateCircleCommand");
                throw;
            }
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
        readonly int leftTopX;
        readonly int leftTopY;
        readonly int rightBottomX;
        readonly int rightBottomY;
        readonly EColor color;
        readonly int lineThickness;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

        public CreateSquareCommand(Dictionary<string, object> arguments)
        {
            try
            {
                leftTopX = (int)arguments["leftTopX"];
                leftTopY = (int)arguments["leftTopY"];
                rightBottomX = (int)arguments["rightBottomX"];
                rightBottomY = (int)arguments["rightBottomY"];
                color = (EColor)arguments["color"];
                lineThickness = (int)arguments["lineThickness"];

                // unnecessary, but if streak is present, other should be too
                if (arguments.ContainsKey("streak"))
                {
                    streak = (bool)arguments["streak"];
                    streakTexture = (EStreakTexture)arguments["streakTexture"];
                    streakColor = (EColor)arguments["streakColor"];
                }
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of CreateSquareCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of CreateSquareCommand");
                throw;
            }
        }

        public void Execute()
        {
            receiver = new Square(leftTopX, leftTopY, rightBottomX, rightBottomY, color, lineThickness);
            if (streak) receiver = new StreakDecorator(receiver, streakColor, streakTexture);
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
        readonly int firstX;
        readonly int firstY;
        readonly int secondX;
        readonly int secondY;
        readonly EColor color;
        readonly int lineThickness;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

        public CreateLineCommand(Dictionary<string, object> arguments)
        {
            try
            {
                firstX = (int)arguments["firstX"];
                firstY = (int)arguments["firstY"];
                secondX = (int)arguments["secondX"];
                secondY = (int)arguments["secondY"];
                color = (EColor)arguments["color"];
                lineThickness = (int)arguments["lineThickness"];

                // unnecessary, but if streak is present, other should be too
                if (arguments.ContainsKey("streak"))
                {
                    streak = (bool)arguments["streak"];
                    streakTexture = (EStreakTexture)arguments["streakTexture"];
                    streakColor = (EColor)arguments["streakColor"];
                }
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of CreateLineCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of CreateLineCommand");
                throw;
            }
        }

        public void Execute()
        {
            receiver = new Line(firstX, firstY, secondX, secondY, color, lineThickness);
            if (streak) receiver = new StreakDecorator(receiver, streakColor, streakTexture);
        }

        public void Unexecute()
        {
            if (receiver == null) throw new CommandUnexecuteBeforeExecuteException();
            receiver.Destroy();
        }
    }

    /// <summary>
    /// Команда для создания нового окна
    /// </summary>
    class CreateWindowCommand : ICommand
    {
        private readonly WindowManager receiver;

        public CreateWindowCommand(Dictionary<string, object> arguments)        // arguments in unused, but it`s a part of interface (not defined in interface)
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
        private readonly WindowManager receiver;
        private readonly int numberOfWindow;

        public SwitchWindowCommand(Dictionary<string, object> arguments)
        {
            receiver = WindowManager.Instance;
            try
            {
                numberOfWindow = (int)arguments["number"];
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of SwitchWindowCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of SwitchWindowCommand");
                throw;
            }
        }

        public void Execute()
        {
            try { receiver.SwitchToWindow(numberOfWindow); }
            catch (WindowDoesntExistException)
            {
                Logger.Instance.Error(string.Format("Trying to switch to window {0}, but window {0} doesn`t exist", numberOfWindow));
                STConsole.Instance.DisplayCommandParsingError(string.Format("Window {0} doesn`t exist!", numberOfWindow));
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
        private readonly WindowManager receiver;
        private readonly int numberOfWindow;

        public DeleteWindowCommand(Dictionary<string, object> arguments)
        {
            receiver = WindowManager.Instance;
            try
            {
                numberOfWindow = (int)arguments["number"];
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of DeleteWindowCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of DeleteWindowCommand");
                throw;
            }
        }

        public void Execute()
        {
            try { receiver.DestroyWindow(numberOfWindow); }
            catch (WindowDoesntExistException)
            {
                Logger.Instance.Error(string.Format("Trying to delete window {0}, but window {0} doesn`t exist", numberOfWindow));
                STConsole.Instance.DisplayCommandParsingError(string.Format("[color:red]Window {0} doesn`t exist![color]", numberOfWindow));
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
        private readonly int leftTopX;
        private readonly int leftTopY;
        private readonly int rightBottomX;
        private readonly int rightBottomY;
        // TODO: receiver

        public SelectCommand(Dictionary<string, object> arguments)
        {
            // TODO: receiver
            try
            {
                int leftTopX = (int)arguments["leftTopX"];
                int leftTopY = (int)arguments["leftTopY"];
                int rightBottomX = (int)arguments["rightBottomX"];
                int rightBottomY = (int)arguments["rightBottomY"];
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of SelectCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of SelectCommand");
                throw;
            }
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
        private readonly int number;

        public UndoCommand(Dictionary<string, object> arguments)
        {
            try
            {
                number = (int)arguments["number"];
            }
            catch (KeyNotFoundException)
            {
                number = 1;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of UndoCommand");
                throw;
            }
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
        private readonly int number;

        public RedoCommand(Dictionary<string, object> arguments)
        {
            try
            {
                if (arguments.ContainsKey("number")) number = (int)arguments["number"];
                else number = 1;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of RedoCommand");
                throw;
            }
        }

        public void Execute()
        {
            try { CommandsFacade.RedoCommands(number); }
            catch (InvalidRedoNumber)
            {
                Logger.Instance.Error("Nothing to redo");
                STConsole.Instance.DisplayWarning("Nothing to redo!");
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

        public SaveCommand(Dictionary<string, object> arguments)
        {
            // TODO: receiver
        }

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
        readonly string filename;

        public SaveAsCommand(Dictionary<string, object> arguments)
        {
            // TODO: receiver
            try
            {
                filename = (string)arguments["filename"];
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of SaveAsCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of SaveAsCommand");
                throw;
            }
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
        readonly string filename;

        public LoadCommand(Dictionary<string, object> arguments)
        {
            // TODO: receiver
            try
            {
                filename = (string)arguments["filename"];
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of LoadCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of LoadCommand");
                throw;
            }
        }

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

        public ExitCommand(Dictionary<string, object> arguments)
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

    class ColorsCommand: ICommand
    {
        private STConsole receiver;

        public ColorsCommand(Dictionary<string, object> arguments)
        {
            receiver = STConsole.Instance;
        }

        public void Execute()
        {
            List<string> colors = ColorsController.GetColorsList();
            string output = "";
            foreach (string color in colors) output += string.Format("[color:{0}]{0}[color] ", color);
            output += "\n";
            receiver.Display(output);
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of colors command");
        }
    }

    class TexturesCommand : ICommand
    {
        private STConsole receiver;

        public TexturesCommand(Dictionary<string, object> arguments)
        {
            receiver = STConsole.Instance;
        }

        public void Execute()
        {
            Dictionary<string, string> textures = TextureController.GetTexturesWithDescriptions();
            string output = "";
            foreach (KeyValuePair<string, string> texture in textures)
            {
                output += texture.Key + "  --  " + texture.Value + "\n";
            }
            receiver.Display(output);
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of textures command");
        }
    }
}
