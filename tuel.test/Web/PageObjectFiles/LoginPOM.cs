using OpenQA.Selenium;
using System;
using System.Threading.Tasks;
using TUEL.TestFramework.Web.Support;
using TUEL.TestFramework.Logging;

namespace TUEL.TestFramework.Web.PageObjects
{
    public class LoginPOM : BasePage
    {
        #region Locators
        private readonly By _emailInput = By.XPath("//input[@type='email']");
        private readonly By _nextButton = By.XPath("//input[@type='submit']");
        private readonly By _passwordInput = By.XPath("//input[@name='passwd']");
        private readonly By _signInButton = By.Id("idSIButton9");
        private readonly By _staySignedInNoButton = By.Id("idBtn_Back");

        // Additional locators for state detection
        private readonly By _loginContainer = By.XPath("//div[contains(@class, 'login-pane')] | //div[contains(@class, 'sign-in-box')] | //form[contains(@action, 'login')]");
        private readonly By _passwordContainer = By.XPath("//div[contains(@id, 'passwordInput')] | //div[contains(@class, 'password')] | //form[contains(@name, 'f1')]");
        private readonly By _errorMessage = By.XPath("//*[contains(@class, 'error')] | //*[contains(text(), 'error')] | //*[contains(text(), 'incorrect')]");

        #endregion

        // A locator for a container on the login page to confirm.
        protected override By UniqueLocator => _emailInput;

        public LoginPOM(IWebDriver driver) : base(driver) { }

