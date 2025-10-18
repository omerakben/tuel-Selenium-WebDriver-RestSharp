using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System;
using System.Globalization;
using fhlb.selenium.common.Initializers;
using OpenQA.Selenium;

namespace loc.test
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

        // Entra ID Configuration
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

        // FHLB Common Library Parameters
        public static string? EmailTo { get; private set; }
        public static bool IsFhlbDriverInitialized { get; private set; }

        // API Client
        public static RestClient? ApiClient { get; private set; }
        #endregion

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            // 1. Load all configuration from .runsettings using the correct parameter names
            LoadConfig(context);
            Console.WriteLine($"--- Assembly Initialized ---");
            Console.WriteLine($"Environment: {ENV}");
            Console.WriteLine($"UI URL (from BaseURL): {UiUrl}");
            Console.WriteLine($"API URL (from BaseurlAPI): {BaseApiUrl}");

            // 2. Prepare FHLB Edge Driver configuration
            PrepareEdgeDriverConfiguration();

            // 3. Initialize the RestSharp API client
            InitializeApiClient();
        }

        private static void PrepareEdgeDriverConfiguration()
        {
            try
            {
                Console.WriteLine("Preparing Edge driver configuration using fhlb EdgeDriverManager");
                IsFhlbDriverInitialized = true; //Switch true for using fhlb.selenium.common
                Console.WriteLine("fhlb EdgeDriverManager configuration prepared successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Edge driver configuration failed. Local WebDriverFactory will be used as fallback. Error: {ex.Message}");
                IsFhlbDriverInitialized = false;
            }
        }

        public static IWebDriver CreateEdgeDriver()
        {
            try
            {
                Console.WriteLine("Creating Edge driver using fhlb EdgeDriverManager");
                return EdgeDriverManager.CreateDriver(Email, ENV);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create Edge driver: {ex.Message}");
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
            DefaultTimeoutSeconds = GetIntContextProperty(context, "DefaultTimeoutSeconds", 60);

            Email = GetContextProperty(context, "UserName");
            Password = GetContextProperty(context, "UserPassword");

            EntraIdTenantId = GetContextProperty(context, "EntraIdTenantId") ?? string.Empty;
            EntraIdClientId = GetContextProperty(context, "EntraIdClientId") ?? string.Empty;
            EntraIdClientSecret = GetContextProperty(context, "EntraIdClientSecret");
            EntraIdApiScope = GetContextProperty(context, "EntraIdApiScope") ?? string.Empty;
            EntraIdAuthority = GetContextProperty(context, "EntraIdAuthority", "https://login.microsoftonline.com")!;

            EntraIdUseLocalJwt = GetBoolContextProperty(context, "EntraIdUseLocalJwt", false);
            EntraIdLocalJwtRole = GetContextProperty(context, "EntraIdLocalJwtRole");

            ApiTimeoutSeconds = GetIntContextProperty(context, "ApiTimeoutSeconds", 30);
            ApiMaxRetryAttempts = GetIntContextProperty(context, "ApiMaxRetryAttempts", 3);
            ApiRetryIntervalMilliseconds = GetIntContextProperty(context, "ApiRetryIntervalMilliseconds", 1000);

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
            Console.WriteLine($"API client initialized for Base URL: {BaseApiUrl}");
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
            Console.WriteLine("AssemblyCleanup: API client disposed.");
        }
    }
}