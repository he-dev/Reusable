using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Abstractions
{
    public interface ILogger
    {
        /// <summary>
        /// Gets middleware root.
        /// </summary>
        LoggerMiddleware Middleware { get; }

        T Use<T>(T next) where T : LoggerMiddleware;

        void Log(LogEntry logEntry);
    }

    public interface ILogger<T> : ILogger { }
}