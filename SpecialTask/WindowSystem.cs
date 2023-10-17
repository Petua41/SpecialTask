using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SpecialTask
{
	/// <summary>
	/// Контролирует создание, удаление и переключение окон
	/// </summary>
	class WindowManager
	{
		private static WindowManager? singleton;

		private WindowToDraw currentWindow;
		private readonly List<WindowToDraw> existingWindows;

		private WindowManager()
		{
			currentWindow = new(0);
			existingWindows = new List<WindowToDraw> { currentWindow };
		}

		public static WindowManager Instance
		{
			get
			{
				singleton ??= new WindowManager();
				return singleton;
			}
		}

		/// <summary>
		/// Создаёт новое окно для рисования. Создавать окна можно ТОЛЬКО так.
		/// </summary>
		/// <returns>Номер созданного окна</returns>
		public int CreateWindow()
		{
			int numberOfNewWindow = existingWindows.Count;
			existingWindows.Add(new(numberOfNewWindow));
			return numberOfNewWindow;
		}

		public void DestroyWindow(int numberOfWindow)
        {
            existingWindows[numberOfWindow].Destroy();
            RemoveWindowFromLists(numberOfWindow);
		}

		public void SwitchToWindow(int numberOfWindow)
		{
			ValidateWindowNumber(numberOfWindow);					// here we pass exception on
			currentWindow = existingWindows[numberOfWindow];
            WindowSwitchedEvent?.Invoke(this, new WindowSwitchedEventArgs(numberOfWindow));
        }

		public List<Shape> ShapesOnCurrentWindow => currentWindow.ShapesOnThisWindow;

		public void DisplayOnCurrentWindow(Shape shape)
		{
			currentWindow.AddShape(shape);

			SaveLoadFacade slf = SaveLoadFacade.Instance;		// so that it will be initialized
            SomethingDisplayed?.Invoke(this, new());
        }

		public void RemoveFromCurrentWindow(Shape shape)
		{
			currentWindow.RemoveShape(shape);
        }

        public int SendBackward(string uniqueName)
        {
            return currentWindow.SendBackward(uniqueName);
        }

        public int BringForward(string uniqueName)
        {
            return currentWindow.BringForward(uniqueName);
        }

        public int SendToBack(string uniqueName)
        {
            return currentWindow.SendToBack(uniqueName);
        }

        public int BringToFront(string uniqueName)
        {
            return currentWindow.BringToFront(uniqueName);
        }

		public void MoveToLayer(string uniqueName, int newLayer)
		{
			currentWindow.MoveToLayer(uniqueName, newLayer);
		}

        public void CloseAll()
		{
			for (int i = existingWindows.Count - 1; i >= 0; i--) DestroyWindow(i);
        }

		public void OnSomeAssotiatedWindowClosed(WindowToDraw winToDraw)
		{
			int idx = existingWindows.IndexOf(winToDraw);
			if (idx >= 0) RemoveWindowFromLists(existingWindows.IndexOf(winToDraw));
		}

		private void RemoveWindowFromLists(int windowNumber)
		{
			try { ValidateWindowNumber(windowNumber); }
			catch (WindowDoesntExistException) { return; }			// if window doesn`t exist, don`t remove it

            for (int i = windowNumber + 1; i < existingWindows.Count; i++)
            {
                existingWindows[i].ChangeTitle(i - 1);
            }
            existingWindows.RemoveAt(windowNumber);
        }

        private void ValidateWindowNumber(int numberOfWindow)
        {
            if (numberOfWindow < 0 || numberOfWindow >= existingWindows.Count) throw new WindowDoesntExistException();
        }

        public event WindowSwitchedEventHandler? WindowSwitchedEvent;

		public event EventHandler? SomethingDisplayed;
	}

	class WindowToDraw
	{
		private readonly DrawingWindow assotiatedWindow;

        private readonly List<Shape> allShapesOnThisWindow = new();		// Stored in creation-time order
		private readonly List<int> zOrder = new();						// Indices in previous list

		public WindowToDraw(int number)
		{
			assotiatedWindow = new();
			assotiatedWindow.DrawingWindowClosedEvent += OnAssotiatedWindowClosed;
			assotiatedWindow.Show();
            ChangeTitle(number);
        }

		public void ChangeTitle(int newNumber)
		{
			assotiatedWindow.ChangeTitle($"Drawing window {newNumber}");
		}

		public List<Shape> ShapesOnThisWindow => allShapesOnThisWindow;

		public void Destroy()
		{
			assotiatedWindow.Close();
		}

		public void AddShape(Shape shape)
		{
			System.Windows.Shapes.Shape wpfShape = shape.WPFShape;
            int z = allShapesOnThisWindow.Count;

            allShapesOnThisWindow.Add(shape);
			Canvas.Children.Add(wpfShape);

            zOrder.Add(z);
			Panel.SetZIndex(wpfShape, z);
        }

		public void RemoveShape(Shape shape)
		{
			if (Canvas.Children.Contains(shape.WPFShape))     // If we cannot find shape on canvas, it won`t be removed from lists
            {
                Canvas.Children.Remove(shape.WPFShape);
                RemoveShapeFromLists(shape.UniqueName);
            }
		}

		public int SendBackward(string uniqueName)
		{
			if (allShapesOnThisWindow.Count <= 1) throw new CannotChangeShapeLayerException();

			for (int i = 0; i < allShapesOnThisWindow.Count; i++)
			{
				if (uniqueName == allShapesOnThisWindow[i].UniqueName)
				{
					if (i == 0) throw new CannotChangeShapeLayerException();

					(zOrder[i], zOrder[i - 1]) = (zOrder[i - 1], zOrder[i]);

					int firstCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[i].WPFShape);
					int secondCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[i - 1].WPFShape);

					Panel.SetZIndex(allShapesOnThisWindow[i].WPFShape, secondCanvasZ);
					Panel.SetZIndex(allShapesOnThisWindow[i - 1].WPFShape, firstCanvasZ);

					return i;
				}
			}
			throw new ShapeNotFoundException();
		}

		public int BringForward(string uniqueName)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new CannotChangeShapeLayerException();

            for (int i = 0; i < allShapesOnThisWindow.Count; i++)
            {
                if (uniqueName == allShapesOnThisWindow[i].UniqueName)
                {
                    if (i == allShapesOnThisWindow.Count - 1) throw new CannotChangeShapeLayerException();

                    (zOrder[i], zOrder[i + 1]) = (zOrder[i + 1], zOrder[i]);

                    int firstCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[i].WPFShape);
                    int secondCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[i + 1].WPFShape);

                    Panel.SetZIndex(allShapesOnThisWindow[i].WPFShape, secondCanvasZ);
                    Panel.SetZIndex(allShapesOnThisWindow[i + 1].WPFShape, firstCanvasZ);

                    return i;
                }
            }
			throw new ShapeNotFoundException();
        }

        public int SendToBack(string uniqueName)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new CannotChangeShapeLayerException();

            for (int i = 0; i < allShapesOnThisWindow.Count; i++)
            {
                if (uniqueName == allShapesOnThisWindow[i].UniqueName)
                {
					for (int j = i; j > 0; j--) SendBackward(allShapesOnThisWindow[j].UniqueName);
					return i;
                }
            }
			throw new ShapeNotFoundException();
        }

        public int BringToFront(string uniqueName)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new CannotChangeShapeLayerException();

            for (int i = 0; i < allShapesOnThisWindow.Count; i++)
            {
                if (uniqueName == allShapesOnThisWindow[i].UniqueName)
                {
					for (int j = i; j < allShapesOnThisWindow.Count - 1; j++) BringForward(allShapesOnThisWindow[j].UniqueName);
					return i;
                }
            }
			throw new ShapeNotFoundException();
        }

		public void MoveToLayer(string uniqueName, int newLayer)
        {
            if (allShapesOnThisWindow.Count <= 1) throw new CannotChangeShapeLayerException();

            if (newLayer < 0 || newLayer > allShapesOnThisWindow.Count) throw new ArgumentException();

            for (int i = 0; i < allShapesOnThisWindow.Count; i++)
            {
                if (uniqueName == allShapesOnThisWindow[i].UniqueName)
                {
                    (zOrder[i], zOrder[newLayer]) = (zOrder[newLayer], zOrder[i]);

                    int firstCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[i].WPFShape);
                    int secondCanvasZ = Panel.GetZIndex(allShapesOnThisWindow[newLayer].WPFShape);

                    Panel.SetZIndex(allShapesOnThisWindow[i].WPFShape, secondCanvasZ);
                    Panel.SetZIndex(allShapesOnThisWindow[newLayer].WPFShape, firstCanvasZ);

                    return;
                }
            }
            throw new ShapeNotFoundException();
        }

        private void RemoveShapeFromLists(string uniqueName)
        {
			int index = -1;
			for (int i = 0; i < allShapesOnThisWindow.Count; i++)
			{
				if (allShapesOnThisWindow[i].UniqueName == uniqueName)
				{
					index = i;
					break;
				}
			}
			if (index >= 0)				// if cannot find, no problem
			{
				allShapesOnThisWindow.RemoveAt(index);
				zOrder.Remove(index);
			}
        }

		private void OnAssotiatedWindowClosed(object? sender, EventArgs e)
		{
			WindowManager.Instance.OnSomeAssotiatedWindowClosed(this);
        }

        private Canvas Canvas => assotiatedWindow.DrawingCanvas;
    }
    public class WindowSwitchedEventArgs : EventArgs
    {
        public WindowSwitchedEventArgs(int newNumber)
        {
            NewNumber = newNumber;
        }

        public int NewNumber { get; set; }
    }

    public delegate void WindowSwitchedEventHandler(object sender, WindowSwitchedEventArgs args);
}
