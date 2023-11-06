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

        public static string CorrectFilename(string filename, string defaultFolder, string neededExtension)
        {
            if (!Path.IsPathFullyQualified(filename)) filename = Path.Combine(defaultFolder, filename);
            if (!Path.HasExtension(filename)) filename += neededExtension;
            return filename;
        }

        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static string LogsDirectory
        {
            get
            {
                if (logsDir is null) throw new InvalidOperationException("You should call InitPaths before you can get LogsDirectory");
                return logsDir.FullName;
            }
            private set
            {
                if (!Directory.Exists(value)) Directory.CreateDirectory(value);
                logsDir = new DirectoryInfo(value);
            }
        }

        public static string DateTimeFilename
        {
            get
            {
                return DateTime.Now.ToString("dd.MM.yyy_HH.mm.ss");
            }
        }

        public static string DefaultSaveDirectory => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
