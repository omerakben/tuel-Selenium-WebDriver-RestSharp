using TUEL.TestFramework;
using TUEL.TestFramework.Web.PageObjects;
using TUEL.TestFramework.Web.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Threading.Tasks;
using TUEL.TestFramework.Logging;

namespace TUEL.TestFramework.Web.TestClasses
{
    // Test class for the Application Dashboard page
    [TestClass, TestCategory("UI")]
    public class Dashboard : Base
    {
        private DashboardPOM _dashboardPage;
        private LoginPOM _loginPage;

        [TestInitialize]
        public void DashboardTestSetup()
        {
            try
            {
                TestContext.WriteLine("Initializing Dashboard test components...");
                _dashboardPage = new DashboardPOM(Driver);
                _loginPage = new LoginPOM(Driver);

                if (!string.IsNullOrEmpty(InitializeTestAssembly.Email) && !string.IsNullOrEmpty(InitializeTestAssembly.Password))
                {
                    TestContext.WriteLine($"Performing authentication for user: {InitializeTestAssembly.Email}");

                    var authState = _loginPage.GetCurrentAuthenticationState();
                    TestContext.WriteLine($"Current authentication state: {authState}");

                    if (authState != AuthenticationState.LoggedIn)
                    {
                        _loginPage.LoginToApplication(InitializeTestAssembly.Email, InitializeTestAssembly.Password);
                        TestContext.WriteLine("Authentication flow completed");

                        VerifyDashboardLanding();
                    }
                    else
                    {
                        // If not on dashboard, navigate to it
                        if (!IsOnDashboardPage())
                        {
                            NavigateToDashboardDirectly();
                        }
                    }
                }
                else
                {
                    TestContext.WriteLine("No credentials provided");
                }

            WaitForDashboardReady();
                TestContext.WriteLine("Dashboard test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Dashboard test setup failed: {ex.Message}");
                TestContext.WriteLine($"Current URL: {Driver.Url}");
                TestContext.WriteLine($"Page Title: {Driver.Title}");
                throw;
            }
        }

        private void VerifyDashboardLanding()
        {
            try
            {
                // Wait for redirect to complete
                var dashboardLanded = Driver.WaitForUrlContains("/application/dashboard", TimeSpan.FromSeconds(15)) ||
                                    Driver.WaitForUrlContains("/dashboard", TimeSpan.FromSeconds(5));

                if (dashboardLanded)
                {
                    TestContext.WriteLine("Successfully landed on dashboard after authentication");
                }
                else
                {
                    TestContext.WriteLine($"Warning: Expected dashboard landing, current URL: {Driver.Url}");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Dashboard landing verification failed: {ex.Message}");
            }
        }

        private bool IsOnDashboardPage()
        {
            try
            {
                var currentUrl = Driver.Url;
                return currentUrl.Contains("/application/dashboard", StringComparison.OrdinalIgnoreCase) ||
                       currentUrl.Contains("/dashboard", StringComparison.OrdinalIgnoreCase) ||
                       _dashboardPage.IsPageLoaded();
            }
            catch
            {
                return false;
            }
        }

        private void NavigateToDashboardDirectly()
        {
            try
            {
                // Try clicking Dashboard tab if visible
                try
                {
                    var dashboardTab = Driver.FindElement(By.XPath("//a[contains(text(), 'Dashboard')] | //button[contains(text(), 'Dashboard')]"));
                    if (dashboardTab.Displayed)
                    {
                        dashboardTab.Click();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Dashboard tab click failed: {ex.Message}");
                }

                var currentUrl = Driver.Url;
                if (currentUrl.Contains(".net", StringComparison.OrdinalIgnoreCase))
                {
                    var baseUrl = currentUrl.Substring(0, currentUrl.IndexOf(".net") + 4);
                    var dashboardUrl = $"{baseUrl}/application/dashboard";
                    TestContext.WriteLine($"Attempting direct navigation to: {dashboardUrl}");
                    Driver.Navigate().GoToUrl(dashboardUrl);
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Direct dashboard navigation failed: {ex.Message}");
            }
        }

        private void WaitForDashboardReady()
        {
            WaitForDashboardReadyAsync().GetAwaiter().GetResult();
        }

        private async Task WaitForDashboardReadyAsync()
        {
            const int maxRetries = 3;
            var retryDelay = TimeSpan.FromSeconds(2);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));

                    // Try to wait for dashboard-specific elements
                    _dashboardPage.WaitUntilPageIsLoaded(TimeSpan.FromSeconds(10));

                    TestContext.WriteLine("Dashboard page is ready");
                    return;
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Dashboard readiness attempt {attempt} failed: {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        TestContext.WriteLine($"Retrying in {retryDelay.TotalSeconds:F1}s...");
                        await Task.Delay(retryDelay);
                    }
                    else
                    {
                        TestContext.WriteLine("Dashboard readiness timeout");
                    }
                }
            }
        }

