using System.IO;

namespace SpecialTask.Infrastructure
{
    internal static class PathsController
    {
        private const string logsDirName = "Logs";

        private static DirectoryInfo? logsDir;

        public static void InitPaths()
        {
            LogsDirectory = Path.GetFullPath(logsDirName);
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
                    _ = Directory.CreateDirectory(value);
                }

                logsDir = new DirectoryInfo(value);
            }
        }

        public static string DateTimeFilename => DateTime.Now.ToString("dd.MM.yyy_HH.mm.ss");

        public static string DefaultSaveDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
