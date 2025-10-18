using OpenQA.Selenium;
using loc.test.Web.Support;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loc.test.Web.PageObjectFiles
{
    public class CompletedPOM : BasePage
    {
        public CompletedPOM(IWebDriver driver) : base(driver)
        {
        }

        // Primary page identifier - Completed Items header
        protected override By UniqueLocator => By.XPath("//div[contains(text(), 'Completed Items')] | //h2[contains(text(), 'Completed Items')] | //h3[contains(text(), 'Completed Items')]");

        #region Page-Specific Elements

        // Completed page specific elements
        private readonly By completedItemsHeader = By.XPath("//div[contains(text(), 'Completed Items')] | //h2[contains(text(), 'Completed Items')] | //h3[contains(text(), 'Completed Items')]");
        private readonly By exportToCsvButton = By.XPath("//button[contains(text(), 'Export to CSV')] | //*[contains(text(), 'Export to CSV')]");

        // Expected column headers in exact order
        private readonly string[] expectedColumnHeaders = new string[]
        {
            "View", "DDA", "Member", "Issue Date", "Expiration Date",
            "Beneficiary", "Amount", "LOC #", "LOC Type", "Letter", "Status"
        };

        // Specific column locators for validation
        private readonly By viewColumn = By.XPath("//th[contains(text(), 'View')] | //th[contains(@title, 'View')]");
        private readonly By ddaColumn = By.XPath("//th[contains(text(), 'DDA')] | //th[contains(@title, 'DDA')]");
        private readonly By memberColumn = By.XPath("//th[contains(text(), 'Member')] | //th[contains(@title, 'Member')]");
        private readonly By issueDateColumn = By.XPath("//th[contains(text(), 'Issue Date')] | //th[contains(@title, 'Issue Date')]");
        private readonly By expirationDateColumn = By.XPath("//th[contains(text(), 'Expiration Date')] | //th[contains(@title, 'Expiration Date')]");
        private readonly By beneficiaryColumn = By.XPath("//th[contains(text(), 'Beneficiary')] | //th[contains(@title, 'Beneficiary')]");
        private readonly By amountColumn = By.XPath("//th[contains(text(), 'Amount')] | //th[contains(@title, 'Amount')]");
        private readonly By locNumberColumn = By.XPath("//th[contains(text(), 'LOC #')] | //th[contains(@title, 'LOC #')]");
        private readonly By locTypeColumn = By.XPath("//th[contains(text(), 'LOC Type')] | //th[contains(@title, 'LOC Type')]");
        private readonly By letterColumn = By.XPath("//th[contains(text(), 'Letter')] | //th[contains(@title, 'Letter')]");
        private readonly By statusColumn = By.XPath("//th[contains(text(), 'Status')] | //th[contains(@title, 'Status')]");

        #endregion

        #region Page-Specific Verification Methods

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

        public bool VerifyCompletedTabActive()
        {
            try
            {
                var completedTabElement = Driver.FindElements(completedTab).FirstOrDefault();
                if (completedTabElement == null) return false;

                // Check for active state indicators
                var classes = completedTabElement.GetAttribute("class") ?? "";
                var ariaCurrent = completedTabElement.GetAttribute("aria-current") ?? "";

                return classes.Contains("active", StringComparison.OrdinalIgnoreCase) ||
                       classes.Contains("selected", StringComparison.OrdinalIgnoreCase) ||
                       classes.Contains("current", StringComparison.OrdinalIgnoreCase) ||
                       ariaCurrent == "page" ||
                       ariaCurrent == "true";
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

        public bool VerifyColumnHeadersInOrder()
        {
            try
            {
                var actualHeaders = GetColumnHeaders();

                if (actualHeaders.Count != expectedColumnHeaders.Length)
                {
                    return false;
                }

                for (int i = 0; i < expectedColumnHeaders.Length; i++)
                {
                    if (!actualHeaders[i].Contains(expectedColumnHeaders[i], StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool VerifySpecificColumns()
        {
            try
            {
                var columnChecks = new[]
                {
                    IsElementVisible(viewColumn),
                    IsElementVisible(ddaColumn),
                    IsElementVisible(memberColumn),
                    IsElementVisible(issueDateColumn),
                    IsElementVisible(expirationDateColumn),
                    IsElementVisible(beneficiaryColumn),
                    IsElementVisible(amountColumn),
                    IsElementVisible(locNumberColumn),
                    IsElementVisible(locTypeColumn),
                    IsElementVisible(letterColumn),
                    IsElementVisible(statusColumn)
                };

                return columnChecks.All(check => check);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Page-Specific Action Methods

        public void ClickCompletedTab()
        {
            try
            {
                var tab = Driver.WaitClickable(completedTab, TimeSpan.FromSeconds(10));
                tab.Click();

                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to click Completed tab: {ex.Message}");
            }
        }

        public void SearchCompletedItems(string searchText)
        {
            try
            {
                var searchField = Driver.WaitVisible(searchInput, TimeSpan.FromSeconds(10));
                searchField.Clear();
                searchField.SendKeys(searchText);
                searchField.SendKeys(Keys.Enter);

                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to search completed items: {ex.Message}");
            }
        }

        public void ClickExportToCsv()
        {
            try
            {
                var exportButton = Driver.WaitClickable(exportToCsvButton, TimeSpan.FromSeconds(10));
                exportButton.Click();

                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to click Export to CSV: {ex.Message}");
            }
        }

        #endregion

        #region Utility Methods

        public bool IsPageLoaded()
        {
            try
            {
                // Check for either the unique locator or the data table
                return IsElementVisible(UniqueLocator) || IsElementVisible(dataTable);
            }
            catch
            {
                return false;
            }
        }

        public void WaitForPageToLoad()
        {
            try
            {
                Driver.WaitVisible(UniqueLocator, TimeSpan.FromSeconds(15));
            }
            catch
            {
                // Fallback to waiting for data table
                Driver.WaitVisible(dataTable, TimeSpan.FromSeconds(15));
            }
        }

        #endregion
    }
}