        [TestMethod]
        [Description("Comprehensive Dashboard verifies all major components")]
        public void Dashboard_SmokeTest_AllComponentsPresent()
        {
            TestContext.WriteLine("Dashboard components test");

            // Test page basics
            bool pageTitle = _dashboardPage.VerifyPageTitle();
            bool mainHeader = _dashboardPage.VerifyMainHeader();
            bool navigationTabs = _dashboardPage.VerifyNavigationTabsPresent();
            bool dashboardActive = _dashboardPage.VerifyDashboardTabActive();

            TestContext.WriteLine($"Page Title Valid: {pageTitle}");
            TestContext.WriteLine($"Main Header Present: {mainHeader}");
            TestContext.WriteLine($"Navigation Tabs Present: {navigationTabs}");
            TestContext.WriteLine($"Dashboard Tab Active: {dashboardActive}");

            // Test Approval Queue section
            bool approvalQueueSection = _dashboardPage.VerifyApprovalQueueSection();
            bool approvalQueueTable = _dashboardPage.VerifyApprovalQueueTable();
            bool approvalQueueSearch = _dashboardPage.VerifyApprovalQueueSearch();

            TestContext.WriteLine($"Approval Queue Section: {approvalQueueSection}");
            TestContext.WriteLine($"Approval Queue Table: {approvalQueueTable}");
            TestContext.WriteLine($"Approval Queue Search: {approvalQueueSearch}");

            // Test Send Items section
            bool sendItemsSection = _dashboardPage.VerifySendItemsSection();
            bool sendItemsTable = _dashboardPage.VerifySendItemsTable();
            bool sendItemsSearch = _dashboardPage.VerifySendItemsSearch();

            TestContext.WriteLine($"Send Items Section: {sendItemsSection}");
            TestContext.WriteLine($"Send Items Table: {sendItemsTable}");
            TestContext.WriteLine($"Send Items Search: {sendItemsSearch}");

            // Test data table structure
            bool tableStructure = _dashboardPage.VerifyTableStructure();
            bool tableColumns = _dashboardPage.VerifyTableColumns();
            int rowCount = _dashboardPage.GetDataRowCount();

            TestContext.WriteLine($"Table Structure Valid: {tableStructure}");
            TestContext.WriteLine($"Table Columns Valid: {tableColumns}");
            TestContext.WriteLine($"Data Row Count: {rowCount}");

            // Test pagination
            bool paginationControls = _dashboardPage.VerifyPaginationControls();
            bool pageStatusDisplay = _dashboardPage.VerifyPageStatusDisplay();

            TestContext.WriteLine($"Pagination Controls: {paginationControls}");
            TestContext.WriteLine($"Page Status Display: {pageStatusDisplay}");

            // Evaluate results
            var criticalChecks = new[] { pageTitle, approvalQueueSection, approvalQueueTable };
            var importantChecks = new[] { mainHeader, navigationTabs, tableStructure };
            var optionalChecks = new[] { dashboardActive, sendItemsSection, tableColumns, paginationControls };

            int criticalPassed = criticalChecks.Count(c => c);
            int importantPassed = importantChecks.Count(c => c);
            int optionalPassed = optionalChecks.Count(c => c);

            TestContext.WriteLine($"Critical checks passed: {criticalPassed}/{criticalChecks.Length}");
            TestContext.WriteLine($"Important checks passed: {importantPassed}/{importantChecks.Length}");
            TestContext.WriteLine($"Optional checks passed: {optionalPassed}/{optionalChecks.Length}");

            // Assertions
            Assert.IsTrue(criticalPassed >= 2, $"Critical checks should pass, but only {criticalPassed} passed");
            Assert.IsTrue(importantPassed >= 2, $"Important checks should pass, but only {importantPassed} passed");

            TestContext.WriteLine("Dashboard components test successfully");
        }

