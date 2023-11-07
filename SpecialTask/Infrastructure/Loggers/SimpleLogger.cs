﻿using System.IO;
using System.Windows;

namespace SpecialTask.Infrastructure.Loggers
{
    internal class SimpleLogger : ILogger
    {
        private static SimpleLogger? singleton;

        private readonly string logFilename;
        private readonly StreamWriter writer;
        private readonly LogLevels logLevel;

        internal SimpleLogger()
        {
#if DEBUG
            logLevel = LogLevels.Info;
#else
			logLevel = LogLevels.Warning;
#endif
            logFilename = Path.Combine(PathsController.LogsDirectory, PathsController.DateTimeFilename);
            logFilename = Path.ChangeExtension(logFilename, ".log");

            writer = new(File.Create(logFilename));
        }

        public static SimpleLogger Instance
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
            Log(message, LogLevels.Info);
        }

        public void Warning(string message)
        {
            Log(message, LogLevels.Warning);
        }

        public void Error(string message)
        {
            Log(message, LogLevels.Error);
        }

        public void Fatal(string message)
        {
            Log("FATAL: " + message, LogLevels.Error);

            MessageBoxImage icon = MessageBoxImage.Error;
            MessageBoxButton button = MessageBoxButton.OK;
            _ = MessageBox.Show(message, "Fatal error", button, icon);

            Application.Current.Shutdown();
        }

        public void Greetings()
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
                "------------ LOG ------------" + Environment.NewLine +
                $"[{DateTime.Now}]: Program started" + Environment.NewLine +
                $"DEBUG: {debug}" + Environment.NewLine +
                $"Platform: {platform}" + Environment.NewLine +
                $".NET version: {dotnetVersion}" + Environment.NewLine +
                $"Username: {userName}" + Environment.NewLine +
                $"Working directory: {workingDir}" + Environment.NewLine +
                "-----------------------------" + Environment.NewLine + Environment.NewLine;
            writer.Write(greetingsText);
        }

        private void Log(string message, LogLevels level)
        {
            if (level < logLevel) { return; }

            writer.WriteLine($"{level}[{DateTime.Now}]: {message}");
        }
    }
}
