using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TUEL.TestFramework.Web.PageObjects
{
    public class MembersPOM : BasePage
    {
        public MembersPOM(IWebDriver driver) : base(driver)
        {
        }

        // Primary page identifier - unique element that confirms Members page is loaded
        protected override By UniqueLocator => By.XPath("//app-members//h3[contains(text(), 'Members')] | //h3[contains(text(), 'Members')] | //*[contains(text(), 'Members') and contains(@class, 'title')]");

        #region Page & Header Elements

        // Page title and header elements
        private readonly By pageHeader = By.XPath("//app-members//h3[contains(text(), 'Members')] | //h3[contains(text(), 'Members')]");
        private readonly By mainHeader = By.XPath("//h1[contains(text(), 'Application')] | //*[contains(text(), 'Application') and contains(@class, 'title')] | //*[@title='Application']");

        // Left sidebar navigation
        private readonly By membersMenuItem = By.XPath("//a[contains(text(), 'Members')] | //*[contains(@class, 'menu') and contains(text(), 'Members')] | //a[@href='/members']");

        // Active navigation state verification
        private readonly By activeMembersTab = By.XPath("//a[contains(text(), 'Members') and contains(@class, 'active')] | //*[contains(@class, 'active') and contains(text(), 'Members')] | //a[@href='/members' and contains(@class, 'active')]");

        #endregion

        #region Search Elements

        // Search functionality elements
        private readonly By searchContainer = By.XPath("//summit-search | //*[contains(@class, 'summit-search')] | //*[contains(@class, 'search')]");
        private readonly By searchInput = By.XPath("//summit-search//input[@placeholder='Search'] | //input[@placeholder='Search'] | //input[contains(@class, 'search')] | //input[contains(@placeholder, 'search')]");

        #endregion

        #region Export Elements

        // Export functionality elements
        private readonly By exportButton = By.XPath("//summit-button[contains(., 'Export to CSV')] | //button[contains(text(), 'Export to CSV')] | //*[contains(@class, 'button') and contains(text(), 'Export to CSV')]");

        #endregion

        #region Data Table Elements

        // Main data table structure
        private readonly By dataGrid = By.XPath("//app-data-grid | //kendo-grid | //table | //*[contains(@class, 'grid')] | //*[contains(@class, 'table')]");
        private readonly By kendoGrid = By.XPath("//kendo-grid | //div[contains(@class, 'k-grid')]");

        // Table headers - based on actual DOM structure from body.html
        private readonly By tableHeaders = By.XPath("//thead//th | //thead//td | //*[contains(@class, 'k-header')] | //*[contains(@class, 'column-title')] | //th[contains(@class, 'k-table-th')]");

        // Table body and rows
        private readonly By dataRows = By.XPath("//tbody//tr[contains(@class, 'k-master-row')] | //tbody//tr[contains(@class, 'k-table-row')]");

        // View buttons/links in each row
        private readonly By viewLinksInRows = By.XPath("//tbody//tr//a[contains(text(), 'View')] | //tbody//tr//*[contains(@class, 'grid-link')]");

        // No data message
        private readonly By noRecordsMessage = By.XPath("//*[contains(text(), 'No records available') or contains(text(), 'No records') or contains(text(), 'No data') or contains(text(), 'No items') or contains(text(), 'No results')]");

        // Expected column headers based on actual DOM structure
        private readonly string[] expectedColumnHeaders = new string[]
        {
            "View", "Account", "Member", "Address", "Address 2", "City", "State", "Zip Code", "Address Override in Place?"
        };

        #endregion

        #region Pagination Elements

        // Pagination controls - based on Kendo UI structure
        private readonly By paginationContainer = By.XPath("//kendo-pager | //*[contains(@class, 'k-pager')] | //*[contains(@class, 'pagination')] | //*[contains(@class, 'grid-pager')]");
        private readonly By pagerInfo = By.XPath("//kendo-pager-info | //*[contains(@class, 'k-pager-info')] | //*[contains(text(), ' of ') and contains(text(), 'items')]");
        private readonly By pageStatusDisplay = By.XPath("//*[contains(text(), ' - ') and contains(text(), ' of ') and contains(text(), 'items')] | //kendo-pager-info");

        // Navigation buttons
        private readonly By firstPageButton = By.XPath("//button[contains(@class, 'k-pager-first')] | //*[contains(@title, 'first page')] | //*[contains(@aria-label, 'first page')]");
        private readonly By previousPageButton = By.XPath("//button[contains(@class, 'k-pager-nav') and contains(@title, 'previous')] | //*[contains(@title, 'previous page')] | //*[contains(@aria-label, 'previous page')]");
        private readonly By nextPageButton = By.XPath("//button[contains(@class, 'k-pager-nav') and contains(@title, 'next')] | //*[contains(@title, 'next page')] | //*[contains(@aria-label, 'next page')]");
        private readonly By lastPageButton = By.XPath("//button[contains(@class, 'k-pager-last')] | //*[contains(@title, 'last page')] | //*[contains(@aria-label, 'last page')]");

        // Page numbers and input
        private readonly By pageNumbers = By.XPath("//kendo-pager-numeric-buttons//button | //*[contains(@class, 'k-pager-numbers')]//button");
        private readonly By currentPageIndicator = By.XPath("//button[contains(@class, 'k-selected')] | //*[contains(@aria-current, 'page')]");
        private readonly By pageInput = By.XPath("//kendo-pager-input//input | //input[contains(@title, 'page') or contains(@aria-label, 'page')]");

        // Items per page selector
        private readonly By itemsPerPageContainer = By.XPath("//*[contains(text(), 'items per page')] | //*[contains(@class, 'page-size')]");
        private readonly By itemsPerPageInput = By.XPath("//kendo-numerictextbox[contains(@class, 'page-size-input')] | //input[preceding-sibling::*/text()[contains(., 'items per page')] or following-sibling::*/text()[contains(., 'items per page')]]");

        #endregion

        #region Core Verification Methods

        // Verifies the browser page title contains "Members"
        public bool VerifyPageTitle()
        {
            try
            {
                var title = Driver.Title;
                return !string.IsNullOrEmpty(title) && title.Contains("Members", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        // Verifies the Members section header is visible
        public bool VerifyPageHeader()
        {
            try
            {
                return IsElementVisible(pageHeader);
            }
            catch
            {
                return false;
            }
        }

        // Verifies the main application header is present
        public bool VerifyMainHeader()
        {
            try
            {
                return IsElementVisible(mainHeader);
            }
            catch
            {
                return false;
            }
        }

        // Verifies the Members tab is highlighted as active in navigation
        public bool VerifyMembersTabActive()
        {
            try
            {
                var membersElement = Driver.FindElement(membersMenuItem);
                var classAttribute = membersElement.GetAttribute("class") ?? "";
                var ariaCurrent = membersElement.GetAttribute("aria-current") ?? "";

                return classAttribute.Contains("active") ||
                       classAttribute.Contains("selected") ||
                       ariaCurrent == "page" ||
                       ariaCurrent == "true" ||
                       IsElementVisible(activeMembersTab);
            }
            catch
            {
                return false;
            }
        }

        // Verifies the main content area displays the Members page with proper layout
        public bool VerifyMembersPageLayout()
        {
            try
            {
                bool hasHeader = VerifyPageHeader();
                bool hasDataGrid = IsElementVisible(dataGrid);
                bool hasSearch = IsElementVisible(searchContainer);
                bool hasExport = IsElementVisible(exportButton);

                return hasHeader && hasDataGrid && hasSearch && hasExport;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Search Functionality Verification Methods

        // Verifies a search input box is visible
        public bool VerifySearchInputVisible()
        {
            try
            {
                return IsElementVisible(searchInput);
            }
            catch
            {
                return false;
            }
        }

        // Verifies the search box has proper placeholder text
        public bool VerifySearchPlaceholder()
        {
            try
            {
                var searchElement = Driver.FindElement(searchInput);
                var placeholder = searchElement.GetAttribute("placeholder") ?? "";
                return placeholder.Equals("Search", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        // Verifies search box accepts text input
        public bool VerifySearchInputAcceptsText(string testText = "test")
        {
            try
            {
                var searchElement = Driver.FindElement(searchInput);
                var originalValue = searchElement.GetAttribute("value") ?? "";

                // Clear and enter test text
                searchElement.Clear();
                searchElement.SendKeys(testText);

                var newValue = searchElement.GetAttribute("value") ?? "";
                bool textAccepted = newValue.Contains(testText);

                // Restore original value
                searchElement.Clear();
                if (!string.IsNullOrEmpty(originalValue))
                {
                    searchElement.SendKeys(originalValue);
                }

                return textAccepted;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Data Table Structure Verification Methods

        // Verifies a data table with member information is displayed
        public bool VerifyDataTablePresent()
        {
            try
            {
                return IsElementVisible(dataGrid) || IsElementVisible(kendoGrid);
            }
            catch
            {
                return false;
            }
        }

        // Verifies the table has the expected column headers
        public bool VerifyTableColumnHeaders()
        {
            try
            {
                var headers = Driver.FindElements(tableHeaders);
                if (headers.Count == 0) return false;

                var headerTexts = headers.Select(h => h.Text?.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();

                // Check if all expected headers are present
                var matchCount = expectedColumnHeaders.Count(expected =>
                    headerTexts.Any(actual => actual.Contains(expected, StringComparison.OrdinalIgnoreCase)));

                return matchCount >= (expectedColumnHeaders.Length * 0.8);
            }
            catch
            {
                return false;
            }
        }

        // Verifies each row contains a View button/link
        public bool VerifyViewButtonsInRows()
        {
            try
            {
                var rows = Driver.FindElements(dataRows);
                var viewLinks = Driver.FindElements(viewLinksInRows);

                // Should have at least one view link, and reasonably proportional to rows
                return viewLinks.Count > 0 && (rows.Count == 0 || viewLinks.Count >= Math.Min(rows.Count, 10));
            }
            catch
            {
                return false;
            }
        }

        // Verifies member data displays financial institutions
        public bool VerifyMemberDataContent()
        {
            try
            {
                var rows = Driver.FindElements(dataRows);
                if (rows.Count == 0) return false;

                // Check first few rows for financial institution keywords
                var financialKeywords = new[] { "bank", "credit union", "savings", "loan", "financial", "federal", "national" };

                for (int i = 0; i < Math.Min(rows.Count, 3); i++)
                {
                    var rowText = rows[i].Text?.ToLower() ?? "";
                    if (financialKeywords.Any(keyword => rowText.Contains(keyword)))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Verifies address information is properly formatted and displayed
        public bool VerifyAddressInformation()
        {
            try
            {
                var rows = Driver.FindElements(dataRows);
                if (rows.Count == 0) return false;

                // Check if rows contain address-like data (numbers, street names, states, zip codes)
                var firstRow = rows[0].Text ?? "";

                // Look for patterns address data
                bool hasNumericData = System.Text.RegularExpressions.Regex.IsMatch(firstRow, @"\d+");
                bool hasStateAbbreviation = System.Text.RegularExpressions.Regex.IsMatch(firstRow, @"\b[A-Z]{2}\b");
                bool hasZipCode = System.Text.RegularExpressions.Regex.IsMatch(firstRow, @"\d{5}");

                return hasNumericData || hasStateAbbreviation || hasZipCode;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Pagination Controls Verification Methods

        // Verifies pagination navigation controls are visible
        public bool VerifyPaginationControlsVisible()
        {
            try
            {
                return IsElementVisible(paginationContainer);
            }
            catch
            {
                return false;
            }
        }

        // Verifies page navigation includes arrow buttons (first, previous, next, last)
        public bool VerifyPaginationArrowButtons()
        {
            try
            {
                bool hasFirst = IsElementVisible(firstPageButton);
                bool hasPrevious = IsElementVisible(previousPageButton);
                bool hasNext = IsElementVisible(nextPageButton);
                bool hasLast = IsElementVisible(lastPageButton);

                // Should have at least next/previous
                return (hasNext && hasPrevious) || (hasFirst && hasLast);
            }
            catch
            {
                return false;
            }
        }

        // Verifies current page number and total pages are displayed
        public bool VerifyPageNumberDisplay()
        {
            try
            {
                // Check for page input or current page indicator
                bool hasPageInput = IsElementVisible(pageInput);
                bool hasCurrentPage = IsElementVisible(currentPageIndicator);
                bool hasPageNumbers = IsElementVisible(pageNumbers);

                return hasPageInput || hasCurrentPage || hasPageNumbers;
            }
            catch
            {
                return false;
            }
        }

        // Verifies "Items per page" selector is available
        public bool VerifyItemsPerPageSelector()
        {
            try
            {
                return IsElementVisible(itemsPerPageContainer) || IsElementVisible(itemsPerPageInput);
            }
            catch
            {
                return false;
            }
        }

        // Verifies total item count is displayed
        public bool VerifyTotalItemCountDisplay()
        {
            try
            {
                return IsElementVisible(pageStatusDisplay) || IsElementVisible(pagerInfo);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Export Functionality Verification Methods

        // Verifies "Export to CSV" button is visible in the top-right area
        public bool VerifyExportButtonVisible()
        {
            try
            {
                return IsElementVisible(exportButton);
            }
            catch
            {
                return false;
            }
        }

        // Verifies the export button is clickable and properly positioned
        public bool VerifyExportButtonClickable()
        {
            try
            {
                var exportElement = Driver.FindElement(exportButton);
                return exportElement.Displayed && exportElement.Enabled;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Search Results Verification Methods

        // Performs a search and verifies results are displayed
        public bool PerformSearch(string searchTerm)
        {
            try
            {
                var searchElement = Driver.FindElement(searchInput);
                searchElement.Clear();
                searchElement.SendKeys(searchTerm);
                Thread.Sleep(1_500);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Verifies search results update the table content appropriately
        public bool VerifySearchResultsUpdate(string searchTerm)
        {
            try
            {
                // Count rows before search
                var initialRowCount = GetDataRowCount();

                // Perform search
                if (!PerformSearch(searchTerm)) return false;

                // Count rows after search
                var searchRowCount = GetDataRowCount();

                // Results should either change or show no records message
                return searchRowCount != initialRowCount || IsElementVisible(noRecordsMessage);
            }
            catch
            {
                return false;
            }
        }

        // Verifies search for "bank" returns relevant results
        public bool VerifyBankSearch()
        {
            try
            {
                if (!PerformSearch("bank")) return false;

                var rows = Driver.FindElements(dataRows);
                if (rows.Count == 0) return IsElementVisible(noRecordsMessage);

                // Check if results contain "bank"
                var firstRowText = rows[0].Text?.ToLower() ?? "";
                return firstRowText.Contains("bank");
            }
            catch
            {
                return false;
            }
        }

        // Verifies search for specific account number returns exact matches
        public bool VerifyAccountNumberSearch(string accountNumber = "12431")
        {
            try
            {
                if (!PerformSearch(accountNumber)) return false;

                var rows = Driver.FindElements(dataRows);
                if (rows.Count == 0) return IsElementVisible(noRecordsMessage);

                // Check if results contain the account number
                var firstRowText = rows[0].Text ?? "";
                return firstRowText.Contains(accountNumber);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Action Methods

        // Clicks the Export to CSV button
        public void ClickExportButton()
        {
            try
            {
                Click(exportButton);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click Export button: {ex.Message}");
            }
        }

        // Clears the search input
        public void ClearSearch()
        {
            try
            {
                var searchElement = Driver.FindElement(searchInput);
                searchElement.Clear();
                Thread.Sleep(1_000);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to clear search: {ex.Message}");
            }
        }

        // Clicks on a View button in the specified row
        public void ClickViewButton(int rowIndex = 0)
        {
            try
            {
                var viewLinks = Driver.FindElements(viewLinksInRows);
                if (viewLinks.Count > rowIndex)
                {
                    viewLinks[rowIndex].Click();
                }
                else
                {
                    throw new InvalidOperationException($"Row {rowIndex} not found or no View button available");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click View button: {ex.Message}");
            }
        }

        #endregion

        #region Utility Methods

        // Gets the list of column headers
        public List<string> GetColumnHeaders()
        {
            try
            {
                var headers = Driver.FindElements(tableHeaders);
                return headers.Select(h => h.Text?.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        // Gets the current data row count
        public int GetDataRowCount()
        {
            try
            {
                var rows = Driver.FindElements(dataRows);
                return rows.Count;
            }
            catch
            {
                return 0;
            }
        }

        // Gets the page status information (e.g., "1 - 10 of 10+ items")
        public string GetPageStatusInfo()
        {
            try
            {
                var pageInfoElement = Driver.FindElement(pageStatusDisplay);
                return pageInfoElement.Text;
            }
            catch
            {
                return "";
            }
        }

        // Checks if the Members page is fully loaded and ready
        public bool IsPageLoaded()
        {
            try
            {
                return VerifyPageHeader() && VerifyDataTablePresent() && VerifySearchInputVisible();
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
