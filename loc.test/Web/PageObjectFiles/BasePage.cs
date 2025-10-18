using fhlb.selenium.common.Extensions;
using loc.test;
using loc.test.Web.Support;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace loc.test.Web.PageObjectFiles
{
    // Abstract base for all Page Object Model (POM) classes.
    public abstract class BasePage
    {
        protected IWebDriver Driver { get; }
        protected string BaseUrl { get; }

        protected BasePage(IWebDriver driver)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));

            // Fetch the URL
            BaseUrl = InitializeTestAssembly.UiUrl;
            if (string.IsNullOrEmpty(BaseUrl))
            {
                throw new InvalidOperationException("The UI base URL is not configured. Check the .runsettings file.");
            }
        }

        // A locator unique to the page, used to confirm the page has loaded correctly.
        protected abstract By UniqueLocator { get; }

        #region Common LOC Application Locators

        // Common elements across LOC pages
        protected readonly By mainHeader = By.XPath("//h1[contains(text(), 'Letters of Credit')] | //*[contains(text(), 'Letters of Credit') and contains(@class, 'title')] | //*[@title='Letters of Credit']");

        // Navigation Tabs (common across all pages)
        protected readonly By navigationTabs = By.XPath("//nav//a | //div[contains(@class, 'nav')]//a | //*[contains(@class, 'tab')] | //a[contains(text(), 'Dashboard')] | //a[contains(text(), 'Completed')] | //a[contains(text(), 'Beneficiaries')]");
        protected readonly By dashboardTab = By.XPath("//a[contains(text(), 'Dashboard')] | //button[contains(text(), 'Dashboard')]");
        protected readonly By completedTab = By.XPath("//a[contains(text(), 'Completed')] | //button[contains(text(), 'Completed')]");
        protected readonly By beneficiariesTab = By.XPath("//a[contains(text(), 'Beneficiaries')] | //button[contains(text(), 'Beneficiaries')]");
        protected readonly By templatesTab = By.XPath("//a[contains(text(), 'Templates')] | //button[contains(text(), 'Templates')]");
        protected readonly By feesTab = By.XPath("//a[contains(text(), 'Fees')] | //button[contains(text(), 'Fees')]");

        // Common table elements
        protected readonly By dataTable = By.XPath("//table | //kendo-grid | //app-data-grid | //*[contains(@class, 'grid')] | //*[contains(@class, 'table')]");
        protected readonly By tableHeaders = By.XPath("//thead//tr/th");
        protected readonly By tableRows = By.XPath("//*[contains(@class, 'data-row')] | //kendo-grid//tr[contains(@class, 'k-master-row')]");
        protected readonly By noRecordsMessage = By.XPath("//*[contains(text(), 'No records available') or contains(text(), 'No records') or contains(text(), 'No data') or contains(text(), 'No items') or contains(text(), 'No results')]");

        // Common pagination elements
        protected readonly By paginationContainer = By.XPath("//*[contains(@class, 'pager')] | //*[contains(@class, 'pagination')] | //kendo-pager");
        protected readonly By pageInfo = By.XPath("//*[contains(text(), 'Page') and contains(text(), 'of')] | //*[contains(text(), ' of ') and contains(text(), 'items')] | //*[contains(@class, 'pager-info')]");
        protected readonly By itemsPerPageSelector = By.XPath("//div[@class='grid-pager']//kendo-pager-numeric-buttons");
        protected readonly By itemsPerPageSelect = By.XPath("//div[@class='grid-pager']//kendo-pager-numeric-buttons/select");
        protected readonly By itemsPage2 = By.XPath("button[title='Page 2'] span[class='k-button-text']");

        // Common search elements
        protected readonly By searchInput = By.XPath("//input[@placeholder='Search'] | //input[contains(@class, 'search')] | //input[contains(@placeholder, 'search')]");

        #endregion

        #region Common LOC Application Methods

        // Common page verification methods
        public virtual bool VerifyPageTitle()
        {
            try
            {
                WaitUntilPageIsLoaded();
                var title = Driver.Title;
                return !string.IsNullOrEmpty(title) && title.Contains("Letters of Credit");
            }
            catch
            {
                return false;
            }
        }

        public virtual bool VerifyMainHeader()
        {
            try
            {
                Driver.WaitVisible(mainHeader);
                return IsElementVisible(mainHeader);
            }
            catch
            {
                return false;
            }
        }

        public virtual bool VerifyNavigationTabsPresent()
        {
            try
            {
                Driver.WaitVisible(navigationTabs);
                return IsElementVisible(navigationTabs);
            }
            catch
            {
                return false;
            }
        }

        // Common table verification methods
        public virtual bool VerifyDataTablePresent()
        {
            try
            {
                Driver.WaitVisible(dataTable);
                return IsElementVisible(dataTable);
            }
            catch
            {
                return false;
            }
        }

        public virtual bool VerifyTableHasData()
        {
            try
            {
                var rows = Driver.FindElements(tableRows);
                return rows.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public virtual bool VerifyNoRecordsMessage()
        {
            try
            {
                Driver.WaitVisible(noRecordsMessage);
                return IsElementVisible(noRecordsMessage);
            }
            catch
            {
                return false;
            }
        }

        public virtual int GetDataRowCount()
        {
            try
            {
                var rows = Driver.FindElements(tableRows);
                return rows.Count;
            }
            catch
            {
                return 0;
            }
        }

        // Common pagination verification methods
        public virtual bool VerifyPaginationControls()
        {
            try
            {
                Driver.WaitVisible(paginationContainer);
                return IsElementVisible(paginationContainer);
            }
            catch
            {
                return false;
            }
        }

        public virtual bool VerifyPageStatusDisplay()
        {
            try
            {
                Driver.WaitVisible(pageInfo);
                return IsElementVisible(pageInfo);
            }
            catch
            {
                return false;
            }
        }

        public virtual bool VerifyItemsPerPageSelector()
        {
            try
            {
                Driver.WaitVisible(itemsPerPageSelector);
                return IsElementVisible(itemsPerPageSelector);
            }
            catch
            {
                return false;
            }
        }

        // Common search methods
        public virtual bool VerifySearchInput()
        {
            try
            {
                Driver.WaitVisible(searchInput);
                return IsElementVisible(searchInput);
            }
            catch
            {
                return false;
            }
        }

        // Common navigation methods
        public virtual void ClickNavigationTab(string tabName)
        {
            try
            {
                By tabLocator = tabName.ToLower() switch
                {
                    "dashboard" => dashboardTab,
                    "completed" => completedTab,
                    "beneficiaries" => beneficiariesTab,
                    "templates" => templatesTab,
                    "fees" => feesTab,
                    _ => throw new ArgumentException($"Unknown tab: {tabName}")
                };

                Click(tabLocator);

                // Wait for page transition
                System.Threading.Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click {tabName} tab: {ex.Message}");
            }
        }

        // Common utility methods
        public virtual List<string> GetColumnHeaders(bool includeEmpty = false, string? emptyPlaceholder = null)
        {
            try
            {
                var headers = Driver.FindElements(tableHeaders);
                var list = new List<string>();
                foreach (var h in headers)
                {
                    var raw = h.Text?.Trim() ?? string.Empty;
                    if (string.IsNullOrEmpty(raw))
                    {
                        if (includeEmpty)
                        {
                            list.Add(emptyPlaceholder ?? string.Empty);
                        }
                    }
                    else
                    {
                        list.Add(raw);
                    }
                }
                return list;
            }
            catch
            {
                return new List<string>();
            }
        }

        public virtual string GetPageStatus()
        {
            try
            {
                var pageInfoElement = Driver.FindElement(pageInfo);
                return pageInfoElement.Text;
            }
            catch
            {
                return "";
            }
        }

        #endregion

        // Waits until the page's unique locator is visible, confirming the page is loaded.
        public virtual void WaitUntilPageIsLoaded(TimeSpan? timeout = null)
        {
            Driver.WaitVisible(UniqueLocator, timeout ?? TimeSpan.FromSeconds(InitializeTestAssembly.DefaultTimeoutSeconds));
        }

        // A helper method to get the page title.
        public string GetPageTitle()
        {
            return Driver.Title;
        }

        /* --- Shorthand helpers for element interactions --- */
        protected void Click(By by) => Driver.ClickElement(by);

        protected void EnterText(By by, string? text, bool clearFirst = true)
        {
            if (text == null) return;
            Driver.EnterText(by, text, clearFirst);
        }

        protected string? GetText(By by) => Driver.GetElementText(by);

        protected bool IsElementVisible(By by) => Driver.IsDisplayedSafe(by);

        #region Generic Search Helpers
        // First whitespace-delimited token from a row (null if none)
        protected static string? ExtractFirstToken(string? rowText)
        {
            if (string.IsNullOrWhiteSpace(rowText)) return null;
            return rowText.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }

        // First matching status keyword (e.g., Charged/Denied) else null
        protected static string? ExtractFirstStatus(string? rowText, params string[] keywords)
        {
            if (string.IsNullOrWhiteSpace(rowText) || keywords == null || keywords.Length == 0) return null;
            foreach (var k in keywords)
            {
                if (!string.IsNullOrWhiteSpace(k) && rowText.Contains(k, StringComparison.OrdinalIgnoreCase))
                    return k;
            }
            return null;
        }

        // Strict search: execute search, gather rows & empty state => tuple
        protected (bool hasMatch, bool emptyState, int rowCount) RunStrictSearch(
            string term,
            Action<string> searchAction,
            Func<List<string>> rowsProvider,
            Func<bool> emptyStateProvider)
        {
            searchAction(term);
            var rows = rowsProvider();
            bool emptyState = emptyStateProvider();
            bool hasMatch = rows.Count > 0 && rows.Any(r => r.Contains(term, StringComparison.OrdinalIgnoreCase));
            return (hasMatch, emptyState, rows.Count);
        }
        #endregion
    }
}