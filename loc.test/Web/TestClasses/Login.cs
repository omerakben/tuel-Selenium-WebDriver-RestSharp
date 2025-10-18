using loc.test.Web.PageObjectFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace loc.test.Web.TestClasses
{
    [TestClass]
    [TestCategory("UI")]
    [TestCategory("Smoke")]
    public class Login : Base
    {
        [TestMethod]
        public void Navigate_BaseUrl_AndServicePrincipalAuth_Succeeds()
        {
            Assert.IsNotNull(Driver, "Driver should have been initialized by the base class.");

            var dashboardPage = new DashboardPOM(Driver);

            // Service principal authentication is handled automatically in Base.cs
            // No need for Email/Password verification

            TestContext.WriteLine("Verifying service principal authentication success...");

            // Wait for dashboard to load after service principal authentication
            dashboardPage.WaitUntilPageIsLoaded();

            // Verify on the Letters of Credit application
            var currentUrl = Driver.Url;
            var pageTitle = Driver.Title;

            TestContext.WriteLine($"Post-authentication URL: {currentUrl}");
            TestContext.WriteLine($"Post-authentication Page Title: {pageTitle}");

            // Verify successful authentication by checking on the LOC application
            bool isOnApplication = currentUrl.Contains("as-badev-nc-loc-ui", StringComparison.OrdinalIgnoreCase) ||
                                 currentUrl.Contains("letters-of-credit", StringComparison.OrdinalIgnoreCase);

            bool hasValidTitle = pageTitle.Contains("Letters of Credit", StringComparison.OrdinalIgnoreCase);

            Assert.IsTrue(isOnApplication || hasValidTitle,
                $"Should be on Letters of Credit application after service principal authentication. URL: {currentUrl}, Title: {pageTitle}");

            TestContext.WriteLine("Service principal authentication verification completed successfully");
        }
    }
}