using TUEL.TestFramework;
using TUEL.TestFramework.Web.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TUEL.TestFramework.Web.PageObjects
{
    // Page Object Model for the Fees page
    public class FeesPOM : BasePage
    {
        public FeesPOM(IWebDriver driver) : base(driver) { }

        // Primary page identifier - unique element confirming Fees page is loaded
        protected override By UniqueLocator => feeActivitySubHeader;

        #region Page & Header Elements
        private readonly By feeActivitySubHeader = By.XPath("//*[contains(text(),'Fee Activity')] | //h2[contains(.,'Fee Activity')] | //h3[contains(.,'Fee Activity')]");

        private readonly By viewFeeParametersButton = By.XPath("//button[contains(normalize-space(),'View Fee Parameters')] | //a[contains(normalize-space(),'View Fee Parameters')]");
        private readonly By exportToCsvButton = By.XPath("//button[contains(normalize-space(),'Export to CSV')] | //a[contains(normalize-space(),'Export to CSV')]");
        private readonly By feeParametersHeader = By.XPath("//h1[normalize-space()='Fee Parameters']");

        // Expected grid column headers exact order
        private static readonly string[] ExpectedHeadersExactOrder =
        [
            "Account",
            "Customer",
            "Product #",
            "Amount",
            "Order Date",
            "Delivery Date",
            "Fee",
            "Charge Date",
            "Fee Start Date",
            "Fee End Date",
            "Status",
            "" // Empty column
        ];
        #endregion

        #region Core Verification Methods
        // Generic page header check
        public bool VerifyFeeActivitySubHeader()
        {
            try
            {
                return IsElementVisible(feeActivitySubHeader);
            }
            catch
            {
                return false;
            }
        }

        // Checks if the Fees tab is active using common attributes
        public bool VerifyFeesTabActive()
        {
            try
            {
                var element = Driver.FindElement(feesTab);
                var cls = element.GetAttribute("class") ?? string.Empty;
                var aria = element.GetAttribute("aria-current") ?? string.Empty;
                return cls.Contains("active", StringComparison.OrdinalIgnoreCase)
                       || cls.Contains("selected", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(aria, "page", StringComparison.OrdinalIgnoreCase)
                       || string.Equals(aria, "true", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyViewFeeParametersButton()
        {
            try
            {
                return IsElementVisible(viewFeeParametersButton);
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

        public bool VerifyFeeParametersHeader()
        {
            try
            {
                Driver.WaitVisible(feeParametersHeader);
                return IsElementVisible(feeParametersHeader);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyColumnHeadersInExactOrder()
        {
            try
            {
                // Include empty headers so we can handle a trailing blank column
                var headers = GetColumnHeaders(includeEmpty: true);
                if (headers == null) return false;

                // If there is an extra trailing blank header, allow it (expected+1 with last empty)
                if (headers.Count == ExpectedHeadersExactOrder.Length + 1 && string.IsNullOrEmpty(headers.Last()))
                {
                    headers = headers.Take(headers.Count - 1).ToList();
                }

                if (headers.Count != ExpectedHeadersExactOrder.Length)
                    return false;

                for (int i = 0; i < ExpectedHeadersExactOrder.Length; i++)
                {
                    if (!string.Equals(headers[i], ExpectedHeadersExactOrder[i], StringComparison.Ordinal))
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyNoRecordsMessageVisible()
        {
            try
            {
                return VerifyNoRecordsMessage();
            }
            catch
            {
                return false;
            }
        }

        // Legacy permissive search verification: considers the search "applied" if either
        // 1) Any row text contains the term OR
        // 2) No rows are returned but the empty-state message is visible.
        // This can MASK BUGS where the grid incorrectly shows 'No records available.' despite existing matches.
        // Prefer using VerifySearchHasMatch for positive validations to avoid false positives.
        [Obsolete("Use VerifySearchHasMatch for positive validations; this method is permissive and may hide defects.")]
        public bool VerifySearchApplied(string term)
        {
            try
            {
                var rows = GetAllRowTexts();
                if (rows.Count == 0)
                {
                    return VerifyNoRecordsMessageVisible();
                }
                return rows.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        // Strict search verification: returns TRUE only if at least one row contains the term.
        // Returns FALSE if zero rows or no-records message appears unexpectedly.
        public bool VerifySearchHasMatch(string term)
        {
            try
            {
                var rows = GetAllRowTexts();
                if (rows.Count == 0)
                {
                    // Explicitly fail strict check when no data; do NOT treat empty-state as success
                    return false;
                }
                return rows.Any(t => t.Contains(term, StringComparison.OrdinalIgnoreCase));
            }
            catch { return false; }
        }

        // First row token & status (if any)
        public (string? token, string? status) GetFirstRowTokenAndStatus(params string[] statusKeywords)
        {
            var rows = GetAllRowTexts();
            if (rows.Count == 0) return (null, null);
            var first = rows[0];
            var token = ExtractFirstToken(first);
            var status = ExtractFirstStatus(first, statusKeywords);
            return (token, status);
        }

        // Strict search wrapper
        public (bool hasMatch, bool emptyState, int rowCount) StrictSearch(string term)
        {
            return RunStrictSearch(term, SearchFees, GetAllRowTexts, VerifyNoRecordsMessageVisible);
        }
        #endregion

        #region Action Methods
        public void SearchFees(string text)
        {
            // Enter search text and wait briefly for grid to refresh (lightweight explicit wait)
            EnterText(searchInput, text ?? string.Empty, clearFirst: true);
            WaitForSearchResults(TimeSpan.FromSeconds(5));
        }

        public void ClickViewFeeParameters()
        {
            Click(viewFeeParametersButton);
        }

        public void ClickExportToCsv()
        {
            Click(exportToCsvButton);
        }

    // Try navigating to page 2 (if it exists)
        public bool ClickSecondPageIfAvailable()
        {
            try
            {
                // Reuse existing pagination locator patterns; look for a button/link with text '2' or title Page 2.
                var candidates = new[]
                {
                    By.XPath("//button[@title='Page 2']"),
                    By.XPath("//a[@title='Page 2']"),
                    By.XPath("//button[normalize-space()='2']"),
                    By.XPath("//a[normalize-space()='2']"),
                    By.XPath("//kendo-pager//a[normalize-space()='2']")
                };

                foreach (var by in candidates)
                {
                    var elems = Driver.FindElements(by);
                    if (elems.Any())
                    {
                        elems.First().Click();
                        // brief wait for page change – reuse existing wait pattern
                        WaitForSearchResults(TimeSpan.FromSeconds(5));
                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        // Clear search box and wait
        public void ClearFeesSearch()
        {
            EnterText(searchInput, string.Empty, clearFirst: true);
            WaitForSearchResults(TimeSpan.FromSeconds(5));
        }

        // Polling wait for search/pagination stabilization
        private void WaitForSearchResults(TimeSpan timeout)
        {
            var end = DateTime.UtcNow + timeout;
            int lastCount = -1;
            while (DateTime.UtcNow < end)
            {
                try
                {
                    var rows = GetAllRowTexts();
                    if (rows.Count != lastCount)
                    {
                        // First stabilization (count changed) – allow one more pass to settle
                        lastCount = rows.Count;
                        Thread.Sleep(150); // minimal sleep to avoid tight loop
                    }
                    else if (rows.Count > 0 || VerifyNoRecordsMessageVisible())
                    {
                        return; // stable enough
                    }
                }
                catch { }
                Thread.Sleep(100);
            }
        }
        #endregion

        #region Utility Methods
        public void WaitForFeesPage(TimeSpan? timeout = null)
        {
            WaitUntilPageIsLoaded(timeout);
        }

        public List<string> GetAllRowTexts()
        {
            try
            {
                // BasePage.tableRows via GetElementText helper patterns
                var rowElements = Driver.FindElements(tableRows);
                return [.. rowElements.Select(e => e.Text?.Trim() ?? string.Empty).Where(t => !string.IsNullOrEmpty(t))];
            }
            catch
            {
                return new List<string>();
            }
        }

        public void NavigateBackToFees()
        {
            Driver.Navigate().Back();
            WaitForFeesPage(TimeSpan.FromSeconds(10));
        }

        public void WaitForFeeParametersPage(TimeSpan? timeout = null)
        {
            Driver.WaitVisible(feeParametersHeader, timeout ?? TimeSpan.FromSeconds(InitializeTestAssembly.DefaultTimeoutSeconds));
        }

        #endregion
    }
}
