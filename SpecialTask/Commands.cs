using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SpecialTask
{
	/// <summary>
	/// Контролирует вызов всех команд, их отмену и повтор
	/// </summary>
	static class CommandsFacade
	{
		private static Stack<ICommand> stack = new();
		private static Stack<ICommand> undoneStack = new();
		
		public static void RegisterAndExecute(ICommand command)
		{
			stack.Push(command);
			command.Execute();
		}

		public static void ExecuteButDontRegister(ICommand command)
		{
			command.Execute();
		}

		public static void UndoCommands(int numberOfCommands=1)
		{
			for (int i = 0; i < numberOfCommands; i++) Undo();
		}

		public static void RedoCommands(int numberOfCommands=1)
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

		private static void Undo()
		{
			ICommand command = stack.Pop();
			command.Unexecute();
			undoneStack.Push(command);
		}

		private static void Redo()
		{
			ICommand command = undoneStack.Pop();
			RegisterAndExecute(command);
		}

		// TODO: чего-то здесь не хватает
	}

	interface ICommand
	{
		void Execute();
		void Unexecute();
	}

	/// <summary>
	/// Представляет команду для редактирования любых атрибутов фигур
	/// </summary>
	class EditShapeAttributesCommand: ICommand
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
	class CreateCircleCommand: ICommand
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
			:this(centerX, centerY, color, radius, lineThickness)
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
	class CreateSquareCommand: ICommand
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

    class HelpCommand: ICommand
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
}
