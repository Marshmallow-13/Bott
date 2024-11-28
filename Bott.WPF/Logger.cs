using Bott.Core;

namespace Bott.WPF;

    internal class Logger(Action<string> log, Action<string> error) : ILogger
    {
        public void Log(string message)
        {
            log?.Invoke(message);
        }

        public void Error(string message)
        {
            error?.Invoke(message);
        }
    }
