using System;
using OpenQA.Selenium;
using TUEL.TestFramework.Logging;
using TUEL.TestFramework;

namespace TUEL.TestFramework.Web.Support
{
    /// <summary>
    /// Coordinates WebDriver creation, pooling, and disposal across tests.
    /// </summary>
    public static class WebDriverLifecycleManager
    {
        private static readonly object SyncRoot = new();
        private static WebDriverLifecycleOptions _options = new();
        private static WebDriverPool? _pool;
        private static string _poolKey = string.Empty;
        private static bool _configured;

        public static void Configure(WebDriverLifecycleOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            lock (SyncRoot)
            {
                _options = options;
                _pool?.Dispose();
                _pool = options.EnablePooling ? new WebDriverPool(options.MaxPoolSize, options.MaxIdleTime) : null;
                _poolKey = BuildPoolKey(options);
                _configured = true;

                TestLogger.LogInformation("WebDriverLifecycleManager: configured provider {0} for {1} (pooling: {2}, maxPoolSize: {3})",
                    options.Provider,
                    options.BrowserName,
                    options.EnablePooling,
                    options.EnablePooling ? options.MaxPoolSize : 0);
            }
        }

        public static IWebDriver AcquireDriver()
        {
            EnsureConfigured();
            var request = BuildRequestOptions();

            IWebDriver driver;
            if (_pool is not null)
            {
                driver = _pool.Acquire(_poolKey, () => WebDriverFactory.CreateDriver(request));
            }
            else
            {
                driver = WebDriverFactory.CreateDriver(request);
            }

            ConfigureDriverTimeouts(driver);
            return driver;
        }

        public static void ReleaseDriver(IWebDriver? driver, bool reusable)
        {
            if (driver is null)
            {
                return;
            }

            EnsureConfigured();

            if (_pool is not null && reusable)
            {
                _pool.Release(_poolKey, driver);
                return;
            }

            SafeDispose(driver);
        }

        public static void Shutdown()
        {
            lock (SyncRoot)
            {
                _pool?.Dispose();
                _pool = null;
                _configured = false;
            }
        }

        private static WebDriverRequestOptions BuildRequestOptions()
        {
            return new WebDriverRequestOptions
            {
                BrowserName = _options.BrowserName,
                Provider = _options.Provider,
                Headless = _options.Headless,
                RemoteServerUri = _options.RemoteServerUri,
                CommandTimeout = _options.CommandTimeout,
                AdditionalCapabilities = _options.AdditionalCapabilities
            };
        }

        private static void ConfigureDriverTimeouts(IWebDriver driver)
        {
            try
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
                driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(InitializeTestAssembly.DefaultTimeoutSeconds);
                driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(InitializeTestAssembly.DefaultTimeoutSeconds);
            }
            catch (Exception ex)
            {
                TestLogger.LogWarning("WebDriverLifecycleManager: failed to configure driver timeouts: {0}", ex.Message);
            }
        }

        private static string BuildPoolKey(WebDriverLifecycleOptions options)
        {
            var remote = options.RemoteServerUri?.ToString() ?? "local";
            return string.Concat(options.Provider, ':', options.BrowserName, ':', options.Headless, ':', remote).ToLowerInvariant();
        }

        private static void SafeDispose(IWebDriver driver)
        {
            try { driver.Close(); } catch { /* ignore */ }
            try { driver.Quit(); } catch { /* ignore */ }
            try { driver.Dispose(); } catch { /* ignore */ }
        }

        private static void EnsureConfigured()
        {
            if (_configured)
            {
                return;
            }

            throw new InvalidOperationException("WebDriverLifecycleManager.Configure must be called before using WebDriver instances.");
        }
    }
}
