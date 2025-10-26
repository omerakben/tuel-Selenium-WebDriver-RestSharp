using TUEL.TestFramework.Web.Support;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace TUEL.TestFramework.Web.PageObjects
{
    // Page Object Model for the Transactions > Transaction Details sub-tab.
    // Mirrors patterns used in TransactionsPOM but with its own expected columns and sub-tab locators.
    public class TransactionDetailsPOM : BasePage
    {
        public TransactionDetailsPOM(IWebDriver driver) : base(driver) { }

        // Reuse the Transaction Items header as the primary unique anchor for the page.
        protected override By UniqueLocator => transactionItemsHeader;

        #region Sub-Tab & Page Specific Locators

        // Sub-tabs within Transactions context
        private readonly By transactionDetailsSubTab = By.XPath("//span[contains(text(),'Transaction Details')] | //*[contains(@class,'tab')][contains(text(),'Transaction Details')]");
        private readonly By productRecordsSubTab = By.XPath("//span[contains(text(),'Product Records')]");

        // Header (same union pattern as TransactionsPOM)
        private readonly By transactionItemsHeader = By.XPath("//*[contains(text(), 'Transaction Items')] | //h3[contains(text(), 'Transaction Items')]");

        // Export button
        private readonly By exportToCsvButton = By.XPath("//button[contains(text(), 'Export to CSV')] | //*[contains(text(), 'Export to CSV')]");

        // Transaction Details expected column headers (exact order)
        private readonly string[] expectedTransactionDetailsHeaders = new string[]
        {
            "View", "Account", "Member", "Effective Date", "Expiration Date", "Amount", "Status"
        };

        #endregion

        #region Verification Methods

        public bool VerifyTransactionItemsHeader()
        {
            try
            {
                return IsElementVisible(transactionItemsHeader);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyProductRecordsSubTabVisible()
        {
            try
            {
                return IsElementVisible(productRecordsSubTab);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyExportToCsvButton()
        {
            try
            {
                return IsElementVisible(exportToCsvButton);
            }
            catch
            {
                return false;
            }
        }
        public List<string> GetAllRowTexts()
        {
            try
            {
                var rows = Driver.FindElements(tableRows);
                return rows.Select(r => r.Text?.Trim() ?? string.Empty).Where(t => !string.IsNullOrEmpty(t)).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public bool VerifyTransactionDetailsColumnHeadersInOrder()
        {
            try
            {
                var actual = GetColumnHeaders();
                if (actual.Count != expectedTransactionDetailsHeaders.Length) return false;
                for (int i = 0; i < expectedTransactionDetailsHeaders.Length; i++)
                {
                    if (!string.Equals(actual[i], expectedTransactionDetailsHeaders[i], StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetTransactionDetailsHeaders() => GetColumnHeaders();

        #endregion

        #region Action Methods

        public void ClickTransactionDetailsSubTab()
        {
            try
            {
                var tab = Driver.FindElements(transactionDetailsSubTab).FirstOrDefault();
                if (tab == null) throw new InvalidOperationException("Transaction Details sub-tab not found");
                var previousUrl = Driver.Url;
                tab.Click();
                var transitioned = Driver.WaitForPageTransition(TimeSpan.FromSeconds(5)) ||
                                   Driver.WaitForUrlChange(previousUrl, TimeSpan.FromSeconds(5));

                if (!transitioned)
                {
                    Driver.WaitVisible(dataTable, TimeSpan.FromSeconds(5));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click Transaction Details sub-tab: {ex.Message}");
            }
        }

        public void SearchTransactionDetails(string searchText)
        {
            try
            {
                var searchOk = VerifySearchInput();
                if (!searchOk) throw new InvalidOperationException("Search input not found on Transaction Details page");
                EnterText(searchInput, string.Empty); // clear first
                EnterText(searchInput, searchText);

                Driver.WaitForPageTransition(TimeSpan.FromSeconds(3));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to perform Transaction Details search: {ex.Message}");
            }
        }

        #endregion

        #region Utility Methods

        public bool IsTransactionDetailsPageLoaded()
        {
            try
            {
                return VerifyTransactionItemsHeader();
            }
            catch
            {
                return false;
            }
        }

        public void WaitForTransactionDetailsPage(TimeSpan? timeout = null)
        {
            var mainTimeout = timeout ?? TimeSpan.FromSeconds(10);
            try
            {
                Driver.WaitVisible(UniqueLocator, mainTimeout);
            }
            catch
            {
                try
                {
                    Driver.WaitVisible(dataTable, mainTimeout);
                }
                catch
                {
                }
            }
        }

        public bool VerifySearchApplied(string term)
        {
            try
            {
                var rows = Driver.FindElements(tableRows);
                if (rows.Count == 0) return true; // Empty and no-records message acceptable

                // If any row contains the term success
                return rows.Any(r => (r.Text ?? string.Empty).IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
