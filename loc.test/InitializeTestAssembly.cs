using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System;
using System.Globalization;
using OpenQA.Selenium;
using TUEL.TestFramework.Configuration;
using TUEL.TestFramework.Logging;

namespace TUEL.TestFramework
{
    [TestClass]
    public static class InitializeTestAssembly
    {
        #region Public Configuration Properties
        // General Configuration
        public static string ENV { get; private set; } = "UNKNOWN";
        public static string Browser { get; private set; } = "";
        public static string UiUrl { get; private set; } = string.Empty;
        public static string BaseApiUrl { get; private set; } = string.Empty;
        public static int DefaultTimeoutSeconds { get; private set; } = 30;

        // User Credentials
        public static string? Email { get; private set; }
        public static string? Password { get; private set; }

        // Azure AD Configuration
        public static string EntraIdTenantId { get; private set; } = string.Empty;
        public static string EntraIdClientId { get; private set; } = string.Empty;
        public static string? EntraIdClientSecret { get; private set; }
        public static string EntraIdApiScope { get; private set; } = string.Empty;
        public static string EntraIdAuthority { get; private set; } = string.Empty;
        public static bool EntraIdUseLocalJwt { get; private set; }
        public static string? EntraIdLocalJwtRole { get; private set; }

        // API Request Configuration
        public static int ApiMaxRetryAttempts { get; private set; } = 3;
        public static int ApiRetryIntervalMilliseconds { get; private set; } = 1000;
        public static int ApiTimeoutSeconds { get; private set; } = 30;

        // Common Library Parameters
        public static string? EmailTo { get; private set; }
        public static bool IsDriverInitialized { get; private set; }

        // API Client
        public static RestClient? ApiClient { get; private set; }
        #endregion

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // 1. Initialize configuration service
            TestConfiguration.Initialize(context);

            // 2. Initialize structured logging
            var logLevel = Enum.TryParse<LogLevel>(TestConfiguration.LogLevel, out var level) ? level : LogLevel.Information;
            TestLogger.SetLogger(new ConsoleTestLogger(logLevel, TestConfiguration.MaskSensitiveDataInLogs));

            // 3. Load all configuration from .runsettings using the correct parameter names
            LoadConfig(context);
            TestLogger.LogInformation("--- Assembly Initialized ---");
            TestLogger.LogInformation("Environment: {0}", ENV);
            TestLogger.LogInformation("UI URL (from BaseURL): {0}", UiUrl);
            TestLogger.LogInformation("API URL (from BaseurlAPI): {0}", BaseApiUrl);

            // 4. Prepare WebDriver configuration
            PrepareWebDriverConfiguration();

            // 5. Initialize the RestSharp API client
            InitializeApiClient();
        }

        private static void PrepareWebDriverConfiguration()
        {
            try
            {
                TestLogger.LogInformation("Preparing WebDriver configuration");
                IsDriverInitialized = true;
                TestLogger.LogInformation("WebDriver configuration prepared successfully.");
            }
            catch (Exception ex)
            {
                TestLogger.LogError("WebDriver configuration failed. Local WebDriverFactory will be used as fallback. Error: {0}", ex.Message);
                IsDriverInitialized = false;
            }
        }

        public static IWebDriver CreateWebDriver()
        {
            try
            {
                TestLogger.LogInformation("Creating WebDriver");
                // This would be replaced with actual WebDriver creation logic
                throw new NotImplementedException("WebDriver creation logic needs to be implemented");
            }
            catch (Exception ex)
            {
                TestLogger.LogError("Failed to create WebDriver: {0}", ex.Message);
                throw;
            }
        }

        private static void LoadConfig(TestContext context)
        {
            if (context?.Properties == null)
            {
                throw new InvalidOperationException("Ensure TestContext is properly initialized and a .runsettings file is loaded.");
            }

            ENV = GetContextProperty(context, "ENV", "UNKNOWN")!;
            Browser = "edge"; // To switch to the local driver, use "local-edge"

            UiUrl = GetContextProperty(context, "BaseURL") ?? string.Empty;
            BaseApiUrl = GetContextProperty(context, "BaseurlAPI") ?? string.Empty;
            DefaultTimeoutSeconds = GetIntContextProperty(context, "DefaultTimeoutSeconds", TestConfiguration.DefaultTimeoutSeconds);

            Email = GetContextProperty(context, "UserName");
            Password = GetContextProperty(context, "UserPassword");

            EntraIdTenantId = GetContextProperty(context, "EntraIdTenantId") ?? string.Empty;
            EntraIdClientId = GetContextProperty(context, "EntraIdClientId") ?? string.Empty;
            EntraIdClientSecret = GetContextProperty(context, "EntraIdClientSecret");
            EntraIdApiScope = GetContextProperty(context, "EntraIdApiScope") ?? string.Empty;
            EntraIdAuthority = GetContextProperty(context, "EntraIdAuthority", "https://login.microsoftonline.com")!;

            EntraIdUseLocalJwt = GetBoolContextProperty(context, "EntraIdUseLocalJwt", false);
            EntraIdLocalJwtRole = GetContextProperty(context, "EntraIdLocalJwtRole");

            ApiTimeoutSeconds = GetIntContextProperty(context, "ApiTimeoutSeconds", TestConfiguration.ApiTimeoutSeconds);
            ApiMaxRetryAttempts = GetIntContextProperty(context, "ApiMaxRetryAttempts", TestConfiguration.MaxRetryAttempts);
            ApiRetryIntervalMilliseconds = GetIntContextProperty(context, "ApiRetryIntervalMilliseconds", TestConfiguration.RetryDelayMilliseconds);

            EmailTo = GetContextProperty(context, "EMailTo");

            // --- Validation ---
            if (string.IsNullOrEmpty(BaseApiUrl))
                throw new InvalidOperationException("Configuration Error: 'BaseurlAPI' must be defined in .runsettings.");
            if (string.IsNullOrEmpty(UiUrl))
                throw new InvalidOperationException("Configuration Error: 'BaseURL' must be defined in .runsettings.");
        }

        private static void InitializeApiClient()
        {
            if (string.IsNullOrEmpty(BaseApiUrl))
            {
                throw new InvalidOperationException("Cannot initialize API client because BaseApiUrl is not configured.");
            }

            var options = new RestClientOptions(BaseApiUrl)
            {
                //MaxTimeout = TimeSpan.FromSeconds(ApiTimeoutSeconds)
            };
            ApiClient = new RestClient(options);
            TestLogger.LogInformation("API client initialized for Base URL: {0}", BaseApiUrl);
        }

        #region Context Property Helpers
        private static string? GetContextProperty(TestContext context, string name, string? defaultValue = null)
        {
            var propertyValue = context.Properties[name]?.ToString();
            return string.IsNullOrEmpty(propertyValue) ? defaultValue : propertyValue;
        }

        private static int GetIntContextProperty(TestContext context, string name, int defaultValue)
        {
            var propertyValue = context.Properties[name]?.ToString();
            return int.TryParse(propertyValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
        }

        private static bool GetBoolContextProperty(TestContext context, string name, bool defaultValue)
        {
            var propertyValue = context.Properties[name]?.ToString();
            return bool.TryParse(propertyValue, out var result) ? result : defaultValue;
        }
        #endregion

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            ApiClient?.Dispose();
            TestLogger.LogInformation("AssemblyCleanup: API client disposed.");
        }
    }
}
