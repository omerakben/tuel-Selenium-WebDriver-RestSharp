using TUEL.TestFramework;
using TUEL.TestFramework.Web.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TUEL.TestFramework.Web.PageObjects
{
    public class DashboardPOM : BasePage
    {
        public DashboardPOM(IWebDriver driver) : base(driver)
        {
        }

        // Primary page identifier
        protected override By UniqueLocator => By.XPath("//div[contains(text(), 'Approval Queue')] | //h1[contains(text(), 'Business Application')]");

        #region Page & Header Elements
        private readonly By mainHeader = By.XPath("//h1[contains(text(), 'Business Application')] | //*[contains(text(), 'Business Application') and contains(@class, 'title')] | //*[@title='Business Application']");

        // Navigation Tabs
        private readonly By navigationTabs = By.XPath("//nav//a | //div[contains(@class, 'nav')]//a | //*[contains(@class, 'tab')] | //a[contains(text(), 'Dashboard')] | //a[contains(text(), 'Completed')] | //a[contains(text(), 'Customers')]");
        private readonly By dashboardTab = By.XPath("//a[contains(text(), 'Dashboard')] | //button[contains(text(), 'Dashboard')] | //*[contains(@class, 'active') and contains(text(), 'Dashboard')]");
        private readonly By completedTab = By.XPath("//a[contains(text(), 'Completed')] | //button[contains(text(), 'Completed')]");
        private readonly By customersTab = By.XPath("//a[contains(text(), 'Customers')] | //button[contains(text(), 'Customers')]");
        private readonly By templatesTab = By.XPath("//a[contains(text(), 'Templates')] | //button[contains(text(), 'Templates')]");
        private readonly By pricingTab = By.XPath("//a[contains(text(), 'Pricing')] | //button[contains(text(), 'Pricing')]");

        // Left sidebar navigation
        private readonly By leftSidebar = By.XPath("//div[contains(@class, 'sidebar')] | //nav[contains(@class, 'sidebar')]");
        private readonly By businessApplicationMenuItem = By.XPath("//a[contains(text(), 'Business Application')] | //*[contains(@class, 'menu') and contains(text(), 'Business Application')]");
        private readonly By healthCheckMenuItem = By.XPath("//a[contains(text(), 'Health Check')] | //*[contains(@class, 'menu') and contains(text(), 'Health Check')]");
        #endregion

        #region Approval Queue Section
        private readonly By approvalQueueSection = By.XPath("//div[contains(text(), 'Approval Queue')] | //*[contains(@class, 'approval') and contains(@class, 'queue')]");
        private readonly By approvalQueueHeader = By.XPath("//div[contains(text(), 'Approval Queue')] | //h2[contains(text(), 'Approval Queue')] | //h3[contains(text(), 'Approval Queue')]");
        private readonly By approvalQueueSearchInput = By.XPath("//input[@placeholder='Search'] | //input[contains(@class, 'search')] | //app-approval-q//input");
        private readonly By approvalQueueTable = By.XPath("//kendo-grid | //table | //app-data-grid | //*[contains(@class, 'grid')] | //*[contains(@class, 'table')]");
        #endregion

        #region Send Items Section
        private readonly By sendItemsSection = By.XPath("//div[contains(text(), 'Send Items')] | //*[contains(@class, 'send') and contains(@class, 'items')]");
        private readonly By sendItemsHeader = By.XPath("//div[contains(text(), 'Send Items')] | //h2[contains(text(), 'Send Items')] | //h3[contains(text(), 'Send Items')]");
        private readonly By sendItemsSearchInput = By.XPath("//app-send-q//input[@placeholder='Search'] | //app-send-q//input[contains(@class, 'search')]");
        private readonly By sendItemsTable = By.XPath("//app-send-q//kendo-grid | //app-send-q//table | //app-send-q//app-data-grid");
        #endregion

        #region Data Table Elements
        // Table structure detection
        private readonly By anyTable = By.XPath("//table | //kendo-grid | //app-data-grid | //*[contains(@class, 'grid')] | //*[contains(@class, 'table')]");
        private readonly By tableHeaders = By.XPath("//th | //thead//td | //*[contains(@class, 'header')] | //*[contains(@class, 'column-title')]");
        private readonly By tableRows = By.XPath("//tbody//tr | //tr[contains(@class, 'row')] | //*[contains(@class, 'data-row')]");
        private readonly By noRecordsMessage = By.XPath("//*[contains(text(), 'No records') or contains(text(), 'No data') or contains(text(), 'No items') or contains(text(), 'No results')]");

        // Key column headers to verify
        private readonly string[] expectedColumnHeaders = new string[]
        {
            "View", "Account", "Customer", "Order Date", "Delivery Date",
            "Customer", "Amount", "Product #", "Product Type", "Created On",
            "Created By", "Comments", "Status", "Priority"
        };
        #endregion

        #region Pagination Elements
        private readonly By paginationContainer = By.XPath("//*[contains(@class, 'pager')] | //*[contains(@class, 'pagination')] | //kendo-pager");
        private readonly By pageInfo = By.XPath("//*[contains(text(), ' of ') and contains(text(), 'items')] | //*[contains(@class, 'pager-info')]");
        private readonly By itemsPerPageSelector = By.XPath("//select[contains(@title, 'items per page')] | //select[contains(@class, 'page-size')] | //*[contains(@class, 'pager')]//select");
        #endregion

        #region Core Verification Methods

        public bool VerifyPageTitle()
        {
            try
            {
                var title = Driver.Title;
                return !string.IsNullOrEmpty(title) && title.Contains("Business Application");
            }
            catch
            {
                return false;
            }
        }

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

        public bool VerifyNavigationTabsPresent()
        {
            try
            {
                return IsElementVisible(navigationTabs);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyDashboardTabActive()
        {
            try
            {
                var dashboardElement = Driver.FindElement(dashboardTab);
                var classAttribute = dashboardElement.GetAttribute("class") ?? "";
                var ariaCurrent = dashboardElement.GetAttribute("aria-current") ?? "";

                return classAttribute.Contains("active") ||
                       classAttribute.Contains("selected") ||
                       ariaCurrent == "page" ||
                       ariaCurrent == "true";
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Section Verification Methods

        public bool VerifyApprovalQueueSection()
        {
            try
            {
                return IsElementVisible(approvalQueueSection) || IsElementVisible(approvalQueueHeader);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyApprovalQueueTable()
        {
            try
            {
                return IsElementVisible(approvalQueueTable) || IsElementVisible(anyTable);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyApprovalQueueSearch()
        {
            try
            {
                return IsElementVisible(approvalQueueSearchInput);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifySendItemsSection()
        {
            try
            {
                return IsElementVisible(sendItemsSection) || IsElementVisible(sendItemsHeader);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifySendItemsTable()
        {
            try
            {
                return IsElementVisible(sendItemsTable);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifySendItemsSearch()
        {
            try
            {
                return IsElementVisible(sendItemsSearchInput);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Data Table Verification Methods

        public bool VerifyTableStructure()
        {
            try
            {
                var headers = Driver.FindElements(tableHeaders);
                return headers.Count >= 10;
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyTableColumns()
        {
            try
            {
                var headers = Driver.FindElements(tableHeaders);
                if (headers.Count == 0) return false;

                var headerTexts = headers.Select(h => h.Text?.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();

                var matchCount = expectedColumnHeaders.Count(expected =>
                    headerTexts.Any(actual => actual.Contains(expected, StringComparison.OrdinalIgnoreCase)));

                return matchCount >= (expectedColumnHeaders.Length * 0.7);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyTableHasData()
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

        public bool VerifyNoRecordsMessageWhenEmpty()
        {
            try
            {
                var rows = Driver.FindElements(tableRows);
                if (rows.Count == 0)
                {
                    return IsElementVisible(noRecordsMessage);
                }
                return true; // If there are rows, no need for no-records message
            }
            catch
            {
                return false;
            }
        }

        public int GetDataRowCount()
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

        #endregion

        #region Pagination Verification Methods

        public bool VerifyPaginationControls()
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

        public bool VerifyPageStatusDisplay()
        {
            try
            {
                return IsElementVisible(pageInfo);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyItemsPerPageSelector()
        {
            try
            {
                return IsElementVisible(itemsPerPageSelector);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Action Methods

        public void ClickCompletedTab()
        {
            try
            {
                Thread.Sleep(2000);
                ClickNavigationTab("completed");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click Completed tab: {ex.Message}");
            }
        }

        public void ClickNavigationTab(string tabName)
        {
            try
            {
                By tabLocator = tabName.ToLower() switch
                {
                    "dashboard" => dashboardTab,
                    "completed" => completedTab,
                    "customers" => customersTab,
                    "templates" => templatesTab,
                    "pricing" => pricingTab,
                    _ => throw new ArgumentException($"Unknown tab: {tabName}")
                };

                Click(tabLocator);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click {tabName} tab: {ex.Message}");
            }
        }

        public void SearchApprovalQueue(string searchText)
        {
            try
            {
                EnterText(approvalQueueSearchInput, searchText);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to search Approval Queue: {ex.Message}");
            }
        }

        public void SearchSendItems(string searchText)
        {
            try
            {
                EnterText(sendItemsSearchInput, searchText);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to search Send Items: {ex.Message}");
            }
        }

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

        #endregion

        #region Utility Methods

        public bool IsPageLoaded()
        {
            try
            {
                return VerifyPageTitle() && (VerifyApprovalQueueSection() || VerifyMainHeader());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to Page Load: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}
