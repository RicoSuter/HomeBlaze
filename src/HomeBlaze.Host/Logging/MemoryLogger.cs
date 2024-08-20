using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;

namespace HomeBlaze.Host.Logging
{
    public class MemoryLogger : ILogger
    {
        public static Collection<LogEntry> LogEntries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

        private readonly string _name;

        public MemoryLogger(string name) => _name = name;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            LogEntries.Insert(0, new LogEntry
            {
                Level = logLevel,
                Time = DateTimeOffset.Now,
                Message = formatter(state, exception),
                Exception = exception != null ? "\n" + exception.ToString() : null
            });

            if (LogEntries.Count > 1000)
            {
                LogEntries.RemoveAt(1000);
            }
        }

        public record LogEntry
        {
            public LogLevel Level { get; init; }

            public DateTimeOffset Time { get; init; }

            public required string Message { get; init; }

            public required string? Exception { get; init; }
        }
    }
}
