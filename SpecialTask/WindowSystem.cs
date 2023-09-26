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
				currentWindow = new();
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
			// TODO
		}

		public void DestroyWindow()
		{
			// TODO
		}

		public void SwitchToWindow(int numberOfWindow)
		{
			// TODO
		}
	}

	class WindowToDraw
	{
		// TODO
	}
}
