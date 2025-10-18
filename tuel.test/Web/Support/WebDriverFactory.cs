using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using System;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace TUEL.TestFramework.Web.Support
{
    // Factory for creating WebDriver instances based on configuration
    public static class WebDriverFactory
    {
        public static IWebDriver CreateDriver()
        {
            var browserName = (InitializeTestAssembly.Browser ?? "Edge").ToLowerInvariant();
            Console.WriteLine($"Creating WebDriver for browser: {browserName}");

            return browserName switch
            {
                "local-chrome" => CreateChromeDriver(),
                "local-edge" => CreateEdgeDriver(),
                _ => throw new NotSupportedException($"Browser '{browserName}' is not supported."),
            };
        }

        private static IWebDriver CreateChromeDriver()
        {
            var options = new ChromeOptions();
            if (!UIHelper.IsPipelineEnvironment())
            {
                // Local run configuration
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("disable-gpu");
                options.AddArgument("start-maximized");
                options.AddArgument("--window-size=1920,1200");
                options.AddArgument("no-sandbox");
                options.AddArgument("--incognito");
            }
            else
            {
                // Pipeline run configuration
                options.AddArgument("--headless");
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("no-sandbox");
                options.AddArgument("disable-gpu");
            }

            return new ChromeDriver(options);
        }

        private static IWebDriver CreateEdgeDriver()
        {
            var options = new EdgeOptions();

            if (!UIHelper.IsPipelineEnvironment())
            {
                // Local run configuration
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("disable-gpu");
                options.AddArgument("start-maximized");
                options.AddArgument("--window-size=1920,1200");
                options.AddArgument("no-sandbox");
                options.AddArgument("--inprivate");
            }
            else
            {
                // Pipeline run configuration
                options.AddArgument("--headless=new");
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("--window-size=1920,1080");
                options.AddArgument("no-sandbox");
                options.AddArgument("disable-gpu");
            }

            return new EdgeDriver(options);
        }
    }
}