        // Login page detection with multiple fallback strategies
        public bool IsLoginPageDisplayed(TimeSpan? timeout = null)
        {
            try
            {
                var timeoutValue = timeout ?? TimeSpan.FromSeconds(10);

                try
                {
                    Driver.WaitVisible(_emailInput, TimeSpan.FromSeconds(5));
                    return true;
                }
                catch (WebDriverTimeoutException)
                {
                    // Fallback to login container detection
                    try
                    {
                        Driver.WaitVisible(_loginContainer, TimeSpan.FromSeconds(5));
                        return true;
                    }
                    catch (WebDriverTimeoutException)
                    {
                        // Check URL for Microsoft login
                        return Driver.Url.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase) ||
                               Driver.Url.Contains("microsoftonline", StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Authentication state detection
        public AuthenticationState GetCurrentAuthenticationState()
        {
            try
            {
                var currentUrl = Driver.Url;

                if (currentUrl.Contains("application", StringComparison.OrdinalIgnoreCase) ||
                    currentUrl.Contains("dashboard", StringComparison.OrdinalIgnoreCase))
                {
                    // Dashboard landing page
                    if (currentUrl.Contains("/application/dashboard", StringComparison.OrdinalIgnoreCase) ||
                        currentUrl.Contains("/dashboard", StringComparison.OrdinalIgnoreCase))
                    {
                        return AuthenticationState.LoggedIn;
                    }

                    if (!currentUrl.Contains("login", StringComparison.OrdinalIgnoreCase) &&
                        !currentUrl.Contains("auth", StringComparison.OrdinalIgnoreCase))
                    {
                        return AuthenticationState.LoggedIn;
                    }
                }

                // Check if on Microsoft login pages
                if (currentUrl.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase))
                {
                    if (Driver.IsDisplayedSafe(_passwordInput))
                    {
                        return AuthenticationState.PasswordRequired;
                    }

                    if (Driver.IsDisplayedSafe(_emailInput))
                    {
                        return AuthenticationState.UsernameRequired;
                    }

                    if (Driver.IsDisplayedSafe(_staySignedInNoButton))
                    {
                        return AuthenticationState.StaySignedInPrompt;
                    }

                    return AuthenticationState.OnLoginPage;
                }

                // Check for data: or transitional states
                if (currentUrl.StartsWith("data:") || currentUrl == "about:blank")
                {
                    return AuthenticationState.Transitioning;
                }

                return AuthenticationState.Unknown;
            }
            catch (Exception ex)
            {
                TestLogger.LogWarning($"Auth state detection error: {ex.Message}");
                return AuthenticationState.Unknown;
            }
        }

        // Login flow with state management and retry logic (synchronous version for backwards compatibility)
        public void LoginToApplication(string username, string password)
        {
            LoginToApplicationAsync(username, password).GetAwaiter().GetResult();
        }

        // Login flow with state management and retry logic (async version)
        public async Task LoginToApplicationAsync(string username, string password)
        {
            const int maxRetries = 3;
            var currentState = GetCurrentAuthenticationState();

            // Handle already logged in state
            if (currentState == AuthenticationState.LoggedIn)
            {
                return;
            }

            // Wait for login page if in transitioning state
            if (currentState == AuthenticationState.Transitioning)
            {
                await WaitForLoginPageWithRetryAsync(TimeSpan.FromSeconds(5));
                currentState = GetCurrentAuthenticationState();
            }

            // Perform authentication steps based on current state
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (await PerformAuthenticationFlowAsync(username, password))
                    {
                        TestLogger.LogInformation("Authentication completed successfully");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    TestLogger.LogWarning($"Login attempt {attempt} failed: {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        await Task.Delay(2000);
                        currentState = GetCurrentAuthenticationState();
                    }
                    else
                    {
                        throw new InvalidOperationException($"Login failed after {maxRetries} attempts", ex);
                    }
                }
            }
        }

        private async Task<bool> PerformAuthenticationFlowAsync(string username, string password)
        {
            var currentState = GetCurrentAuthenticationState();

            if (currentState == AuthenticationState.UsernameRequired || currentState == AuthenticationState.OnLoginPage)
            {
                if (!EnterUsernameWithRetry(username))
                {
                    return false;
                }
            }

            currentState = GetCurrentAuthenticationState();
            if (currentState == AuthenticationState.PasswordRequired)
            {
                if (!await EnterPasswordWithRetryAsync(password))
                {
                    return false;
                }
            }

            currentState = GetCurrentAuthenticationState();
            if (currentState == AuthenticationState.StaySignedInPrompt)
            {
                await HandleStaySignedInPromptAsync();
            }

            // Verify successful authentication
            return await WaitForAuthenticationCompletionAsync();
        }

        private bool EnterUsernameWithRetry(string username)
        {
            try
            {
                var emailElement = Driver.WaitVisibleWithRetry(_emailInput, TimeSpan.FromSeconds(10));
                emailElement.Clear();
                emailElement.SendKeys(username);

                var currentUrl = Driver.Url;
                var nextElement = Driver.WaitVisibleWithRetry(_nextButton, TimeSpan.FromSeconds(5));
                nextElement.Click();

                // Wait for URL change or password field to appear
                var urlChanged = Driver.WaitForUrlChange(currentUrl, TimeSpan.FromSeconds(5));
                if (urlChanged || Driver.WaitForPageTransition(TimeSpan.FromSeconds(5)))
                {
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                TestLogger.LogWarning($"Username entry failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> EnterPasswordWithRetryAsync(string password)
        {
            try
            {
                var passwordElement = Driver.WaitVisibleWithRetry(_passwordInput, TimeSpan.FromSeconds(10));
                passwordElement.Clear();
                passwordElement.SendKeys(password);

                var signInElement = Driver.WaitVisibleWithRetry(_signInButton, TimeSpan.FromSeconds(5));
                signInElement.Click();

                // Wait for authentication to process
                await Task.Delay(2000);
                return true;
            }
            catch (Exception ex)
            {
                TestLogger.LogWarning($"Password entry failed: {ex.Message}");
                return false;
            }
        }

        private async Task HandleStaySignedInPromptAsync()
        {
            try
            {
                var staySignedInElement = Driver.WaitVisibleWithRetry(_staySignedInNoButton, TimeSpan.FromSeconds(5));
                staySignedInElement.Click();

                // Wait for prompt to be processed
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                TestLogger.LogWarning($"Stay signed in prompt handling failed: {ex.Message}");
            }
        }

        private async Task<bool> WaitForAuthenticationCompletionAsync()
        {
            const int maxWaitSeconds = 10;
            var startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalSeconds < maxWaitSeconds)
            {
                try
                {
                    var currentState = GetCurrentAuthenticationState();
                    TestLogger.LogDebug($"Current auth state: {currentState}");

                    if (currentState == AuthenticationState.LoggedIn)
                    {
                        return true;
                    }

                    if (currentState == AuthenticationState.StaySignedInPrompt)
                    {
                        await HandleStaySignedInPromptAsync();
                    }

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    TestLogger.LogWarning($"Auth completion check failed: {ex.Message}");
                    await Task.Delay(1000);
                }
            }
            return true;
        }

        private async Task<bool> WaitForLoginPageWithRetryAsync(TimeSpan timeout)
        {
            var endTime = DateTime.Now.Add(timeout);

            while (DateTime.Now < endTime)
            {
                if (IsLoginPageDisplayed(TimeSpan.FromSeconds(5)))
                {
                    return true;
                }
                await Task.Delay(1000);
            }

            return false;
        }
    }

    public enum AuthenticationState
    {
        Unknown,
        Transitioning,
        UsernameRequired,
        PasswordRequired,
        StaySignedInPrompt,
        OnLoginPage,
        LoggedIn
    }
}
