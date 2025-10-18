using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;
using fhlb.selenium.common.Extensions;

namespace loc.test.Web.Support
{
    // Static helper / extension class for robust Selenium actions.
    public static class UIHelper
    {
        /* Config */
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(
            InitializeTestAssembly.DefaultTimeoutSeconds);

        /* Environment Detection */
        public static bool IsPipelineEnvironment()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SYSTEM_DEFINITIONID"));
        }

        public const int DefaultRetries = 3;
        public static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

        /* Element‑state waits */
        public static IWebElement WaitVisible(this IWebDriver driver, By by, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(InitializeTestAssembly.DefaultTimeoutSeconds));
            return wait.Until(d =>
            {
                try
                {
                    var element = d.FindElement(by);
                    if (element.Displayed && element.Enabled)
                    {
                        return element;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    // Element is stale, let the wait continue to find it again
                }
                catch (NoSuchElementException)
                {
                    // Element not found yet, let the wait continue
                }
                return null;
            });
        }

        public static IWebElement WaitClickable(this IWebDriver driver, By by, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(InitializeTestAssembly.DefaultTimeoutSeconds));
            return wait.Until(ExpectedConditions.ElementToBeClickable(by));
        }

        public static void ClickElement(this IWebDriver driver, By by, TimeSpan? timeout = null)
        {
            var element = driver.WaitClickable(by, timeout ?? TimeSpan.FromSeconds(InitializeTestAssembly.DefaultTimeoutSeconds));
            element.Click();
        }

        public static void EnterText(this IWebDriver driver, By by, string? text, bool clearFirst = true, TimeSpan? timeout = null)
        {
            if (text == null) return;
            var element = driver.WaitVisible(by, timeout ?? TimeSpan.FromSeconds(InitializeTestAssembly.DefaultTimeoutSeconds));
            if (clearFirst)
            {
                element.Clear();
            }
            element.SendKeys(text);
        }

        public static string GetElementText(this IWebDriver driver, By by, TimeSpan? timeout = null)
        {
            var element = driver.WaitVisible(by, timeout ?? TimeSpan.FromSeconds(InitializeTestAssembly.DefaultTimeoutSeconds));
            return element.Text;
        }

        public static bool IsDisplayedSafe(this IWebDriver driver, By by)
        {
            try
            {
                return driver.FindElement(by).Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        // Enhanced WaitVisible with retry logic for stale element references
        public static IWebElement WaitVisibleWithRetry(this IWebDriver driver, By by, TimeSpan? timeout = null, int maxRetries = 3)
        {
            var individualTimeout = timeout ?? TimeSpan.FromSeconds(InitializeTestAssembly.DefaultTimeoutSeconds);
            var retryTimeout = TimeSpan.FromSeconds(individualTimeout.TotalSeconds / maxRetries);

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return driver.WaitVisible(by, retryTimeout);
                }
                catch (WebDriverTimeoutException) when (i < maxRetries - 1)
                {
                    // Wait before retry to allow page transitions
                    Thread.Sleep(1000);
                    Console.WriteLine($"Retry {i + 1}/{maxRetries} for element: {by}");
                }
                catch (StaleElementReferenceException) when (i < maxRetries - 1)
                {
                    // Wait for page to stabilize before retry
                    Thread.Sleep(1500);
                    Console.WriteLine($"Stale element retry {i + 1}/{maxRetries} for: {by}");
                }
            }

            return driver.WaitVisible(by, individualTimeout);
        }

        // Wait for page transition to complete
        public static bool WaitForPageTransition(this IWebDriver driver, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(10));
            try
            {
                // Wait for document ready state
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                // Additional wait for any dynamic content
                Thread.Sleep(500);
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        // Wait for URL to change from a previous URL
        public static bool WaitForUrlChange(this IWebDriver driver, string previousUrl, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(10));
            try
            {
                return wait.Until(d => !d.Url.Equals(previousUrl, StringComparison.OrdinalIgnoreCase));
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        // Wait for URL to contain a specific part
        public static bool WaitForUrlContains(this IWebDriver driver, string urlPart, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(driver, timeout ?? TimeSpan.FromSeconds(10));
            try
            {
                return wait.Until(d => d.Url.Contains(urlPart, StringComparison.OrdinalIgnoreCase));
            }
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        // Generic retry mechanism for actions
        public static void Retry(Action act, int times = DefaultRetries)
        {
            Exception last = null!;
            for (var i = 0; i < times; i++)
            {
                try { act(); return; }
                catch (Exception ex)
                {
                    last = ex;
                    if (i < times - 1) Thread.Sleep(RetryDelay);
                }
            }
            throw last;
        }
    }
}