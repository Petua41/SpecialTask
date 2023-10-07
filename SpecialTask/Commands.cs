using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SpecialTask
{
	/// <summary>
	/// Controlls execution, undo and redo of all Commands
	/// </summary>
	static class CommandsFacade			// Facade and Mediator at the same time.
	{
		private static int undoStackDepth = 15;
		private static int currentWindowNumber = 0;
		private static readonly Dictionary<int, PseudoDeque<ICommand>> stacks = new();
		private static readonly Dictionary<int, Stack<ICommand>> undoneStacks = new();

		static CommandsFacade()
		{
			WindowManager.Instance.WindowSwitchedEvent += OnWindowSwitched;
		}

		public static void RegisterAndExecute(ICommand command)
		{
			Push(command);
            ExecuteButDontRegister(command);
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
			if (numberOfCommands > UndoneStack.Count) throw new InvalidRedoNumber();
			for (int i = 0; i < numberOfCommands; i++) Redo();
		}

		public static void ChangeUndoStackDepth(int depth)
		{
			undoStackDepth = depth;
		}

		private static void Undo()
		{
			try
			{
				ICommand command = Stack.Pop();
				command.Unexecute();
				UndoneStack.Push(command);
			}
			catch (UnderflowException)
			{
				Logger.Instance.Error("Noting to undo!");
				MiddleConsole.HighConsole.DisplayWarning("Nothung to undo!");
				throw;
			}
		}

		private static void Redo()
		{
			if (UndoneStack.Count > 0)
			{
				ICommand command = UndoneStack.Pop();
				RegisterAndExecute(command);
			}
			else throw new InvalidRedoNumber();
		}

		private static void Push(ICommand command)
		{
			if (Stack.Count >= undoStackDepth) Stack.PopBottom();
			Stack.Push(command);
		}

		private static PseudoDeque<ICommand> Stack
		{
			get
			{
				if (stacks.ContainsKey(currentWindowNumber)) return stacks[currentWindowNumber];
				PseudoDeque<ICommand> newStack = new();
				stacks.Add(currentWindowNumber, newStack);
				return newStack;
			}
		}

		private static Stack<ICommand> UndoneStack
		{
			get
			{
				if (undoneStacks.ContainsKey(currentWindowNumber)) return undoneStacks[currentWindowNumber];
				Stack<ICommand> newStack = new();
				undoneStacks.Add(currentWindowNumber, newStack);
				return newStack;
			}
		}

		private static void OnWindowSwitched(object sender, WindowSwitchedEventArgs e)
		{
			currentWindowNumber = e.NewNumber;
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
    /// Команда для добавления прямоугольника на экран
    /// </summary>
    class CreateTextCommand : ICommand
    {
        private Shape? receiver;
        readonly int leftTopX;
        readonly int leftTopY;
        readonly int fontSize;
        readonly string textValue;
        readonly EColor color;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

        public CreateTextCommand(Dictionary<string, object> arguments)
        {
            try
            {
                leftTopX = (int)arguments["leftTopX"];
                leftTopY = (int)arguments["leftTopY"];
                fontSize = (int)arguments["fontSize"];
                textValue = (string)arguments["textValue"];
                color = (EColor)arguments["color"];

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
            receiver = new Text(leftTopX, leftTopY, fontSize, textValue, color);
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
				MiddleConsole.HighConsole.DisplayError(string.Format("Window {0} doesn`t exist!", numberOfWindow));
			}
		}

		public void Unexecute()
		{
			Logger.Instance.Warning("Unexecution of window command");
		}
	}

	/// <summary>
	/// Command to delete window
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
				MiddleConsole.HighConsole.DisplayError(string.Format("[color:red]Window {0} doesn`t exist![color]", numberOfWindow));
			}
		}

		public void Unexecute()
		{
			Logger.Instance.Warning("Unexecution of window command");
		}
	}

	/// <summary>
	/// Command for selectiong shapes on specified area
	/// </summary>
	class SelectCommand : ICommand
	{
		private readonly int leftTopX;
		private readonly int leftTopY;
		private readonly int rightBottomX;
		private readonly int rightBottomY;

		public SelectCommand(Dictionary<string, object> arguments)
		{
			try
			{
				leftTopX = (int)arguments["leftTopX"];
				leftTopY = (int)arguments["leftTopY"];
				rightBottomX = (int)arguments["rightBottomX"];
				rightBottomY = (int)arguments["rightBottomY"];
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
			SelectionMarker marker = new(leftTopX, leftTopY, rightBottomX, rightBottomY);

			SelectPasteHandler.SaveArea(leftTopX, leftTopY, rightBottomX, rightBottomY);
		}

		public void Unexecute()
		{
			Logger.Instance.Warning("Unexecution of select command");
		}
	}

    /// <summary>
    /// Команда для переключения на заданное окно
    /// </summary>
    class PasteCommand : ICommand
    {
        private readonly int leftTopX;
        private readonly int leftTopY;
		// TODO: receiver -- something to save area

		private List<Shape> pastedShapes = new();

        public PasteCommand(Dictionary<string, object> arguments)
        {
            try
            {
                leftTopX = (int)arguments["leftTopX"];
                leftTopY = (int)arguments["leftTopY"];
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of PasteCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of PasteCommand");
                throw;
            }
        }

        public void Execute()
        {
			pastedShapes = SelectPasteHandler.PasteArea(leftTopX, leftTopY);
        }

        public void Unexecute()
        {
			foreach (Shape shape in pastedShapes) shape.Destroy();
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
				MiddleConsole.HighConsole.DisplayWarning("Nothing to redo!");
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
        // no receiver, because SaveLoadFacade is static

        public SaveCommand(Dictionary<string, object> arguments)
		{
			// nothing to do
		}

		public void Execute()
		{
			try { SaveLoadFacade.Instance.Save(); }
			catch (NothingToSaveException)
			{
				Logger.Instance.Warning("Nothing to save");
				MiddleConsole.HighConsole.DisplayWarning("File is already saved");
			}
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
		// no receiver, because SaveLoadFacade is static
		readonly string filename;

		public SaveAsCommand(Dictionary<string, object> arguments)
		{
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
			try { SaveLoadFacade.Instance.SaveAs(filename); }
			catch (IOException)
			{
				int idx = filename.LastIndexOf('\\');
				string dir = filename[..idx];
				if (Directory.Exists(dir))
				{
					Logger.Instance.Error(string.Format("Cannot save to {0}: invalid characters", filename));
					MiddleConsole.HighConsole.DisplayError("Filename cannot contain theese characters: /\\:*?\"<>|");
				}
				else
				{
					Logger.Instance.Error(string.Format("Cannot save to {0}: directory {1} doesn`t exists", filename, dir));
					MiddleConsole.HighConsole.DisplayError(string.Format("Directory {0} doesn`t exist", dir));
				}
			}
			catch (UnauthorizedAccessException)
			{
				Logger.Instance.Error(string.Format("Cannot save to: {0}: no permissions", filename));
				MiddleConsole.HighConsole.DisplayError(string.Format("you have no permission to write to {0}. This incident will be reported", filename));
			}
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
        // no receiver, because SaveLoadFacade is static
        readonly string filename;
        readonly bool clearScreen = false;

		public LoadCommand(Dictionary<string, object> arguments)
		{
			try
			{
				filename = (string)arguments["filename"];
				if (arguments.ContainsKey("clearScreen")) clearScreen = (bool)arguments["clearScreen"];
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

		public void Execute()
		{
			if (clearScreen)
			{
				CommandsFacade.RegisterAndExecute(new ClearCommand(new()));
			}

			try { SaveLoadFacade.Instance.Load(filename); }
			catch (LoadXMLError)
			{
				Logger.Instance.Error(string.Format("Cannot load {0}: invalid file format", filename));
				MiddleConsole.HighConsole.DisplayError("Invalid file format");
			}
			catch (FileNotFoundException)
			{
				Logger.Instance.Error(string.Format("Cannot load {0}: file not found", filename));
				MiddleConsole.HighConsole.DisplayError("File not found");
			}
		}

		public void Unexecute()
		{
			Logger.Instance.Warning("Unexecution of load command");
		}
	}

	class ClearCommand: ICommand
	{
		private readonly WindowManager receiver;
		private List<Shape> destroyedShapes = new();

		public ClearCommand(Dictionary<string, object> arguments)
		{
			receiver = WindowManager.Instance;
		}

		public void Execute()
		{
			destroyedShapes = new(receiver.ShapesOnCurrentWindow());

			foreach (Shape shape in destroyedShapes) shape.Destroy();
		}

		public void Unexecute()
		{
			foreach (Shape shape in destroyedShapes) shape.Redraw();

			destroyedShapes.Clear();
		}
	}

	/// <summary>
	/// Команда, чтобы закрыть приложение
	/// </summary>
	class ExitCommand : ICommand
	{
		enum EYesNoSaveAnswer { None, Yes, No, Save }

		private readonly System.Windows.Application receiver;
		private EYesNoSaveAnswer answer = EYesNoSaveAnswer.None;
		private Task task;
		private CancellationTokenSource tokenSource;

        public ExitCommand(Dictionary<string, object> arguments)
		{
			receiver = System.Windows.Application.Current;
			MiddleConsole.HighConsole.SomethingTranferred += OnSomethingTransferred;
            tokenSource = new();
            task = new Task(EmptyTask, tokenSource.Token);
        }

		public void Execute()
		{
            if (SaveLoadFacade.Instance.NeedsSave)
            {
                MiddleConsole.HighConsole.TransferringInput = true;
                MiddleConsole.HighConsole.InputBlocked = false;
                MiddleConsole.HighConsole.DisplayQuestion("File is not saved. Exit? [y, s, n] (default=n)");

				GetInputIfNotSaved();
            }
            else receiver.Shutdown();
        }

		public void Unexecute()
		{
			Logger.Instance.Warning("Unexecution of exit command.");
		}

		private async void GetInputIfNotSaved()
		{
			try { await task; }
			catch (TaskCanceledException)
			{
				// continue
			}

            MiddleConsole.HighConsole.TransferringInput = false;

            switch (answer)
            {
                case EYesNoSaveAnswer.Yes:
                    receiver.Shutdown();
                    break;
                case EYesNoSaveAnswer.No:
					MiddleConsole.HighConsole.NewLine();
					MiddleConsole.HighConsole.DisplayPrompt();
                    break;
                case EYesNoSaveAnswer.Save:
                    MiddleConsole.HighConsole.Display("saving...");
					CommandsFacade.ExecuteButDontRegister(new SaveCommand(new()));		// We don`t need to check anything here. SaveLoadFacade will do
                    receiver.Shutdown();
                    break;
                default:
                    Logger.Instance.Error("None answer in exit command");
                    return;
            }
        }

		private void OnSomethingTransferred(object? sender, EventArgs e)
		{
			if (MiddleConsole.HighConsole.TransferredChar == 'y' || MiddleConsole.HighConsole.TransferredChar == 'Y' || 
				MiddleConsole.HighConsole.TransferredString.ToLower() == "yes") answer = EYesNoSaveAnswer.Yes;

			else if (MiddleConsole.HighConsole.TransferredChar == 's' || MiddleConsole.HighConsole.TransferredChar == 'S' ||
				MiddleConsole.HighConsole.TransferredString.ToLower() == "save") answer = EYesNoSaveAnswer.Save;

			else answer = EYesNoSaveAnswer.No;

			tokenSource.Cancel(true);
		}

		private void EmptyTask()
		{
			while (true) ;
		}
	}

	class ColorsCommand: ICommand
	{
		private IHighConsole receiver;

		public ColorsCommand(Dictionary<string, object> arguments)
		{
			receiver = MiddleConsole.HighConsole;
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
		private readonly IHighConsole receiver;

		public TexturesCommand(Dictionary<string, object> arguments)
		{
			receiver = MiddleConsole.HighConsole;
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
