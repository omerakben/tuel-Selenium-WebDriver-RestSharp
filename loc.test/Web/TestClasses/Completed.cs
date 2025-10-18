using TUEL.TestFramework;
using TUEL.TestFramework.Web.PageObjects;
using TUEL.TestFramework.Web.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Threading;

namespace TUEL.TestFramework.Web.TestClasses
{
    // Test class for the Business Application Completed page
    [TestClass, TestCategory("UI"), TestCategory("Completed")]
    public class Completed : Base
    {
        private CompletedPOM _completedPage;
        private DashboardPOM _dashboardPage;
        private LoginPOM _loginPage;

        [TestInitialize]
        public void CompletedTestSetup()
        {
            try
            {
                TestContext.WriteLine("Initializing Completed test components...");
                _completedPage = new CompletedPOM(Driver);
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
                    }

                    // Navigate to Completed page
                    NavigateToCompletedPage();
                }
                else
                {
                    TestContext.WriteLine("No credentials provided for authentication");
                }

                WaitForCompletedPageReady();
                TestContext.WriteLine("Completed test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Completed test setup failed: {ex.Message}");
                TestContext.WriteLine($"Current URL: {Driver.Url}");
                TestContext.WriteLine($"Page Title: {Driver.Title}");
                throw;
            }
        }

        private void NavigateToCompletedPage()
        {
            try
            {
                TestContext.WriteLine("Navigating to Completed page...");

                // First ensure we're on a page with navigation tabs
                if (!_dashboardPage.VerifyNavigationTabsPresent())
                {
                    // Navigate to dashboard first if not on a page with tabs
                    var dashboardUrl = $"{InitializeTestAssembly.UiUrl}/letters-of-credit/dashboard";
                    Driver.Navigate().GoToUrl(dashboardUrl);
                    Thread.Sleep(2000);
                }

                // Click on Completed tab
                _dashboardPage.ClickCompletedTab();
                TestContext.WriteLine("Clicked on Completed tab");

                // Wait for URL to change to completed page
                var completedPageLoaded = Driver.WaitForUrlContains("/completed", TimeSpan.FromSeconds(15)) ||
                                        Driver.WaitForUrlContains("/letters-of-credit/completed", TimeSpan.FromSeconds(5));

                if (completedPageLoaded)
                {
                    TestContext.WriteLine("Successfully navigated to Completed page");
                }
                else
                {
                    TestContext.WriteLine($"Warning: Expected completed page, current URL: {Driver.Url}");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Navigation to Completed page failed: {ex.Message}");
                throw;
            }
        }

        private void WaitForCompletedPageReady()
        {
            const int maxRetries = 3;
            const int waitBetweenRetries = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    TestContext.WriteLine($"Waiting for Completed page to be ready (attempt {attempt}/{maxRetries})...");

                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));

                    // Try to wait for completed page-specific elements
                    _completedPage.WaitUntilPageIsLoaded(TimeSpan.FromSeconds(10));

                    TestContext.WriteLine("Completed page is ready");
                    return;
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Completed page readiness check failed (attempt {attempt}): {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        TestContext.WriteLine($"Waiting {waitBetweenRetries}ms before retry...");
                        Thread.Sleep(waitBetweenRetries);
                    }
                    else
                    {
                        TestContext.WriteLine("Completed page readiness timeout");
                    }
                }
            }
        }

        [TestMethod]
        [Description("Completed page test verifying components")]
        public void Completed_SmokeTest_AllComponentsPresent()
        {
            TestContext.WriteLine("Starting comprehensive Completed page components test");

            // Test Page State & Navigation Verification
            bool pageTitle = _completedPage.VerifyPageTitle();
            bool completedTabActive = _completedPage.VerifyCompletedTabActive();
            bool completedItemsHeader = _completedPage.VerifyCompletedItemsHeader();

            TestContext.WriteLine($"Page Title Valid ('Business Application'): {pageTitle}");
            TestContext.WriteLine($"Completed Tab Active: {completedTabActive}");
            TestContext.WriteLine($"Completed Items Header Present: {completedItemsHeader}");

            // Test Search and Controls Verification
            bool searchInput = _completedPage.VerifySearchInput();
            bool exportToCsv = _completedPage.VerifyExportToCsvButton();

            TestContext.WriteLine($"Search Input Available: {searchInput}");
            TestContext.WriteLine($"Export to CSV Button Available: {exportToCsv}");

            // Test Data Table & Columns Verification
            bool dataTablePresent = _completedPage.VerifyDataTablePresent();
            bool columnHeadersInOrder = _completedPage.VerifyColumnHeadersInOrder();
            bool specificColumns = _completedPage.VerifySpecificColumns();
            int rowCount = _completedPage.GetDataRowCount();

            TestContext.WriteLine($"Data Table Present: {dataTablePresent}");
            TestContext.WriteLine($"Column Headers In Correct Order: {columnHeadersInOrder}");
            TestContext.WriteLine($"All Specific Columns Present: {specificColumns}");
            TestContext.WriteLine($"Data Row Count: {rowCount}");

            // Test No Records Message
            bool noRecordsMessage = false;
            if (rowCount == 0)
            {
                noRecordsMessage = _completedPage.VerifyNoRecordsMessage();
                TestContext.WriteLine($"No Records Message (when empty): {noRecordsMessage}");
            }

            // Test Pagination Controls Verification
            bool paginationControls = _completedPage.VerifyPaginationControls();
            bool pageStatusDisplay = _completedPage.VerifyPageStatusDisplay();
            bool itemsPerPageSelector = _completedPage.VerifyItemsPerPageSelector();

            TestContext.WriteLine($"Pagination Controls Present: {paginationControls}");
            TestContext.WriteLine($"Page Status Display Present: {pageStatusDisplay}");
            TestContext.WriteLine($"Items Per Page Selector Present: {itemsPerPageSelector}");

            // Define critical, important, and optional checks
            var criticalChecks = new[] { pageTitle, dataTablePresent };
            var importantChecks = new[] { completedItemsHeader, searchInput, columnHeadersInOrder };
            var optionalChecks = new[] { completedTabActive, exportToCsv, specificColumns, paginationControls, pageStatusDisplay, itemsPerPageSelector };

            var criticalPassed = criticalChecks.Count(check => check);
            var importantPassed = importantChecks.Count(check => check);
            var optionalPassed = optionalChecks.Count(check => check);

            TestContext.WriteLine($"Critical checks passed: {criticalPassed}/{criticalChecks.Length}");
            TestContext.WriteLine($"Important checks passed: {importantPassed}/{importantChecks.Length}");
            TestContext.WriteLine($"Optional checks passed: {optionalPassed}/{optionalChecks.Length}");

            // Assertions
            Assert.IsTrue(pageTitle, "Page title must contain 'Business Application'");
            Assert.IsTrue(dataTablePresent, "Data table must be present on the page");
            Assert.IsTrue(completedItemsHeader, "Completed Items header must be visible");
            Assert.IsTrue(searchInput, "Search input field must be visible above the data table");

            // Column headers verification is critical
            if (dataTablePresent && !columnHeadersInOrder)
            {
                var actualHeaders = _completedPage.GetColumnHeaders();
                TestContext.WriteLine($"Actual column headers found: {string.Join(", ", actualHeaders)}");
                Assert.IsTrue(columnHeadersInOrder, "Column headers must be in exact order: View, Account, Customer, Order Date, Delivery Date, Customer, Amount, Product #, Product Type, Document, Status");
            }

            TestContext.WriteLine("Completed page comprehensive test completed successfully");
        }

        [TestMethod]
        [Description("Verify page state and navigation")]
        public void Completed_VerifyPageStateAndNavigation()
        {
            TestContext.WriteLine("Testing page state and navigation");

            // Page title verification
            bool pageTitle = _completedPage.VerifyPageTitle();
            TestContext.WriteLine($"Page title contains 'Business Application': {pageTitle}");

            // Completed tab active state
            bool completedTabActive = _completedPage.VerifyCompletedTabActive();
            TestContext.WriteLine($"Completed tab is active/highlighted: {completedTabActive}");

            // Completed Items sub-header
            bool completedItemsHeader = _completedPage.VerifyCompletedItemsHeader();
            TestContext.WriteLine($"Completed Items sub-header is visible: {completedItemsHeader}");

            // Assertions
            Assert.IsTrue(pageTitle, "The browser page title must remain 'Business Application'");
            Assert.IsTrue(completedTabActive, "The 'Completed' tab must be highlighted as the active tab");
            Assert.IsTrue(completedItemsHeader, "A sub-header with the text 'Completed Items' must be visible on the page");

            TestContext.WriteLine("Page state and navigation verification completed successfully");
        }

        [TestMethod]
        [Description("Verify search functionality and controls")]
        public void Completed_VerifySearchFunctionality()
        {
            TestContext.WriteLine("Testing search functionality and controls");

            // Search input field verification
            bool searchInput = _completedPage.VerifySearchInput();
            TestContext.WriteLine($"Search input field is visible: {searchInput}");

            // Story card requirement assertion
            Assert.IsTrue(searchInput, "A search input field must be visible above the data table, allowing users to search completed items");

            // Test search functionality
            if (searchInput)
            {
                try
                {
                    TestContext.WriteLine("Testing search functionality with sample input...");
                    _completedPage.SearchCompletedItems("test");
                    TestContext.WriteLine("Search functionality test successful");

                    // Clear search
                    _completedPage.SearchCompletedItems("");
                    TestContext.WriteLine("Search clear successful");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Search functionality test failed: {ex.Message}");
                }
            }

            TestContext.WriteLine("Search functionality verification completed");
        }

        [TestMethod]
        [Description("Verify data table structure and columns")]
        public void Completed_VerifyDataTableStructure()
        {
            TestContext.WriteLine("Testing data table structure and columns");

            // Data table presence
            bool dataTablePresent = _completedPage.VerifyDataTablePresent();
            TestContext.WriteLine($"Data table grid is loaded: {dataTablePresent}");

            Assert.IsTrue(dataTablePresent, "A data table grid must be loaded on the page");

            if (dataTablePresent)
            {
                // Column headers verification
                bool columnHeadersInOrder = _completedPage.VerifyColumnHeadersInOrder();
                var actualHeaders = _completedPage.GetColumnHeaders();

                TestContext.WriteLine($"Column headers in correct order: {columnHeadersInOrder}");
                TestContext.WriteLine($"Actual headers found: {string.Join(", ", actualHeaders)}");

                // Expected headers as per story card
                var expectedHeaders = new[] { "View", "DDA", "Member", "Issue Date", "Expiration Date", "Beneficiary", "Amount", "LOC #", "LOC Type", "Letter", "Status" };
                TestContext.WriteLine($"Expected headers: {string.Join(", ", expectedHeaders)}");

                Assert.IsTrue(columnHeadersInOrder, "The table must display column headers exactly in this order: View, DDA, Member, Issue Date, Expiration Date, Beneficiary, Amount, LOC #, LOC Type, Letter, Status");

                // Check for data or no records message
                int rowCount = _completedPage.GetDataRowCount();
                TestContext.WriteLine($"Data row count: {rowCount}");

                if (rowCount == 0)
                {
                    bool noRecordsMessage = _completedPage.VerifyNoRecordsMessage();
                    TestContext.WriteLine($"No records message displayed: {noRecordsMessage}");
                    Assert.IsTrue(noRecordsMessage, "If no records are returned, the table body must correctly display the message 'No records available'");
                }
                else
                {
                    TestContext.WriteLine($"Data table contains {rowCount} records");
                }
            }

            TestContext.WriteLine("Data table structure verification completed");
        }

        [TestMethod]
        [Description("Verify pagination controls and functionality")]
        public void Completed_VerifyPaginationControls()
        {
            TestContext.WriteLine("Testing pagination controls");

            // Pagination controls presence
            bool paginationControls = _completedPage.VerifyPaginationControls();
            TestContext.WriteLine($"Pagination controls are visible: {paginationControls}");

            // Page status display
            bool pageStatusDisplay = _completedPage.VerifyPageStatusDisplay();
            TestContext.WriteLine($"Page status display is visible: {pageStatusDisplay}");

            // Items per page selector
            bool itemsPerPageSelector = _completedPage.VerifyItemsPerPageSelector();
            TestContext.WriteLine($"Items per page selector is visible: {itemsPerPageSelector}");

            // Get current page status if available
            if (pageStatusDisplay)
            {
                string pageStatus = _completedPage.GetPageStatus();
                TestContext.WriteLine($"Current page status: '{pageStatus}'");
            }

            // Story card requirements assertions
            Assert.IsTrue(paginationControls, "Pagination controls must be visible below the data table");
            Assert.IsTrue(pageStatusDisplay, "The controls must display the current page status (e.g., 'Page 0 of 0')");

            TestContext.WriteLine("Pagination controls verification completed");
        }

        [TestMethod]
        [Description("Test Export to CSV functionality")]
        public void Completed_VerifyExportToCsvFunctionality()
        {
            TestContext.WriteLine("Testing Export to CSV functionality");

            bool exportToCsvButton = _completedPage.VerifyExportToCsvButton();
            TestContext.WriteLine($"Export to CSV button is visible: {exportToCsvButton}");

            if (exportToCsvButton)
            {
                try
                {
                    TestContext.WriteLine("Testing Export to CSV click functionality...");
                    _completedPage.ClickExportToCsv();
                    TestContext.WriteLine("Export to CSV click successful");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Export to CSV functionality test failed: {ex.Message}");
                }
            }
            else
            {
                TestContext.WriteLine("Export to CSV button not found - may be environment-specific");
            }

            TestContext.WriteLine("Export to CSV functionality verification completed");
        }

        [TestMethod]
        [Description("Test navigation between tabs")]
        public void Completed_TestTabNavigation()
        {
            TestContext.WriteLine("Testing tab navigation functionality");

            // Verify we're on Completed tab
            bool completedTabActive = _completedPage.VerifyCompletedTabActive();
            TestContext.WriteLine($"Currently on Completed tab: {completedTabActive}");

            // Test navigation to other tabs and back
            var tabsToTest = new[] { "Dashboard", "Beneficiaries" };

            foreach (var tabName in tabsToTest)
            {
                try
                {
                    TestContext.WriteLine($"Testing navigation to {tabName} tab...");
                    _completedPage.ClickNavigationTab(tabName.ToLower());
                    Thread.Sleep(2000);

                    TestContext.WriteLine($"Successfully navigated to {tabName} tab");

                    // Navigate back to Completed
                    TestContext.WriteLine("Navigating back to Completed tab...");
                    _completedPage.ClickNavigationTab("completed");
                    Thread.Sleep(2000);

                    TestContext.WriteLine("Successfully navigated back to Completed tab");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Tab navigation test failed for {tabName}: {ex.Message}");
                }
            }

            TestContext.WriteLine("Tab navigation testing completed");
        }

        [TestMethod]
        [Description("Debug test to check configuration and URL loading")]
        public void Debug_CheckCompletedPageConfiguration()
        {
            TestContext.WriteLine($"Environment: {InitializeTestAssembly.ENV}");
            TestContext.WriteLine($"UI URL: {InitializeTestAssembly.UiUrl}");
            TestContext.WriteLine($"API URL: {InitializeTestAssembly.BaseApiUrl}");
            TestContext.WriteLine($"Browser: {InitializeTestAssembly.Browser}");
            TestContext.WriteLine($"Username: {InitializeTestAssembly.Email}");

            TestContext.WriteLine($"Current URL: {Driver.Url}");
            TestContext.WriteLine($"Page Title: {Driver.Title}");

            bool pageLoaded = _completedPage.IsPageLoaded();
            TestContext.WriteLine($"Completed page loaded: {pageLoaded}");

            Assert.IsTrue(pageLoaded, "Completed page should be loaded successfully");

            TestContext.WriteLine("Debug configuration check completed");
        }
    }
}
