using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SpecialTask
{
	/// <summary>
	/// Класс-обёртка для консоли. Выступает посредником между бизнес-классом STConsole и "низкоуровневой" частью приложения. 
	/// НЕ ПРЕДНАЗНАЧЕН для использования отдельно от STConsole
	/// </summary>
	public class WPFConsole
	{
		private static WPFConsole? singleton;
		private readonly MainWindow mainWindowInstance;

		private WPFConsole()
		{
			if (singleton != null) throw new SingletonError();
			try { mainWindowInstance = (MainWindow)Application.Current.MainWindow; }
			catch (Exception ex)
			{
				Logger.Instance.Error(string.Format("{0} exception while trying to get MainWindow instance!", ex.GetType().ToString()));
				throw;
			}
		}

		public static WPFConsole Instance 
		{ 
			get
			{
				singleton ??= new WPFConsole();
				return singleton;
			} 
		}

		public void Display(string message, EColor color=EColor.None)
		{
			mainWindowInstance.Display(message, ColorsController.GetWPFColor(color));
		}

		public void NewLine()
		{
			Display(Environment.NewLine);
		}

		/// <summary>
		/// Выводит приглашение ко вводу и разрешает ввод. ДОЛЖЕН вызываться каждый раз, когда пользователь может что-то ввести
		/// </summary>
		public void DisplayPrompt()
		{
			Display(">> ", EColor.Green);
			UnlockInput();
		}

		public void ProcessInputString(string input)
		{
			STConsole.Instance.ProcessInput(input);
			DisplayPrompt();
		}

		public string Autocomplete(string currentInput)
		{
			return STConsole.Instance.Autocomplete(currentInput);
		}

		public string ProcessUpArrow()
		{
			// TODO: я ещё не решил, как будут храниться предыдущие команды
			throw new NotImplementedException();
		}

        public string ProcessDownArrow()
        {
            // TODO: я ещё не решил, как будут храниться предыдущие команды
            throw new NotImplementedException();
        }

		public void ChangeUndoStackDepth(int depth)
		{
			CommandsFacade.ChangeUndoStackDepth(depth);
		}

        /// <summary>
        /// Блокирует ввод в косоль. ДОЛЖЕН вызываться каждый раз перед выводом текста, состоящего из нескольких частей
        /// </summary>
        public void LockInput()
		{
			// TODO: поскольку пока что не всё ясно с "низкоуровневой" консолью, неизвестно что тут будет. Но точно не больше одной строчки
		}

		/// <summary>
		/// Разрешает ввод в консоль. Не должен вызываться извне
		/// </summary>
		private void UnlockInput()
		{
            // TODO: поскольку пока что не всё ясно с "низкоуровневой" консолью, неизвестно что тут будет. Но точно не больше одной строчки
        }
    }
}
