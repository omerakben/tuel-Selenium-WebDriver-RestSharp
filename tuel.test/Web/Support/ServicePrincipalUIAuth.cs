using TUEL.TestFramework.API.Auth;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUEL.TestFramework.Web.Support
{
    // Handles programmatic authentication for UI tests using service principal (same as API tests)
    // This eliminates the need for user credentials and interactive login
    public static class ServicePrincipalUIAuth
    {
        // Authenticates UI tests using service principal - bypasses interactive login
        // Uses the same EntraAuthHelper as API tests (Client Credentials flow)
        public static async Task<bool> AuthenticateUIWithServicePrincipalAsync(IWebDriver driver)
        {
            try
            {
                // Get access token using the same method as API tests
                var accessToken = await EntraAuthHelper.GetAccessTokenAsync();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return false;
                }

                // Navigate to the application first (required for localStorage injection)
                driver.Navigate().GoToUrl(InitializeTestAssembly.UiUrl);

                // Wait a moment for page to load
                await Task.Delay(2000);

                // Inject the token into browser storage
                InjectAuthenticationToken(driver, accessToken);

                // Refresh to apply authentication
                driver.Navigate().Refresh();

                // Wait for page to process authentication
                await Task.Delay(3000);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Injects the access token into browser storage to bypass interactive login
        private static void InjectAuthenticationToken(IWebDriver driver, string accessToken)
        {
            var jsExecutor = (IJavaScriptExecutor)driver;

            // Inject token into localStorage (common pattern for SPAs)
            var script = $@"
                localStorage.setItem('access_token', '{accessToken}');
                localStorage.setItem('token_type', 'Bearer');
                localStorage.setItem('authenticated', 'true');
                
                // Also set in sessionStorage as backup
                sessionStorage.setItem('access_token', '{accessToken}');
                sessionStorage.setItem('token_type', 'Bearer');
                sessionStorage.setItem('authenticated', 'true');
                
                console.log('Authentication tokens injected successfully');
            ";

            jsExecutor.ExecuteScript(script);
        }

        // Checks if the current page indicates successful authentication
        public static bool IsAuthenticationSuccessful(IWebDriver driver)
        {
            try
            {
                var currentUrl = driver.Url;

                // Check if we're on a protected page (not login page)
                var isOnLoginPage = currentUrl.Contains("login", StringComparison.OrdinalIgnoreCase) ||
                                   currentUrl.Contains("signin", StringComparison.OrdinalIgnoreCase) ||
                                   currentUrl.Contains("microsoft", StringComparison.OrdinalIgnoreCase);

                if (isOnLoginPage)
                {
                    return false;
                }

                // Check for typical authenticated page indicators
                var hasAuthenticatedContent = driver.PageSource.Contains("TUEL Records") ||
                                            driver.PageSource.Contains("Dashboard") ||
                                            driver.PageSource.Contains("logout", StringComparison.OrdinalIgnoreCase);

                return hasAuthenticatedContent;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
