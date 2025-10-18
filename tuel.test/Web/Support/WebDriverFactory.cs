using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Remote;
using TUEL.TestFramework.Logging;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace TUEL.TestFramework.Web.Support
{
    /// <summary>
    /// Creates configured WebDriver instances for local and remote providers.
    /// </summary>
    public static class WebDriverFactory
    {
        public static IWebDriver CreateDriver(WebDriverRequestOptions request)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            TestLogger.LogInformation("WebDriverFactory: Creating {0} driver (provider: {1}, headless: {2})",
                request.BrowserName,
                request.Provider,
                request.Headless);

            return request.Provider switch
            {
                WebDriverProviderType.Local => CreateLocalDriver(request),
                WebDriverProviderType.SeleniumGrid => CreateRemoteDriver(request),
                _ => throw new NotSupportedException($"Unsupported WebDriver provider '{request.Provider}'.")
            };
        }

        private static IWebDriver CreateLocalDriver(WebDriverRequestOptions request)
        {
            return NormalizeBrowserName(request.BrowserName) switch
            {
                "chrome" => CreateLocalChromeDriver(request),
                "edge" => CreateLocalEdgeDriver(request),
                _ => throw new NotSupportedException($"Browser '{request.BrowserName}' is not supported for local runs.")
            };
        }

        private static IWebDriver CreateRemoteDriver(WebDriverRequestOptions request)
        {
            if (request.RemoteServerUri is null)
            {
                throw new InvalidOperationException("Remote WebDriver requested but RemoteServerUri is not configured.");
            }

            DriverOptions options = NormalizeBrowserName(request.BrowserName) switch
            {
                "chrome" => BuildChromeOptions(request),
                "edge" => BuildEdgeOptions(request),
                _ => throw new NotSupportedException($"Browser '{request.BrowserName}' is not supported for remote execution.")
            };

            return new RemoteWebDriver(request.RemoteServerUri, options.ToCapabilities(), request.CommandTimeout);
        }

        private static IWebDriver CreateLocalChromeDriver(WebDriverRequestOptions request)
        {
            new DriverManager().SetUpDriver(new ChromeConfig());
            var options = BuildChromeOptions(request);
            return new ChromeDriver(options);
        }

        private static IWebDriver CreateLocalEdgeDriver(WebDriverRequestOptions request)
        {
            new DriverManager().SetUpDriver(new EdgeConfig());
            var options = BuildEdgeOptions(request);
            return new EdgeDriver(options);
        }

        private static ChromeOptions BuildChromeOptions(WebDriverRequestOptions request)
        {
            var options = new ChromeOptions
            {
                AcceptInsecureCertificates = true
            };

            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-dev-shm-usage");

            if (request.Headless)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--window-size=1920,1080");
            }
            else
            {
                options.AddArgument("--start-maximized");
                options.AddArgument("--window-size=1920,1200");
            }

            ApplyAdditionalCapabilities(options, request.AdditionalCapabilities);
            return options;
        }

        private static EdgeOptions BuildEdgeOptions(WebDriverRequestOptions request)
        {
            var options = new EdgeOptions
            {
                AcceptInsecureCertificates = true
            };

            options.AddArgument("--ignore-certificate-errors");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-dev-shm-usage");

            if (request.Headless)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--window-size=1920,1080");
            }
            else
            {
                options.AddArgument("--start-maximized");
                options.AddArgument("--window-size=1920,1200");
            }

            ApplyAdditionalCapabilities(options, request.AdditionalCapabilities);
            return options;
        }

        private static void ApplyAdditionalCapabilities(DriverOptions options, IDictionary<string, object>? additionalCapabilities)
        {
            if (additionalCapabilities is null)
            {
                return;
            }

            foreach (var capability in additionalCapabilities)
            {
                options.AddAdditionalOption(capability.Key, capability.Value);
            }
        }

        private static string NormalizeBrowserName(string browser)
        {
            return string.IsNullOrWhiteSpace(browser) ? "edge" : browser.Trim().ToLowerInvariant();
        }
    }
}
