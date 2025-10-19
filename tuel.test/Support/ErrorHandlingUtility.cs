using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TUEL.TestFramework.Logging;

namespace TUEL.TestFramework.Support
{
    /// <summary>
    /// Comprehensive error handling utility for test framework operations.
    /// Provides standardized error handling, retry mechanisms, and detailed logging.
    /// </summary>
    public static class ErrorHandlingUtility
    {
        /// <summary>
        /// Executes an action with comprehensive error handling and retry logic.
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="operationName">Name of the operation for logging purposes</param>
        /// <param name="maxRetries">Maximum number of retry attempts</param>
        /// <param name="retryDelay">Delay between retry attempts</param>
        /// <param name="throwOnFailure">Whether to throw exception on final failure</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static bool ExecuteWithRetry(
            Action action,
            string operationName,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            bool throwOnFailure = true)
        {
            var delay = retryDelay ?? TimeSpan.FromMilliseconds(500);
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    TestLogger.LogDebug("Executing {0} (attempt {1}/{2})", operationName, attempt, maxRetries);
                    action();
                    TestLogger.LogInformation("Operation {0} completed successfully", operationName);
                    return true;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    TestLogger.LogWarning("Operation {0} failed on attempt {1}/{2}: {3}",
                        operationName, attempt, maxRetries, ex.Message);

                    if (attempt < maxRetries)
                    {
                        TestLogger.LogDebug("Retrying {0} after {1}ms delay", operationName, delay.TotalMilliseconds);
                        Task.Delay(delay).Wait();
                    }
                }
            }

            TestLogger.LogError("Operation {0} failed after {1} attempts", operationName, maxRetries);

            if (throwOnFailure && lastException != null)
            {
                throw new InvalidOperationException($"Operation '{operationName}' failed after {maxRetries} attempts", lastException);
            }

            return false;
        }

        /// <summary>
        /// Executes an async action with comprehensive error handling and retry logic.
        /// </summary>
        /// <param name="action">The async action to execute</param>
        /// <param name="operationName">Name of the operation for logging purposes</param>
        /// <param name="maxRetries">Maximum number of retry attempts</param>
        /// <param name="retryDelay">Delay between retry attempts</param>
        /// <param name="throwOnFailure">Whether to throw exception on final failure</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static async Task<bool> ExecuteWithRetryAsync(
            Func<Task> action,
            string operationName,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            bool throwOnFailure = true)
        {
            var delay = retryDelay ?? TimeSpan.FromMilliseconds(500);
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    TestLogger.LogDebug("Executing async {0} (attempt {1}/{2})", operationName, attempt, maxRetries);
                    await action();
                    TestLogger.LogInformation("Async operation {0} completed successfully", operationName);
                    return true;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    TestLogger.LogWarning("Async operation {0} failed on attempt {1}/{2}: {3}",
                        operationName, attempt, maxRetries, ex.Message);

                    if (attempt < maxRetries)
                    {
                        TestLogger.LogDebug("Retrying async {0} after {1}ms delay", operationName, delay.TotalMilliseconds);
                        await Task.Delay(delay);
                    }
                }
            }

            TestLogger.LogError("Async operation {0} failed after {1} attempts", operationName, maxRetries);

            if (throwOnFailure && lastException != null)
            {
                throw new InvalidOperationException($"Async operation '{operationName}' failed after {maxRetries} attempts", lastException);
            }

            return false;
        }

        /// <summary>
        /// Executes a function with comprehensive error handling and retry logic.
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="function">The function to execute</param>
        /// <param name="operationName">Name of the operation for logging purposes</param>
        /// <param name="maxRetries">Maximum number of retry attempts</param>
        /// <param name="retryDelay">Delay between retry attempts</param>
        /// <param name="defaultValue">Default value to return on failure</param>
        /// <returns>Result of the function or default value on failure</returns>
        public static T ExecuteWithRetry<T>(
            Func<T> function,
            string operationName,
            int maxRetries = 3,
            TimeSpan? retryDelay = null,
            T defaultValue = default(T))
        {
            var delay = retryDelay ?? TimeSpan.FromMilliseconds(500);
            Exception? lastException = null;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    TestLogger.LogDebug("Executing {0} (attempt {1}/{2})", operationName, attempt, maxRetries);
                    var result = function();
                    TestLogger.LogInformation("Operation {0} completed successfully", operationName);
                    return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    TestLogger.LogWarning("Operation {0} failed on attempt {1}/{2}: {3}",
                        operationName, attempt, maxRetries, ex.Message);

                    if (attempt < maxRetries)
                    {
                        TestLogger.LogDebug("Retrying {0} after {1}ms delay", operationName, delay.TotalMilliseconds);
                        Task.Delay(delay).Wait();
                    }
                }
            }

            TestLogger.LogError("Operation {0} failed after {1} attempts, returning default value", operationName, maxRetries);
            return defaultValue;
        }

        /// <summary>
        /// Safely executes an action and logs any exceptions without throwing.
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="operationName">Name of the operation for logging purposes</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static bool SafeExecute(Action action, string operationName)
        {
            try
            {
                TestLogger.LogDebug("Safely executing {0}", operationName);
                action();
                TestLogger.LogDebug("Safe execution of {0} completed successfully", operationName);
                return true;
            }
            catch (Exception ex)
            {
                TestLogger.LogException(ex, "Safe execution of {0} failed", operationName);
                return false;
            }
        }

        /// <summary>
        /// Safely executes a function and logs any exceptions without throwing.
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="function">The function to execute</param>
        /// <param name="operationName">Name of the operation for logging purposes</param>
        /// <param name="defaultValue">Default value to return on failure</param>
        /// <returns>Result of the function or default value on failure</returns>
        public static T SafeExecute<T>(Func<T> function, string operationName, T defaultValue = default(T))
        {
            try
            {
                TestLogger.LogDebug("Safely executing {0}", operationName);
                var result = function();
                TestLogger.LogDebug("Safe execution of {0} completed successfully", operationName);
                return result;
            }
            catch (Exception ex)
            {
                TestLogger.LogException(ex, "Safe execution of {0} failed", operationName);
                return defaultValue;
            }
        }

        /// <summary>
        /// Creates a standardized error message with context information.
        /// </summary>
        /// <param name="operation">The operation that failed</param>
        /// <param name="context">Additional context information</param>
        /// <param name="innerException">The underlying exception</param>
        /// <returns>Formatted error message</returns>
        public static string CreateErrorMessage(string operation, string context = "", Exception? innerException = null)
        {
            var message = $"Operation '{operation}' failed";

            if (!string.IsNullOrEmpty(context))
            {
                message += $": {context}";
            }

            if (innerException != null)
            {
                message += $". Inner exception: {innerException.Message}";
            }

            return message;
        }

        /// <summary>
        /// Determines if an exception is retryable based on its type and message.
        /// </summary>
        /// <param name="exception">The exception to evaluate</param>
        /// <returns>True if the exception is retryable, false otherwise</returns>
        public static bool IsRetryableException(Exception exception)
        {
            return exception switch
            {
                System.Net.Http.HttpRequestException => true,
                System.TimeoutException => true,
                OpenQA.Selenium.WebDriverTimeoutException => true,
                OpenQA.Selenium.StaleElementReferenceException => true,
                OpenQA.Selenium.NoSuchElementException => true,
                System.Net.Sockets.SocketException => true,
                _ => false
            };
        }

        /// <summary>
        /// Gets a user-friendly error message for common exception types.
        /// </summary>
        /// <param name="exception">The exception to analyze</param>
        /// <returns>User-friendly error message</returns>
        public static string GetUserFriendlyErrorMessage(Exception exception)
        {
            return exception switch
            {
                System.Net.Http.HttpRequestException => "Network communication failed. Please check your internet connection and try again.",
                System.TimeoutException => "The operation timed out. Please try again or check if the service is available.",
                OpenQA.Selenium.WebDriverTimeoutException => "The web page took too long to respond. Please try again.",
                OpenQA.Selenium.StaleElementReferenceException => "The page content has changed. Please refresh and try again.",
                OpenQA.Selenium.NoSuchElementException => "The requested element was not found on the page.",
                System.Net.Sockets.SocketException => "Unable to connect to the server. Please check your network connection.",
                System.UnauthorizedAccessException => "Access denied. Please check your credentials and permissions.",
                System.ArgumentException => "Invalid input provided. Please check your parameters and try again.",
                _ => $"An unexpected error occurred: {exception.Message}"
            };
        }
    }
}
