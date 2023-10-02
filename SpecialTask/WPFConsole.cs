﻿using System;
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
		private readonly RichTextBox consoleRTB;
		private readonly STConsole stConsole;

		private WPFConsole()
		{
			if (singleton != null) throw new SingletonError();

			try { consoleRTB = (RichTextBox)Application.Current.FindResource("ConsoleRTB"); }
			catch (ResourceReferenceKeyNotFoundException ex)
			{
				Logger.Instance.Error(string.Format("{0} exception while trying to get ConsoleRichTextBox!", ex.GetType().ToString()));
				throw;
			}

			stConsole = STConsole.Instance;
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
			consoleRTB.Foreground = new System.Windows.Media.SolidColorBrush(ColorsController.GetWPFColor(color));
			consoleRTB.AppendText(message);
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

		public int HeightInCharacterCells
		{
			get
			{
				// TODO
				throw new NotImplementedException();
			}
		}

		public int WidthInCharacterCells
		{
			get
			{
				// TODO
				throw new NotImplementedException();
			}
		}

		public string Autocomplete(string currentInput)
		{
			return stConsole.Autocomplete(currentInput);
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
