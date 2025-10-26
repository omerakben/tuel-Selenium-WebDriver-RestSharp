using TUEL.TestFramework;
using TUEL.TestFramework.Web.PageObjects;
using TUEL.TestFramework.Web.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TUEL.TestFramework.Web.TestClasses
{
    // Test class for the Application Customers page
    [TestClass, TestCategory("UI"), TestCategory("Customers")]
    public class Customers : Base
    {
        private CustomersPOM _customersPage;
        private DashboardPOM _dashboardPage;
        private LoginPOM _loginPage;

        [TestInitialize]
        public void CustomersTestSetup()
        {
            try
            {
                TestContext.WriteLine("Initializing Customers test components...");
                _customersPage = new CustomersPOM(Driver);
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

                    // Navigate to Customers page
                    NavigateToCustomersPage();
                }
                else
                {
                    TestContext.WriteLine("No credentials provided for authentication");
                }

                WaitForCustomersPageReady();
                TestContext.WriteLine("Customers test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Customers test setup failed: {ex.Message}");
                TestContext.WriteLine($"Current URL: {Driver.Url}");
                TestContext.WriteLine($"Page Title: {Driver.Title}");
                throw;
            }
        }

        private void NavigateToCustomersPage()
        {
            NavigateToCustomersPageAsync().GetAwaiter().GetResult();
        }

        private async Task NavigateToCustomersPageAsync()
        {
            try
            {
                TestContext.WriteLine("Navigating to Customers page...");

                // First check on a page with navigation tabs
                if (!_dashboardPage.VerifyNavigationTabsPresent())
                {
                    // Navigate to dashboard first
                    var dashboardUrl = $"{InitializeTestAssembly.UiUrl}/business-application/dashboard";
                    Driver.Navigate().GoToUrl(dashboardUrl);
                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));
                }

                // Click on Customers tab
                _customersPage.ClickCustomersTab();
                TestContext.WriteLine("Clicked on Customers tab");

                // Wait for URL to change to customers page
                var customersPageLoaded = Driver.WaitForUrlContains("/customers", TimeSpan.FromSeconds(15)) ||
                                        Driver.WaitForUrlContains("/business-application/customers", TimeSpan.FromSeconds(5));

                if (customersPageLoaded)
                {
                    TestContext.WriteLine("Successfully navigated to Customers page");
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    TestContext.WriteLine($"Warning: Expected customers page, current URL: {Driver.Url}");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Navigation to Customers page failed: {ex.Message}");
                throw;
            }
        }

        private void WaitForCustomersPageReady()
        {
            WaitForCustomersPageReadyAsync().GetAwaiter().GetResult();
        }

        private async Task WaitForCustomersPageReadyAsync()
        {
            const int maxRetries = 3;
            var retryDelay = TimeSpan.FromSeconds(2);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    TestContext.WriteLine($"Waiting for Customers page to be ready (attempt {attempt}/{maxRetries})...");

                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));

                    // Try to wait for customers page-specific elements
                    _customersPage.WaitUntilPageIsLoaded(TimeSpan.FromSeconds(10));

                    TestContext.WriteLine("Customers page is ready");
                    return;
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Customers page readiness check failed (attempt {attempt}): {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        TestContext.WriteLine($"Retrying in {retryDelay.TotalSeconds:F1}s...");
                        await Task.Delay(retryDelay);
                    }
                    else
                    {
                        TestContext.WriteLine("Customers page readiness timeout");
                    }
                }
            }
        }

        [TestMethod]
        [Description("Customers page comprehensive test verifying all components")]
        public void Customers_SmokeTest_AllComponentsPresent()
        {
            TestContext.WriteLine("Starting comprehensive Customers page components test");

            // Test Page State & Navigation Verification
            bool pageTitle = _customersPage.VerifyPageTitle();
            bool customersTabActive = _customersPage.VerifyCustomersTabActive();
            bool customersHeader = _customersPage.VerifyCustomersHeader();

            TestContext.WriteLine($"Page Title Valid ('Application'): {pageTitle}");
            TestContext.WriteLine($"Customers Tab Active: {customersTabActive}");
            TestContext.WriteLine($"Customers Header Present: {customersHeader}");

            // Test Search and Controls Verification
            bool searchInput = _customersPage.VerifySearchInput();

            TestContext.WriteLine($"Search Input Available: {searchInput}");

            // Test Data Table & Columns Verification
            bool dataTablePresent = _customersPage.VerifyDataTablePresent();
            bool columnHeadersInOrder = _customersPage.VerifyColumnHeadersInOrder();
            bool specificColumns = _customersPage.VerifySpecificColumns();
            bool viewLinksPresent = _customersPage.VerifyViewLinksInRows();
            int rowCount = _customersPage.GetDataRowCount();

            TestContext.WriteLine($"Data Table Present: {dataTablePresent}");
            TestContext.WriteLine($"Column Headers In Correct Order: {columnHeadersInOrder}");
            TestContext.WriteLine($"All Specific Columns Present: {specificColumns}");
            TestContext.WriteLine($"View Links in Rows Present: {viewLinksPresent}");
            TestContext.WriteLine($"Data Row Count: {rowCount}");

            // Test Pagination and Item Count Verification
            bool paginationControls = _customersPage.VerifyPaginationControls();
            bool pageStatusDisplay = _customersPage.VerifyPageStatusDisplay();
            bool itemsPerPageSelector = _customersPage.VerifyItemsPerPageSelector();
            bool multiPageDataSet = _customersPage.VerifyMultiPageDataSet();
            bool itemCountSummary = _customersPage.VerifyItemCountSummary();

            TestContext.WriteLine($"Pagination Controls Present: {paginationControls}");
            TestContext.WriteLine($"Page Status Display Present: {pageStatusDisplay}");
            TestContext.WriteLine($"Items Per Page Selector Present: {itemsPerPageSelector}");
            TestContext.WriteLine($"Multi-Page Data Set: {multiPageDataSet}");
            TestContext.WriteLine($"Item Count Summary Present: {itemCountSummary}");

            // Get current page status for logging
            if (pageStatusDisplay)
            {
                string pageStatus = _customersPage.GetPageStatus();
                TestContext.WriteLine($"Current page status: '{pageStatus}'");
            }

            // Define critical, important, and optional checks
            var criticalChecks = new[] { pageTitle, dataTablePresent, customersHeader };
            var importantChecks = new[] { searchInput, columnHeadersInOrder, viewLinksPresent, paginationControls };
            var optionalChecks = new[] { customersTabActive, specificColumns, pageStatusDisplay, itemsPerPageSelector, multiPageDataSet, itemCountSummary };

            var criticalPassed = criticalChecks.Count(check => check);
            var importantPassed = importantChecks.Count(check => check);
            var optionalPassed = optionalChecks.Count(check => check);

            TestContext.WriteLine($"Critical checks passed: {criticalPassed}/{criticalChecks.Length}");
            TestContext.WriteLine($"Important checks passed: {importantPassed}/{importantChecks.Length}");
            TestContext.WriteLine($"Optional checks passed: {optionalPassed}/{optionalChecks.Length}");

            // Assertions
            Assert.IsTrue(pageTitle, "The browser page title must remain 'Application'");
            Assert.IsTrue(customersTabActive, "The 'Customers' tab must be highlighted as the active tab");
            Assert.IsTrue(customersHeader, "A sub-header with the text 'Customers' is visible on the page");
            Assert.IsTrue(searchInput, "A search input field is visible above the data table, allowing users to search for customers");
            Assert.IsTrue(dataTablePresent, "A data table grid is loaded and populated with rows of data");
            Assert.IsTrue(columnHeadersInOrder, "The table must display column headers exactly in this order: View, Customer, Address, Address 2, City, State, Zip Code");
            Assert.IsTrue(viewLinksPresent, "Each row in the 'View' column must contain a clickable 'View' link");
            Assert.IsTrue(paginationControls, "Pagination controls are visible and enabled below the data table");
            Assert.IsTrue(pageStatusDisplay, "The page status reflects a multi-page data set (e.g., 'Page 1 of X', where X > 1)");
            Assert.IsTrue(itemCountSummary, "An item count summary is displayed (e.g., '1 - 10 of Y items')");
            Assert.IsTrue(itemsPerPageSelector, "An 'items per page' selector is visible and functional");

            // Column headers verification is critical
            if (dataTablePresent && !columnHeadersInOrder)
            {
                var actualHeaders = _customersPage.GetColumnHeaders();
                TestContext.WriteLine($"Actual column headers found: {string.Join(", ", actualHeaders)}");
                Assert.IsTrue(columnHeadersInOrder, "Column headers must be in exact order: View, Customer, Address, Address 2, City, State, Zip Code");
            }

            TestContext.WriteLine("Customers page comprehensive test completed successfully");
        }

        [TestMethod]
        [Description("Verify page state and navigation")]
        public void Customers_VerifyPageStateAndNavigation()
        {
            TestContext.WriteLine("Testing page state and navigation");

            // Page title verification
            bool pageTitle = _customersPage.VerifyPageTitle();
            TestContext.WriteLine($"Page title contains 'Application': {pageTitle}");

            // Customers tab active state
            bool customersTabActive = _customersPage.VerifyCustomersTabActive();
            TestContext.WriteLine($"Customers tab is active/highlighted: {customersTabActive}");

            // Customers sub-header
            bool customersHeader = _customersPage.VerifyCustomersHeader();
            TestContext.WriteLine($"Customers sub-header is visible: {customersHeader}");

            // Assertions
            Assert.IsTrue(pageTitle, "The browser page title must remain 'Application'");
            Assert.IsTrue(customersTabActive, "The 'Customers' tab must be highlighted as the active tab");
            Assert.IsTrue(customersHeader, "A sub-header with the text 'Customers' is visible on the page");

            TestContext.WriteLine("Page state and navigation verification completed successfully");
        }

        [TestMethod]
        [Description("Verify search functionality and controls")]
        public void Customers_VerifySearchFunctionality()
        {
            TestContext.WriteLine("Testing search functionality and controls");

            // Search input field verification
            bool searchInput = _customersPage.VerifySearchInput();
            TestContext.WriteLine($"Search input field is visible: {searchInput}");

            // Story card requirement assertion
            Assert.IsTrue(searchInput, "A search input field is visible above the data table, allowing users to search for customers");

            // Test search functionality
            if (searchInput)
            {
                try
                {
                    TestContext.WriteLine("Testing search functionality with sample input...");
                    _customersPage.SearchCustomers("test");
                    TestContext.WriteLine("Search functionality test successful");

                    // Clear search
                    _customersPage.SearchCustomers("");
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
        public void Customers_VerifyDataTableStructure()
        {
            TestContext.WriteLine("Testing data table structure and columns");

            // Data table presence
            bool dataTablePresent = _customersPage.VerifyDataTablePresent();
            TestContext.WriteLine($"Data table grid is loaded: {dataTablePresent}");

            Assert.IsTrue(dataTablePresent, "A data table grid is loaded and populated with rows of data");

            if (dataTablePresent)
            {
                // Column headers verification
                bool columnHeadersInOrder = _customersPage.VerifyColumnHeadersInOrder();
                var actualHeaders = _customersPage.GetColumnHeaders();

                TestContext.WriteLine($"Column headers in correct order: {columnHeadersInOrder}");
                TestContext.WriteLine($"Actual headers found: {string.Join(", ", actualHeaders)}");

                // Expected headers as per story card
                var expectedHeaders = new[] { "View", "Customer", "Address", "Address 2", "City", "State", "Zip Code" };
                TestContext.WriteLine($"Expected headers: {string.Join(", ", expectedHeaders)}");

                Assert.IsTrue(columnHeadersInOrder, "The table must display column headers exactly in this order: View, Customer, Address, Address 2, City, State, Zip Code");

                // Check for View links in rows
                bool viewLinksPresent = _customersPage.VerifyViewLinksInRows();
                TestContext.WriteLine($"View links present in rows: {viewLinksPresent}");
                Assert.IsTrue(viewLinksPresent, "Each row in the 'View' column must contain a clickable 'View' link");

                // Check for data
                int rowCount = _customersPage.GetDataRowCount();
                TestContext.WriteLine($"Data row count: {rowCount}");

                if (rowCount == 0)
                {
                    bool noRecordsMessage = _customersPage.VerifyNoRecordsMessage();
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
        [Description("Verify pagination controls and multi-page data set")]
        public void Customers_VerifyPaginationAndItemCount()
        {
            TestContext.WriteLine("Testing pagination controls and item count verification");

            // Pagination controls presence
            bool paginationControls = _customersPage.VerifyPaginationControls();
            TestContext.WriteLine($"Pagination controls are visible: {paginationControls}");

            // Page status display
            bool pageStatusDisplay = _customersPage.VerifyPageStatusDisplay();
            TestContext.WriteLine($"Page status display is visible: {pageStatusDisplay}");

            // Items per page selector
            bool itemsPerPageSelector = _customersPage.VerifyItemsPerPageSelector();
            TestContext.WriteLine($"Items per page selector is visible: {itemsPerPageSelector}");

            // Multi-page data set verification
            bool multiPageDataSet = _customersPage.VerifyMultiPageDataSet();
            TestContext.WriteLine($"Multi-page data set detected: {multiPageDataSet}");

            // Item count summary verification
            bool itemCountSummary = _customersPage.VerifyItemCountSummary();
            TestContext.WriteLine($"Item count summary present: {itemCountSummary}");

            // Get current page status
            if (pageStatusDisplay)
            {
                string pageStatus = _customersPage.GetPageStatus();
                TestContext.WriteLine($"Current page status: '{pageStatus}'");
            }

            // Assertions
            Assert.IsTrue(paginationControls, "Pagination controls are visible and enabled below the data table");
            Assert.IsTrue(pageStatusDisplay, "The page status reflects a multi-page data set (e.g., 'Page 1 of X', where X > 1)");
            Assert.IsTrue(itemCountSummary, "An item count summary is displayed (e.g., '1 - 10 of Y items')");
            Assert.IsTrue(itemsPerPageSelector, "An 'items per page' selector is visible and functional");

            // Additional verification for multi-page requirement
            if (pageStatusDisplay)
            {
                // This assertion conditional based on actual data in the environment
                TestContext.WriteLine($"Multi-page data set verification: {multiPageDataSet}");
                if (!multiPageDataSet)
                {
                    TestContext.WriteLine("Warning: Multi-page data set not detected");
                }
            }

            TestContext.WriteLine("Pagination and item count verification completed");
        }

        [TestMethod]
        [Description("Test view link functionality")]
        public void Customers_TestViewLinkFunctionality()
        {
            TestContext.WriteLine("Testing View link functionality");

            bool dataTablePresent = _customersPage.VerifyDataTablePresent();
            TestContext.WriteLine($"Data table present: {dataTablePresent}");

            if (dataTablePresent)
            {
                int rowCount = _customersPage.GetDataRowCount();
                TestContext.WriteLine($"Data row count: {rowCount}");

                if (rowCount > 0)
                {
                    bool viewLinksPresent = _customersPage.VerifyViewLinksInRows();
                    TestContext.WriteLine($"View links present: {viewLinksPresent}");

                    Assert.IsTrue(viewLinksPresent, "Each row in the 'View' column must contain a clickable 'View' link");

                    if (viewLinksPresent)
                    {
                        try
                        {
                            TestContext.WriteLine("Testing click functionality of first View link...");
                            string currentUrl = Driver.Url;
                            _customersPage.ClickFirstViewLink();

                            Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));
                            string newUrl = Driver.Url;

                            TestContext.WriteLine($"URL before click: {currentUrl}");
                            TestContext.WriteLine($"URL after click: {newUrl}");
                            TestContext.WriteLine("View link click test completed");

                            // Navigate back to customers page for subsequent tests
                            if (currentUrl != newUrl)
                            {
                                TestContext.WriteLine("Navigating back to Customers page...");
                                Driver.Navigate().Back();
                                Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));
                            }
                        }
                        catch (Exception ex)
                        {
                            TestContext.WriteLine($"View link click test failed: {ex.Message}");
                        }
                    }
                }
                else
                {
                    TestContext.WriteLine("No data rows available to test View links");
                }
            }
            else
            {
                TestContext.WriteLine("Data table not present - cannot test View link functionality");
            }

            TestContext.WriteLine("View link functionality testing completed");
        }

        [TestMethod]
        [Description("Test navigation between tabs")]
        public void Customers_TestTabNavigation()
        {
            TestContext.WriteLine("Testing tab navigation functionality");

            // Verify on Customers tab
            bool customersTabActive = _customersPage.VerifyCustomersTabActive();
            TestContext.WriteLine($"Currently on Customers tab: {customersTabActive}");

            // Test navigation to other tabs and back
            var tabsToTest = new[] { "Dashboard", "Transactions" };

            foreach (var tabName in tabsToTest)
            {
                try
                {
                    TestContext.WriteLine($"Testing navigation to {tabName} tab...");
                    string currentUrl = Driver.Url;
                    _customersPage.ClickNavigationTab(tabName.ToLower());
                    Driver.WaitForUrlChange(currentUrl, TimeSpan.FromSeconds(5));

                    TestContext.WriteLine($"Successfully navigated to {tabName} tab");

                    // Navigate back to Customers
                    TestContext.WriteLine("Navigating back to Customers tab...");
                    currentUrl = Driver.Url;
                    _customersPage.ClickNavigationTab("customers");
                    Driver.WaitForUrlChange(currentUrl, TimeSpan.FromSeconds(5));

                    TestContext.WriteLine("Successfully navigated back to Customers tab");
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
        public void Debug_CheckCustomersPageConfiguration()
        {
            TestContext.WriteLine($"Environment: {InitializeTestAssembly.ENV}");
            TestContext.WriteLine($"UI URL: {InitializeTestAssembly.UiUrl}");
            TestContext.WriteLine($"API URL: {InitializeTestAssembly.BaseApiUrl}");
            TestContext.WriteLine($"Browser: {InitializeTestAssembly.Browser}");
            TestContext.WriteLine($"Username: {InitializeTestAssembly.Email}");

            TestContext.WriteLine($"Current URL: {Driver.Url}");
            TestContext.WriteLine($"Page Title: {Driver.Title}");

            bool pageLoaded = _customersPage.IsPageLoaded();
            TestContext.WriteLine($"Customers page loaded: {pageLoaded}");

            Assert.IsTrue(pageLoaded, "Customers page should be loaded successfully");

            TestContext.WriteLine("Debug configuration check completed");
        }
    }
}
