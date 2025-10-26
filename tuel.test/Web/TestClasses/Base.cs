using TUEL.TestFramework;
using TUEL.TestFramework.Web.Support;
using TUEL.TestFramework.Web.PageObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Threading.Tasks;

namespace TUEL.TestFramework.Web.TestClasses
{
    // Serves as the base class for all UI test classes.
    // It handles the creation and disposal of the WebDriver for each test.
    [TestClass]
    public abstract class Base
    {
        // It is initialized before each test and disposed of after each test.
        protected IWebDriver? Driver { get; private set; }

        // Gets or sets the test context which provides information
        public TestContext TestContext { get; set; }

        // Runs before each test method. Initializes the WebDriver and navigates to the base URL.
        [TestInitialize]
        public void TestSetup()
        {
            TestContext.WriteLine($"Starting Web test: {TestContext.TestName}. Environment: {InitializeTestAssembly.ENV}");

            Driver = InitializeTestAssembly.CreateWebDriver();

            if (Driver == null)
            {
                throw new InvalidOperationException("Driver creation failed");
            }

            if (string.IsNullOrEmpty(InitializeTestAssembly.UiUrl))
            {
                throw new InvalidOperationException("UI Url ('BaseURL') is not configured in the .runsettings file.");
            }

            NavigateToApplicationWithRetryAsync().GetAwaiter().GetResult();

            // Perform login using LoginPOM ROPC
            PerformLoginAsync().GetAwaiter().GetResult();
        }

        private async Task PerformLoginAsync()
        {
            try
            {
                TestContext.WriteLine("Starting Login Process");

                if (Driver == null)
                {
                    throw new InvalidOperationException("Driver is not initialized");
                }

                var loginPOM = new LoginPOM(Driver);

                // Check if already on the login page or need to navigate
                var currentState = loginPOM.GetCurrentAuthenticationState();
                TestContext.WriteLine($"Current authentication state: {currentState}");

                if (currentState == AuthenticationState.LoggedIn)
                {
                    TestContext.WriteLine("Already logged in no authentication needed");
                    return;
                }

                // Wait for login page if needed
                if (!loginPOM.IsLoginPageDisplayed(TimeSpan.FromSeconds(10)))
                {
                    TestContext.WriteLine("Login page waiting for redirect...");
                    await Task.Delay(2_000); // Allow time for redirect to login page
                }

                // Get credentials from configuration
                var username = InitializeTestAssembly.Email;
                var password = InitializeTestAssembly.Password;

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    throw new InvalidOperationException("Username or password not configured in .runsettings");
                }

                TestContext.WriteLine($"Attempting login with username(Email): {username}");
                await loginPOM.LoginToApplicationAsync(username, password);

                TestContext.WriteLine("Login process completed");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Error during login: {ex.Message}");
                TestContext.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Login failed: {ex.Message}", ex);
            }
        }

        private async Task NavigateToApplicationWithRetryAsync()
        {
            const int maxAttempts = 3;
            Exception? lastException = null;

            if (Driver == null)
            {
                throw new InvalidOperationException("Driver is not initialized");
            }

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    TestContext.WriteLine($"Navigation attempt {attempt}/{maxAttempts} to: {InitializeTestAssembly.UiUrl}");
                    var startTime = DateTime.Now;

                    Driver.Navigate().GoToUrl(InitializeTestAssembly.UiUrl);

                    if (await WaitForPageStabilizationAsync())
                    {
                        var loadTime = DateTime.Now - startTime;
                        TestContext.WriteLine($"Page navigation completed in: {loadTime.TotalSeconds:F2} seconds");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    TestContext.WriteLine($"Navigation attempt {attempt} failed: {ex.Message}");

                    if (attempt < maxAttempts)
                    {
                        TestContext.WriteLine($"Waiting before retry attempt {attempt + 1}...");
                        await Task.Delay(2000);
                    }
                }
            }

            throw new InvalidOperationException($"Failed to navigate to application after {maxAttempts} attempts. Last error: {lastException?.Message}", lastException);
        }

        private async Task<bool> WaitForPageStabilizationAsync()
        {
            const int maxWaitSeconds = 10;
            var startTime = DateTime.Now;

            if (Driver == null) return false;

            while ((DateTime.Now - startTime).TotalSeconds < maxWaitSeconds)
            {
                try
                {
                    var currentUrl = Driver.Url;

                    // Handle data: URL - wait for redirect
                    if (currentUrl.StartsWith("data:"))
                    {
                        await Task.Delay(1000);
                        continue;
                    }

                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));

                    if (IsPageStable())
                    {
                        return true;
                    }

                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Page stabilization check failed: {ex.Message}");
                    await Task.Delay(1000);
                }
            }

            return false;
        }

        private bool IsPageStable()
        {
            try
            {
                if (Driver == null) return false;

                var currentUrl = Driver.Url;

                // Check page (not data: or blank)
                if (string.IsNullOrEmpty(currentUrl) || currentUrl.StartsWith("data:") || currentUrl == "about:blank")
                {
                    return false;
                }

                var hasTitle = !string.IsNullOrEmpty(Driver.Title);
                var hasBody = Driver.FindElements(By.TagName("body")).Count > 0;

                return hasTitle && hasBody;
            }
            catch
            {
                return false;
            }
        }

        // Runs after each test method. Cleans up by closing and quitting the WebDriver.
        [TestCleanup]
        public void TestCleanup()
        {
            try
            {
                TestContext.WriteLine($"Finished Web test: {TestContext.TestName}");
                var shouldReuse = TestContext?.CurrentTestOutcome == UnitTestOutcome.Passed;
                WebDriverLifecycleManager.ReleaseDriver(Driver, shouldReuse);
                Driver = null;
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"An error occurred during WebDriver cleanup: {ex.Message}");
            }
        }
    }
}