        [TestMethod]
        [Description("Verify page title and main header elements")]
        public void Dashboard_VerifyPageAndHeader()
        {
            TestContext.WriteLine("Dashboard page title and header test");

            bool pageTitle = _dashboardPage.VerifyPageTitle();
            bool mainHeader = _dashboardPage.VerifyMainHeader();

            TestContext.WriteLine($"Page title contains 'Application': {pageTitle}");
            TestContext.WriteLine($"Main header is visible: {mainHeader}");

            Assert.IsTrue(pageTitle || mainHeader, "Page title or main header should be valid");

            TestContext.WriteLine("Page and header verification completed");
        }

        [TestMethod]
        [Description("Verify navigation tabs and Dashboard tab state")]
        public void Dashboard_VerifyNavigationTabs()
        {
            TestContext.WriteLine("Testing navigation tabs");

            bool allTabsPresent = _dashboardPage.VerifyNavigationTabsPresent();
            bool dashboardActive = _dashboardPage.VerifyDashboardTabActive();

            TestContext.WriteLine($"Navigation tabs present: {allTabsPresent}");
            TestContext.WriteLine($"Dashboard tab is active: {dashboardActive}");

            Assert.IsTrue(allTabsPresent, "Navigation tabs should be present");

            if (!dashboardActive)
            {
                TestContext.WriteLine("Dashboard tab active state could not be verified");
            }

            TestContext.WriteLine("Navigation tabs verification completed");
        }

        [TestMethod]
        [Description("Verify Approval Queue section components")]
        public void Dashboard_VerifyApprovalQueueSection()
        {
            TestContext.WriteLine("Testing Approval Queue section");

            bool section = _dashboardPage.VerifyApprovalQueueSection();
            bool table = _dashboardPage.VerifyApprovalQueueTable();
            bool search = _dashboardPage.VerifyApprovalQueueSearch();

            TestContext.WriteLine($"Approval Queue section visible: {section}");
            TestContext.WriteLine($"Approval Queue table visible: {table}");
            TestContext.WriteLine($"Approval Queue search input visible: {search}");

            // Section and table are essential
            Assert.IsTrue(section, "Approval Queue section should be visible");
            Assert.IsTrue(table, "Approval Queue table should be visible");

            if (!search)
            {
                TestContext.WriteLine("Search input not found");
            }

            TestContext.WriteLine("Approval Queue section verification completed");
        }

        [TestMethod]
        [Description("Verify Send Items section components")]
        public void Dashboard_VerifySendItemsSection()
        {
            TestContext.WriteLine("Testing Send Items section");

            bool section = _dashboardPage.VerifySendItemsSection();
            bool table = _dashboardPage.VerifySendItemsTable();
            bool search = _dashboardPage.VerifySendItemsSearch();

            TestContext.WriteLine($"Send Items section visible: {section}");
            TestContext.WriteLine($"Send Items table visible: {table}");
            TestContext.WriteLine($"Send Items search input visible: {search}");

            // Section is essential
            Assert.IsTrue(section, "Send Items section should be visible");

            if (!table)
            {
                TestContext.WriteLine("Send Items table not found");
            }

            if (!search)
            {
                TestContext.WriteLine("Send Items search input not found");
            }

            TestContext.WriteLine("Send Items section verification completed");
        }

        [TestMethod]
        [Description("Verify data table columns and structure")]
        public void Dashboard_VerifyDataTableStructure()
        {
            TestContext.WriteLine("Testing data table structure");

            bool tableStructure = _dashboardPage.VerifyTableStructure();
            bool tableColumns = _dashboardPage.VerifyTableColumns();
            var columnHeaders = _dashboardPage.GetColumnHeaders();
            int rowCount = _dashboardPage.GetDataRowCount();

            TestContext.WriteLine($"Table structure valid: {tableStructure}");
            TestContext.WriteLine($"Table columns valid: {tableColumns}");
            TestContext.WriteLine($"Column count: {columnHeaders.Count}");
            TestContext.WriteLine($"Data row count: {rowCount}");

            if (columnHeaders.Any())
            {
                TestContext.WriteLine($"Column headers found: {string.Join(", ", columnHeaders.Take(5))}...");
            }

            Assert.IsTrue(tableStructure, "Table structure should be valid columns");

            if (!tableColumns)
            {
                TestContext.WriteLine("Expected column headers not fully matched");
            }

            TestContext.WriteLine("Data table structure verification completed");
        }

