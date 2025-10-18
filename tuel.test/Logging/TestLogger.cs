using System;
using System.Collections.Generic;
using System.Text;

namespace TUEL.TestFramework.Logging
{
    /// <summary>
    /// Log levels for structured logging
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }

    /// <summary>
    /// Structured logging interface for test framework
    /// </summary>
    public interface ITestLogger
    {
        void Log(LogLevel level, string message, params object[] args);
        void LogTrace(string message, params object[] args);
        void LogDebug(string message, params object[] args);
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(string message, params object[] args);
        void LogCritical(string message, params object[] args);
        void LogException(Exception exception, string message = null, params object[] args);
    }

    /// <summary>
    /// Console-based test logger implementation
    /// </summary>
    public class ConsoleTestLogger : ITestLogger
    {
        private readonly LogLevel _minimumLevel;
        private readonly bool _maskSensitiveData;

        public ConsoleTestLogger(LogLevel minimumLevel = LogLevel.Information, bool maskSensitiveData = true)
        {
            _minimumLevel = minimumLevel;
            _maskSensitiveData = maskSensitiveData;
        }

        public void Log(LogLevel level, string message, params object[] args)
        {
            if (level < _minimumLevel) return;

            var formattedMessage = FormatMessage(level, message, args);
            var color = GetConsoleColor(level);

            Console.ForegroundColor = color;
            Console.WriteLine(formattedMessage);
            Console.ResetColor();
        }

        public void LogTrace(string message, params object[] args) => Log(LogLevel.Trace, message, args);
        public void LogDebug(string message, params object[] args) => Log(LogLevel.Debug, message, args);
        public void LogInformation(string message, params object[] args) => Log(LogLevel.Information, message, args);
        public void LogWarning(string message, params object[] args) => Log(LogLevel.Warning, message, args);
        public void LogError(string message, params object[] args) => Log(LogLevel.Error, message, args);
        public void LogCritical(string message, params object[] args) => Log(LogLevel.Critical, message, args);

        public void LogException(Exception exception, string message = null, params object[] args)
        {
            var fullMessage = string.IsNullOrEmpty(message)
                ? $"Exception occurred: {exception.Message}"
                : string.Format(message, args) + $"\nException: {exception.Message}";

            LogError(fullMessage);
            LogDebug($"Stack Trace: {exception.StackTrace}");
        }

        private string FormatMessage(LogLevel level, string message, params object[] args)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var levelStr = level.ToString().ToUpper().PadRight(9);
            var formattedMessage = args?.Length > 0 ? string.Format(message, args) : message;

            if (_maskSensitiveData)
            {
                formattedMessage = MaskSensitiveData(formattedMessage);
            }

            return $"[{timestamp}] {levelStr} {formattedMessage}";
        }

        private string MaskSensitiveData(string message)
        {
            if (string.IsNullOrEmpty(message)) return message;

            var sensitivePatterns = new Dictionary<string, string>
            {
                { @"password\s*=\s*[^\s,]+", "password=***" },
                { @"token\s*=\s*[^\s,]+", "token=***" },
                { @"secret\s*=\s*[^\s,]+", "secret=***" },
                { @"key\s*=\s*[^\s,]+", "key=***" },
                { @"Bearer\s+[A-Za-z0-9\-\._~\+\/]+=*", "Bearer ***" },
                { @"client_secret\s*=\s*[^\s,]+", "client_secret=***" }
            };

            var result = message;
            foreach (var pattern in sensitivePatterns)
            {
                result = System.Text.RegularExpressions.Regex.Replace(result, pattern.Key, pattern.Value,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            return result;
        }

        private ConsoleColor GetConsoleColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => ConsoleColor.Gray,
                LogLevel.Debug => ConsoleColor.Cyan,
                LogLevel.Information => ConsoleColor.White,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.Magenta,
                _ => ConsoleColor.White
            };
        }
    }

    /// <summary>
    /// Static logger instance for global access
    /// </summary>
    public static class TestLogger
    {
        private static ITestLogger _instance = new ConsoleTestLogger();

        /// <summary>
        /// Set the logger instance
        /// </summary>
        public static void SetLogger(ITestLogger logger)
        {
            _instance = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get the current logger instance
        /// </summary>
        public static ITestLogger Instance => _instance;

        // Convenience methods for static access
        public static void LogTrace(string message, params object[] args) => _instance.LogTrace(message, args);
        public static void LogDebug(string message, params object[] args) => _instance.LogDebug(message, args);
        public static void LogInformation(string message, params object[] args) => _instance.LogInformation(message, args);
        public static void LogWarning(string message, params object[] args) => _instance.LogWarning(message, args);
        public static void LogError(string message, params object[] args) => _instance.LogError(message, args);
        public static void LogCritical(string message, params object[] args) => _instance.LogCritical(message, args);
        public static void LogException(Exception exception, string message = null, params object[] args) =>
            _instance.LogException(exception, message, args);
    }
}
