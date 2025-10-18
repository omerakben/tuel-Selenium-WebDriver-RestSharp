using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using TUEL.TestFramework.Configuration;
using TUEL.TestFramework.Logging;

namespace TUEL.TestFramework.Web.Support
{
    // Static helper / extension class for robust Selenium actions.
    public static class UIHelper
    {
        /* Config */
        public static readonly TimeSpan DefaultTimeout = TestConfiguration.GetDefaultTimeout();
        public static readonly TimeSpan ElementVisibilityTimeout = TestConfiguration.GetElementVisibilityTimeout();
        public static readonly TimeSpan ElementClickabilityTimeout = TestConfiguration.GetElementClickabilityTimeout();
        public static readonly TimeSpan PageTransitionTimeout = TestConfiguration.GetPageTransitionTimeout();
        public static readonly TimeSpan RetryDelay = TestConfiguration.GetRetryDelay();
        public static readonly int DefaultRetries = TestConfiguration.MaxRetryAttempts;

        /* Environment Detection */
        public static bool IsPipelineEnvironment()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SYSTEM_DEFINITIONID"));
        }

        /* Element‑state waits */
        public static IWebElement WaitVisible(this IWebDriver driver, By by, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(driver, timeout ?? ElementVisibilityTimeout);
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
            var wait = new WebDriverWait(driver, timeout ?? ElementClickabilityTimeout);
            return wait.Until(ExpectedConditions.ElementToBeClickable(by));
        }

        public static void ClickElement(this IWebDriver driver, By by, TimeSpan? timeout = null)
        {
            var element = driver.WaitClickable(by, timeout ?? ElementClickabilityTimeout);
            element.Click();
        }

        public static void EnterText(this IWebDriver driver, By by, string? text, bool clearFirst = true, TimeSpan? timeout = null)
        {
            if (text == null) return;
            var element = driver.WaitVisible(by, timeout ?? ElementVisibilityTimeout);
            if (clearFirst)
            {
                element.Clear();
            }
            element.SendKeys(text);
        }

        public static string GetElementText(this IWebDriver driver, By by, TimeSpan? timeout = null)
        {
            var element = driver.WaitVisible(by, timeout ?? ElementVisibilityTimeout);
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
        public static IWebElement WaitVisibleWithRetry(this IWebDriver driver, By by, TimeSpan? timeout = null, int maxRetries = -1)
        {
            if (maxRetries == -1) maxRetries = DefaultRetries;
            var individualTimeout = timeout ?? ElementVisibilityTimeout;
            var retryTimeout = TimeSpan.FromSeconds(individualTimeout.TotalSeconds / maxRetries);

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return driver.WaitVisible(by, retryTimeout);
                }
                catch (WebDriverTimeoutException) when (i < maxRetries - 1)
                {
                    // Wait for page transitions using WebDriverWait instead of Thread.Sleep
                    WaitForPageTransition(driver, TimeSpan.FromSeconds(1));
                    TestLogger.LogDebug("Retry {0}/{1} for element: {2}", i + 1, maxRetries, by);
                }
                catch (StaleElementReferenceException) when (i < maxRetries - 1)
                {
                    // Wait for page to stabilize using WebDriverWait
                    WaitForPageTransition(driver, TimeSpan.FromSeconds(1.5));
                    TestLogger.LogDebug("Stale element retry {0}/{1} for: {2}", i + 1, maxRetries, by);
                }
            }

            return driver.WaitVisible(by, individualTimeout);
        }

        // Wait for page transition to complete
        public static bool WaitForPageTransition(this IWebDriver driver, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(driver, timeout ?? PageTransitionTimeout);
            try
            {
                // Wait for document ready state
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                // Wait for any pending AJAX requests to complete (if enabled)
                if (TestConfiguration.WaitForAjaxCompletion)
                {
                    try
                    {
                        wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return jQuery.active == 0").Equals(true));
                    }
                    catch (Exception)
                    {
                        // jQuery not available or AJAX detection failed, continue
                        TestLogger.LogDebug("AJAX completion detection not available, continuing");
                    }
                }
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                TestLogger.LogWarning("Page transition timeout after {0} seconds", PageTransitionTimeout.TotalSeconds);
                return false;
            }
        }

        // Wait for URL to change from a previous URL
        public static bool WaitForUrlChange(this IWebDriver driver, string previousUrl, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(driver, timeout ?? PageTransitionTimeout);
            try
            {
                return wait.Until(d => !d.Url.Equals(previousUrl, StringComparison.OrdinalIgnoreCase));
            }
            catch (WebDriverTimeoutException)
            {
                TestLogger.LogWarning("URL change timeout after {0} seconds", PageTransitionTimeout.TotalSeconds);
                return false;
            }
        }

        // Wait for URL to contain a specific part
        public static bool WaitForUrlContains(this IWebDriver driver, string urlPart, TimeSpan? timeout = null)
        {
            var wait = new WebDriverWait(driver, timeout ?? PageTransitionTimeout);
            try
            {
                return wait.Until(d => d.Url.Contains(urlPart, StringComparison.OrdinalIgnoreCase));
            }
            catch (WebDriverTimeoutException)
            {
                TestLogger.LogWarning("URL contains '{0}' timeout after {1} seconds", urlPart, PageTransitionTimeout.TotalSeconds);
                return false;
            }
        }

        // Generic retry mechanism for actions with WebDriverWait instead of Thread.Sleep
        public static void Retry(Action act, int times = -1)
        {
            if (times == -1) times = DefaultRetries;
            Exception last = null!;
            for (var i = 0; i < times; i++)
            {
                try { act(); return; }
                catch (Exception ex)
                {
                    last = ex;
                    if (i < times - 1)
                    {
                        // Use Task.Delay instead of Thread.Sleep for async compatibility
                        Task.Delay(RetryDelay).Wait();
                        TestLogger.LogDebug("Retry attempt {0}/{1} after {2}ms delay", i + 1, times, RetryDelay.TotalMilliseconds);
                    }
                }
            }
            TestLogger.LogError("All {0} retry attempts failed", times);
            throw last;
        }

        // Async version of retry mechanism
        public static async Task RetryAsync(Func<Task> act, int times = -1)
        {
            if (times == -1) times = DefaultRetries;
            Exception last = null!;
            for (var i = 0; i < times; i++)
            {
                try
                {
                    await act();
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (i < times - 1)
                    {
                        await Task.Delay(RetryDelay);
                        TestLogger.LogDebug("Async retry attempt {0}/{1} after {2}ms delay", i + 1, times, RetryDelay.TotalMilliseconds);
                    }
                }
            }
            TestLogger.LogError("All {0} async retry attempts failed", times);
            throw last;
        }
    }
}
