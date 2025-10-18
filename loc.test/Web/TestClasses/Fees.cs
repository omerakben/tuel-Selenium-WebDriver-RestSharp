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
    // UI tests for the Fees page
    [TestClass, TestCategory("UI"), TestCategory("Fees")]
    public class Fees : Base
    {
        private DashboardPOM? _dashboardPage;
        private FeesPOM? _feesPage;

    private FeesPOM FeesPage => _feesPage ?? throw new AssertInconclusiveException("FeesPOM not initialized");

        [TestInitialize]
        public void FeesTestSetup()
        {
            try
            {
                if (Driver == null)
                {
                    Assert.Inconclusive("WebDriver not initialized");
                }

                TestContext.WriteLine("Initializing Fees test components...");
                _dashboardPage = new DashboardPOM(Driver);
                _feesPage = new FeesPOM(Driver);

                NavigateToFeesPage();
                WaitForFeesPageReady();
                TestContext.WriteLine("Fees test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Fees test setup failed: {ex.Message}");
                try
                {
                    TestContext.WriteLine($"Current URL: {Driver!.Url}");
                    TestContext.WriteLine($"Page Title: {Driver.Title}");
                }
                catch { }
                throw;
            }
        }

        private void NavigateToFeesPage()
        {
            try
            {
                TestContext.WriteLine("Navigating to Fees tab via dashboard navigation");
                _dashboardPage?.ClickNavigationTab("fees");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Failed to navigate to Fees tab: {ex.Message}");
            }
        }

        private bool IsOnFeesPage() =>
            Driver != null && (FeesPage.VerifyFeeActivitySubHeader() || FeesPage.VerifyFeesTabActive());


        private void WaitForFeesPageReady()
        {
            TestContext.WriteLine("Waiting for Fees page to be ready");
            _feesPage?.WaitForFeesPage(TimeSpan.FromSeconds(10));
            IsOnFeesPage();
            TestContext.WriteLine("Fees page is ready");
        }

        [TestMethod]
        [Description("Fees smoke test: page title, active tab, subheader, buttons, grid with headers, pagination controls")]
        public void Fees_SmokeTest_AllComponentsPresent()
        {
            TestContext.WriteLine("Starting Fees page smoke test (state, controls, grid, pagination)");
            // Page/title & navigation
            bool pageTitle = FeesPage.VerifyPageTitle();
            bool feesTabActive = FeesPage.VerifyFeesTabActive();
            bool feeActivityHeader = FeesPage.VerifyFeeActivitySubHeader();

            TestContext.WriteLine($"Browser title contains 'Business Application': {pageTitle}");
            TestContext.WriteLine($"Fees tab highlighted as active: {feesTabActive}");
            TestContext.WriteLine($"'Fee Activity' sub-header visible: {feeActivityHeader}");

            // Controls
            bool searchVisible = FeesPage.VerifySearchInput();
            bool viewParamsVisible = FeesPage.VerifyViewFeeParametersButton();
            bool exportVisible = FeesPage.VerifyExportToCsvButton();

            TestContext.WriteLine($"Search input visible: {searchVisible}");
            TestContext.WriteLine($"View Fee Parameters button visible: {viewParamsVisible}");
            TestContext.WriteLine($"Export to CSV button visible: {exportVisible}");

            // Grid
            bool gridPresent = FeesPage.VerifyDataTablePresent();
            bool headersExact = FeesPage.VerifyColumnHeadersInExactOrder();

            TestContext.WriteLine($"Data table present: {gridPresent}");
            TestContext.WriteLine($"Column headers match exact expected order: {headersExact}");

            // Pagination
            bool pagerVisible = FeesPage.VerifyPaginationControls();
            bool pageStatusVisible = FeesPage.VerifyPageStatusDisplay();
            bool itemsPerPageVisible = FeesPage.VerifyItemsPerPageSelector();

            TestContext.WriteLine($"Pagination controls visible: {pagerVisible}");
            TestContext.WriteLine($"Page status visible: {pageStatusVisible}");
            TestContext.WriteLine($"Items-per-page selector visible: {itemsPerPageVisible}");

            // Assertions
            Assert.IsTrue(feeActivityHeader, "'Fee Activity' sub-header should be visible");
            Assert.IsTrue(pageTitle || feesTabActive, "Either browser title should indicate app or the Fees tab should be active");
            Assert.IsTrue(searchVisible, "Search input should be visible");
            Assert.IsTrue(viewParamsVisible, "View Fee Parameters button should be visible");
            Assert.IsTrue(exportVisible, "Export to CSV button should be visible");
            Assert.IsTrue(gridPresent, "Data table should be visible");
            Assert.IsTrue(headersExact, "Column headers should match the exact expected order");
            Assert.IsTrue(pagerVisible, "Pagination controls should be visible");
            Assert.IsTrue(pageStatusVisible, "Page status (e.g., 'Page 1 of N') should be visible");
            Assert.IsTrue(itemsPerPageVisible, "Items-per-page selector should be visible");

            TestContext.WriteLine("Fees smoke test completed successfully");

        }

        [TestMethod]
        [Description("Verify search positive: enter a term and expect matching results or empty state renders appropriately")]
        public void Fees_VerifySearch_Positive()
        {
            TestContext.WriteLine("Starting positive search verification on Fees page");
            var initialRows = FeesPage.GetAllRowTexts();
            if (initialRows.Count == 0)
            {
                // If no data exists
                bool noRecords = FeesPage.VerifyNoRecordsMessageVisible();
                TestContext.WriteLine($"No records present initially, empty-state message visible: {noRecords}");
                Assert.IsTrue(noRecords, "No-records message should be visible when there are no rows.");
                return;
            }
            // Use generic helper to derive first-row token & status
            var (token, status) = FeesPage.GetFirstRowTokenAndStatus("Charged", "Denied");
            TestContext.WriteLine($"Derived token: {token ?? "(null)"}; status: {status ?? "(null)"}");
            Assert.IsNotNull(token, "Unable to derive a token from first row.");

            var tokenResult = FeesPage.StrictSearch(token!);
            TestContext.WriteLine($"Token strict search => hasMatch={tokenResult.hasMatch}, emptyState={tokenResult.emptyState}, rows={tokenResult.rowCount}");
            Assert.IsFalse(tokenResult.emptyState, "Empty-state unexpectedly visible after token search (possible BUG).");
            Assert.IsTrue(tokenResult.hasMatch, $"No matching rows found for token '{token}'.");

            Assert.IsNotNull(status, "First row must contain status 'Charged' or 'Denied' for status-based validation.");
            var statusResult = FeesPage.StrictSearch(status!);
            TestContext.WriteLine($"Status strict search => hasMatch={statusResult.hasMatch}, emptyState={statusResult.emptyState}, rows={statusResult.rowCount}");
            Assert.IsFalse(statusResult.emptyState, "Empty-state unexpectedly visible after status search (possible BUG).");
            Assert.IsTrue(statusResult.hasMatch, $"No matching rows found for status '{status}'.");

            TestContext.WriteLine("Positive search verification completed (generic + status).");

        }

        [TestMethod]
        [Description("Verify search negative 'No records available.'")]
        public void Fees_VerifySearch_Negative()
        {
            TestContext.WriteLine("Starting negative search verification on Fees page");
            var unlikely = $"zzq-{Guid.NewGuid():N}";
            FeesPage.SearchFees(unlikely);

            bool noRecords = FeesPage.VerifyNoRecordsMessageVisible();
            TestContext.WriteLine($"Unlikely term: '{unlikely}', empty-state message visible: {noRecords}");

            Assert.IsTrue(noRecords, "No-records message not visible for unlikely search term.");
            TestContext.WriteLine("Negative search verification completed");

        }

        [TestMethod]
        [Description("Verify View Fee Parameters button is clickable")]
        public void Fees_Verify_ViewFeeParameters_Button()
        {
            TestContext.WriteLine("Starting Fee Parameters navigation verification");
            bool buttonVisible = FeesPage.VerifyViewFeeParametersButton();
            TestContext.WriteLine($"View Fee Parameters button visible: {buttonVisible}");
            Assert.IsTrue(buttonVisible, "View Fee Parameters button not visible.");

            FeesPage.ClickViewFeeParameters();
            FeesPage.WaitForFeeParametersPage(TimeSpan.FromSeconds(10));

            // Verify Fee Parameters page header visible
            bool headerVisible = FeesPage.VerifyFeeParametersHeader();
            TestContext.WriteLine($"Fee Parameters header visible after navigation: {headerVisible}");
            Assert.IsTrue(headerVisible, "Fee Parameters page header not visible after navigation.");

            // Navigate back and verify Fees page
            FeesPage.NavigateBackToFees();
            bool backOnFees = FeesPage.VerifyFeeActivitySubHeader();
            TestContext.WriteLine($"Returned to Fees page (Fee Activity header visible): {backOnFees}");
            Assert.IsTrue(backOnFees, "Fee Activity sub-header not visible after navigating back.");
            TestContext.WriteLine("Fee Parameters navigation verification completed");

        }

        [TestMethod]
        [Description("Verify Export to CSV button is visible (and clickable)")]
        public void Fees_Verify_ExportToCsv_Button()
        {
            TestContext.WriteLine("Starting Export to CSV button verification");
            bool exportVisible = FeesPage.VerifyExportToCsvButton();
            TestContext.WriteLine($"Export to CSV button visible: {exportVisible}");
            Assert.IsTrue(exportVisible, "Export to CSV button not visible.");

            // Click without validating download pipeline side-effects
            FeesPage.ClickExportToCsv();
            TestContext.WriteLine("Clicked Export to CSV button");

        }

        [TestMethod]
        [Description("Verify items-per-page selector is visible")]
        public void Fees_Verify_ItemsPerPage_Functionality()
        {

            TestContext.WriteLine("Starting items-per-page functionality verification");
            bool selectorVisible = FeesPage.VerifyItemsPerPageSelector();
            TestContext.WriteLine($"Items-per-page selector visible: {selectorVisible}");
            Assert.IsTrue(selectorVisible, "Items per page selector not visible.");

        }

        [TestMethod]
        [Description("Combined search: first-row DDA token then status (simple strict validation)")]
        public void Fees_VerifySearch_WithActualDataOnFirstRow()
        {
            TestContext.WriteLine("Starting first-row based search verification (strict mode)");
            var initialRows = FeesPage.GetAllRowTexts();
            if (initialRows.Count == 0)
            {
                Assert.Inconclusive("No data present to perform search validation.");
            }

            // First row only
            var dynamicTokens = initialRows.Take(1)
                                           .Select(r => r.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
                                           .Where(s => !string.IsNullOrWhiteSpace(s))
                                           .Distinct(StringComparer.OrdinalIgnoreCase)
                                           .ToList();

            // Status keywords from data
            if (initialRows.Any(r => r.Contains("Active", StringComparison.OrdinalIgnoreCase))) dynamicTokens.Add("Active");
            if (initialRows.Any(r => r.Contains("Inactive", StringComparison.OrdinalIgnoreCase))) dynamicTokens.Add("Inactive");

            var positiveTerms = dynamicTokens.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            TestContext.WriteLine($"Positive terms: {string.Join(", ", positiveTerms)}");

            bool first = true;
            foreach (var term in positiveTerms)
            {
                if (!first)
                {
                    // Clear before switching from DDA to status to let grid repopulate
                    FeesPage.ClearFeesSearch();
                }
                first = false;
                var result = FeesPage.StrictSearch(term!);
                TestContext.WriteLine($"Term '{term}': hasMatch={result.hasMatch}, rowCount={result.rowCount}");
                Assert.IsTrue(result.hasMatch, $"Expected at least one row to contain '{term}'.");
            }

            TestContext.WriteLine("First-row search verification completed.");
        }

        [TestMethod]
        [Description("Lightweight pagination: attempt to navigate to page 2 if available")]
        public void Fees_Verify_Pagination_Page2()
        {
            bool pager = FeesPage.VerifyPaginationControls();
            Assert.IsTrue(pager, "Pagination controls must be visible to test page 2 navigation.");

            var page2Clicked = FeesPage.ClickSecondPageIfAvailable();
            TestContext.WriteLine($"Attempted to click Page 2: {page2Clicked}");
            if (!page2Clicked)
            {
                Assert.Inconclusive("Page 2 not available in current dataset.");
            }
            else
            {
                // Optional light assertion – page status should still be visible
                Assert.IsTrue(FeesPage.VerifyPageStatusDisplay(), "Page status should remain visible after navigating to page 2.");
            }
        }
    }
}
