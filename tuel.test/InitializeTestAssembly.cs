using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System;
using System.Globalization;
using OpenQA.Selenium;
using TUEL.TestFramework.Configuration;
using TUEL.TestFramework.Logging;
using TUEL.TestFramework.Security.Secrets;
using TUEL.TestFramework.Web.Support;
using LogLevel = TUEL.TestFramework.Logging.LogLevel;

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
        public static string EntraIdLocalJwtSigningAlgorithm { get; private set; } = "RS256";
        public static string? EntraIdLocalJwtPrivateKey { get; private set; }
        public static string? EntraIdLocalJwtKeyId { get; private set; }
        public static string? EntraIdLocalJwtIssuer { get; private set; }
        public static string? EntraIdLocalJwtAudience { get; private set; }
        public static int EntraIdLocalJwtLifetimeMinutes { get; private set; } = 60;
        public static bool EntraIdLocalJwtIncludeScopeClaim { get; private set; } = true;

        // API Request Configuration
        public static int ApiMaxRetryAttempts { get; private set; } = 3;
        public static int ApiRetryIntervalMilliseconds { get; private set; } = 1000;
        public static int ApiTimeoutSeconds { get; private set; } = 30;

        // Common Library Parameters
        public static string? EmailTo { get; private set; }
        public static bool IsDriverInitialized { get; private set; }

        // WebDriver Configuration
        public static bool WebDriverEnablePooling { get; private set; } = true;
        public static int WebDriverMaxPoolSize { get; private set; } = 2;
        public static int WebDriverIdleTimeoutMinutes { get; private set; } = 10;
        public static string WebDriverProvider { get; private set; } = "local";
        public static string? SeleniumGridUrl { get; private set; }
        public static bool WebDriverHeadless { get; private set; }
        public static int WebDriverCommandTimeoutSeconds { get; private set; } = 180;

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

            // 3. Initialize secure secret management
            SecretManager.Initialize(context);

            // 4. Load all configuration from .runsettings using the correct parameter names
            LoadConfig(context);
            TestLogger.LogInformation("--- Assembly Initialized ---");
            TestLogger.LogInformation("Environment: {0}", ENV);
            TestLogger.LogInformation("UI URL (from BaseURL): {0}", UiUrl);
            TestLogger.LogInformation("API URL (from BaseurlAPI): {0}", BaseApiUrl);

            // 5. Prepare WebDriver configuration
            PrepareWebDriverConfiguration();

            // 6. Initialize the RestSharp API client
            InitializeApiClient();
        }

        private static void PrepareWebDriverConfiguration()
        {
            try
            {
                TestLogger.LogInformation("Preparing WebDriver configuration");

                var requestedBrowser = Browser;
                var (browserName, provider, remoteServerUri) = ResolveWebDriverProvider();
                var headless = WebDriverHeadless || UIHelper.IsPipelineEnvironment();

                var options = new WebDriverLifecycleOptions
                {
                    EnablePooling = WebDriverEnablePooling,
                    MaxPoolSize = WebDriverMaxPoolSize,
                    MaxIdleTime = TimeSpan.FromMinutes(WebDriverIdleTimeoutMinutes),
                    BrowserName = browserName,
                    Headless = headless,
                    Provider = provider,
                    RemoteServerUri = remoteServerUri,
                    CommandTimeout = TimeSpan.FromSeconds(WebDriverCommandTimeoutSeconds)
                };

                WebDriverLifecycleManager.Configure(options);

                IsDriverInitialized = true;
                TestLogger.LogInformation("WebDriver configuration prepared successfully (requested: {0}, resolved: {1}, provider: {2}, headless: {3}).",
                    requestedBrowser,
                    browserName,
                    provider,
                    headless);
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
                if (!IsDriverInitialized)
                {
                    TestLogger.LogWarning("WebDriver lifecycle manager is not initialized. Attempting to configure with defaults.");
                    PrepareWebDriverConfiguration();
                }

                if (IsDriverInitialized)
                {
                    return WebDriverLifecycleManager.AcquireDriver();
                }

                var fallbackBrowser = string.IsNullOrWhiteSpace(Browser) ? "edge" : Browser;
                var fallbackHeadless = UIHelper.IsPipelineEnvironment();
                TestLogger.LogWarning("Falling back to direct WebDriverFactory creation for browser '{0}'.", fallbackBrowser);

                var request = new WebDriverRequestOptions
                {
                    BrowserName = fallbackBrowser,
                    Provider = WebDriverProviderType.Local,
                    Headless = fallbackHeadless,
                    CommandTimeout = TimeSpan.FromSeconds(WebDriverCommandTimeoutSeconds)
                };

                return WebDriverFactory.CreateDriver(request);
            }
            catch (Exception ex)
            {
                TestLogger.LogError("Failed to create WebDriver: {0}", ex.Message);
                throw;
            }
        }

        private static (string BrowserName, WebDriverProviderType Provider, Uri? RemoteServerUri) ResolveWebDriverProvider()
        {
            var input = (Browser ?? "edge").Trim();
            var normalized = input.ToLowerInvariant();

            var provider = ParseWebDriverProvider(WebDriverProvider);
            var browserName = normalized;

            if (normalized.StartsWith("local-", StringComparison.Ordinal))
            {
                provider = WebDriverProviderType.Local;
                browserName = normalized["local-".Length..];
            }
            else if (normalized.StartsWith("remote-", StringComparison.Ordinal) || normalized.StartsWith("grid-", StringComparison.Ordinal))
            {
                provider = WebDriverProviderType.SeleniumGrid;
                var dashIndex = normalized.IndexOf('-', StringComparison.Ordinal);
                browserName = dashIndex >= 0 && dashIndex < normalized.Length - 1 ? normalized[(dashIndex + 1)..] : "edge";
            }

            if (string.IsNullOrWhiteSpace(browserName))
            {
                browserName = "edge";
            }

            Browser = browserName;

            Uri? remoteUri = null;
            if (provider == WebDriverProviderType.SeleniumGrid)
            {
                var gridUrl = SeleniumGridUrl?.Trim();
                if (string.IsNullOrWhiteSpace(gridUrl))
                {
                    throw new InvalidOperationException("Selenium Grid provider selected but 'SeleniumGridUrl' is not configured.");
                }

                if (!Uri.TryCreate(gridUrl, UriKind.Absolute, out remoteUri))
                {
                    throw new InvalidOperationException($"The configured SeleniumGridUrl '{gridUrl}' is not a valid absolute URI.");
                }
            }

            return (browserName, provider, remoteUri);
        }

        private static WebDriverProviderType ParseWebDriverProvider(string? provider)
        {
            var normalized = provider?.Trim().ToLowerInvariant();
            return normalized switch
            {
                "grid" => WebDriverProviderType.SeleniumGrid,
                "selenium-grid" => WebDriverProviderType.SeleniumGrid,
                "remote" => WebDriverProviderType.SeleniumGrid,
                _ => WebDriverProviderType.Local
            };
        }

        private static void LoadConfig(TestContext context)
        {
            if (context?.Properties == null)
            {
                throw new InvalidOperationException("Ensure TestContext is properly initialized and a .runsettings file is loaded.");
            }

            ENV = GetContextProperty(context, "ENV", "UNKNOWN")!;
            Browser = GetContextProperty(context, "Browser", "edge")!;

            UiUrl = GetContextProperty(context, "BaseURL") ?? string.Empty;
            BaseApiUrl = GetContextProperty(context, "BaseurlAPI") ?? string.Empty;
            DefaultTimeoutSeconds = GetIntContextProperty(context, "DefaultTimeoutSeconds", TestConfiguration.DefaultTimeoutSeconds);

            Email = SecretManager.ResolveSecret(GetContextProperty(context, "UserName"), "UserName", warnOnPlaintext: false);
            Password = SecretManager.ResolveSecret(GetContextProperty(context, "UserPassword"), "UserPassword");

            EntraIdTenantId = GetContextProperty(context, "EntraIdTenantId") ?? string.Empty;
            EntraIdClientId = GetContextProperty(context, "EntraIdClientId") ?? string.Empty;
            EntraIdClientSecret = SecretManager.ResolveSecret(GetContextProperty(context, "EntraIdClientSecret"), "EntraIdClientSecret");
            EntraIdApiScope = GetContextProperty(context, "EntraIdApiScope") ?? string.Empty;
            EntraIdAuthority = GetContextProperty(context, "EntraIdAuthority", "https://login.microsoftonline.com")!;

            EntraIdUseLocalJwt = GetBoolContextProperty(context, "EntraIdUseLocalJwt", false);
            EntraIdLocalJwtRole = GetContextProperty(context, "EntraIdLocalJwtRole");
            EntraIdLocalJwtSigningAlgorithm = GetContextProperty(context, "EntraIdLocalJwtSigningAlgorithm", "RS256")!;
            EntraIdLocalJwtPrivateKey = SecretManager.ResolveSecret(GetContextProperty(context, "EntraIdLocalJwtPrivateKey"), "EntraIdLocalJwtPrivateKey");
            EntraIdLocalJwtKeyId = GetContextProperty(context, "EntraIdLocalJwtKeyId");
            EntraIdLocalJwtIssuer = GetContextProperty(context, "EntraIdLocalJwtIssuer");
            EntraIdLocalJwtAudience = GetContextProperty(context, "EntraIdLocalJwtAudience");
            EntraIdLocalJwtLifetimeMinutes = GetIntContextProperty(context, "EntraIdLocalJwtLifetimeMinutes", 60);
            EntraIdLocalJwtIncludeScopeClaim = GetBoolContextProperty(context, "EntraIdLocalJwtIncludeScopeClaim", true);

            if (!string.IsNullOrWhiteSpace(EntraIdLocalJwtSigningAlgorithm))
            {
                EntraIdLocalJwtSigningAlgorithm = EntraIdLocalJwtSigningAlgorithm.ToUpperInvariant();
            }

            EntraIdLocalJwtLifetimeMinutes = Math.Clamp(EntraIdLocalJwtLifetimeMinutes, 5, 240);

            if (EntraIdUseLocalJwt && string.IsNullOrWhiteSpace(EntraIdLocalJwtPrivateKey))
            {
                throw new InvalidOperationException("Configuration Error: 'EntraIdLocalJwtPrivateKey' must be provided when EntraIdUseLocalJwt is true. Use env://, kv:// or enc:// secrets to avoid plaintext configuration.");
            }

            ApiTimeoutSeconds = GetIntContextProperty(context, "ApiTimeoutSeconds", TestConfiguration.ApiTimeoutSeconds);
            ApiMaxRetryAttempts = GetIntContextProperty(context, "ApiMaxRetryAttempts", TestConfiguration.MaxRetryAttempts);
            ApiRetryIntervalMilliseconds = GetIntContextProperty(context, "ApiRetryIntervalMilliseconds", TestConfiguration.RetryDelayMilliseconds);

            WebDriverEnablePooling = GetBoolContextProperty(context, "WebDriverEnablePooling", true);
            WebDriverMaxPoolSize = GetIntContextProperty(context, "WebDriverMaxPoolSize", 2);
            WebDriverIdleTimeoutMinutes = GetIntContextProperty(context, "WebDriverIdleTimeoutMinutes", 10);
            WebDriverProvider = GetContextProperty(context, "WebDriverProvider", "local")!;
            SeleniumGridUrl = GetContextProperty(context, "SeleniumGridUrl");
            WebDriverHeadless = GetBoolContextProperty(context, "WebDriverHeadless", false);
            WebDriverCommandTimeoutSeconds = GetIntContextProperty(context, "WebDriverCommandTimeoutSeconds", 180);

            WebDriverMaxPoolSize = Math.Clamp(WebDriverMaxPoolSize, 1, 10);
            WebDriverIdleTimeoutMinutes = Math.Clamp(WebDriverIdleTimeoutMinutes, 1, 60);
            WebDriverCommandTimeoutSeconds = Math.Clamp(WebDriverCommandTimeoutSeconds, 30, 600);
            WebDriverProvider = WebDriverProvider.Trim().ToLowerInvariant();

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
            WebDriverLifecycleManager.Shutdown();
            SecretManager.Shutdown();
            ApiClient?.Dispose();
            TestLogger.LogInformation("AssemblyCleanup complete: WebDriver and API resources disposed.");
        }
    }
}
