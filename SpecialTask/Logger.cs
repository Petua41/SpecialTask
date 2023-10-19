using System;
using System.IO;
using System.Windows;

namespace SpecialTask
{
	enum ELogLevels { Info, Warning, Error }

	class Logger: IDisposable
	{
		private static Logger? singleton;

		private readonly string logFilename;
		private readonly StreamWriter writer;
		private readonly ELogLevels logLevel;

		Logger()
		{
#if DEBUG
			logLevel = ELogLevels.Info;
#else
			logLevel = ELogLevels.Warning;
#endif
			logFilename = $"Logs/log_{DateTime.Now.ToString().Replace(' ', '_').Replace(':', '.')}";
			if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
			writer = new(File.Create(logFilename));
			LogGreetings();
		}

		public static Logger Instance
		{
			get
			{
				singleton ??= new();
				return singleton;
			}
        }

        public void Dispose()
        {
            writer.Close();
            singleton = null;
        }

        public void Info(string message)
		{
			Log(message, ELogLevels.Info);
		}

		public void Warning(string message)
		{
			Log(message, ELogLevels.Warning);
		}

		public void Error(string message)
		{
			Log(message, ELogLevels.Error);
		}

		public void Fatal(string message)
        {
            Log("FATAL: " + message, ELogLevels.Error);

            MessageBoxImage icon = MessageBoxImage.Error;
			MessageBoxButton button = MessageBoxButton.OK;
			MessageBox.Show(message, "Fatal error", button, icon);

			Application.Current.Shutdown();
		}

		private void Log(string message, ELogLevels level) 
		{
			if (level < logLevel) { return; }

			writer.WriteLine($"{level}[{DateTime.Now}]: {message}");
        }

		private void LogGreetings()
		{
			bool debug = false;
#if DEBUG
			debug = true;
#endif
			string platform = Environment.OSVersion.ToString();
			string dotnetVersion = Environment.Version.ToString();
			string userName = Environment.UserName;
			string workingDir = Environment.CurrentDirectory;

			string greetingsText = 
				"------------ LOG ------------\n" + 
				$"[{DateTime.Now}]: Program started\n" + 
				$"DEBUG: {debug}\n" + 
				$"Platform: {platform}\n" + 
				$".NET version: {dotnetVersion}\n" + 
				$"Username: {userName}\n" + 
				$"Working directory: {workingDir}\n" +
                "-----------------------------\n\n";
			writer.Write(greetingsText);
        }
	}
}
