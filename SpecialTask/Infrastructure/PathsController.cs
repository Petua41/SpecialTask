using SpecialTask.Infrastructure.Exceptions;
using System.IO;

namespace SpecialTask.Infrastructure
{
    internal static class PathsController
    {
        private const string logsDirName = "Logs";

        private static DirectoryInfo? logsDir;
        private static DirectoryInfo? defSaveDir;

        public static void InitPaths()
        {
            LogsDirectory = Path.GetFullPath(logsDirName);
            DefaultSaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }

        public static string CorrectFilename(string filename, string defaultFolder, string desiredExtension)
        {
            if (!Path.IsPathFullyQualified(filename))
            {
                filename = Path.Combine(defaultFolder, filename);
            }

            if (!Path.HasExtension(filename))
            {
                filename = Path.ChangeExtension(filename, desiredExtension);
            }

            return filename;
        }

        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string LogsDirectory
        {
            get => logsDir is null
                    ? throw new InvalidOperationException("You should call InitPaths before you can get LogsDirectory")
                    : logsDir.FullName;
            private set
            {
                if (!Directory.Exists(value))
                {
                    logsDir = Directory.CreateDirectory(value);
                }
                else logsDir = new DirectoryInfo(value);
            }
        }

        public static string DateTimeFilename => DateTime.Now.ToString("dd.MM.yyy_HH.mm.ss");

        public static string DefaultSaveDirectory
        {
            get => defSaveDir is null
                    ? throw new InvalidOperationException("You should call InitPaths before you can get DefaultSaveDirectory")
                    : defSaveDir.FullName;
            set
            {
                if (Directory.Exists(value))
                {
                    defSaveDir = new DirectoryInfo(value);
                }
                else
                {
                    // Ask user for help
                    // It will happen almost never, so we can do like this
                    throw new FatalError($"Default save directory doesn`t exist! ({value})\n" +
                        $"Please re-run application and specify default save directory (SpecialTask --default_save_dir=/some/path)");
                }
            }
        }
    }
}
