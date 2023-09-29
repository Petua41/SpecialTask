using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialTask
{
	/// <summary>
	/// Контролирует создание, удаление и переключение окон
	/// </summary>
	class WindowManager
	{
		private static WindowManager? singleton;

		private WindowToDraw currentWindow;
		private List<WindowToDraw> existingWindows;

		private WindowManager()
		{
			if (singleton != null) throw new SingletonError();
			else
			{
				currentWindow = new(0);
				existingWindows = new List<WindowToDraw> { currentWindow };
			}
		}

		public static WindowManager Instance
		{
			get
			{
				singleton ??= new WindowManager();
				return singleton;
			}
		}

		public void CreateWindow()
		{
			existingWindows.Add(new WindowToDraw(existingWindows.Count));
		}

		public void DestroyWindow(int numberOfWindow)						// Это надо потестить
		{
			ValidateWindowNumber(numberOfWindow);
			for (int i = numberOfWindow + 1; i < existingWindows.Count; i++)
			{
				existingWindows[i].ChangeTitle(i - 1);
			}
			existingWindows[numberOfWindow].Destroy();
			existingWindows.RemoveAt(numberOfWindow);
		}

		public void SwitchToWindow(int numberOfWindow)
		{
			ValidateWindowNumber(numberOfWindow);
			// TODO
		}

		public List<Shape> ShapesOnCurrentWindow()
		{
			return currentWindow.ShapesOnThisWindow();
		}

		private void ValidateWindowNumber(int numberOfWindow)
		{
			if (numberOfWindow < 0 || numberOfWindow >= existingWindows.Count) throw new WindowDoesntExistException();
		}
	}

	class WindowToDraw
	{
		private System.Windows.Controls.Canvas canvas;
		private System.Windows.Window window;
		private List<Shape> allShapesOnThisWindow = new();					// Хранятся в порядке создания
		private List<int> zOrder = new List<int>();							// Индексы в предыдущем списке в Z-порядке

		public WindowToDraw(int number)
		{
			// TODO: здесь надо создавать окно WPF, Canvas WPF (скорее всего из темплейта), и присваивать это всё куда надо
			ChangeTitle(number);
		}
		
		public System.Windows.Controls.Canvas Canvas
		{ get { return canvas; } }

		public void ChangeTitle(int newNumber)
		{
			// TODO: это совсем потом
		}

		public List<Shape> ShapesOnThisWindow()
		{
			return allShapesOnThisWindow;
		}

		public void RemoveShapeFromLists(Shape shapeToDelete)
		{
			// TODO
		}

		public void Destroy()
		{
			// TODO
		}
	}
}
