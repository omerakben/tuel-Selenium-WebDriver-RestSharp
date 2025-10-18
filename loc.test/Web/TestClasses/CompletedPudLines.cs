using loc.test.Web.PageObjectFiles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;

namespace loc.test.Web.TestClasses
{
    // Test class validating the Completed > PUD Lines sub-tab.
    [TestClass, TestCategory("UI"), TestCategory("Completed"), TestCategory("PudLines")]
    public class CompletedPudLines : Base
    {
        private CompletedPOM? _completedPage;
        private PudLinesPOM? _pudLinesPage;
        private DashboardPOM? _dashboardPage;
        private LoginPOM? _loginPage;

        [TestInitialize]
        public void PudLinesTestSetup()
        {
            try
            {
                TestContext.WriteLine("Initializing PUD Lines test components...");
                _completedPage = new CompletedPOM(Driver!);
                _pudLinesPage = new PudLinesPOM(Driver!);
                _dashboardPage = new DashboardPOM(Driver!);
                _loginPage = new LoginPOM(Driver!);

                if (!string.IsNullOrEmpty(InitializeTestAssembly.Email) && !string.IsNullOrEmpty(InitializeTestAssembly.Password))
                {
                    var authState = _loginPage!.GetCurrentAuthenticationState();
                    TestContext.WriteLine($"Current authentication state: {authState}");
                    if (authState != AuthenticationState.LoggedIn)
                    {
                        _loginPage.LoginToApplication(InitializeTestAssembly.Email, InitializeTestAssembly.Password);
                        TestContext.WriteLine("Authentication completed");
                    }

                    NavigateToCompletedPudLines();
                }
                else
                {
                    TestContext.WriteLine("No credentials provided; navigation actions may fail.");
                }

                _pudLinesPage!.WaitForPudLinesPage();
                TestContext.WriteLine("PUD Lines test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"PUD Lines test setup failed: {ex.Message}");
                TestContext.WriteLine($"Current URL: {Driver!.Url}");
                TestContext.WriteLine($"Page Title: {Driver.Title}");
                throw;
            }
        }

        private void NavigateToCompletedPudLines()
        {
            try
            {
                TestContext.WriteLine("Navigating to Completed > PUD Lines tab.");

                if (!_dashboardPage!.VerifyNavigationTabsPresent())
                {
                    var dashboardUrl = $"{InitializeTestAssembly.UiUrl}/letters-of-credit/dashboard";
                    Driver!.Navigate().GoToUrl(dashboardUrl);
                    Thread.Sleep(1500);
                }

                _dashboardPage.ClickCompletedTab();
                TestContext.WriteLine("Clicked Completed tab");

                // Wait for completed indicator
                var completedLoaded = WaitForUrlFragment("/completed", TimeSpan.FromSeconds(10));
                TestContext.WriteLine($"Completed URL detected: {completedLoaded}");

                _pudLinesPage!.ClickPudLinesSubTab();
                TestContext.WriteLine("Clicked PUD Lines sub-tab");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Navigation to PUD Lines failed: {ex.Message}");
                throw;
            }
        }

        [TestMethod]
        [Description("PUD Lines smoke test validating all primary components")]
        public void CompletedPudLines_SmokeTest_AllComponentsPresent()
        {
            TestContext.WriteLine("Starting PUD Lines components test");

            // Page State & Navigation
            bool pageTitle = _pudLinesPage!.VerifyPageTitle();
            bool completedItemsHeader = _pudLinesPage.VerifyCompletedItemsHeader();
            bool completedTabActive = _completedPage!.VerifyCompletedTabActive();
            bool locSubTabVisible = _pudLinesPage.VerifyLettersOfCreditSubTabVisible();

            TestContext.WriteLine($"Page Title Valid: {pageTitle}");
            TestContext.WriteLine($"Completed Items Header: {completedItemsHeader}");
            TestContext.WriteLine($"Completed Tab Active: {completedTabActive}");
            TestContext.WriteLine($"Letters Of Credit Sub-Tab Visible: {locSubTabVisible}");

            // Search & Controls
            bool searchInput = _pudLinesPage.VerifySearchInput();
            bool exportToCsv = _pudLinesPage.VerifyExportToCsvButton();
            TestContext.WriteLine($"Search Input: {searchInput}");
            TestContext.WriteLine($"Export To CSV: {exportToCsv}");

            // Table & Columns
            bool dataTablePresent = _pudLinesPage.VerifyDataTablePresent();
            bool headersInOrder = _pudLinesPage.VerifyPudLinesColumnHeadersInOrder();
            int rowCount = _pudLinesPage.GetDataRowCount();
            TestContext.WriteLine($"Data Table Present: {dataTablePresent}");
            TestContext.WriteLine($"Headers In Order: {headersInOrder}");
            TestContext.WriteLine($"Row Count: {rowCount}");

            bool noRecordsMsg = false;
            if (rowCount == 0)
            {
                noRecordsMsg = _pudLinesPage.VerifyNoRecordsMessage();
                TestContext.WriteLine($"No Records Message: {noRecordsMsg}");
            }

            // Pagination
            bool paginationControls = _pudLinesPage.VerifyPaginationControls();
            bool pageStatusDisplay = _pudLinesPage.VerifyPageStatusDisplay();
            bool itemsPerPageSelector = _pudLinesPage.VerifyItemsPerPageSelector();
            TestContext.WriteLine($"Pagination Controls: {paginationControls}");
            TestContext.WriteLine($"Page Status Display: {pageStatusDisplay}");
            TestContext.WriteLine($"Items Per Page Selector: {itemsPerPageSelector}");

            // Classification
            var critical = new[] { pageTitle, dataTablePresent };
            var important = new[] { completedItemsHeader, searchInput, headersInOrder };
            var optional = new[] { completedTabActive, locSubTabVisible, exportToCsv, paginationControls, pageStatusDisplay, itemsPerPageSelector };

            TestContext.WriteLine($"Critical passed: {critical.Count(x => x)}/{critical.Length}");
            TestContext.WriteLine($"Important passed: {important.Count(x => x)}/{important.Length}");
            TestContext.WriteLine($"Optional passed: {optional.Count(x => x)}/{optional.Length}");

            // Assertions
            Assert.IsTrue(pageTitle, "Page title must contain 'Letters of Credit'");
            Assert.IsTrue(completedItemsHeader, "'Completed Items' header must be visible");
            Assert.IsTrue(completedTabActive, "Completed tab must be active");
            Assert.IsTrue(searchInput, "Search input must be visible above table");
            Assert.IsTrue(dataTablePresent, "Data table must be present");
            Assert.IsTrue(headersInOrder, "Column headers must match the expected order for PUD Lines");

            if (rowCount == 0)
            {
                Assert.IsTrue(noRecordsMsg, "No records message must be visible when the table is empty");
            }

            TestContext.WriteLine("PUD Lines smoke test completed");
        }

        [TestMethod]
        [Description("Verify page state & navigation for PUD Lines")]
        public void CompletedPudLines_VerifyPageStateAndNavigation()
        {
            bool pageTitle = _pudLinesPage!.VerifyPageTitle();
            bool completedTabActive = _completedPage!.VerifyCompletedTabActive();
            bool locTabVisible = _pudLinesPage.VerifyLettersOfCreditSubTabVisible();
            bool header = _pudLinesPage.VerifyCompletedItemsHeader();

            TestContext.WriteLine($"Page Title: {pageTitle}");
            TestContext.WriteLine($"Completed Tab Active: {completedTabActive}");
            TestContext.WriteLine($"LOC Sub-Tab Visible: {locTabVisible}");
            TestContext.WriteLine($"Completed Items Header: {header}");

            Assert.IsTrue(pageTitle, "Title must contain 'Letters of Credit'");
            Assert.IsTrue(completedTabActive, "Completed tab should be active");
            Assert.IsTrue(header, "Completed Items header must be visible");
        }

        [TestMethod]
        [Description("Verify PUD Lines search capability")]
        public void CompletedPudLines_VerifySearchFunctionality()
        {
            bool searchInput = _pudLinesPage!.VerifySearchInput();
            TestContext.WriteLine($"Search Input Visible: {searchInput}");
            Assert.IsTrue(searchInput, "Search input must be visible");
            var terms = new[] { "Submitted", "Approved" };
            foreach (var term in terms)
            {
                try
                {
                    TestContext.WriteLine($"Searching for term: '{term}'");
                    _pudLinesPage.SearchPudLines(term);

                    // Verify filtering applied (must have at least one matching row for these known status terms)
                    var rows = _pudLinesPage.GetAllRowTexts();
                    bool anyMatch = rows.Any(r => r.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
                    TestContext.WriteLine($"Row count after search: {rows.Count}. Any match for '{term}': {anyMatch}");
                    Assert.IsTrue(anyMatch, $"Expected at least one row containing '{term}' after search.");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Search for '{term}' failed: {ex.Message}");
                    Assert.Fail($"Search term '{term}' failed");
                }
            }
            // Clear search at end
            _pudLinesPage.SearchPudLines(string.Empty);
        }

        [TestMethod]
        [Description("Verify PUD Lines table structure & headers")]
        public void CompletedPudLines_VerifyDataTableStructure()
        {
            bool tablePresent = _pudLinesPage!.VerifyDataTablePresent();
            TestContext.WriteLine($"Table Present: {tablePresent}");
            Assert.IsTrue(tablePresent, "Data table must be present");

            var inOrder = _pudLinesPage.VerifyPudLinesColumnHeadersInOrder();
            var actual = _pudLinesPage.GetPudLinesHeaders();
            TestContext.WriteLine($"Headers In Order: {inOrder}");
            TestContext.WriteLine($"Actual Headers: {string.Join(", ", actual)}");
            Assert.IsTrue(inOrder, "Headers must match expected exact order");

            int rows = _pudLinesPage.GetDataRowCount();
            TestContext.WriteLine($"Row Count: {rows}");
            if (rows == 0)
            {
                bool noRecords = _pudLinesPage.VerifyNoRecordsMessage();
                TestContext.WriteLine($"No Records Message: {noRecords}");
                Assert.IsTrue(noRecords, "No records message must display when empty");
            }
        }

        [TestMethod]
        [Description("Verify PUD Lines pagination controls")]
        public void CompletedPudLines_VerifyPaginationControls()
        {
            bool pagination = _pudLinesPage!.VerifyPaginationControls();
            bool pageStatus = _pudLinesPage.VerifyPageStatusDisplay();
            bool itemsSelector = _pudLinesPage.VerifyItemsPerPageSelector();

            TestContext.WriteLine($"Pagination Controls: {pagination}");
            TestContext.WriteLine($"Page Status: {pageStatus}");
            TestContext.WriteLine($"Items Per Page Selector: {itemsSelector}");

            Assert.IsTrue(pagination, "Pagination controls must be visible");
            Assert.IsTrue(pageStatus, "Page status must be visible");
        }

        [TestMethod]
        [Description("Validate items-per-page default is set 10")]
        public void CompletedPudLines_TestItemsPerPageFunctionality()
        {
            bool selectorVisible = _pudLinesPage!.VerifyItemsPerPageSelector();
            TestContext.WriteLine($"Items Per Page Selector Visible: {selectorVisible}");
            Assert.IsTrue(selectorVisible, "Items per page selector must be visible");

            var baselineStatus = _pudLinesPage.GetPageStatus();
            var baselineRows = _pudLinesPage.GetDataRowCount();
            TestContext.WriteLine($"Baseline Status: {baselineStatus}");
            TestContext.WriteLine($"Baseline Rows: {baselineRows}");

            Assert.AreEqual(10, baselineRows, "Default value should be 10 items per page.");
        }

        [TestMethod]
        [Description("Verify Export to CSV presence")]
        public void CompletedPudLines_VerifyExportToCsv()
        {
            bool exportBtn = _pudLinesPage!.VerifyExportToCsvButton();
            TestContext.WriteLine($"Export Button Visible: {exportBtn}");

            Assert.IsTrue(exportBtn, "Export to CSV button must be visible");
        }
        private bool WaitForUrlFragment(string fragment, TimeSpan timeout)
        {
            var end = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < end)
            {
                try
                {
                    if (Driver!.Url.Contains(fragment, StringComparison.OrdinalIgnoreCase)) return true;
                }
                catch
                {
                }
            }
            return false;
        }
    }
}
