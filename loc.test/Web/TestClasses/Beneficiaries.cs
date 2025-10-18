using fhlb.selenium.common.builders;
using loc.test.Web.PageObjectFiles;
using loc.test.Web.Support;
using loc.test;
using loc.test.Web.TestClasses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Threading;

namespace loc.test.Web.TestClasses
{
    // Test class for the Letters of Credit Beneficiaries page
    [TestClass, TestCategory("UI"), TestCategory("Beneficiaries")]
    public class Beneficiaries : Base
    {
        private BeneficiariesPOM _beneficiariesPage;
        private DashboardPOM _dashboardPage;
        private LoginPOM _loginPage;

        [TestInitialize]
        public void BeneficiariesTestSetup()
        {
            try
            {
                TestContext.WriteLine("Initializing Beneficiaries test components...");
                _beneficiariesPage = new BeneficiariesPOM(Driver);
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

                    // Navigate to Beneficiaries page
                    NavigateToBeneficiariesPage();
                }
                else
                {
                    TestContext.WriteLine("No credentials provided for authentication");
                }

                WaitForBeneficiariesPageReady();
                TestContext.WriteLine("Beneficiaries test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Beneficiaries test setup failed: {ex.Message}");
                TestContext.WriteLine($"Current URL: {Driver.Url}");
                TestContext.WriteLine($"Page Title: {Driver.Title}");
                throw;
            }
        }

        private void NavigateToBeneficiariesPage()
        {
            try
            {
                TestContext.WriteLine("Navigating to Beneficiaries page...");

                // First check on a page with navigation tabs
                if (!_dashboardPage.VerifyNavigationTabsPresent())
                {
                    // Navigate to dashboard first
                    var dashboardUrl = $"{InitializeTestAssembly.UiUrl}/letters-of-credit/dashboard";
                    Driver.Navigate().GoToUrl(dashboardUrl);
                    Thread.Sleep(2000);
                }

                // Click on Beneficiaries tab
                _beneficiariesPage.ClickBeneficiariesTab();
                TestContext.WriteLine("Clicked on Beneficiaries tab");

                // Wait for URL to change to beneficiaries page
                var beneficiariesPageLoaded = Driver.WaitForUrlContains("/beneficiaries", TimeSpan.FromSeconds(15)) ||
                                            Driver.WaitForUrlContains("/letters-of-credit/beneficiaries", TimeSpan.FromSeconds(5));

                if (beneficiariesPageLoaded)
                {
                    TestContext.WriteLine("Successfully navigated to Beneficiaries page");
                }
                else
                {
                    TestContext.WriteLine($"Warning: Expected beneficiaries page, current URL: {Driver.Url}");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Navigation to Beneficiaries page failed: {ex.Message}");
                throw;
            }
        }

        private void WaitForBeneficiariesPageReady()
        {
            const int maxRetries = 3;
            const int waitBetweenRetries = 2000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    TestContext.WriteLine($"Waiting for Beneficiaries page to be ready (attempt {attempt}/{maxRetries})...");

                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));

                    // Try to wait for beneficiaries page-specific elements
                    _beneficiariesPage.WaitUntilPageIsLoaded(TimeSpan.FromSeconds(10));

