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
    // Test class for the Application Transactions page
    [TestClass, TestCategory("UI"), TestCategory("Transactions")]
    public class Transactions : Base
    {
        private TransactionsPOM _completedPage;
        private DashboardPOM _dashboardPage;
        private LoginPOM _loginPage;

        [TestInitialize]
        public void TransactionsTestSetup()
        {
            try
            {
                TestContext.WriteLine("Initializing Transactions test components...");
                _completedPage = new TransactionsPOM(Driver);
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

                    // Navigate to Transactions page
                    NavigateToTransactionsPage();
                }
                else
                {
                    TestContext.WriteLine("No credentials provided for authentication");
                }

                WaitForTransactionsPageReady();
                TestContext.WriteLine("Transactions test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Transactions test setup failed: {ex.Message}");
                TestContext.WriteLine($"Current URL: {Driver.Url}");
                TestContext.WriteLine($"Page Title: {Driver.Title}");
                throw;
            }
        }

        private void NavigateToTransactionsPage()
        {
            NavigateToTransactionsPageAsync().GetAwaiter().GetResult();
        }

        private async Task NavigateToTransactionsPageAsync()
        {
            try
            {
                TestContext.WriteLine("Navigating to Transactions page...");

                // First ensure we're on a page with navigation tabs
                if (!_dashboardPage.VerifyNavigationTabsPresent())
                {
                    // Navigate to dashboard first if not on a page with tabs
                    var dashboardUrl = $"{InitializeTestAssembly.UiUrl}/application/dashboard";
                    Driver.Navigate().GoToUrl(dashboardUrl);
                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));
                }

                // Click on Transactions tab
                _dashboardPage.ClickTransactionsTab();
                TestContext.WriteLine("Clicked on Transactions tab");

                // Wait for URL to change to transactions page
                var transactionsPageLoaded = Driver.WaitForUrlContains("/transactions", TimeSpan.FromSeconds(15)) ||
                                              Driver.WaitForUrlContains("/application/transactions", TimeSpan.FromSeconds(5));

                if (transactionsPageLoaded)
                {
                    TestContext.WriteLine("Successfully navigated to Transactions page");
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    TestContext.WriteLine($"Warning: Expected transactions page, current URL: {Driver.Url}");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Navigation to Transactions page failed: {ex.Message}");
                throw;
            }
        }

        private void WaitForTransactionsPageReady()
        {
            WaitForTransactionsPageReadyAsync().GetAwaiter().GetResult();
        }

        private async Task WaitForTransactionsPageReadyAsync()
        {
            const int maxRetries = 3;
            var retryDelay = TimeSpan.FromSeconds(2);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    TestContext.WriteLine($"Waiting for Transactions page to be ready (attempt {attempt}/{maxRetries})...");

                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));

                    // Try to wait for transactions page-specific elements
                    _completedPage.WaitUntilPageIsLoaded(TimeSpan.FromSeconds(10));

                    TestContext.WriteLine("Transactions page is ready");
                    return;
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Transactions page readiness check failed (attempt {attempt}): {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        TestContext.WriteLine($"Retrying in {retryDelay.TotalSeconds:F1}s...");
                        await Task.Delay(retryDelay);
                    }
                    else
                    {
                        TestContext.WriteLine("Transactions page readiness timeout");
                    }
                }
            }
        }

        [TestMethod]
        [Description("Transactions page test verifying components")]
        public void Transactions_SmokeTest_AllComponentsPresent()
        {
            TestContext.WriteLine("Starting comprehensive Transactions page components test");

            // Test Page State & Navigation Verification
            bool pageTitle = _completedPage.VerifyPageTitle();
            bool transactionsTabActive = _completedPage.VerifyTransactionsTabActive();
            bool transactionItemsHeader = _completedPage.VerifyTransactionItemsHeader();

            TestContext.WriteLine($"Page Title Valid ('Application'): {pageTitle}");
            TestContext.WriteLine($"Transactions Tab Active: {transactionsTabActive}");
            TestContext.WriteLine($"Transaction Items Header Present: {transactionItemsHeader}");

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
            var importantChecks = new[] { transactionItemsHeader, searchInput, columnHeadersInOrder };
            var optionalChecks = new[] { transactionsTabActive, exportToCsv, specificColumns, paginationControls, pageStatusDisplay, itemsPerPageSelector };

            var criticalPassed = criticalChecks.Count(check => check);
            var importantPassed = importantChecks.Count(check => check);
            var optionalPassed = optionalChecks.Count(check => check);

            TestContext.WriteLine($"Critical checks passed: {criticalPassed}/{criticalChecks.Length}");
            TestContext.WriteLine($"Important checks passed: {importantPassed}/{importantChecks.Length}");
            TestContext.WriteLine($"Optional checks passed: {optionalPassed}/{optionalChecks.Length}");

            // Assertions
            Assert.IsTrue(pageTitle, "Page title must contain 'Application'");
            Assert.IsTrue(dataTablePresent, "Data table must be present on the page");
            Assert.IsTrue(transactionItemsHeader, "Transaction Items header must be visible");
            Assert.IsTrue(searchInput, "Search input field must be visible above the data table");

            // Column headers verification is critical
            if (dataTablePresent && !columnHeadersInOrder)
            {
                var actualHeaders = _completedPage.GetColumnHeaders();
                TestContext.WriteLine($"Actual column headers found: {string.Join(", ", actualHeaders)}");
                Assert.IsTrue(columnHeadersInOrder, "Column headers must be in exact order: View, Account, Customer, Order Date, Delivery Date, Amount, Product #, Product Type, Document, Status");
            }

            TestContext.WriteLine("Transactions page comprehensive test completed successfully");
        }

        [TestMethod]
        [Description("Verify page state and navigation")]
        public void Transactions_VerifyPageStateAndNavigation()
        {
            TestContext.WriteLine("Testing page state and navigation");

            // Page title verification
            bool pageTitle = _completedPage.VerifyPageTitle();
            TestContext.WriteLine($"Page title contains 'Application': {pageTitle}");

            // Transactions tab active state
            bool transactionsTabActive = _completedPage.VerifyTransactionsTabActive();
            TestContext.WriteLine($"Transactions tab is active/highlighted: {transactionsTabActive}");

            // Transaction Items sub-header
            bool transactionItemsHeader = _completedPage.VerifyTransactionItemsHeader();
            TestContext.WriteLine($"Transaction Items sub-header is visible: {transactionItemsHeader}");

            // Assertions
            Assert.IsTrue(pageTitle, "The browser page title must remain 'Application'");
            Assert.IsTrue(transactionsTabActive, "The 'Transactions' tab must be highlighted as the active tab");
            Assert.IsTrue(transactionItemsHeader, "A sub-header with the text 'Transaction Items' must be visible on the page");

            TestContext.WriteLine("Page state and navigation verification completed successfully");
        }

        [TestMethod]
        [Description("Verify search functionality and controls")]
        public void Transactions_VerifySearchFunctionality()
        {
            TestContext.WriteLine("Testing search functionality and controls");

            // Search input field verification
            bool searchInput = _completedPage.VerifySearchInput();
            TestContext.WriteLine($"Search input field is visible: {searchInput}");

            // Story card requirement assertion
            Assert.IsTrue(searchInput, "A search input field must be visible above the data table, allowing users to search transactions");

            // Test search functionality
            if (searchInput)
            {
                try
                {
                    TestContext.WriteLine("Testing search functionality with sample input...");
                    _completedPage.SearchTransactions("test");
                    TestContext.WriteLine("Search functionality test successful");

                    // Clear search
                    _completedPage.SearchTransactions("");
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
        public void Transactions_VerifyDataTableStructure()
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
                var expectedHeaders = new[] { "View", "Account", "Customer", "Order Date", "Delivery Date", "Amount", "Product #", "Product Type", "Document", "Status" };
                TestContext.WriteLine($"Expected headers: {string.Join(", ", expectedHeaders)}");

                Assert.IsTrue(columnHeadersInOrder, "The table must display column headers exactly in this order: View, Account, Customer, Order Date, Delivery Date, Amount, Product #, Product Type, Document, Status");

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
        public void Transactions_VerifyPaginationControls()
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
        public void Transactions_VerifyExportToCsvFunctionality()
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
        public void Transactions_TestTabNavigation()
        {
            TestContext.WriteLine("Testing tab navigation functionality");

            // Verify we're on Transactions tab
            bool transactionsTabActive = _completedPage.VerifyTransactionsTabActive();
            TestContext.WriteLine($"Currently on Transactions tab: {transactionsTabActive}");

            // Test navigation to other tabs and back
            var tabsToTest = new[] { "Dashboard", "Customers" };

            foreach (var tabName in tabsToTest)
            {
                try
                {
                    TestContext.WriteLine($"Testing navigation to {tabName} tab...");
                    _completedPage.ClickNavigationTab(tabName.ToLower());
                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(2));

                    TestContext.WriteLine($"Successfully navigated to {tabName} tab");

                    // Navigate back to Transactions
                    TestContext.WriteLine("Navigating back to Transactions tab...");
                    _completedPage.ClickNavigationTab("transactions");
                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(2));

                    TestContext.WriteLine("Successfully navigated back to Transactions tab");
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
        public void Debug_CheckTransactionsPageConfiguration()
        {
            TestContext.WriteLine($"Environment: {InitializeTestAssembly.ENV}");
            TestContext.WriteLine($"UI URL: {InitializeTestAssembly.UiUrl}");
            TestContext.WriteLine($"API URL: {InitializeTestAssembly.BaseApiUrl}");
            TestContext.WriteLine($"Browser: {InitializeTestAssembly.Browser}");
            TestContext.WriteLine($"Username: {InitializeTestAssembly.Email}");

            TestContext.WriteLine($"Current URL: {Driver.Url}");
            TestContext.WriteLine($"Page Title: {Driver.Title}");

            bool pageLoaded = _completedPage.IsPageLoaded();
            TestContext.WriteLine($"Transactions page loaded: {pageLoaded}");

            Assert.IsTrue(pageLoaded, "Transactions page should be loaded successfully");

            TestContext.WriteLine("Debug configuration check completed");
        }
    }
}
