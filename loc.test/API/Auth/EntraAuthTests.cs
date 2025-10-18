using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System;

namespace TUEL.TestFramework.API.Auth
{
    [TestClass]
    [TestCategory("AuthenticationTests")]
    public class EntraAuthTests
    {
        // Gets or sets the test context which provides information about
        // and functionality for the current test run.
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInit()
        {
            // TestConfig is initialized by AssemblyInitializer.
            Console.WriteLine($"Executing EntraAuthTest: {TestContext?.TestName}. Environment: {InitializeTestAssembly.ENV}");
            EntraAuthHelper.ClearCache(); // Clear cache before each auth test
        }

        [TestMethod]
        [TestCategory("Authentication")]
        public async Task GetAccessToken_ShouldReturnValidToken()
        {
            Console.WriteLine("Attempting to acquire access token...");
            string accessToken = string.Empty;

            try
            {
                accessToken = await EntraAuthHelper.GetAccessTokenAsync();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Token acquisition failed with exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            Assert.IsFalse(string.IsNullOrEmpty(accessToken), "Access token should not be null or empty.");
            Console.WriteLine("Access token acquired successfully (first attempt).");

            // Call again to test caching
            string cachedToken = string.Empty;
            try
            {
                cachedToken = await EntraAuthHelper.GetAccessTokenAsync();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Cached token acquisition failed with exception: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            Assert.AreEqual(accessToken, cachedToken, "Cached token should be the same as the initially acquired token.");
            Console.WriteLine("Cached token retrieved successfully and matches the original.");
        }

        [TestMethod]
        [TestCategory("Authentication")]
        public async Task GetAccessToken_AfterCacheClear_ShouldReturnNewTokenOrSameIfStillValid()
        {
            // First acquisition
            string token1 = await EntraAuthHelper.GetAccessTokenAsync();
            Assert.IsFalse(string.IsNullOrEmpty(token1), "First token should not be null or empty.");
            Console.WriteLine("First token acquired.");

            // Clear cache
            EntraAuthHelper.ClearCache();
            Console.WriteLine("Token cache cleared.");

            // Second acquisition after cache clear
            string token2 = string.Empty;
            try
            {
                token2 = await EntraAuthHelper.GetAccessTokenAsync();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Token acquisition after cache clear failed: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

            Assert.IsFalse(string.IsNullOrEmpty(token2), "Second token (after cache clear) should not be null or empty.");
            Console.WriteLine("Second token acquired after cache clear.");

            // The primary check is that a valid token is returned.
            Assert.IsTrue(token2.Split('.').Length == 3, "Second token does not appear to be in a valid JWT format.");
        }

        [TestMethod]
        [TestCategory("Authentication")]
        public void GetAuthenticationInfo_ShouldReturnConfiguredDetails()
        {
            // This test primarily verifies that the diagnostic information can be retrieved
            // and contains some expected configuration values.
            string authInfo = EntraAuthHelper.GetAuthenticationInfoForTestContext();

            Assert.IsFalse(string.IsNullOrEmpty(authInfo), "Authentication info string should not be null or empty.");
            Console.WriteLine("Authentication Info:\n" + authInfo);

            // Check for presence of key configuration details (using TestConfig for verification)
            StringAssert.Contains(authInfo, InitializeTestAssembly.ENV, "Auth info should contain the Environment.");
            StringAssert.Contains(authInfo, InitializeTestAssembly.EntraIdTenantId, "Auth info should contain the Tenant ID.");
            StringAssert.Contains(authInfo, InitializeTestAssembly.EntraIdClientId, "Auth info should contain the Client ID.");
            StringAssert.Contains(authInfo, InitializeTestAssembly.EntraIdApiScope, "Auth info should contain the API Scope.");

            if (!string.IsNullOrEmpty(InitializeTestAssembly.Email))
            {
                StringAssert.Contains(authInfo, InitializeTestAssembly.Email, "Auth info should contain the configured email if ROPC is implied.");
            }
        }
    }
}
