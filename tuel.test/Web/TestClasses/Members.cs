using TUEL.TestFramework;
using TUEL.TestFramework.Web.PageObjects;
using TUEL.TestFramework.Web.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TUEL.TestFramework.Web.TestClasses
{
    [TestClass, TestCategory("UI")]
    public class Members : Base
    {
        private MembersPOM? _membersPage;
        private MembersPOM MembersPage => _membersPage ?? throw new InvalidOperationException("Members page not initialized");

        [TestInitialize]
        public void MembersTestSetup()
        {
            try
            {
                TestContext?.WriteLine("Initializing Members test components...");
                var driver = Driver ?? throw new InvalidOperationException("Driver not initialized");
                _membersPage = new MembersPOM(driver);

                // Navigation to Members page
                NavigateToMembersPage();

                WaitForMembersPageReady();
                TestContext?.WriteLine("Members test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext?.WriteLine($"Members test setup failed: {ex.Message}");
                TestContext?.WriteLine($"Current URL: {Driver?.Url}");
                TestContext?.WriteLine($"Page Title: {Driver?.Title}");
                throw;
            }
        }

        private void NavigateToMembersPage()
        {
            NavigateToMembersPageAsync().GetAwaiter().GetResult();
        }

        private async Task NavigateToMembersPageAsync()
        {
            try
            {
                var currentUrl = Driver?.Url ?? string.Empty;
                TestContext.WriteLine($"Current URL: {currentUrl}");

                // Check if already on Members page
                if (IsOnMembersPage())
                {
                    TestContext.WriteLine("Already on Members page");
                    return;
                }

                // Try to navigate to Members page directly
                if (currentUrl.Contains("application", StringComparison.OrdinalIgnoreCase))
                {
                    var baseUrl = ExtractBaseUrl(currentUrl);
                    var membersUrl = $"{baseUrl}/members";
                    TestContext.WriteLine($"Attempting direct navigation to: {membersUrl}");
                    Driver?.Navigate().GoToUrl(membersUrl);
                }
                else
                {
                    try
                    {
                    var membersTab = Driver?.FindElement(By.XPath("//a[contains(text(), 'Members')] | //a[@href='/members']"));
                    if (membersTab?.Displayed == true)
                        {
                            TestContext?.WriteLine("Clicking Members navigation tab");
                            membersTab.Click();
                        }
                    }
                    catch (Exception ex)
                    {
                        TestContext?.WriteLine($"Members navigation click failed: {ex.Message}");
                        // Fall back to direct URL
                        var membersUrl = $"{InitializeTestAssembly.UiUrl}/members";
                        TestContext?.WriteLine($"Falling back to direct URL: {membersUrl}");
                        Driver?.Navigate().GoToUrl(membersUrl);
                    }
                }

                // Wait for navigation to complete
                var navigated = Driver?.WaitForPageTransition(TimeSpan.FromSeconds(5)) == true ||
                                 Driver?.WaitForUrlContains("/members", TimeSpan.FromSeconds(5)) == true;

                if (!navigated)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }

                TestContext?.WriteLine($"Navigation completed. Current URL: {Driver?.Url}");
            }
            catch (Exception ex)
            {
                TestContext?.WriteLine($"Members page navigation failed: {ex.Message}");
                throw;
            }
        }

        private string ExtractBaseUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return $"{uri.Scheme}://{uri.Host}{(uri.Port != 80 && uri.Port != 443 ? $":{uri.Port}" : "")}";
            }
            catch
            {
                if (url.Contains(".net"))
                {
                    return url.Substring(0, url.IndexOf(".net") + 4);
                }
                return url.Split('/')[0] + "//" + url.Split('/')[2];
            }
        }

        private bool IsOnMembersPage()
        {
            try
            {
                var currentUrl = Driver?.Url ?? string.Empty;
                return currentUrl.Contains("/members", StringComparison.OrdinalIgnoreCase) ||
                       MembersPage.IsPageLoaded();
            }
            catch
            {
                return false;
            }
        }

        private void WaitForMembersPageReady()
        {
            WaitForMembersPageReadyAsync().GetAwaiter().GetResult();
        }

        private async Task WaitForMembersPageReadyAsync()
        {
            const int maxRetries = 3;
            var retryDelay = TimeSpan.FromSeconds(2);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    TestContext.WriteLine($"Waiting for Members page readiness (attempt {attempt}/{maxRetries})");

                    // Wait for page transition
                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));

                    // Try to wait for Members-specific elements
                    MembersPage.WaitUntilPageIsLoaded(TimeSpan.FromSeconds(10));

                    TestContext.WriteLine("Members page is ready");
                    return;
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Members page readiness check failed (attempt {attempt}): {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        TestContext.WriteLine($"Retrying in {retryDelay.TotalSeconds:F1}s...");
                        await Task.Delay(retryDelay);
                    }
                    else
                    {
                        TestContext.WriteLine("Members page readiness timeout - proceeding with tests");
                    }
                }
            }
        }

        [TestMethod]
        [Description("Comprehensive Members page verification - verifies all major components according to requirements")]
        public void Members_SmokeTest_AllComponentsPresent()
        {
            TestContext?.WriteLine("Starting comprehensive Members page verification");

            // Page State & Navigation Verification
            var membersPage = MembersPage;

            bool pageTitle = MembersPage.VerifyPageTitle();
            bool pageHeader = MembersPage.VerifyPageHeader();
            bool membersTabActive = MembersPage.VerifyMembersTabActive();
            bool pageLayout = MembersPage.VerifyMembersPageLayout();

            TestContext?.WriteLine($"Page Title Contains 'Members': {pageTitle}");
            TestContext?.WriteLine($"Members Page Header Present: {pageHeader}");
            TestContext?.WriteLine($"Members Tab Active in Navigation: {membersTabActive}");
            TestContext?.WriteLine($"Members Page Layout Proper: {pageLayout}");

            // Search Functionality Verification
            bool searchVisible = MembersPage.VerifySearchInput();
            bool searchPlaceholder = MembersPage.VerifySearchPlaceholder();
            bool searchAcceptsText = MembersPage.VerifySearchInputAcceptsText();

            TestContext.WriteLine($"Search Input Box Visible: {searchVisible}");
            TestContext.WriteLine($"Search Placeholder Proper: {searchPlaceholder}");
            TestContext.WriteLine($"Search Box Accepts Text: {searchAcceptsText}");

            // Data Table Structure Verification
            bool dataTablePresent = MembersPage.VerifyDataTablePresent();
            bool tableHeaders = MembersPage.VerifyTableColumnHeaders();
            bool viewButtons = MembersPage.VerifyViewButtonsInRows();
            bool memberDataContent = MembersPage.VerifyMemberDataContent();
            bool addressInfo = MembersPage.VerifyAddressInformation();

            TestContext.WriteLine($"Data Table Present: {dataTablePresent}");
            TestContext.WriteLine($"Table Column Headers Valid: {tableHeaders}");
            TestContext.WriteLine($"View Buttons in Rows: {viewButtons}");
            TestContext.WriteLine($"Member Data Content Valid: {memberDataContent}");
            TestContext.WriteLine($"Address Information Present: {addressInfo}");

            // Pagination Controls Verification
            bool paginationVisible = MembersPage.VerifyPaginationControls();
            bool arrowButtons = MembersPage.VerifyPaginationArrowButtons();
            bool pageNumbers = MembersPage.VerifyPageNumberDisplay();
            bool itemsPerPage = MembersPage.VerifyItemsPerPageSelector();
            bool totalItemCount = MembersPage.VerifyPageStatusDisplay();

            TestContext.WriteLine($"Pagination Controls Visible: {paginationVisible}");
            TestContext.WriteLine($"Pagination Arrow Buttons Present: {arrowButtons}");
            TestContext.WriteLine($"Page Numbers Display: {pageNumbers}");
            TestContext.WriteLine($"Items Per Page Selector: {itemsPerPage}");
            TestContext.WriteLine($"Total Item Count Display: {totalItemCount}");

            // Export Functionality Verification
            bool exportVisible = membersPage.VerifyExportButtonVisible();
            bool exportClickable = membersPage.VerifyExportButtonClickable();

            TestContext.WriteLine($"Export Button Visible: {exportVisible}");
            TestContext.WriteLine($"Export Button Clickable: {exportClickable}");

            // Display column headers for reference
            var columnHeaders = MembersPage.GetColumnHeaders();
            if (columnHeaders.Any())
            {
                TestContext.WriteLine($"Found Column Headers: {string.Join(", ", columnHeaders)}");
            }

            // Display current data statistics
            int rowCount = MembersPage.GetDataRowCount();
            string pageInfo = membersPage.GetPageStatusInfo();
            TestContext.WriteLine($"Current Data Row Count: {rowCount}");
            TestContext.WriteLine($"Page Status Info: {pageInfo}");

            // Evaluate results based on requirement categories
            var criticalChecks = new[] { pageHeader, dataTablePresent, searchVisible };
            var importantChecks = new[] { pageTitle, membersTabActive, tableHeaders, paginationVisible };
            var optionalChecks = new[] { pageLayout, searchPlaceholder, viewButtons, exportVisible };

            int criticalPassed = criticalChecks.Count(c => c);
            int importantPassed = importantChecks.Count(c => c);
            int optionalPassed = optionalChecks.Count(c => c);

            TestContext.WriteLine($"Critical checks passed: {criticalPassed}/{criticalChecks.Length}");
            TestContext.WriteLine($"Important checks passed: {importantPassed}/{importantChecks.Length}");
            TestContext.WriteLine($"Optional checks passed: {optionalPassed}/{optionalChecks.Length}");

            // Assertions based on requirements
            Assert.IsTrue(criticalPassed >= 2, $"Critical Members page elements should be present, but only {criticalPassed} passed");
            Assert.IsTrue(importantPassed >= 3, $"Important Members page elements should be present, but only {importantPassed} passed");

            TestContext.WriteLine("Comprehensive Members page verification completed successfully");
        }

        [TestMethod]
        [Description("Verify page title and main header elements")]
        public void Members_VerifyPageStateAndNavigation()
        {
            TestContext.WriteLine("Testing Members page state and navigation");

            var membersPage = MembersPage;

            bool pageTitle = membersPage.VerifyPageTitle();
            bool pageHeader = membersPage.VerifyPageHeader();
            bool membersTabActive = membersPage.VerifyMembersTabActive();
            bool pageLayout = membersPage.VerifyMembersPageLayout();

            TestContext.WriteLine($"Page title contains 'Members': {pageTitle}");
            TestContext.WriteLine($"Members section header visible: {pageHeader}");
            TestContext.WriteLine($"Members tab highlighted as active: {membersTabActive}");
            TestContext.WriteLine($"Main content area displays proper layout: {pageLayout}");

            // Page header
            Assert.IsTrue(pageHeader, "Members page header should be visible");

            // At least one navigation indicator should be present
            Assert.IsTrue(pageTitle || membersTabActive, "Page title or active tab should indicate Members page");

            TestContext.WriteLine("Page state and navigation verification completed");
        }

        [TestMethod]
        [Description("Verify search functionality including input box, placeholder, and text acceptance")]
        public void Members_VerifySearchFunctionality()
        {
            var membersPage = MembersPage;
            TestContext.WriteLine("Testing Members search functionality");

            bool searchVisible = MembersPage.VerifySearchInput();
            bool searchPlaceholder = MembersPage.VerifySearchPlaceholder();
            bool searchAcceptsText = MembersPage.VerifySearchInputAcceptsText("test");

            TestContext.WriteLine($"Search input box visible: {searchVisible}");
            TestContext.WriteLine($"Search placeholder text proper: {searchPlaceholder}");
            TestContext.WriteLine($"Search box accepts text input: {searchAcceptsText}");

            // Search functionality
            Assert.IsTrue(searchVisible, "Search input box should be visible");
            Assert.IsTrue(searchPlaceholder, "Search box should have proper placeholder text");
            Assert.IsTrue(searchAcceptsText, "Search box should accept text input");

            TestContext.WriteLine("Search functionality verification completed");
        }

        [TestMethod]
        [Description("Verify data table structure including columns and content")]
        public void Members_VerifyDataTableStructure()
        {
            var membersPage = MembersPage;
            TestContext.WriteLine("Testing Members data table structure");

            bool dataTablePresent = MembersPage.VerifyDataTablePresent();
            bool tableHeaders = MembersPage.VerifyTableColumnHeaders();
            bool viewButtons = MembersPage.VerifyViewButtonsInRows();
            bool memberDataContent = MembersPage.VerifyMemberDataContent();
            bool addressInfo = MembersPage.VerifyAddressInformation();

            var columnHeaders = MembersPage.GetColumnHeaders();
            int rowCount = MembersPage.GetDataRowCount();

            TestContext.WriteLine($"Data table present: {dataTablePresent}");
            TestContext.WriteLine($"Table column headers valid: {tableHeaders}");
            TestContext.WriteLine($"View buttons in rows: {viewButtons}");
            TestContext.WriteLine($"Member data content valid: {memberDataContent}");
            TestContext.WriteLine($"Address information formatted: {addressInfo}");
            TestContext.WriteLine($"Column count: {columnHeaders.Count}");
            TestContext.WriteLine($"Data row count: {rowCount}");

            if (columnHeaders.Any())
            {
                TestContext.WriteLine($"Column headers found: {string.Join(", ", columnHeaders)}");
            }

            // Table structure
            Assert.IsTrue(dataTablePresent, "Data table with member information should be displayed");
            Assert.IsTrue(tableHeaders, "Table should have proper column headers");

            if (rowCount > 0)
            {
                Assert.IsTrue(viewButtons, "Each row should contain a View button/link");
                TestContext.WriteLine($"Data verification: Found {rowCount} rows with proper structure");
            }
            else
            {
                TestContext.WriteLine("No data rows found - may be environment-specific");
            }

            TestContext.WriteLine("Data table structure verification completed");
        }

        [TestMethod]
        [Description("Verify pagination controls including navigation and status display")]
        public void Members_VerifyPaginationControls()
        {
            TestContext.WriteLine("Testing Members pagination controls");

            bool paginationVisible = MembersPage.VerifyPaginationControls();
            bool arrowButtons = MembersPage.VerifyPaginationArrowButtons();
            bool pageNumbers = MembersPage.VerifyPageNumberDisplay();
            bool itemsPerPage = MembersPage.VerifyItemsPerPageSelector();
            bool totalItemCount = MembersPage.VerifyPageStatusDisplay();

            string pageInfo = MembersPage.GetPageStatusInfo();
            int rowCount = MembersPage.GetDataRowCount();

            TestContext.WriteLine($"Pagination controls visible: {paginationVisible}");
            TestContext.WriteLine($"Arrow buttons (first, previous, next, last): {arrowButtons}");
            TestContext.WriteLine($"Page numbers displayed: {pageNumbers}");
            TestContext.WriteLine($"Items per page selector available: {itemsPerPage}");
            TestContext.WriteLine($"Total item count displayed: {totalItemCount}");
            TestContext.WriteLine($"Page status info: {pageInfo}");
            TestContext.WriteLine($"Current row count: {rowCount}");

            if (rowCount > 0)
            {
                // If there's data, pagination controls should be present
                Assert.IsTrue(paginationVisible, "Pagination controls should be visible when data is present");
            }
            else
            {
                TestContext.WriteLine("No data present - pagination controls may not be needed");
            }

            TestContext.WriteLine("Pagination controls verification completed");
        }

        [TestMethod]
        [Description("Verify export functionality including button visibility and interaction")]
        public void Members_VerifyExportFunctionality()
        {
            TestContext.WriteLine("Testing Members export functionality");

            bool exportVisible = MembersPage.VerifyExportButtonVisible();
            bool exportClickable = MembersPage.VerifyExportButtonClickable();

            TestContext.WriteLine($"Export to CSV button visible: {exportVisible}");
            TestContext.WriteLine($"Export button clickable and positioned: {exportClickable}");

            // Export functionality
            Assert.IsTrue(exportVisible, "Export to CSV button should be visible in the top-right area");
            Assert.IsTrue(exportClickable, "Export button should be clickable and properly positioned");

            // Test clicking the export button (without verifying download)
            try
            {
                    MembersPage.ClickExportButton();
                TestContext.WriteLine("Export button click successful");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Export button interaction failed: {ex.Message}");
            }

            TestContext.WriteLine("Export functionality verification completed");
        }

        [TestMethod]
        [Description("Test search results verification including specific searches")]
        public void Members_VerifySearchResults()
        {
            TestContext.WriteLine("Testing Members search results");

            // Test search for "bank" (should return security-related institutions)
            bool bankSearch = false;
            try
            {
                bankSearch = MembersPage.VerifyBankSearch();
                TestContext.WriteLine($"Search for 'bank' returned relevant results: {bankSearch}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Bank search failed: {ex.Message}");
            }

            // Test search for specific account number "123"
            bool accountSearch = false;
            try
            {
                accountSearch = MembersPage.VerifyAccountNumberSearch("123");
                TestContext.WriteLine($"Search for account '123' returned exact match: {accountSearch}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Account search failed: {ex.Message}");
            }

            // Test general search results update
            bool searchUpdates = false;
            try
            {
                searchUpdates = MembersPage.VerifySearchResultsUpdate("test");
                TestContext.WriteLine($"Search results update table content appropriately: {searchUpdates}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Search results update test failed: {ex.Message}");
            }

            // Clear search to restore original state
            try
            {
                    MembersPage.ClearSearch();
                    Driver?.WaitForPageTransition(TimeSpan.FromSeconds(3));
                    TestContext.WriteLine("Search cleared successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Search clear failed: {ex.Message}");
            }

            // At least one search test should work
            Assert.IsTrue(bankSearch || accountSearch || searchUpdates,
                "At least one search functionality should work properly");

            TestContext.WriteLine("Search results verification completed");
        }

        [TestMethod]
        [Description("Test View button functionality in table rows")]
        public void Members_TestViewButtonFunctionality()
        {
            TestContext.WriteLine("Testing View button functionality");

            bool viewButtons = MembersPage.VerifyViewButtonsInRows();
            int rowCount = MembersPage.GetDataRowCount();

            TestContext.WriteLine($"View buttons present in rows: {viewButtons}");
            TestContext.WriteLine($"Total rows available: {rowCount}");

            if (rowCount > 0 && viewButtons)
            {
                try
                {
                    // Test clicking the first View button
                    string currentUrl = Driver.Url;
                    MembersPage.ClickViewButton(0);

                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));

                    string newUrl = Driver.Url;
                    TestContext.WriteLine($"Original URL: {currentUrl}");
                    TestContext.WriteLine($"After View click URL: {newUrl}");

                    bool navigationOccurred = !newUrl.Equals(currentUrl, StringComparison.OrdinalIgnoreCase);
                    TestContext.WriteLine($"Navigation occurred after View click: {navigationOccurred}");

                    if (navigationOccurred)
                    {
                        NavigateToMembersPage();
                        WaitForMembersPageReady();
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"View button click test failed: {ex.Message}");
                }
            }
            else
            {
                TestContext.WriteLine("No data rows or View buttons available for testing");
            }

            Assert.IsTrue(rowCount == 0 || viewButtons, "View buttons should be present when data rows exist");

            TestContext.WriteLine("View button functionality testing completed");
        }

        [TestMethod]
        [Description("Verify Members page loads correctly and all essential elements are present")]
        public void Members_VerifyPageLoading()
        {
            TestContext.WriteLine("Testing Members page loading and essential elements");

            bool pageLoaded = MembersPage.IsPageLoaded();
            bool pageHeader = MembersPage.VerifyPageHeader();
            bool dataTable = MembersPage.VerifyDataTablePresent();
            bool searchInput = MembersPage.VerifySearchInput();

            TestContext.WriteLine($"Members page loaded successfully: {pageLoaded}");
            TestContext.WriteLine($"Page header present: {pageHeader}");
            TestContext.WriteLine($"Data table present: {dataTable}");
            TestContext.WriteLine($"Search input visible: {searchInput}");

            string currentUrl = Driver.Url;
            string pageTitle = Driver.Title;
            TestContext.WriteLine($"Current URL: {currentUrl}");
            TestContext.WriteLine($"Page Title: {pageTitle}");

            // Essential elements for Members page
            Assert.IsTrue(pageLoaded, "Members page should load successfully with essential elements");
            Assert.IsTrue(pageHeader, "Members page header should be present");
            Assert.IsTrue(dataTable, "Data table should be present on Members page");

            TestContext.WriteLine("Page loading verification completed");
        }

        [TestMethod]
        [Description("Debug test to check Members page configuration and element detection")]
        public void Debug_CheckMembersPageElements()
        {
            TestContext.WriteLine($"Environment: {InitializeTestAssembly.ENV}");
            TestContext.WriteLine($"UI URL: {InitializeTestAssembly.UiUrl}");
            TestContext.WriteLine($"Browser: {InitializeTestAssembly.Browser}");

            TestContext.WriteLine($"Current URL: {Driver.Url}");
            TestContext.WriteLine($"Page Title: {Driver.Title}");

            // Check all major element categories
            var elementChecks = new Dictionary<string, bool>
            {
                ["Page Header"] = MembersPage.VerifyPageHeader(),
                ["Data Table"] = MembersPage.VerifyDataTablePresent(),
                ["Search Input"] = MembersPage.VerifySearchInput(),
                ["Export Button"] = MembersPage.VerifyExportButtonVisible(),
                ["Pagination"] = MembersPage.VerifyPaginationControls(),
                ["Members Tab Active"] = MembersPage.VerifyMembersTabActive(),
                ["Table Headers"] = MembersPage.VerifyTableColumnHeaders()
            };

            foreach (var check in elementChecks)
            {
                TestContext.WriteLine($"{check.Key}: {check.Value}");
            }

            var columnHeaders = MembersPage.GetColumnHeaders();
            TestContext.WriteLine($"Column Headers Count: {columnHeaders.Count}");
            if (columnHeaders.Any())
            {
                TestContext.WriteLine($"Column Headers: {string.Join(" | ", columnHeaders)}");
            }

            int rowCount = MembersPage.GetDataRowCount();
            string pageInfo = MembersPage.GetPageStatusInfo();
            TestContext.WriteLine($"Data Rows: {rowCount}");
            TestContext.WriteLine($"Page Info: {pageInfo}");

            bool overallPageHealth = elementChecks.Values.Count(v => v) >= 4;
            TestContext.WriteLine($"Overall Page Health: {overallPageHealth}");

            Assert.IsTrue(overallPageHealth, "Members page should have most essential elements working");

            TestContext.WriteLine("Debug element check completed");
        }
    }
}
