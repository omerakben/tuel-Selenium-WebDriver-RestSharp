using TUEL.TestFramework.Web.PageObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;

namespace TUEL.TestFramework.Web.TestClasses
{
    // Test class validating the Transactions > Transaction Details sub-tab.
    [TestClass, TestCategory("UI"), TestCategory("Transactions"), TestCategory("TransactionDetails")]
    public class TransactionDetails : Base
    {
        private TransactionsPOM? _completedPage;
        private TransactionDetailsPOM? _pudLinesPage;
        private DashboardPOM? _dashboardPage;
        private LoginPOM? _loginPage;

        [TestInitialize]
        public void TransactionDetailsTestSetup()
        {
            try
            {
                TestContext.WriteLine("Initializing Transaction Details test components...");
                _completedPage = new TransactionsPOM(Driver!);
                _pudLinesPage = new TransactionDetailsPOM(Driver!);
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

                    NavigateToTransactionDetails();
                }
                else
                {
                    TestContext.WriteLine("No credentials provided; navigation actions may fail.");
                }

                _pudLinesPage!.WaitForTransactionDetailsPage();
                TestContext.WriteLine("Transaction Details test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Transaction Details test setup failed: {ex.Message}");
                TestContext.WriteLine($"Current URL: {Driver!.Url}");
                TestContext.WriteLine($"Page Title: {Driver.Title}");
                throw;
            }
        }

        private void NavigateToTransactionDetails()
        {
            try
            {
                TestContext.WriteLine("Navigating to Transactions > Transaction Details tab.");

                if (!_dashboardPage!.VerifyNavigationTabsPresent())
                {
                    var dashboardUrl = $"{InitializeTestAssembly.UiUrl}/application/dashboard";
                    Driver!.Navigate().GoToUrl(dashboardUrl);
                    Thread.Sleep(1500);
                }

                _dashboardPage.ClickTransactionsTab();
                TestContext.WriteLine("Clicked Transactions tab");

                // Wait for transactions indicator
                var transactionsLoaded = WaitForUrlFragment("/transactions", TimeSpan.FromSeconds(10));
                TestContext.WriteLine($"Transactions URL detected: {transactionsLoaded}");

                _pudLinesPage!.ClickTransactionDetailsSubTab();
                TestContext.WriteLine("Clicked Transaction Details sub-tab");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Navigation to Transaction Details failed: {ex.Message}");
                throw;
            }
        }

        [TestMethod]
        [Description("Transaction Details smoke test validating all primary components")]
        public void TransactionDetails_SmokeTest_AllComponentsPresent()
        {
            TestContext.WriteLine("Starting Transaction Details components test");

            // Page State & Navigation
            bool pageTitle = _pudLinesPage!.VerifyPageTitle();
            bool transactionItemsHeader = _pudLinesPage.VerifyTransactionItemsHeader();
            bool transactionsTabActive = _completedPage!.VerifyTransactionsTabActive();
            bool productSubTabVisible = _pudLinesPage.VerifyProductRecordsSubTabVisible();

            TestContext.WriteLine($"Page Title Valid: {pageTitle}");
            TestContext.WriteLine($"Transaction Items Header: {transactionItemsHeader}");
            TestContext.WriteLine($"Transactions Tab Active: {transactionsTabActive}");
            TestContext.WriteLine($"Product Records Sub-Tab Visible: {productSubTabVisible}");

            // Search & Controls
            bool searchInput = _pudLinesPage.VerifySearchInput();
            bool exportToCsv = _pudLinesPage.VerifyExportToCsvButton();
            TestContext.WriteLine($"Search Input: {searchInput}");
            TestContext.WriteLine($"Export To CSV: {exportToCsv}");

            // Table & Columns
            bool dataTablePresent = _pudLinesPage.VerifyDataTablePresent();
            bool headersInOrder = _pudLinesPage.VerifyTransactionDetailsColumnHeadersInOrder();
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
            var important = new[] { transactionItemsHeader, searchInput, headersInOrder };
            var optional = new[] { transactionsTabActive, productSubTabVisible, exportToCsv, paginationControls, pageStatusDisplay, itemsPerPageSelector };

            TestContext.WriteLine($"Critical passed: {critical.Count(x => x)}/{critical.Length}");
            TestContext.WriteLine($"Important passed: {important.Count(x => x)}/{important.Length}");
            TestContext.WriteLine($"Optional passed: {optional.Count(x => x)}/{optional.Length}");

            // Assertions
            Assert.IsTrue(pageTitle, "Page title must contain 'Application'");
            Assert.IsTrue(transactionItemsHeader, "'Transaction Items' header must be visible");
            Assert.IsTrue(transactionsTabActive, "Transactions tab must be active");
            Assert.IsTrue(searchInput, "Search input must be visible above table");
            Assert.IsTrue(dataTablePresent, "Data table must be present");
            Assert.IsTrue(headersInOrder, "Column headers must match the expected order for Transaction Details");

            if (rowCount == 0)
            {
                Assert.IsTrue(noRecordsMsg, "No records message must be visible when the table is empty");
            }

            TestContext.WriteLine("Transaction Details smoke test completed");
        }

        [TestMethod]
        [Description("Verify page state & navigation for Transaction Details")]
        public void TransactionDetails_VerifyPageStateAndNavigation()
        {
            bool pageTitle = _pudLinesPage!.VerifyPageTitle();
            bool transactionsTabActive = _completedPage!.VerifyTransactionsTabActive();
            bool productTabVisible = _pudLinesPage.VerifyProductRecordsSubTabVisible();
            bool header = _pudLinesPage.VerifyTransactionItemsHeader();

            TestContext.WriteLine($"Page Title: {pageTitle}");
            TestContext.WriteLine($"Transactions Tab Active: {transactionsTabActive}");
            TestContext.WriteLine($"Product Sub-Tab Visible: {productTabVisible}");
            TestContext.WriteLine($"Transaction Items Header: {header}");

            Assert.IsTrue(pageTitle, "Title must contain 'Application'");
            Assert.IsTrue(transactionsTabActive, "Transactions tab should be active");
            Assert.IsTrue(header, "Transaction Items header must be visible");
        }

        [TestMethod]
        [Description("Verify Transaction Details search capability")]
        public void TransactionDetails_VerifySearchFunctionality()
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
                    _pudLinesPage.SearchTransactionDetails(term);

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
            _pudLinesPage.SearchTransactionDetails(string.Empty);
        }

        [TestMethod]
        [Description("Verify Transaction Details table structure & headers")]
        public void TransactionDetails_VerifyDataTableStructure()
        {
            bool tablePresent = _pudLinesPage!.VerifyDataTablePresent();
            TestContext.WriteLine($"Table Present: {tablePresent}");
            Assert.IsTrue(tablePresent, "Data table must be present");

            var inOrder = _pudLinesPage.VerifyTransactionDetailsColumnHeadersInOrder();
            var actual = _pudLinesPage.GetTransactionDetailsHeaders();
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
        [Description("Verify Transaction Details pagination controls")]
        public void TransactionDetails_VerifyPaginationControls()
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
        public void TransactionDetails_TestItemsPerPageFunctionality()
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
        public void TransactionDetails_VerifyExportToCsv()
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
