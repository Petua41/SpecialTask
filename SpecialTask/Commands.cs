using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace SpecialTask
{
	enum ELayerDirection { None, Forward, Backward, Front, Back }

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
			try { command.Execute(); }
			catch (KeyboardInterruptException) { MiddleConsole.HighConsole.DisplayError("Keyboard interrupt"); }
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
				receiver = (Shape)arguments["shape"];

				// YANDERE
				string stringNewValue = (string)arguments["newValue"];
				newValue = attribute switch
				{
					"text" => stringNewValue,
					"color" or "streakColor" => ColorsController.Parse(stringNewValue),
					"streakTexture" => TextureController.Parse(stringNewValue),
					"points" => EArgumentType.Points.ParseValue(stringNewValue),
					_ => int.Parse(stringNewValue)
				};
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
			try { oldValue = receiver.Edit(attribute, newValue); }
			catch (InvalidShapeAttributeException)
			{
				Logger.Instance.Error($"Cannot change {receiver.UniqueName}`s attribute {attribute}: invalid attribute");
				MiddleConsole.HighConsole.DisplayError($"{receiver.UniqueName} has no attribute {attribute}");
			}
			catch (ShapeAttributeCastException)
			{
                Logger.Instance.Error($"Cannot change {receiver.UniqueName}`s attribute {attribute}: invalid cast");
                MiddleConsole.HighConsole.DisplayError($"");
            }
		}

		public void Unexecute()
		{
			if (oldValue == null) Logger.Instance.Warning("EditShapeAttributesCommand unexecute before execute. Maybe execute exitted with error.");
			else receiver.Edit(attribute, newValue);
		}
	}

	/// <summary>
	/// Command to move shapes up and down
	/// </summary>
	class EditLayerCommand: ICommand
	{
		private readonly WindowManager receiver;
        private readonly string uniqueName;
		private readonly ELayerDirection direction;

		private int oldLayer = -1;
		private bool layerChanged = false;

		public EditLayerCommand(string uniqueName,  ELayerDirection direction)
		{
			receiver = WindowManager.Instance;
			this.uniqueName = uniqueName;
			this.direction = direction;
		}

        public void Execute()
        {
			try
			{
				switch (direction)
				{
					case ELayerDirection.Forward:
                        oldLayer = receiver.BringForward(uniqueName);
                        layerChanged = true;
                        break;
                    case ELayerDirection.Backward:
						oldLayer = receiver.SendBackward(uniqueName);
						layerChanged = true;
						break;
					case ELayerDirection.Front:
						oldLayer = receiver.BringToFront(uniqueName);
						layerChanged = true;
						break;
					case ELayerDirection.Back:
                        oldLayer = receiver.SendToBack(uniqueName);
                        layerChanged = true;
                        break;
                }
            }
            catch (ShapeNotFoundException)
            {
                Logger.Instance.Error($"Shape {uniqueName} not found, while changing layer");
				throw;
            }
        }

        public void Unexecute()
        {
			if (!layerChanged) return;
            try
            {
                switch (direction)
                {
                    case ELayerDirection.Forward:
                        try { receiver.SendBackward(uniqueName); }
                        catch (CannotChangeShapeLayerException)
						{
							Logger.Instance.Error($"[undo] Cannot send {uniqueName} backward: already on back");
							MiddleConsole.HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on back");
                        }
                        break;
                    case ELayerDirection.Backward:
                        try { receiver.BringForward(uniqueName); }
                        catch (CannotChangeShapeLayerException)
						{
                            Logger.Instance.Error($"[undo] Cannot bring {uniqueName} forward: already on top");
                            MiddleConsole.HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on top");
                        }
                        break;
                    case ELayerDirection.Front:
						try { receiver.MoveToLayer(uniqueName, oldLayer); }
						catch (ArgumentException) { receiver.SendToBack(uniqueName); }
                        break;
                    case ELayerDirection.Back:
                        try { receiver.MoveToLayer(uniqueName, oldLayer); }
                        catch (ArgumentException) { receiver.BringToFront(uniqueName); }
                        break;
                }
            }
            catch (ShapeNotFoundException)
            {
                Logger.Instance.Error($"Shape {uniqueName} not found, while changing layer");
				throw;
            }
        }
    }

	/// <summary>
	/// Wrapper (console-side) to edit shapes
	/// </summary>
    class  EditCommand: ICommand
    {
        private ICommand? receiver = null;
		private readonly ESortingOrder sortingOrder;

		private List<Shape> listOfShapes = new();

		private int selectedNumber = -1;
		private string interString = "";
		private bool waitingForNumber = false;
		private bool waitingForString = false;

		private Task task;
		private CancellationTokenSource tokenSource;

		private bool ctrlCPressed = false;		// YANDERE

		private Shape? shapeToEdit;

		public EditCommand(Dictionary<string, object> parameters)
		{
            sortingOrder = (parameters.ContainsKey("coordinates") && (bool)parameters["coordinates"]) ? 
				ESortingOrder.Coordinates : ESortingOrder.CreationTime;
			IteratorsFacade.SetConcreteIterator(sortingOrder);

			tokenSource = new();
			task = new(EmptyTask, tokenSource.Token);

			MiddleConsole.HighConsole.SomethingTranferred += OnSomethingTransferred;
		}

		// TODO: this method is TOO long
        public async void Execute()
        {
            MiddleConsole.HighConsole.TransferringInput = true;
            MiddleConsole.HighConsole.InputBlocked = false;

            try 
			{
                listOfShapes = IteratorsFacade.GetCompleteResult();

                if (listOfShapes.Count == 0)
                {
                    MiddleConsole.HighConsole.DisplayWarning("Nothing to edit");
                    return;					// FIXME: spare prompt
                }

                DisplayShapeSelectionPrompt((from shape in listOfShapes select shape.UniqueName).ToList());

                selectedNumber = -1;
                waitingForNumber = true;

                while (selectedNumber < 0)
                {
                    tokenSource = new();
                    task = new(EmptyTask, tokenSource.Token);

                    try { await task; }
                    catch (TaskCanceledException) { /* continue */ }
                }
                MiddleConsole.HighConsole.NewLine();
				if (ctrlCPressed) throw new KeyboardInterruptException();

                if (selectedNumber >= listOfShapes.Count) throw new InvalidInputException();

                shapeToEdit = listOfShapes[selectedNumber];

                DisplayWhatToEditSelectionPrompt();

                selectedNumber = -1;
                waitingForNumber = true;

                while (selectedNumber < 0)
                {
                    tokenSource = new();
                    task = new(EmptyTask, tokenSource.Token);

                    try { await task; }
                    catch (TaskCanceledException) { /* continue */ }
                }
                MiddleConsole.HighConsole.NewLine();
                if (ctrlCPressed) throw new KeyboardInterruptException();

                switch (selectedNumber)
                {
                    case 0:
                        // edit layer:
                        DisplayLayerOperationSelectionPrompt(shapeToEdit.UniqueName);

                        selectedNumber = -1;
                        waitingForNumber = true;

                        while (selectedNumber < 0)
                        {
                            tokenSource = new();
                            task = new(EmptyTask, tokenSource.Token);

                            try { await task; }
                            catch (TaskCanceledException) { /* continue */ }
                        }
                        MiddleConsole.HighConsole.NewLine();
                        if (ctrlCPressed) throw new KeyboardInterruptException();

                        ELayerDirection dir = selectedNumber switch
                        {
                            0 => ELayerDirection.Backward,
                            1 => ELayerDirection.Forward,
                            2 => ELayerDirection.Back,
                            3 => ELayerDirection.Front,
                            _ => throw new InvalidInputException()
                        };

                        receiver = new EditLayerCommand(shapeToEdit.UniqueName, dir);
                        CommandsFacade.ExecuteButDontRegister(receiver);

                        break;
                    case 1:
						// edit attributes:
						MyMap<string, string> attrsWithNames = shapeToEdit.AttributesToEditWithNames;
						// TODO: add streak, if there`s no

						DisplayAttributeSelectionPrompt(shapeToEdit.UniqueName, attrsWithNames.Keys);

                        selectedNumber = -1;
                        waitingForNumber = true;

                        while (selectedNumber < 0)
                        {
                            tokenSource = new();
                            task = new(EmptyTask, tokenSource.Token);

                            try { await task; }
                            catch (TaskCanceledException) { /* continue */ }
                        }
                        MiddleConsole.HighConsole.NewLine();
                        if (ctrlCPressed) throw new KeyboardInterruptException();

                        if (selectedNumber >= attrsWithNames.Count) throw new InvalidInputException();

						KeyValuePair<string, string> kvp = attrsWithNames[selectedNumber];
						DisplayNewAttributePrompt(kvp.Value);

						interString = "";
						waitingForString = true;

                        while (interString.Length == 0)
                        {
                            tokenSource = new();
                            task = new(EmptyTask, tokenSource.Token);

                            try { await task; }
                            catch (TaskCanceledException) { /* continue */ }
                        }
						MiddleConsole.HighConsole.NewLine();
                        if (ctrlCPressed) throw new KeyboardInterruptException();

                        receiver = new EditShapeAttributesCommand(new() { { "shape", shapeToEdit }, { "attribute", kvp.Key }, { "newValue", interString } });
						CommandsFacade.ExecuteButDontRegister(receiver);

                        break;
                    default:
                        throw new InvalidInputException();
                }
            }
			catch (InvalidInputException) 
			{
				Logger.Instance.Error("Edit: invalid input");
				MiddleConsole.HighConsole.DisplayError("Invalid input"); 
			}
			catch (KeyboardInterruptException)
			{
				Logger.Instance.Error("Edit: keyboard interrupt");
				MiddleConsole.HighConsole.DisplayError("Kyboard interrupt");
			}
			finally
			{
                MiddleConsole.HighConsole.TransferringInput = false;
                MiddleConsole.HighConsole.InputBlocked = false;
				MiddleConsole.HighConsole.NewLine();
				MiddleConsole.HighConsole.DisplayPrompt();
            }
        }

		private static void DisplayShapeSelectionPrompt(List<string> lst)
		{
			MiddleConsole.HighConsole.Display("Select figure to edit: ");
			MiddleConsole.HighConsole.NewLine();
			for (int i = 0; i < lst.Count; i++)
			{
                MiddleConsole.HighConsole.Display($"{i}. {lst[i]}");
                MiddleConsole.HighConsole.NewLine();
            }
		}

		private static void DisplayWhatToEditSelectionPrompt()
		{
			MiddleConsole.HighConsole.Display("Select what to edit: ");
			MiddleConsole.HighConsole.NewLine();
			MiddleConsole.HighConsole.Display("0. Layer\n1. Figure attributes");
			MiddleConsole.HighConsole.NewLine();
			MiddleConsole.HighConsole.DisplayPrompt();
		}

		private static void DisplayLayerOperationSelectionPrompt(string uniqueName)
		{
			MiddleConsole.HighConsole.Display($"Select what to do with [color:green]{uniqueName}[color]: ");
			MiddleConsole.HighConsole.NewLine();
			MiddleConsole.HighConsole.Display("0. Send backwards\n1. Bring forward\n2. Send to back\n3. Bring to front");
			MiddleConsole.HighConsole.NewLine();
            MiddleConsole.HighConsole.DisplayPrompt();
        }

		private static void DisplayAttributeSelectionPrompt(string uniqueName, List<string> names)
		{
            MiddleConsole.HighConsole.Display($"Availible attributes for [color:green]{uniqueName}[color]: ");
            MiddleConsole.HighConsole.NewLine();
			for (int i = 0; i < names.Count; i++)
			{
				MiddleConsole.HighConsole.Display($"{i}. {names[i]}");
				MiddleConsole.HighConsole.NewLine();
			}
            MiddleConsole.HighConsole.DisplayPrompt();
        }

		private static void DisplayNewAttributePrompt(string attrName)
		{
			MiddleConsole.HighConsole.DisplayQuestion($"Enter new value for {attrName}:");
        }

        private void OnSomethingTransferred(object? sender, EventArgs e)
        {
			ESpecialKeyCombinations comb = MiddleConsole.HighConsole.TransferredCombination;		// We must save it, because it`s erased on get

			// FIXME: YANDERE
			if (comb == ESpecialKeyCombinations.CtrlC)
			{
				ctrlCPressed = true;

				interString = "KBI";	// so that if (sele.. or if (inter.. ends
				selectedNumber = 10;

                tokenSource.Cancel(true);
                return;
			}

			if (waitingForNumber)
			{
                char trChar = MiddleConsole.HighConsole.TransferredChar ?? ' ';

                if (char.IsNumber(trChar))
                {
                    selectedNumber = int.Parse(trChar.ToString());
					waitingForNumber = false;
                    tokenSource.Cancel(true);
                }
            }
			else if (waitingForString)
			{
				if (comb == ESpecialKeyCombinations.Enter)
				{
					string trString = MiddleConsole.HighConsole.TransferredString;
					if (trString.Length > 0)
					{
						interString = trString;
						waitingForString = false;
						tokenSource.Cancel(true);
					}
				}
			}
        }

        private void EmptyTask()
        {
            while (true) ;
        }

        public void Unexecute()
		{
			if (receiver == null)
			{
				Logger.Instance.Warning("Edit command unexecute before execute. Maybe execute was interrupted by keyboard");
			}
			else receiver.Unexecute();
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
    /// Command to add text
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
	/// Command to add polygon
	/// </summary>
    class CreatePolygonCommand : ICommand
    {
        private Shape? receiver;
        readonly List<(int, int)> points;
        readonly int lineThickness;
        readonly EColor color;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

        public CreatePolygonCommand(Dictionary<string, object> arguments)
        {
            try
            {
                points = (List<(int, int)>)arguments["points"];
                lineThickness = (int)arguments["lineThickness"];
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
            receiver = new Polygon(points, lineThickness, color);
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
				Logger.Instance.Error($"Trying to switch to window {numberOfWindow}, but window {numberOfWindow} doesn`t exist");
				MiddleConsole.HighConsole.DisplayError($"Window {numberOfWindow} doesn`t exist!");
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
	/// Command for selecting shapes on specified area
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
    /// Command to paste selected shapes
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
					Logger.Instance.Error($"Cannot save to {filename}: invalid characters");
					MiddleConsole.HighConsole.DisplayError("Filename cannot contain theese characters: /\\:*?\"<>|");
				}
				else
				{
					Logger.Instance.Error($"Cannot save to {filename}: directory {dir} doesn`t exists");
					MiddleConsole.HighConsole.DisplayError($"Directory {dir} doesn`t exist");
				}
			}
			catch (UnauthorizedAccessException)
			{
				Logger.Instance.Error($"Cannot save to: {filename}: no permissions");
				MiddleConsole.HighConsole.DisplayError($"you have no permission to write to {filename}. This incident will be reported");
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
				Logger.Instance.Error($"Cannot load {filename}: invalid file format");
				MiddleConsole.HighConsole.DisplayError("Invalid file format");
			}
			catch (FileNotFoundException)
			{
				Logger.Instance.Error($"Cannot load {filename}: file not found");
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
			destroyedShapes = new(receiver.ShapesOnCurrentWindow);

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
		private readonly Task task;
		private readonly CancellationTokenSource tokenSource;

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
			catch (TaskCanceledException) { /* continue */ }

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

	/// <summary>
	/// Displays list of colors (with examples!)
	/// </summary>
	class ColorsCommand: ICommand
	{
		private readonly IHighConsole receiver;

		public ColorsCommand(Dictionary<string, object> arguments)
		{
			receiver = MiddleConsole.HighConsole;
		}

		public void Execute()
		{
			List<string> colors = ColorsController.GetColorsList();
			string output = "";
			foreach (string color in colors) output += $"[color:{color}]{color}[color] ";
			output += "\n";
			receiver.Display(output);
		}

		public void Unexecute()
		{
			Logger.Instance.Warning("Unexecution of colors command");
		}
	}

	/// <summary>
	/// Display list of textures (with descriptions)
	/// </summary>
	class TexturesCommand : ICommand
	{
		private readonly IHighConsole receiver;

		public TexturesCommand(Dictionary<string, object> arguments)
		{
			receiver = MiddleConsole.HighConsole;
		}

		public void Execute()
		{
			Dictionary<string, string> textures = TextureController.TexturesWithDescriptions;
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