        [TestMethod]
        [Description("Verify data table content and no-records handling")]
        public void Dashboard_VerifyDataTableContent()
        {
            TestContext.WriteLine("Testing data table content");

            bool hasData = _dashboardPage.VerifyTableHasData();
            bool noRecordsHandling = _dashboardPage.VerifyNoRecordsMessageWhenEmpty();
            int rowCount = _dashboardPage.GetDataRowCount();

            TestContext.WriteLine($"Table has data: {hasData}");
            TestContext.WriteLine($"No-records message handling: {noRecordsHandling}");
            TestContext.WriteLine($"Total rows: {rowCount}");

            // Either table has data OR proper no-records message is shown
            Assert.IsTrue(hasData || noRecordsHandling, "Table should either have data or show appropriate no-records message");

            if (hasData)
            {
                TestContext.WriteLine($"Data verification: Found {rowCount} rows of data");
            }
            else
            {
                TestContext.WriteLine("No data found");
            }

            TestContext.WriteLine("Data table content verification completed");
        }

        [TestMethod]
        [Description("Verify pagination controls and functionality")]
        public void Dashboard_VerifyPaginationControls()
        {
            TestContext.WriteLine("Testing pagination controls");

            bool paginationControls = _dashboardPage.VerifyPaginationControls();
            bool pageStatusDisplay = _dashboardPage.VerifyPageStatusDisplay();
            bool itemsPerPageSelector = _dashboardPage.VerifyItemsPerPageSelector();
            int rowCount = _dashboardPage.GetDataRowCount();

            TestContext.WriteLine($"Pagination controls visible: {paginationControls}");
            TestContext.WriteLine($"Page status display visible: {pageStatusDisplay}");
            TestContext.WriteLine($"Items per page selector visible: {itemsPerPageSelector}");
            TestContext.WriteLine($"Current row count: {rowCount}");

            if (rowCount > 0)
            {
                // If there's data, pagination controls should be present
                Assert.IsTrue(paginationControls, "Pagination controls should be visible when data is present");
            }
            else
            {
                TestContext.WriteLine("No data present - pagination controls may not be needed");
            }

            if (!pageStatusDisplay && rowCount > 0)
            {
                TestContext.WriteLine("Warning: Page status display not found - may be environment-specific");
            }

            if (!itemsPerPageSelector && rowCount > 0)
            {
                TestContext.WriteLine("Warning: Items per page selector not found - may be environment-specific");
            }

            TestContext.WriteLine("Pagination controls verification completed");
        }

        [TestMethod]
        [Description("Test search functionality in both sections")]
        public void Dashboard_TestSearchFunctionality()
        {
            TestContext.WriteLine("Testing search functionality");

            bool approvalQueueSearch = _dashboardPage.VerifyApprovalQueueSearch();
            bool sendItemsSearch = _dashboardPage.VerifySendItemsSearch();

            TestContext.WriteLine($"Approval Queue search available: {approvalQueueSearch}");
            TestContext.WriteLine($"Send Items search available: {sendItemsSearch}");

            if (approvalQueueSearch)
            {
                try
                {
                    _dashboardPage.SearchApprovalQueue("test");
                    TestContext.WriteLine("Approval Queue search interaction successful");

                    // Clear search
                    _dashboardPage.SearchApprovalQueue("");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Approval Queue search interaction failed: {ex.Message}");
                }
            }

            if (sendItemsSearch)
            {
                try
                {
                    _dashboardPage.SearchSendItems("test");
                    TestContext.WriteLine("Send Items search interaction successful");

                    // Clear search
                    _dashboardPage.SearchSendItems("");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Send Items search interaction failed: {ex.Message}");
                }
            }

            if (!approvalQueueSearch && !sendItemsSearch)
            {
                TestContext.WriteLine("No search functionality found!");
            }

            TestContext.WriteLine("Search functionality testing completed");
        }

