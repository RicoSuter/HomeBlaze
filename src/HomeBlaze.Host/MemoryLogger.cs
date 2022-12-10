using Microsoft.Extensions.Logging;

namespace HomeBlaze.Host
{
    public class MemoryLogger : ILogger
    {
        public static string CurrentOutput { get; private set; } = "";

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

            var newOutput =
                $"{logLevel.ToString().Substring(0, 4)} - {_name} | {DateTimeOffset.Now.ToString("O")}\n" +
                $"       {formatter(state, exception)}\n{(exception != null ? exception.ToString() + "\n" : string.Empty)}" + CurrentOutput;

            CurrentOutput = string.Join('\n', newOutput.Split('\n').Take(1000));
        }
    }
}
