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

		public WindowToDraw? GetWindowByNumber(int number)
		{
			if (0 <= number && number < existingWindows.Count) return existingWindows[number];
			else
			{
				Logger.Instance.Error(string.Format("Window number {0} doesn`t exist!", number));
				return null;
			}
		}

		public void DisplayOnCurrentWindow(Shape shape)
		{
			// TODO
		}

		private void ValidateWindowNumber(int numberOfWindow)
		{
			if (numberOfWindow < 0 || numberOfWindow >= existingWindows.Count) throw new WindowDoesntExistException();
		}
	}

	class WindowToDraw
	{
		private System.Windows.Controls.Canvas canvas;
		private List<Shape> allShapesOnThisWindow = new();					// Хранятся в порядке создания
		private List<int> zOrder = new List<int>();                         // Индексы в предыдущем списке в Z-порядке
		private DrawingWindow assotiatedWindow;

		public WindowToDraw(int number)
		{
			// TODO: здесь надо создавать окно WPF, Canvas WPF (скорее всего из темплейта), и присваивать это всё куда надо
			assotiatedWindow = new DrawingWindow();
			assotiatedWindow.Show();
			canvas = assotiatedWindow.DrawingCanvas;
            ChangeTitle(number);
        }
		
		public System.Windows.Controls.Canvas Canvas
		{ get { return canvas; } }

		public void ChangeTitle(int newNumber)
		{
			assotiatedWindow.ChangeTitle(string.Format("Drawing window {0}", newNumber));
		}

		public List<Shape> ShapesOnThisWindow()
		{
			return allShapesOnThisWindow;
		}

		public void RemoveShapeFromLists(Shape shapeToDelete)
		{
			// TODO: если мы считаем, что фигура в состоянии сама себя удалить с экрана (а это вряд ли), то здесь надо просто находить её (по уникальному имени или по ссылке), удалять из списка и следить за индексами
		}

		public void Destroy()
		{
			assotiatedWindow.Close();
		}
	}
}