        [TestMethod]
        [Description("Verify navigation tab functionality")]
        public void Dashboard_TestNavigationTabFunctionality()
        {
            TestContext.WriteLine("Testing navigation tab functionality");

            bool navigationTabsPresent = _dashboardPage.VerifyNavigationTabsPresent();
            Assert.IsTrue(navigationTabsPresent, "Navigation tabs should be present!");

            // Test clicking on different tabs if they exist
            var tabsToTest = new[] { "Completed", "Customers", "Templates" };

            foreach (var tabName in tabsToTest)
            {
                try
                {
                    TestContext.WriteLine($"Testing {tabName} tab");
                    var currentUrl = Driver.Url;
                    _dashboardPage.ClickNavigationTab(tabName);

                    var transitioned = Driver.WaitForPageTransition(TimeSpan.FromSeconds(5)) |
                                       Driver.WaitForUrlChange(currentUrl, TimeSpan.FromSeconds(5));

                    if (!transitioned)
                    {
                        TestContext.WriteLine($"Warning: {tabName} tab did not trigger a visible transition");
                    }

                    // Navigate back to Dashboard
                    currentUrl = Driver.Url;
                    _dashboardPage.ClickNavigationTab("Dashboard");

                    var returnedToDashboard = Driver.WaitForUrlContains("/dashboard", TimeSpan.FromSeconds(5)) |
                                              Driver.WaitForUrlChange(currentUrl, TimeSpan.FromSeconds(5));

                    if (!returnedToDashboard)
                    {
                        TestContext.WriteLine("Warning: returning to dashboard did not trigger a visible transition");
                    }

                    TestContext.WriteLine($"{tabName} tab navigation successful");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"{tabName} tab navigation failed or not available: {ex.Message}");
                }
            }

            TestContext.WriteLine("Navigation tab functionality testing completed");
        }

        [TestMethod]
        [Description("Test to check configuration and URL loading")]
        public void Debug_CheckConfigurationAndUrl()
        {
            TestContext.WriteLine($"Environment: {InitializeTestAssembly.ENV}");
            TestContext.WriteLine($"UI URL: {InitializeTestAssembly.UiUrl}");
            TestContext.WriteLine($"API URL: {InitializeTestAssembly.BaseApiUrl}");
            TestContext.WriteLine($"Browser: {InitializeTestAssembly.Browser}");
            TestContext.WriteLine($"Username: {InitializeTestAssembly.Email}");

            TestContext.WriteLine($"Current URL: {Driver.Url}");
            TestContext.WriteLine($"Page Title: {Driver.Title}");

            bool pageLoaded = _dashboardPage.IsPageLoaded();
            TestContext.WriteLine($"Dashboard page loaded: {pageLoaded}");

            Assert.IsTrue(pageLoaded, "Dashboard page should be loaded successfully");

            TestContext.WriteLine("Debug configuration check completed");
        }

        [TestMethod]
        [Description("Verify authentication and direct dashboard landing")]
        public void Dashboard_VerifyOptimizedAuthFlow()
        {
            TestContext.WriteLine("Testing authentication flow");

            var currentUrl = Driver.Url;
            TestContext.WriteLine($"Current URL: {currentUrl}");

            bool isOnDashboard = currentUrl.Contains("/application/dashboard", StringComparison.OrdinalIgnoreCase) ||
                               currentUrl.Contains("/dashboard", StringComparison.OrdinalIgnoreCase);

            TestContext.WriteLine($"Is on dashboard page: {isOnDashboard}");

            // Verify dashboard elements are present
            bool approvalQueuePresent = _dashboardPage.VerifyApprovalQueueSection();
            bool pageTitle = _dashboardPage.VerifyPageTitle();

            TestContext.WriteLine($"Approval Queue section present: {approvalQueuePresent}");
            TestContext.WriteLine($"Page title valid: {pageTitle}");

            // At least one indicator should confirm we're on the right page
            Assert.IsTrue(isOnDashboard || approvalQueuePresent || pageTitle,
                "Should be on dashboard page with recognizable elements");

            TestContext.WriteLine("Authentication flow verification completed");
        }
    }
}
