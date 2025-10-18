using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TUEL.TestFramework.Configuration
{
    /// <summary>
    /// Centralized configuration service for test framework settings
    /// </summary>
    public static class TestConfiguration
    {
        #region Timeout Configuration
        /// <summary>
        /// Default timeout for UI operations in seconds
        /// </summary>
        public static int DefaultTimeoutSeconds { get; private set; } = 30;

        /// <summary>
        /// Default timeout for API operations in seconds
        /// </summary>
        public static int ApiTimeoutSeconds { get; private set; } = 30;

        /// <summary>
        /// Default timeout for page transitions in seconds
        /// </summary>
        public static int PageTransitionTimeoutSeconds { get; private set; } = 10;

        /// <summary>
        /// Default timeout for element visibility checks in seconds
        /// </summary>
        public static int ElementVisibilityTimeoutSeconds { get; private set; } = 15;

        /// <summary>
        /// Default timeout for element clickability checks in seconds
        /// </summary>
        public static int ElementClickabilityTimeoutSeconds { get; private set; } = 10;

        /// <summary>
        /// Default retry delay in milliseconds
        /// </summary>
        public static int RetryDelayMilliseconds { get; private set; } = 500;

        /// <summary>
        /// Maximum retry attempts for operations
        /// </summary>
        public static int MaxRetryAttempts { get; private set; } = 3;
        #endregion

        #region Wait Strategy Configuration
        /// <summary>
        /// Enable smart wait strategies (WebDriverWait instead of Thread.Sleep)
        /// </summary>
        public static bool UseSmartWaitStrategies { get; private set; } = true;

        /// <summary>
        /// Enable AJAX wait detection
        /// </summary>
        public static bool WaitForAjaxCompletion { get; private set; } = true;

        /// <summary>
        /// Enable page load state detection
        /// </summary>
        public static bool WaitForPageLoadState { get; private set; } = true;
        #endregion

        #region Performance Configuration
        /// <summary>
        /// Enable parallel test execution
        /// </summary>
        public static bool EnableParallelExecution { get; private set; } = false;

        /// <summary>
        /// Maximum parallel test threads
        /// </summary>
        public static int MaxParallelThreads { get; private set; } = Environment.ProcessorCount;

        /// <summary>
        /// Enable performance metrics collection
        /// </summary>
        public static bool EnablePerformanceMetrics { get; private set; } = true;
        #endregion

        #region Security Configuration
        /// <summary>
        /// Force HTTPS for all external communications
        /// </summary>
        public static bool ForceHttps { get; private set; } = false;

        /// <summary>
        /// Enable secure token storage
        /// </summary>
        public static bool EnableSecureTokenStorage { get; private set; } = false;

        /// <summary>
        /// Enable audit logging for security events
        /// </summary>
        public static bool EnableAuditLogging { get; private set; } = false;
        #endregion

        #region Logging Configuration
        /// <summary>
        /// Enable structured logging
        /// </summary>
        public static bool EnableStructuredLogging { get; private set; } = true;

        /// <summary>
        /// Log level for test execution
        /// </summary>
        public static string LogLevel { get; private set; } = "Information";

        /// <summary>
        /// Enable sensitive data masking in logs
        /// </summary>
        public static bool MaskSensitiveDataInLogs { get; private set; } = true;
        #endregion

        /// <summary>
        /// Initialize configuration from test context
        /// </summary>
        /// <param name="context">Test context containing configuration parameters</param>
        public static void Initialize(TestContext context)
        {
            if (context?.Properties == null)
            {
                throw new InvalidOperationException("TestContext is required for configuration initialization");
            }

            // Load timeout configurations
            DefaultTimeoutSeconds = GetIntProperty(context, "DefaultTimeoutSeconds", 30);
            ApiTimeoutSeconds = GetIntProperty(context, "ApiTimeoutSeconds", 30);
            PageTransitionTimeoutSeconds = GetIntProperty(context, "PageTransitionTimeoutSeconds", 10);
            ElementVisibilityTimeoutSeconds = GetIntProperty(context, "ElementVisibilityTimeoutSeconds", 15);
            ElementClickabilityTimeoutSeconds = GetIntProperty(context, "ElementClickabilityTimeoutSeconds", 10);
            RetryDelayMilliseconds = GetIntProperty(context, "RetryDelayMilliseconds", 500);
            MaxRetryAttempts = GetIntProperty(context, "MaxRetryAttempts", 3);

            // Load wait strategy configurations
            UseSmartWaitStrategies = GetBoolProperty(context, "UseSmartWaitStrategies", true);
            WaitForAjaxCompletion = GetBoolProperty(context, "WaitForAjaxCompletion", true);
            WaitForPageLoadState = GetBoolProperty(context, "WaitForPageLoadState", true);

            // Load performance configurations
            EnableParallelExecution = GetBoolProperty(context, "EnableParallelExecution", false);
            MaxParallelThreads = GetIntProperty(context, "MaxParallelThreads", Environment.ProcessorCount);
            EnablePerformanceMetrics = GetBoolProperty(context, "EnablePerformanceMetrics", true);

            // Load security configurations
            ForceHttps = GetBoolProperty(context, "ForceHttps", false);
            EnableSecureTokenStorage = GetBoolProperty(context, "EnableSecureTokenStorage", false);
            EnableAuditLogging = GetBoolProperty(context, "EnableAuditLogging", false);

            // Load logging configurations
            EnableStructuredLogging = GetBoolProperty(context, "EnableStructuredLogging", true);
            LogLevel = GetStringProperty(context, "LogLevel", "Information");
            MaskSensitiveDataInLogs = GetBoolProperty(context, "MaskSensitiveDataInLogs", true);
        }

        /// <summary>
        /// Get timeout as TimeSpan for UI operations
        /// </summary>
        public static TimeSpan GetDefaultTimeout() => TimeSpan.FromSeconds(DefaultTimeoutSeconds);

        /// <summary>
        /// Get timeout as TimeSpan for API operations
        /// </summary>
        public static TimeSpan GetApiTimeout() => TimeSpan.FromSeconds(ApiTimeoutSeconds);

        /// <summary>
        /// Get timeout as TimeSpan for page transitions
        /// </summary>
        public static TimeSpan GetPageTransitionTimeout() => TimeSpan.FromSeconds(PageTransitionTimeoutSeconds);

        /// <summary>
        /// Get timeout as TimeSpan for element visibility
        /// </summary>
        public static TimeSpan GetElementVisibilityTimeout() => TimeSpan.FromSeconds(ElementVisibilityTimeoutSeconds);

        /// <summary>
        /// Get timeout as TimeSpan for element clickability
        /// </summary>
        public static TimeSpan GetElementClickabilityTimeout() => TimeSpan.FromSeconds(ElementClickabilityTimeoutSeconds);

        /// <summary>
        /// Get retry delay as TimeSpan
        /// </summary>
        public static TimeSpan GetRetryDelay() => TimeSpan.FromMilliseconds(RetryDelayMilliseconds);

        #region Private Helper Methods
        private static string GetStringProperty(TestContext context, string name, string defaultValue)
        {
            var propertyValue = context.Properties[name]?.ToString();
            return string.IsNullOrEmpty(propertyValue) ? defaultValue : propertyValue;
        }

        private static int GetIntProperty(TestContext context, string name, int defaultValue)
        {
            var propertyValue = context.Properties[name]?.ToString();
            return int.TryParse(propertyValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
        }

        private static bool GetBoolProperty(TestContext context, string name, bool defaultValue)
        {
            var propertyValue = context.Properties[name]?.ToString();
            return bool.TryParse(propertyValue, out var result) ? result : defaultValue;
        }
        #endregion
    }
}
