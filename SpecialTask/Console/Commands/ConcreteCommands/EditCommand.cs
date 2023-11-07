using SpecialTask.Console.Commands.ConcreteCommands.Internal;
using SpecialTask.Drawing.Shapes;
using SpecialTask.Drawing.Shapes.Decorators;
using SpecialTask.Infrastructure;
using SpecialTask.Infrastructure.Collections;
using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Events;
using SpecialTask.Infrastructure.Iterators;
using System.Collections.Specialized;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Wrapper (console-side) to edit shapes
    /// </summary>
    internal class EditCommand : ICommand
    {
        private ICommand? receiver = null;

        private readonly ESortingOrder sortingOrder;

        private IReadOnlyList<Shape> listOfShapes = new List<Shape>();
        private string interString = string.Empty;
        private int selectedNumber = -1;
        private bool hasStreak = false;

        private CancellationTokenSource tokenSource = new();
        private bool ctrlCPressed = false;
        private Shape? shapeToEdit;

        public EditCommand(object[] args)
        {
            sortingOrder = (bool)args[0] ? ESortingOrder.Coordinates : ESortingOrder.CreationTime;

            IteratorsFacade.SetConcreteIterator(sortingOrder);

            HighConsole.SomethingTranferred += OnStringTransferred;
            HighConsole.CtrlCTransferred += OnCtrlCTransferred;
        }

        // TODO: this method is TOO long
        public async void Execute()
        {
            HighConsole.TransferringInput = true;

            try
            {
                listOfShapes = IteratorsFacade.GetCompleteResult();

                if (listOfShapes.Count == 0)
                {
                    HighConsole.DisplayWarning("Nothing to edit");
                    return;
                }

                DisplayShapeSelectionPrompt(listOfShapes.Select(sh => sh.UniqueName).ToList());

                await GetSelectedNumber();      // maybe pass "limiting" predicate to this method?

                if (selectedNumber >= listOfShapes.Count)
                {
                    throw new InvalidInputException();
                }

                shapeToEdit = listOfShapes[selectedNumber];

                if (shapeToEdit is StreakDecorator)
                {
                    hasStreak = true;
                }

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
                        CommandsFacade.Execute(receiver);

                        break;
                    case 1:
                        // edit attributes:
                        Pairs<string, string> attrsWithNames = shapeToEdit.AttributesToEditWithNames;	// MyMap, because it`s ordered (OrderedDictionary works very wrong)

                        DisplayAttributeSelectionPrompt(shapeToEdit.UniqueName, attrsWithNames.Keys);

                        await GetSelectedNumber();

                        if (selectedNumber >= attrsWithNames.Count)
                        {
                            throw new InvalidInputException();
                        }

                        KeyValuePair<string, string> kvp = attrsWithNames[selectedNumber];

                        DisplayNewAttributePrompt(kvp.Value);

                        interString = string.Empty;

                        await GetInterString();

                        receiver = new EditShapeAttributesCommand(shape: shapeToEdit, attribute: kvp.Key, newValue: interString);
                        CommandsFacade.Execute(receiver);

                        break;
                    case 2:
                        // remove shape:
                        receiver = new RemoveShapeCommand(shapeToEdit);
                        CommandsFacade.Execute(receiver);
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
                        CommandsFacade.Execute(receiver);

                        break;
                    default:
                        throw new InvalidInputException();
                }
            }
            catch (InvalidInputException)
            {
                Logger.Error("Edit: invalid input");
                HighConsole.DisplayError("Invalid input");
            }
            catch (KeyboardInterruptException)
            {
                Logger.Error("Edit: keyboard interrupt");
                HighConsole.DisplayError("Kyboard interrupt");
            }
            finally
            {
                HighConsole.TransferringInput = false;
                HighConsole.NewLine();
                HighConsole.DisplayPrompt();
            }
        }

        private async Task GetSelectedNumber()
        {
            await GetInterString();

            if (!int.TryParse(interString, out selectedNumber)) throw new InvalidInputException();
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
                throw new KeyboardInterruptException();
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
