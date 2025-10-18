using loc.test.Web.Support;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace loc.test.Web.PageObjectFiles
{
    // Page Object Model for the Completed > PUD Lines sub-tab.
    // Mirrors patterns used in CompletedPOM but with its own expected columns and sub-tab locators.
    public class PudLinesPOM : BasePage
    {
        public PudLinesPOM(IWebDriver driver) : base(driver) { }

        // Reuse the Completed Items header as the primary unique anchor for the page.
        protected override By UniqueLocator => completedItemsHeader;

        #region Sub-Tab & Page Specific Locators

        // Sub-tabs within Completed context
        private readonly By pudLinesSubTab = By.XPath("//span[contains(text(),'PUD Lines')] | //*[contains(@class,'tab')][contains(text(),'PUD Lines')]");
        private readonly By lettersOfCreditSubTab = By.XPath("//span[contains(text(),'Letters of Credit')]");

        // Header (same union pattern as CompletedPOM)
        private readonly By completedItemsHeader = By.XPath("//*[contains(text(), 'Completed Items')] | //h3[contains(text(), 'Completed Items')]");

        // Export button
        private readonly By exportToCsvButton = By.XPath("//button[contains(text(), 'Export to CSV')] | //*[contains(text(), 'Export to CSV')]");

        // PUD Lines expected column headers (exact order)
        private readonly string[] expectedPudLinesHeaders = new string[]
        {
            "View", "DDA", "Member", "Effective Date", "Expiration Date", "Amount", "Status"
        };

        #endregion

        #region Verification Methods

        public bool VerifyCompletedItemsHeader()
        {
            try
            {
                return IsElementVisible(completedItemsHeader);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyLettersOfCreditSubTabVisible()
        {
            try
            {
                return IsElementVisible(lettersOfCreditSubTab);
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

        public bool VerifyPudLinesColumnHeadersInOrder()
        {
            try
            {
                var actual = GetColumnHeaders();
                if (actual.Count != expectedPudLinesHeaders.Length) return false;
                for (int i = 0; i < expectedPudLinesHeaders.Length; i++)
                {
                    if (!string.Equals(actual[i], expectedPudLinesHeaders[i], StringComparison.OrdinalIgnoreCase))
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetPudLinesHeaders() => GetColumnHeaders();

        #endregion

        #region Action Methods

        public void ClickPudLinesSubTab()
        {
            try
            {
                var tab = Driver.FindElements(pudLinesSubTab).FirstOrDefault();
                if (tab == null) throw new InvalidOperationException("PUD Lines sub-tab not found");
                tab.Click();
                Thread.Sleep(1500); // Allow route change
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click PUD Lines sub-tab: {ex.Message}");
            }
        }

        public void SearchPudLines(string searchText)
        {
            try
            {
                var searchOk = VerifySearchInput();
                if (!searchOk) throw new InvalidOperationException("Search input not found on PUD Lines page");
                EnterText(searchInput, string.Empty); // clear first
                EnterText(searchInput, searchText);
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to perform PUD Lines search: {ex.Message}");
            }
        }

        #endregion

        #region Utility Methods

        public bool IsPudLinesPageLoaded()
        {
            try
            {
                return VerifyCompletedItemsHeader();
            }
            catch
            {
                return false;
            }
        }

        public void WaitForPudLinesPage(TimeSpan? timeout = null)
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