namespace SpecialTask.Infrastructure.Loggers
{
    internal enum LogLevels { Info, Warning, Error }

    public interface ILogger : IDisposable
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Fatal(string message);
        void Greetings();
    }
}
