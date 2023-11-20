using SpecialTask.Console.Commands.ConcreteCommands.Internal;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Events;
using SpecialTask.Infrastructure.Exceptions;
using SpecialTask.Infrastructure.Iterators;
using static SpecialTask.Infrastructure.Extensoins.KeyValuePairListExtensions;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Wrapper (console-side) to edit shapes
    /// </summary>
    internal class EditCommand : ICommand
    {
        private enum WhatToDo { EditLayer, EditAttributes, RemoveShape, AddStreak }

        private ICommand? receiver = null;

        private readonly SortingOrder sortingOrder;

        private string interString = string.Empty;
        private int selectedNumber = -1;

        private CancellationTokenSource tokenSource = new();
        private bool ctrlCPressed = false;

        public EditCommand(object[] args)
        {
            sortingOrder = (bool)args[0] ? SortingOrder.Coordinates : SortingOrder.CreationTime;

            IteratorsFacade.SetConcreteIterator(sortingOrder);

            HighConsole.SomethingTranferred += OnStringTransferred;
            HighConsole.CtrlCTransferred += OnCtrlCTransferred;
        }

        public async void Execute()
        {
            if (receiver is not null)
            {   // if it`s redo, repeat previous actions
                receiver.Execute();
                return;
            }

            HighConsole.TransferringInput = true;

            try
            {
                IReadOnlyList<Shape> listOfShapes = IteratorsFacade.GetCompleteResult();

                Shape shapeToEdit = await SelectShape(listOfShapes);

                WhatToDo action = await SelectAction(shapeToEdit is StreakDecorator);

                switch (action)
                {
                    case WhatToDo.EditLayer:
                        await EditLayer(shapeToEdit);
                        break;
                    case WhatToDo.EditAttributes:
                        await EditAttributes(shapeToEdit);
                        break;
                    case WhatToDo.RemoveShape:
                        RemoveShape(shapeToEdit);
                        break;
                    case WhatToDo.AddStreak:
                        await AddStreak(shapeToEdit);
                        break;
                }
            }
            catch (Exception e) when (e is InvalidInputException or KeyboardInterruptException or InvalidOperationException)
            {       // we catch different exceptions, but handle `em same way
                Logger.Error(e.Message);
                HighConsole.DisplayError(e.Message);
            }
            finally
            {
                HighConsole.TransferringInput = false;
                HighConsole.NewLine();
                HighConsole.DisplayPrompt();
            }
        }

        private async Task<Shape> SelectShape(IReadOnlyList<Shape> listOfShapes)
        {
            if (listOfShapes.Count == 0)
            {
                throw new InvalidOperationException("Nothing to edit");
            }

            DisplayShapeSelectionPrompt(listOfShapes.Select(sh => sh.UniqueName).ToList());

            await GetSelectedNumber(listOfShapes.Count - 1);

            return listOfShapes[selectedNumber];
        }

        private async Task<WhatToDo> SelectAction(bool hasStreak)
        {
            DisplayWhatToEditSelectionPrompt(hasStreak);

            await GetSelectedNumber(3);

            return (WhatToDo)selectedNumber;    // selected number is between 0 and 3, so we can do this safely
        }

        private async Task EditLayer(Shape shapeToEdit)
        {
            DisplayLayerOperationSelectionPrompt(shapeToEdit.UniqueName);

            await GetSelectedNumber(3);

            LayerDirection dir = selectedNumber switch
            {
                0 => LayerDirection.Backward,
                1 => LayerDirection.Forward,
                2 => LayerDirection.Back,
                3 => LayerDirection.Front,
                _ => throw new InvalidInputException($"{selectedNumber} is not valid value here")
            };

            receiver = new EditLayerCommand(shapeToEdit.UniqueName, dir);
            CommandsFacade.Execute(receiver);
        }

        private async Task EditAttributes(Shape shapeToEdit)
        {
            List<KeyValuePair<string, string>> attrsWithNames = shapeToEdit.AttributesToEditWithNames;

            DisplayAttributeSelectionPrompt(shapeToEdit.UniqueName, attrsWithNames.Keys());

            await GetSelectedNumber(attrsWithNames.Count - 1);

            KeyValuePair<string, string> kvp = attrsWithNames[selectedNumber];

            DisplayNewAttributePrompt(kvp.Value);

            interString = string.Empty;

            await GetInterString();

            receiver = new EditShapeAttributesCommand(shape: shapeToEdit, attribute: kvp.Key, newValue: interString);
            CommandsFacade.Execute(receiver);
        }

        private void RemoveShape(Shape shapeToEdit)
        {
            receiver = new RemoveShapeCommand(shapeToEdit);
            CommandsFacade.Execute(receiver);
        }

        private async Task AddStreak(Shape shapeToEdit)
        {
            DisplayNewAttributePrompt("Streak color");
            await GetInterString();

            InternalColor color = interString.ParseColor();

            DisplayNewAttributePrompt("Streak texture");
            await GetInterString();

            StreakTexture texture = interString.ParseStreakTexture();

            receiver = new AddStreakCommand(shapeToEdit, color, texture);
            CommandsFacade.Execute(receiver);
        }

        private async Task GetSelectedNumber(int maxValue)
        {
            await GetInterString();

            if (!int.TryParse(interString, out selectedNumber))
            {
                throw new InvalidInputException($"{interString} is not integer", interString);
            }

            if (selectedNumber < 0 || selectedNumber > maxValue)
            {
                throw new InvalidInputException($"{selectedNumber} is not valid here");
            }
        }

        private async Task GetInterString()
        {
            tokenSource = new();
            Task task = new(EmptyTask, tokenSource.Token);

            try { await task; }
            catch (TaskCanceledException) { /* continue */  }

            HighConsole.NewLine();
            if (ctrlCPressed)
            {
                throw new KeyboardInterruptException("Keyboard interrupt");
            }
        }

        private static void DisplayShapeSelectionPrompt(IReadOnlyList<string> lst)
        {
            HighConsole.NewLine();
            HighConsole.Display("Select figure to edit: ");
            HighConsole.NewLine();
            for (int i = 0; i < lst.Count - 1; i++)
            {
                HighConsole.Display($"{i}. {lst[i]}");      // I could use string.Join, but we need numbers
                HighConsole.NewLine();
            }
            HighConsole.Display($"{lst.Count - 1}. {lst[^1]}");		// so that there`s no spare NewLine
        }

        private static void DisplayWhatToEditSelectionPrompt(bool hasDecorator)
        {
            HighConsole.Display("Select what to edit: ");
            HighConsole.NewLine();
            HighConsole.Display($"0. Layer{Environment.NewLine}1. Figure attributes{Environment.NewLine}2. Remove shape");
            if (!hasDecorator)
            {
                HighConsole.Display($"{Environment.NewLine}3. Add streak");
            }
        }

        private static void DisplayLayerOperationSelectionPrompt(string uniqueName)
        {
            HighConsole.Display($"Select what to do with [color:green]{uniqueName}[color]: ");
            HighConsole.NewLine();
            HighConsole.Display(
                $"0. Send backwards{Environment.NewLine}1. Bring forward{Environment.NewLine}2. Send to back{Environment.NewLine}3. Bring to front");
        }

        private static void DisplayAttributeSelectionPrompt(string uniqueName, IReadOnlyList<string> names)
        {
            HighConsole.Display($"Availible attributes for [color:green]{uniqueName}[color]: ");
            HighConsole.NewLine();

            for (int i = 0; i < names.Count - 1; i++)
            {
                HighConsole.Display($"{i}. {names[i]}");
                HighConsole.NewLine();
            }
            if (names.Count > 0)
            {
                HighConsole.Display($"{names.Count - 1}. {names[^1]}");               // here too
            }
        }

        private static void DisplayNewAttributePrompt(string attrName)
        {
            HighConsole.DisplayQuestion($"Enter new value for {attrName}:");
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
            while (true)
            {
                ;
            }
        }

        public void Unexecute()
        {
            if (receiver is null)
            {
                Logger.Warning("Edit command unexecute before execute. Maybe execute was interrupted by keyboard or invalid input");
            }
            else
            {
                receiver.Unexecute();
            }
        }
    }
}
