using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace HomeBlaze
{
    public sealed class MemoryLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, MemoryLogger> _loggers =
            new ConcurrentDictionary<string, MemoryLogger>();

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new MemoryLogger(name));

        public void Dispose() => _loggers.Clear();
    }
}
