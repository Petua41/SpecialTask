using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SpecialTaskConverter;

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
		private static readonly Dictionary<int, LimitedStack<ICommand>> stacks = new();
		private static readonly Dictionary<int, Stack<ICommand>> undoneStacks = new();

		static CommandsFacade()
		{
			WindowManager.Instance.WindowSwitchedEvent += OnWindowSwitched;
		}

		public static void RegisterAndExecute(ICommand command)
		{
			Stack.Push(command);
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
			if (numberOfCommands > UndoneStack.Count) throw new InvalidOperationException();
			for (int i = 0; i < numberOfCommands; i++) Redo();
		}

		public static void ChangeUndoStackDepth(int depth)
		{
			undoStackDepth = depth;
			Stack = new(Stack, undoStackDepth);
		}

		private static void Undo()
		{
			ICommand command = Stack.Pop();
			command.Unexecute();
			UndoneStack.Push(command);
        }

		private static void Redo()
		{
			if (UndoneStack.Count > 0)
			{
				ICommand command = UndoneStack.Pop();
				RegisterAndExecute(command);
			}
		}

		private static LimitedStack<ICommand> Stack
		{
			get
			{
				if (stacks.ContainsKey(currentWindowNumber)) return stacks[currentWindowNumber];
				LimitedStack<ICommand> newStack = new(undoStackDepth);
				stacks.Add(currentWindowNumber, newStack);
				return newStack;
			}
			set
			{
				stacks[currentWindowNumber] = value;
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
		private const string ATTRIBUTE = "attribute";
		private const string SHAPE = "shape";
		private const string NEW_VALUE = "newValue";

		private readonly Shape receiver;

		private readonly string attribute;
		private readonly string newValue;

		private object? oldValue;

		public EditShapeAttributesCommand(Dictionary<string, object> arguments)
		{
			try
			{
				attribute = (string)arguments[ATTRIBUTE];
				receiver = (Shape)arguments[SHAPE];
				newValue = (string)arguments[NEW_VALUE];
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
			catch (ArgumentException)
			{
				Logger.Instance.Error($"Cannot change {receiver.UniqueName}`s attribute {attribute}: invalid attribute");
				MiddleConsole.HighConsole.DisplayError($"{receiver.UniqueName} has no attribute {attribute}");
			}
			catch (ShapeAttributeCastException)
			{
                Logger.Instance.Error($"Cannot change {receiver.UniqueName}`s attribute {attribute}: invalid cast");
                MiddleConsole.HighConsole.DisplayError($"Invalid value for {attribute}: {newValue}");
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

		public EditLayerCommand(string uniqueName, ELayerDirection direction)
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
                        catch (InvalidOperationException)
						{
							Logger.Instance.Error($"[undo] Cannot send {uniqueName} backward: already on back");
							MiddleConsole.HighConsole.DisplayError($"Cannot undo: {uniqueName} is already on back");
                        }
                        break;
                    case ELayerDirection.Backward:
                        try { receiver.BringForward(uniqueName); }
                        catch (InvalidOperationException)
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
	/// Command to remove shape
	/// </summary>
	class RemoveShapeCommand : ICommand
    {
        private readonly Shape receiver;

        public RemoveShapeCommand(Shape shape)
        {
            receiver = shape;
        }

        public void Execute()
        {
			receiver.Destroy();
        }

        public void Unexecute()
        {
            receiver.Redraw();
        }
    }

	/// <summary>
	/// Command to decorate shape with StreakDecorator
	/// </summary>
    class AddStreakCommand : ICommand
    {
        private readonly Shape? receiver;

		private readonly EColor streakColor;
		private readonly EStreakTexture streakTexture;

		private StreakDecorator? decorator;

        public AddStreakCommand(Shape shape, EColor streakColor, EStreakTexture streakTexture)
        {
            receiver = shape;
            this.streakColor = streakColor;
            this.streakTexture = streakTexture;
        }

        public void Execute()
        {
            decorator = new(receiver, streakColor, streakTexture);
        }

        public void Unexecute()
        {
			decorator?.Destroy();
            receiver?.Redraw();
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
		private string interString = "";
		private int selectedNumber = -1;
		private bool hasStreak = false;

		private CancellationTokenSource tokenSource = new();
		private bool ctrlCPressed = false;
		private Shape? shapeToEdit;

		public EditCommand(Dictionary<string, object> parameters)
		{
            sortingOrder = (parameters.ContainsKey("coordinates") && (bool)parameters["coordinates"])
				? ESortingOrder.Coordinates
				: ESortingOrder.CreationTime;

			IteratorsFacade.SetConcreteIterator(sortingOrder);

			MiddleConsole.HighConsole.SomethingTranferred += OnStringTransferred;
			MiddleConsole.HighConsole.CtrlCTransferred += OnCtrlCTransferred;
		}

		// TODO: this method is TOO long
        public async void Execute()
        {
            MiddleConsole.HighConsole.TransferringInput = true;

            try 
			{
                listOfShapes = IteratorsFacade.GetCompleteResult();

                if (listOfShapes.Count == 0)
                {
                    MiddleConsole.HighConsole.DisplayWarning("Nothing to edit");
                    return;
                }

                DisplayShapeSelectionPrompt((from shape in listOfShapes select shape.UniqueName).ToList());

				await GetSelectedNumber();

                if (selectedNumber >= listOfShapes.Count) throw new InvalidInputException();

                shapeToEdit = listOfShapes[selectedNumber];

				if (shapeToEdit is StreakDecorator) hasStreak = true;

                DisplayWhatToEditSelectionPrompt(hasStreak);

                await GetSelectedNumber();

                switch (selectedNumber)
                {
                    case 0:
                        // edit layer:
                        DisplayLayerOperationSelectionPrompt(shapeToEdit.UniqueName);

                        await GetSelectedNumber();

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
                        MyMap<string, string> attrsWithNames = shapeToEdit.AttributesToEditWithNames;	// MyMap, because it`s ordered

                        DisplayAttributeSelectionPrompt(shapeToEdit.UniqueName, attrsWithNames.Keys);

                        await GetSelectedNumber();

                        if (selectedNumber >= attrsWithNames.Count) throw new InvalidInputException();

						KeyValuePair<string, string> kvp = attrsWithNames[selectedNumber];

                        DisplayNewAttributePrompt(kvp.Value);

						interString = "";

						await GetInterString();

                        receiver = new EditShapeAttributesCommand(new() { { "shape", shapeToEdit }, { "attribute", kvp.Key }, { "newValue", interString } });
						CommandsFacade.ExecuteButDontRegister(receiver);

                        break;
					case 2:
						// remove shape:
						receiver = new RemoveShapeCommand(shapeToEdit);
						CommandsFacade.ExecuteButDontRegister(receiver);
						break;
					case 3:
						// add streak:
						DisplayNewAttributePrompt("Streak color");
						await GetInterString();

						EColor color = ColorsController.Parse(interString);

						DisplayNewAttributePrompt("Streak texture");
						await GetInterString();

						EStreakTexture texture = TextureController.Parse(interString);

						receiver = new AddStreakCommand(shapeToEdit, color, texture);
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
				MiddleConsole.HighConsole.NewLine();
				MiddleConsole.HighConsole.DisplayPrompt();
            }
        }

		private async Task GetSelectedNumber()
		{
			await GetInterString();

            try { selectedNumber = int.Parse(interString); }
            catch (FormatException) { throw new InvalidInputException(); }
        }

		private async Task GetInterString()
		{
			tokenSource = new();
            Task task = new(EmptyTask, tokenSource.Token);

            try { await task; }
            catch (TaskCanceledException) { /* continue */ }

            // проверить можно ли написать так:
            // Task.Run(EmptyTask, tokenSource.Token)
            /*
			task.ContinueWith(t =>
			{
				MiddleConsole.HighConsole.NewLine();
				if (ctrlCPressed) throw new KeyboardInterruptException();
			});
			*/

            MiddleConsole.HighConsole.NewLine();
            if (ctrlCPressed) throw new KeyboardInterruptException();
        }

		private static void DisplayShapeSelectionPrompt(List<string> lst)
		{
			MiddleConsole.HighConsole.NewLine();
			MiddleConsole.HighConsole.Display("Select figure to edit: ");
			MiddleConsole.HighConsole.NewLine();
			for (int i = 0; i < lst.Count - 1; i++)
			{
                MiddleConsole.HighConsole.Display($"{i}. {lst[i]}");
                MiddleConsole.HighConsole.NewLine();
            }
            MiddleConsole.HighConsole.Display($"{lst.Count - 1}. {lst[^1]}");		// so that there`s no spare NewLine
        }

		private static void DisplayWhatToEditSelectionPrompt(bool hasDecorator)
		{
			MiddleConsole.HighConsole.Display("Select what to edit: ");
			MiddleConsole.HighConsole.NewLine();
			MiddleConsole.HighConsole.Display("0. Layer\n1. Figure attributes\n2. Remove shape");
			if (!hasDecorator) MiddleConsole.HighConsole.Display("\n3. Add streak");
		}

		private static void DisplayLayerOperationSelectionPrompt(string uniqueName)
		{
			MiddleConsole.HighConsole.Display($"Select what to do with [color:green]{uniqueName}[color]: ");
			MiddleConsole.HighConsole.NewLine();
			MiddleConsole.HighConsole.Display("0. Send backwards\n1. Bring forward\n2. Send to back\n3. Bring to front");
        }

		private static void DisplayAttributeSelectionPrompt(string uniqueName, List<string> names)
		{
            MiddleConsole.HighConsole.Display($"Availible attributes for [color:green]{uniqueName}[color]: ");
            MiddleConsole.HighConsole.NewLine();
			for (int i = 0; i < names.Count - 1; i++)
			{
				MiddleConsole.HighConsole.Display($"{i}. {names[i]}");
				MiddleConsole.HighConsole.NewLine();
			}
            if (names.Count > 0) MiddleConsole.HighConsole.Display($"{names.Count - 1}. {names[^1]}");				// here too
        }

		private static void DisplayNewAttributePrompt(string attrName)
		{
			MiddleConsole.HighConsole.DisplayQuestion($"Enter new value for {attrName}:");
        }

		private void OnCtrlCTransferred(object? sender, EventArgs e)
		{
            ctrlCPressed = true;

            tokenSource.Cancel(true);
            return;
        }

        private void OnStringTransferred(object? sender, TransferringEventArgs e)
        {
			interString = e.Input;
			tokenSource.Cancel(true);
		}

        private void EmptyTask()
        {
            while (true) ;
        }

        public void Unexecute()
		{
			if (receiver == null)
			{
				Logger.Instance.Warning("Edit command unexecute before execute. Maybe execute was interrupted by keyboard or invalid input");
			}
			else receiver.Unexecute();
		}
    }

    /// <summary>
    /// Команда для добавления круга на экран
    /// </summary>
    class CreateCircleCommand : ICommand
	{
		private Shape? receiver;        // Needed for Unexecution
		
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
			if (receiver == null) throw new InvalidOperationException();
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
			if (receiver == null) throw new InvalidOperationException();
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
			if (receiver == null) throw new InvalidOperationException();
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
            if (receiver == null) throw new InvalidOperationException();
            receiver.Destroy();
        }
    }

	/// <summary>
	/// Command to add polygon
	/// </summary>
    class CreatePolygonCommand : ICommand
    {
        private Shape? receiver;
        
		readonly List<Point> points;
        readonly int lineThickness;
        readonly EColor color;
        readonly bool streak;
        readonly EColor streakColor;
        readonly EStreakTexture streakTexture;

        public CreatePolygonCommand(Dictionary<string, object> arguments)
        {
            try
            {
                points = (List<Point>)arguments["points"];
                lineThickness = (int)arguments["lineThickness"];
                color = (EColor)arguments["color"];

                // unnecessary, but if streak is present, others should be too
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
            if (receiver == null) throw new InvalidOperationException();
            receiver.Destroy();
        }
    }

    /// <summary>
    /// Command to create window
    /// </summary>
    class CreateWindowCommand : ICommand
	{
		private readonly WindowManager receiver;

#pragma warning disable IDE0060
        public CreateWindowCommand(Dictionary<string, object> arguments)        // arguments in unused, but it`s a part of interface (not defined in interface)
		{
			receiver = WindowManager.Instance;
		}
#pragma warning restore IDE0060

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
			catch (ArgumentException)
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
			catch (ArgumentException)
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
			SelectionMarker _ = new(leftTopX, leftTopY, rightBottomX, rightBottomY);

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
			catch (InvalidOperationException)
			{
                Logger.Instance.Error("Noting to undo!");
                MiddleConsole.HighConsole.DisplayWarning("Nothung to undo!");
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
			catch (InvalidOperationException)
			{
				Logger.Instance.Warning("Nothing to redo");
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

		public SaveCommand(Dictionary<string, object> arguments) { }

		public void Execute()
		{
			try { SaveLoadFacade.Instance.Save(); }
			catch (InvalidOperationException)
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
				string dir = filename[..idx]; // use Path.... here
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
			MiddleConsole.HighConsole.CtrlCTransferred += OnCtrlCTransferred;

            tokenSource = new();
            task = new Task(EmptyTask, tokenSource.Token);
        }

		public void Execute()
		{
            if (SaveLoadFacade.Instance.NeedsSave)
            {
                MiddleConsole.HighConsole.TransferringInput = true;
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

		private void OnSomethingTransferred(object? sender, TransferringEventArgs e)
		{
			string trString = e.Input;

			answer = trString.ToLower() switch
			{
				"y" or "yes" => EYesNoSaveAnswer.Yes,
				"s" or "save" => EYesNoSaveAnswer.Save,
				_ => EYesNoSaveAnswer.No
			};

			tokenSource.Cancel(true);
		}

		private void OnCtrlCTransferred(object? sender, EventArgs e)
		{
			answer = EYesNoSaveAnswer.No;
			tokenSource.Cancel(true);
		}

		private void EmptyTask()
		{
			while (true) ;
		}
	}

	/// <summary>
	/// Displays list of colors (with examples)
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
			receiver.NewLine();

			string output = string.Join(' ', from color in ColorsController.ColorsList select $"[color:{color}]{color}[color]");

			receiver.Display(output);
			receiver.NewLine();
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

			string output = string.Join('\n', from kvp in textures select $"{kvp.Key} -- {kvp.Value}");

			receiver.Display(output);
		}

		public void Unexecute()
		{
			Logger.Instance.Warning("Unexecution of textures command");
		}
	}

	/// <summary>
	/// Export SVG
	/// </summary>
	class ExportSVGCommand: ICommand
	{
        private readonly STConverter? receiver;

		private readonly string inFilename = "";
		private readonly string outFilename;
		
		private readonly bool createdTempFile = false;

        public ExportSVGCommand(Dictionary<string, object> arguments)
        {
            try
            {
				inFilename = (string)arguments["inFilename"];
                outFilename = (string)arguments["outFilename"];

				if (inFilename.Length == 0 || inFilename == "_")
				{
					inFilename = SaveLoadFacade.CorrectFilename(DateTime.Now.ToString().Replace(':', '.'));
					CommandsFacade.ExecuteButDontRegister(new SaveAsCommand(new() { { "filename", inFilename } }));
					createdTempFile = true;
				}
				else inFilename = SaveLoadFacade.CorrectFilename(inFilename);

				try { receiver = new(inFilename); }
				catch (FileNotFoundException)
				{
					Logger.Instance.Error($"Cannot export SVG: File {inFilename} not found");
					MiddleConsole.HighConsole.DisplayError($"File {inFilename} not found");
				}
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of ExportSVGCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of ExportSVGCommand");
                throw;
            }
        }

        public void Execute()
        {
			receiver?.ToSVG(SaveLoadFacade.CorrectFilename(outFilename, ".svg"));

			if (createdTempFile)		// FIXME: Stream не успевает закрыться до этого момента
			{
                try { File.Delete(inFilename); }
                catch (Exception) { /* ignore */ }
			}
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of export SVG command");
        }
    }

    /// <summary>
    /// Export PDF
    /// </summary>
    class ExportPDFCommand : ICommand
    {
        private readonly STConverter receiver;

        private readonly string inFilename = "";
        private readonly string outFilename;

        public ExportPDFCommand(Dictionary<string, object> arguments)
        {
            try
            {
                inFilename = (string)arguments["inFilename"];
                outFilename = (string)arguments["outFilename"];

                if (inFilename.Length == 0 || inFilename == "_")
                {
                    inFilename = SaveLoadFacade.CorrectFilename(DateTime.Now.ToString().Replace(':', '.'));
                    CommandsFacade.ExecuteButDontRegister(new SaveAsCommand(new() { { "filename", inFilename } }));
                }
				else inFilename = SaveLoadFacade.CorrectFilename(inFilename);

                receiver = new(inFilename);
            }
            catch (KeyNotFoundException)
            {
                Logger.Instance.Error("Cannot find a parameter while creating an instance of ExportPDFCommand");
                throw;
            }
            catch (InvalidCastException)
            {
                Logger.Instance.Error("Cannot cast a parameter while creating an instance of ExportPDFCommand");
                throw;
            }
        }

        public async void Execute()
        {
			string correctedFilename = SaveLoadFacade.CorrectFilename(outFilename, ".pdf");

            try { await Task.Run(() => { receiver.ToPDF(correctedFilename); }); }
			catch (IOException)
			{
				Logger.Instance.Error($"Cannot export PDF: cannot open {correctedFilename} for writing");
				MiddleConsole.HighConsole.DisplayError($"Cannot open {correctedFilename} for writing");
			}
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of export PDF command");
        }
    }
}
