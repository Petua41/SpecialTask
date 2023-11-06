using SpecialTask.Infrastructure.Loggers;

namespace SpecialTask.Infrastructure
{
    internal enum ConcreteLoggers { SimpleLogger }

    internal static class LoggerKeeper
    {
        private static ILogger? logger;

        /// <summary>
        /// Initializes concrete logger. <paramref name="concLogger"/> is logger name. Other parameters passed to logger`s constructor
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        public static void InitializeLogger(ConcreteLoggers concLogger, params object[] args)
        {
            logger = concLogger switch
            {
                ConcreteLoggers.SimpleLogger => SimpleLogger.Instance,  // maybe it shouldn`t be singleton, if we initialize it this way?
                _ => throw new ArgumentException($"{concLogger} is not implemented yet", nameof(concLogger))
            };

            logger.Greetings();     // I think, it should be here, so that we won`t forget to log greetings when we initialize logger
        }

        public static ILogger Logger
        {
            get
            {
                if (logger is null) throw new InvalidOperationException("You must call InitializeLogger before getting Logger");
                return logger;
            }
        }
    }
}
