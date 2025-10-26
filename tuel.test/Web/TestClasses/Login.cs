using TUEL.TestFramework;
using TUEL.TestFramework.Web.PageObjects;
using TUEL.TestFramework.Web.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Threading;

namespace TUEL.TestFramework.Web.TestClasses
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

            // Verify on the Application
            var currentUrl = Driver.Url;
            var pageTitle = Driver.Title;

            TestContext.WriteLine($"Post-authentication URL: {currentUrl}");
            TestContext.WriteLine($"Post-authentication Page Title: {pageTitle}");

            // Verify successful authentication by checking on the Application
            bool isOnApplication = currentUrl.Contains("application", StringComparison.OrdinalIgnoreCase) ||
                                 currentUrl.Contains("dashboard", StringComparison.OrdinalIgnoreCase);

            bool hasValidTitle = pageTitle.Contains("Application", StringComparison.OrdinalIgnoreCase);

            Assert.IsTrue(isOnApplication || hasValidTitle,
                $"Should be on Application after service principal authentication. URL: {currentUrl}, Title: {pageTitle}");

            TestContext.WriteLine("Service principal authentication verification completed successfully");
        }
    }
}
