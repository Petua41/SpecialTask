using System;
using System.Collections.Generic;

namespace SpecialTask.Commands
{
    enum ELayerDirection { None, Forward, Backward, Front, Back }

    /// <summary>
    /// Controlls execution, undo and redo of all Commands
    /// </summary>
    static class CommandsFacade         // Facade and Mediator at the same time.
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
}
