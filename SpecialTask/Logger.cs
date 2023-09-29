using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SpecialTask
{
	enum ELogLevels { Info, Warning, Error }

	static class Logger
	{
		private static string logFilename;
		private static StreamWriter writer;
		private static ELogLevels logLevel;

		static Logger()
		{
#if DEBUG
			logLevel = ELogLevels.Info;
#else
			logLevel = ELogLevels.Warning;
#endif
			logFilename = string.Format("Logs/log_{0}", DateTime.Now.ToString().Replace(' ', '_'));
			if (!Directory.Exists("Logs")) Directory.CreateDirectory("Logs");
			writer = new(logFilename);
			LogGreetings();
		}

		public static void Info(string message)
		{
			Log(message, ELogLevels.Info);
		}

		public static void Warning(string message)
		{
			Log(message, ELogLevels.Warning);
		}

		public static void Error(string message)
		{
			Log(message, ELogLevels.Error);
		}

		private static void Log(string message, ELogLevels level) 
		{
			if (level < logLevel) { return; }

			string logLevelString = logLevel switch
			{
				ELogLevels.Info => "INFO",
				ELogLevels.Warning => "WARNING",
				ELogLevels.Error => "ERROR",
				_ => "UNKNOWN",
			};

			writer.WriteLine(string.Format("{0}[{1}]: {2}", logLevelString, DateTime.Now.ToString(), message));
        }

		private static void LogGreetings()
		{
			bool debug = false;
#if DEBUG
			debug = true;
#endif
			string platform = Environment.OSVersion.ToString();
			string dotnetVersion = Environment.Version.ToString();
			string userName = Environment.UserName;
			string workingDir = Environment.CurrentDirectory;

			string greetingsText = string.Format("------------ LOG ------------" + 
				"[{0}]: Program started" + 
				"DEBUG: {1}" + 
				"Platform: {2}" + 
				".NET version: {3}" + 
				"Username: {4}" + 
				"Working directory: {5}" +
                "-----------------------------", DateTime.Now.ToString(), debug.ToString(), platform, dotnetVersion, userName, workingDir);
			writer.WriteLine(greetingsText);
        }
	}
}
