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
            "View", "Account", "Customer", "Order Date", "Delivery Date",
            "Customer", "Amount", "Product #", "Product Type", "Document", "Status"
        };

        // Specific column locators for validation
        private readonly By viewColumn = By.XPath("//th[contains(text(), 'View')] | //th[contains(@title, 'View')]");
        private readonly By accountColumn = By.XPath("//th[contains(text(), 'Account')] | //th[contains(@title, 'Account')]");
        private readonly By customerColumn = By.XPath("//th[contains(text(), 'Customer')] | //th[contains(@title, 'Customer')]");
        private readonly By orderDateColumn = By.XPath("//th[contains(text(), 'Order Date')] | //th[contains(@title, 'Order Date')]");
        private readonly By deliveryDateColumn = By.XPath("//th[contains(text(), 'Delivery Date')] | //th[contains(@title, 'Delivery Date')]");
        private readonly By amountColumn = By.XPath("//th[contains(text(), 'Amount')] | //th[contains(@title, 'Amount')]");
        private readonly By productNumberColumn = By.XPath("//th[contains(text(), 'Product #')] | //th[contains(@title, 'Product #')]");
        private readonly By productTypeColumn = By.XPath("//th[contains(text(), 'Product Type')] | //th[contains(@title, 'Product Type')]");
        private readonly By documentColumn = By.XPath("//th[contains(text(), 'Document')] | //th[contains(@title, 'Document')]");
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
                    IsElementVisible(accountColumn),
                    IsElementVisible(customerColumn),
                    IsElementVisible(orderDateColumn),
                    IsElementVisible(deliveryDateColumn),
                    IsElementVisible(amountColumn),
                    IsElementVisible(productNumberColumn),
                    IsElementVisible(productTypeColumn),
                    IsElementVisible(documentColumn),
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
