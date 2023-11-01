using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
    /// Wrapper (console-side) to edit shapes
    /// </summary>
    class EditCommand : ICommand
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

        public EditCommand(object[] args)
        {
            sortingOrder = (bool)args[0] ? ESortingOrder.Coordinates : ESortingOrder.CreationTime;

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
                        CommandsFacade.Execute(receiver);

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
            catch (TaskCanceledException) { /* continue */  }

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
            MiddleConsole.HighConsole.Display($"0. Layer{Environment.NewLine}1. Figure attributes{Environment.NewLine}2. Remove shape");
            if (!hasDecorator) MiddleConsole.HighConsole.Display($"{Environment.NewLine}3. Add streak");
        }

        private static void DisplayLayerOperationSelectionPrompt(string uniqueName)
        {
            MiddleConsole.HighConsole.Display($"Select what to do with [color:green]{uniqueName}[color]: ");
            MiddleConsole.HighConsole.NewLine();
            MiddleConsole.HighConsole.Display(
                $"0. Send backwards{Environment.NewLine}1. Bring forward{Environment.NewLine}2. Send to back{Environment.NewLine}3. Bring to front");
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
}