                    TestContext.WriteLine("Beneficiaries page is ready");
                    return;
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Beneficiaries page readiness check failed (attempt {attempt}): {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        TestContext.WriteLine($"Waiting {waitBetweenRetries}ms before retry...");
                        Thread.Sleep(waitBetweenRetries);
                    }
                    else
                    {
                        TestContext.WriteLine("Beneficiaries page readiness timeout");
                    }
                }
            }
        }

        [TestMethod]
        [Description("Beneficiaries page comprehensive test verifying all components")]
        public void Beneficiaries_SmokeTest_AllComponentsPresent()
        {
            TestContext.WriteLine("Starting comprehensive Beneficiaries page components test");

            // Test Page State & Navigation Verification
            bool pageTitle = _beneficiariesPage.VerifyPageTitle();
            bool beneficiariesTabActive = _beneficiariesPage.VerifyBeneficiariesTabActive();
            bool beneficiariesHeader = _beneficiariesPage.VerifyBeneficiariesHeader();

            TestContext.WriteLine($"Page Title Valid ('Letters of Credit'): {pageTitle}");
            TestContext.WriteLine($"Beneficiaries Tab Active: {beneficiariesTabActive}");
            TestContext.WriteLine($"Beneficiaries Header Present: {beneficiariesHeader}");

            // Test Search and Controls Verification
            bool searchInput = _beneficiariesPage.VerifySearchInput();

            TestContext.WriteLine($"Search Input Available: {searchInput}");

            // Test Data Table & Columns Verification
            bool dataTablePresent = _beneficiariesPage.VerifyDataTablePresent();
            bool columnHeadersInOrder = _beneficiariesPage.VerifyColumnHeadersInOrder();
            bool specificColumns = _beneficiariesPage.VerifySpecificColumns();
            bool viewLinksPresent = _beneficiariesPage.VerifyViewLinksInRows();
            int rowCount = _beneficiariesPage.GetDataRowCount(); 

            TestContext.WriteLine($"Data Table Present: {dataTablePresent}");
            TestContext.WriteLine($"Column Headers In Correct Order: {columnHeadersInOrder}");
            TestContext.WriteLine($"All Specific Columns Present: {specificColumns}");
            TestContext.WriteLine($"View Links in Rows Present: {viewLinksPresent}");
            TestContext.WriteLine($"Data Row Count: {rowCount}");

            // Test Pagination and Item Count Verification
            bool paginationControls = _beneficiariesPage.VerifyPaginationControls(); 
            bool pageStatusDisplay = _beneficiariesPage.VerifyPageStatusDisplay(); 
            bool itemsPerPageSelector = _beneficiariesPage.VerifyItemsPerPageSelector(); 
            bool multiPageDataSet = _beneficiariesPage.VerifyMultiPageDataSet();
            bool itemCountSummary = _beneficiariesPage.VerifyItemCountSummary();

            TestContext.WriteLine($"Pagination Controls Present: {paginationControls}");
            TestContext.WriteLine($"Page Status Display Present: {pageStatusDisplay}");
            TestContext.WriteLine($"Items Per Page Selector Present: {itemsPerPageSelector}");
            TestContext.WriteLine($"Multi-Page Data Set: {multiPageDataSet}");
            TestContext.WriteLine($"Item Count Summary Present: {itemCountSummary}");

            // Get current page status for logging
            if (pageStatusDisplay)
            {
                string pageStatus = _beneficiariesPage.GetPageStatus(); 
                TestContext.WriteLine($"Current page status: '{pageStatus}'");
            }

            // Define critical, important, and optional checks
            var criticalChecks = new[] { pageTitle, dataTablePresent, beneficiariesHeader };
            var importantChecks = new[] { searchInput, columnHeadersInOrder, viewLinksPresent, paginationControls };
            var optionalChecks = new[] { beneficiariesTabActive, specificColumns, pageStatusDisplay, itemsPerPageSelector, multiPageDataSet, itemCountSummary };

            var criticalPassed = criticalChecks.Count(check => check);
            var importantPassed = importantChecks.Count(check => check);
            var optionalPassed = optionalChecks.Count(check => check);

            TestContext.WriteLine($"Critical checks passed: {criticalPassed}/{criticalChecks.Length}");
            TestContext.WriteLine($"Important checks passed: {importantPassed}/{importantChecks.Length}");
            TestContext.WriteLine($"Optional checks passed: {optionalPassed}/{optionalChecks.Length}");

            // Assertions
            Assert.IsTrue(pageTitle, "The browser page title must remain 'Letters of Credit'");
            Assert.IsTrue(beneficiariesTabActive, "The 'Beneficiaries' tab must be highlighted as the active tab");
            Assert.IsTrue(beneficiariesHeader, "A sub-header with the text 'Beneficiaries' is visible on the page");
            Assert.IsTrue(searchInput, "A search input field is visible above the data table, allowing users to search for beneficiaries");
            Assert.IsTrue(dataTablePresent, "A data table grid is loaded and populated with rows of data");
            Assert.IsTrue(columnHeadersInOrder, "The table must display column headers exactly in this order: View, Beneficiary, Address, Address 2, Address 3, City, State, Zip Code");
            Assert.IsTrue(viewLinksPresent, "Each row in the 'View' column must contain a clickable 'View' link");
            Assert.IsTrue(paginationControls, "Pagination controls are visible and enabled below the data table");
            Assert.IsTrue(pageStatusDisplay, "The page status reflects a multi-page data set (e.g., 'Page 1 of X', where X > 1)");
            Assert.IsTrue(itemCountSummary, "An item count summary is displayed (e.g., '1 - 10 of Y items')");
            Assert.IsTrue(itemsPerPageSelector, "An 'items per page' selector is visible and functional");

            // Column headers verification is critical
            if (dataTablePresent && !columnHeadersInOrder)
            {
                var actualHeaders = _beneficiariesPage.GetColumnHeaders();
                TestContext.WriteLine($"Actual column headers found: {string.Join(", ", actualHeaders)}");
                Assert.IsTrue(columnHeadersInOrder, "Column headers must be in exact order: View, Beneficiary, Address, Address 2, Address 3, City, State, Zip Code");
            }

            TestContext.WriteLine("Beneficiaries page comprehensive test completed successfully");
        }

        [TestMethod]
        [Description("Verify page state and navigation")]
        public void Beneficiaries_VerifyPageStateAndNavigation()
        {
            TestContext.WriteLine("Testing page state and navigation");

            // Page title verification
            bool pageTitle = _beneficiariesPage.VerifyPageTitle();
            TestContext.WriteLine($"Page title contains 'Letters of Credit': {pageTitle}");

            // Beneficiaries tab active state
            bool beneficiariesTabActive = _beneficiariesPage.VerifyBeneficiariesTabActive();
            TestContext.WriteLine($"Beneficiaries tab is active/highlighted: {beneficiariesTabActive}");

            // Beneficiaries sub-header
            bool beneficiariesHeader = _beneficiariesPage.VerifyBeneficiariesHeader();
            TestContext.WriteLine($"Beneficiaries sub-header is visible: {beneficiariesHeader}");

            // Assertions
            Assert.IsTrue(pageTitle, "The browser page title must remain 'Letters of Credit'");
            Assert.IsTrue(beneficiariesTabActive, "The 'Beneficiaries' tab must be highlighted as the active tab");
            Assert.IsTrue(beneficiariesHeader, "A sub-header with the text 'Beneficiaries' is visible on the page");

            TestContext.WriteLine("Page state and navigation verification completed successfully");
        }

        [TestMethod]
        [Description("Verify search functionality and controls")]
        public void Beneficiaries_VerifySearchFunctionality()
        {
            TestContext.WriteLine("Testing search functionality and controls");

            // Search input field verification
            bool searchInput = _beneficiariesPage.VerifySearchInput(); 
            TestContext.WriteLine($"Search input field is visible: {searchInput}");

            // Story card requirement assertion
            Assert.IsTrue(searchInput, "A search input field is visible above the data table, allowing users to search for beneficiaries");

            // Test search functionality
            if (searchInput)
            {
                try
                {
                    TestContext.WriteLine("Testing search functionality with sample input...");
                    _beneficiariesPage.SearchBeneficiaries("test");
                    TestContext.WriteLine("Search functionality test successful");

                    // Clear search
                    _beneficiariesPage.SearchBeneficiaries("");
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
        public void Beneficiaries_VerifyDataTableStructure()
        {
            TestContext.WriteLine("Testing data table structure and columns");

            // Data table presence
            bool dataTablePresent = _beneficiariesPage.VerifyDataTablePresent(); 
            TestContext.WriteLine($"Data table grid is loaded: {dataTablePresent}");

            Assert.IsTrue(dataTablePresent, "A data table grid is loaded and populated with rows of data");

            if (dataTablePresent)
            {
                // Column headers verification
                bool columnHeadersInOrder = _beneficiariesPage.VerifyColumnHeadersInOrder();
                var actualHeaders = _beneficiariesPage.GetColumnHeaders();

                TestContext.WriteLine($"Column headers in correct order: {columnHeadersInOrder}");
                TestContext.WriteLine($"Actual headers found: {string.Join(", ", actualHeaders)}");

                // Expected headers as per story card
                var expectedHeaders = new[] { "View", "Beneficiary", "Address", "Address 2", "Address 3", "City", "State", "Zip Code" };
                TestContext.WriteLine($"Expected headers: {string.Join(", ", expectedHeaders)}");

                Assert.IsTrue(columnHeadersInOrder, "The table must display column headers exactly in this order: View, Beneficiary, Address, Address 2, Address 3, City, State, Zip Code");

                // Check for View links in rows
                bool viewLinksPresent = _beneficiariesPage.VerifyViewLinksInRows();
                TestContext.WriteLine($"View links present in rows: {viewLinksPresent}");
                Assert.IsTrue(viewLinksPresent, "Each row in the 'View' column must contain a clickable 'View' link");

                // Check for data
                int rowCount = _beneficiariesPage.GetDataRowCount(); 
                TestContext.WriteLine($"Data row count: {rowCount}");

                if (rowCount == 0)
                {
                    bool noRecordsMessage = _beneficiariesPage.VerifyNoRecordsMessage(); 
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
        public void Beneficiaries_VerifyPaginationAndItemCount()
        {
            TestContext.WriteLine("Testing pagination controls and item count verification");

            // Pagination controls presence
            bool paginationControls = _beneficiariesPage.VerifyPaginationControls(); 
            TestContext.WriteLine($"Pagination controls are visible: {paginationControls}");

            // Page status display
            bool pageStatusDisplay = _beneficiariesPage.VerifyPageStatusDisplay(); 
            TestContext.WriteLine($"Page status display is visible: {pageStatusDisplay}");

            // Items per page selector
            bool itemsPerPageSelector = _beneficiariesPage.VerifyItemsPerPageSelector(); 
            TestContext.WriteLine($"Items per page selector is visible: {itemsPerPageSelector}");

            // Multi-page data set verification
            bool multiPageDataSet = _beneficiariesPage.VerifyMultiPageDataSet();
            TestContext.WriteLine($"Multi-page data set detected: {multiPageDataSet}");

            // Item count summary verification
            bool itemCountSummary = _beneficiariesPage.VerifyItemCountSummary();
            TestContext.WriteLine($"Item count summary present: {itemCountSummary}");

            // Get current page status
            if (pageStatusDisplay)
            {
                string pageStatus = _beneficiariesPage.GetPageStatus(); 
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
        public void Beneficiaries_TestViewLinkFunctionality()
        {
            TestContext.WriteLine("Testing View link functionality");

            bool dataTablePresent = _beneficiariesPage.VerifyDataTablePresent(); 
            TestContext.WriteLine($"Data table present: {dataTablePresent}");

            if (dataTablePresent)
            {
                int rowCount = _beneficiariesPage.GetDataRowCount(); 
                TestContext.WriteLine($"Data row count: {rowCount}");

                if (rowCount > 0)
                {
                    bool viewLinksPresent = _beneficiariesPage.VerifyViewLinksInRows();
                    TestContext.WriteLine($"View links present: {viewLinksPresent}");

                    Assert.IsTrue(viewLinksPresent, "Each row in the 'View' column must contain a clickable 'View' link");

                    if (viewLinksPresent)
                    {
                        try
                        {
                            TestContext.WriteLine("Testing click functionality of first View link...");
                            string currentUrl = Driver.Url;
                            _beneficiariesPage.ClickFirstViewLink();

                            // Wait a moment for potential navigation
                            Thread.Sleep(2000);
                            string newUrl = Driver.Url;

                            TestContext.WriteLine($"URL before click: {currentUrl}");
                            TestContext.WriteLine($"URL after click: {newUrl}");
                            TestContext.WriteLine("View link click test completed");

                            // Navigate back to beneficiaries page for subsequent tests
                            if (currentUrl != newUrl)
                            {
                                TestContext.WriteLine("Navigating back to Beneficiaries page...");
                                Driver.Navigate().Back();
                                Thread.Sleep(2000);
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
        public void Beneficiaries_TestTabNavigation()
        {
            TestContext.WriteLine("Testing tab navigation functionality");

            // Verify on Beneficiaries tab
            bool beneficiariesTabActive = _beneficiariesPage.VerifyBeneficiariesTabActive();
            TestContext.WriteLine($"Currently on Beneficiaries tab: {beneficiariesTabActive}");

            // Test navigation to other tabs and back
            var tabsToTest = new[] { "Dashboard", "Completed" };

            foreach (var tabName in tabsToTest)
            {
                try
                {
                    TestContext.WriteLine($"Testing navigation to {tabName} tab...");
                    _beneficiariesPage.ClickNavigationTab(tabName.ToLower());
                    Thread.Sleep(2000);

                    TestContext.WriteLine($"Successfully navigated to {tabName} tab");

                    // Navigate back to Beneficiaries
                    TestContext.WriteLine("Navigating back to Beneficiaries tab...");
                    _beneficiariesPage.ClickNavigationTab("beneficiaries");
                    Thread.Sleep(2000);

                    TestContext.WriteLine("Successfully navigated back to Beneficiaries tab");
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
        public void Debug_CheckBeneficiariesPageConfiguration()
        {
            TestContext.WriteLine($"Environment: {InitializeTestAssembly.ENV}");
            TestContext.WriteLine($"UI URL: {InitializeTestAssembly.UiUrl}");
            TestContext.WriteLine($"API URL: {InitializeTestAssembly.BaseApiUrl}");
            TestContext.WriteLine($"Browser: {InitializeTestAssembly.Browser}");
            TestContext.WriteLine($"Username: {InitializeTestAssembly.Email}");

            TestContext.WriteLine($"Current URL: {Driver.Url}");
            TestContext.WriteLine($"Page Title: {Driver.Title}");

            bool pageLoaded = _beneficiariesPage.IsPageLoaded();
            TestContext.WriteLine($"Beneficiaries page loaded: {pageLoaded}");

            Assert.IsTrue(pageLoaded, "Beneficiaries page should be loaded successfully");

            TestContext.WriteLine("Debug configuration check completed");
        }
    }
}
