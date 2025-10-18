using OpenQA.Selenium;
using loc.test.Web.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace loc.test.Web.PageObjectFiles
{
    public class BeneficiariesPOM : BasePage
    {
        public BeneficiariesPOM(IWebDriver driver) : base(driver)
        {
        }

        // Primary page identifier - Beneficiaries header
        protected override By UniqueLocator => By.XPath("//h3[contains(text(), 'Beneficiaries')] | //div[contains(text(), 'Beneficiaries')] | //h2[contains(text(), 'Beneficiaries')]");

        #region Page-Specific Elements

        // Beneficiaries page specific elements
        private readonly By beneficiariesHeader = By.XPath("//h3[contains(text(), 'Beneficiaries')] | //div[contains(text(), 'Beneficiaries')] | //h2[contains(text(), 'Beneficiaries')]");
        private readonly By addNewBeneficiaryButton = By.XPath("//button[contains(text(), 'Add New Beneficiary')] | //*[contains(text(), 'Add New Beneficiary')]");

        // Expected column headers in exact order for Beneficiaries page
        private readonly string[] expectedColumnHeaders = new string[]
        {
            "View", "Beneficiary", "Address", "Address 2", "Address 3", "City", "State", "Zip Code"
        };

        // Specific column locators for validation - based on the HTML structure
        private readonly By viewColumn = By.XPath("//th[contains(.//span, 'View')] | //th[contains(@title, 'View')] | //span[contains(@class, 'k-column-title') and contains(text(), 'View')]");
        private readonly By beneficiaryColumn = By.XPath("//th[contains(.//span, 'Beneficiary')] | //th[contains(@title, 'Beneficiary')] | //span[contains(@class, 'k-column-title') and contains(text(), 'Beneficiary')]");
        private readonly By addressColumn = By.XPath("//th[contains(.//span, 'Address') and not(contains(.//span, 'Address 2')) and not(contains(.//span, 'Address 3'))] | //span[contains(@class, 'k-column-title') and text()='Address']");
        private readonly By address2Column = By.XPath("//th[contains(.//span, 'Address 2')] | //span[contains(@class, 'k-column-title') and contains(text(), 'Address 2')]");
        private readonly By address3Column = By.XPath("//th[contains(.//span, 'Address 3')] | //span[contains(@class, 'k-column-title') and contains(text(), 'Address 3')]");
        private readonly By cityColumn = By.XPath("//th[contains(.//span, 'City')] | //span[contains(@class, 'k-column-title') and contains(text(), 'City')]");
        private readonly By stateColumn = By.XPath("//th[contains(.//span, 'State')] | //span[contains(@class, 'k-column-title') and contains(text(), 'State')]");
        private readonly By zipCodeColumn = By.XPath("//th[contains(.//span, 'Zip Code')] | //span[contains(@class, 'k-column-title') and contains(text(), 'Zip Code')]");

        // Kendo Grid specific locators
        private readonly By kendoGrid = By.XPath("//kendo-grid | //*[contains(@class, 'k-grid')]");
        private readonly By kendoGridHeaders = By.XPath("//kendo-grid//thead//th | //*[contains(@class, 'k-grid')]//thead//th");
        private readonly By kendoGridRows = By.XPath("//kendo-grid//tbody//tr[contains(@class, 'k-master-row')] | //*[contains(@class, 'k-grid')]//tbody//tr[contains(@class, 'k-master-row')]");

        // View links in the first column
        private readonly By viewLinks = By.XPath("//a[contains(@class, 'grid-link') and contains(text(), 'View')] | //a[contains(@title, 'View this beneficiary')]");

        // Export to CSV specific to beneficiaries page
        private readonly By exportToCsvButton = By.XPath("//button[contains(text(), 'Export to CSV')] | //*[contains(text(), 'Export to CSV')]");

        // Kendo Pager specific elements - more specific locators based on HTML structure
        private readonly By kendoPager = By.XPath("//kendo-pager | //*[contains(@class, 'k-pager')]");
        private readonly By kendoPagerInfo = By.XPath("//kendo-pager-info | //*[contains(@class, 'k-pager-info')]");
        private readonly By kendoPagerItemsPerPage = By.XPath("//kendo-numerictextbox[contains(@class, 'page-size-input')] | //div[contains(text(), 'items per page')]");
        private readonly By kendoPagerPageNumbers = By.XPath("//kendo-pager-numeric-buttons | //*[contains(@class, 'k-pager-numbers')]");

        #endregion

        #region Page-Specific Verification Methods

        public bool VerifyBeneficiariesHeader()
        {
            try
            {
                return IsElementVisible(beneficiariesHeader);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyBeneficiariesTabActive()
        {
            try
            {
                var beneficiariesTabElement = Driver.FindElements(beneficiariesTab).FirstOrDefault();
                if (beneficiariesTabElement == null) return false;

                // Check for active state indicators based on HTML structure
                var classes = beneficiariesTabElement.GetAttribute("class") ?? "";
                var ariaCurrent = beneficiariesTabElement.GetAttribute("aria-current") ?? "";

                return classes.Contains("active", StringComparison.OrdinalIgnoreCase) ||
                       classes.Contains("summit-page-nav--active-link", StringComparison.OrdinalIgnoreCase) ||
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
                    IsElementVisible(beneficiaryColumn),
                    IsElementVisible(addressColumn),
                    IsElementVisible(address2Column),
                    IsElementVisible(address3Column),
                    IsElementVisible(cityColumn),
                    IsElementVisible(stateColumn),
                    IsElementVisible(zipCodeColumn)
                };

                return columnChecks.All(check => check);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyViewLinksInRows()
        {
            try
            {
                var viewLinksElements = Driver.FindElements(viewLinks);
                var dataRows = Driver.FindElements(kendoGridRows);

                if (dataRows.Count == 0)
                {
                    // If no data rows, no view links expected
                    return true;
                }

                // Check that have at least some view links
                return viewLinksElements.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyMultiPageDataSet()
        {
            try
            {
                var pageStatusText = GetPageStatus();
                if (string.IsNullOrEmpty(pageStatusText))
                {
                    return false;
                }

                // Check if page status indicates multiple pages ("Page 1 of X" where X > 1)
                var pagePattern = System.Text.RegularExpressions.Regex.Match(pageStatusText, @"Page\s+\d+\s+of\s+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (pagePattern.Success)
                {
                    var totalPages = int.Parse(pagePattern.Groups[1].Value);
                    return totalPages > 1;
                }

                // Alternative pattern for item count ("1 - 10 of many items")
                var itemPattern = System.Text.RegularExpressions.Regex.Match(pageStatusText, @"(\d+)\s*-\s*\d+\s+of\s+(\d+)\s+items?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (itemPattern.Success)
                {
                    var totalItems = int.Parse(itemPattern.Groups[2].Value);
                    var startItem = int.Parse(itemPattern.Groups[1].Value);
                    return totalItems > 10 && startItem == 1;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyItemCountSummary()
        {
            try
            {
                var pageStatusText = GetPageStatus();
                if (string.IsNullOrEmpty(pageStatusText))
                {
                    return false;
                }

                // Check for patterns like "1 - 10 of any items"
                var itemCountPattern = System.Text.RegularExpressions.Regex.Match(pageStatusText, @"\d+\s*-\s*\d+\s+of\s+\d+\s+items?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                return itemCountPattern.Success;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Page-Specific Action Methods

        public void ClickBeneficiariesTab()
        {
            try
            {
                Thread.Sleep(2000);
                var tab = Driver.WaitClickable(beneficiariesTab, TimeSpan.FromSeconds(10));
                tab.Click();
                
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to click Beneficiaries tab: {ex.Message}");
            }
        }

        public void SearchBeneficiaries(string searchText)
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
                throw new Exception($"Failed to search beneficiaries: {ex.Message}");
            }
        }

        public void ClickAddNewBeneficiary()
        {
            try
            {
                var addButton = Driver.WaitClickable(addNewBeneficiaryButton, TimeSpan.FromSeconds(10));
                addButton.Click();

                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to click Add New Beneficiary: {ex.Message}");
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

        public void ClickFirstViewLink()
        {
            try
            {
                var firstViewLink = Driver.WaitClickable(viewLinks, TimeSpan.FromSeconds(10));
                firstViewLink.Click();

                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to click first View link: {ex.Message}");
            }
        }

        #endregion

        #region Override Common Methods for Kendo Grid

        public override List<string> GetColumnHeaders()
        {
            try
            {
                // Try Kendo Grid specific headers first
                var kendoHeaders = Driver.FindElements(By.XPath("//kendo-grid//thead//th//span[contains(@class, 'k-column-title')]"));
                if (kendoHeaders.Count > 0)
                {
                    return kendoHeaders.Select(h => h.Text.Trim()).Where(text => !string.IsNullOrEmpty(text)).ToList();
                }

                // Fallback to standard headers
                var headers = Driver.FindElements(kendoGridHeaders);
                if (headers.Count > 0)
                {
                    return headers.Select(h => h.Text.Trim()).Where(text => !string.IsNullOrEmpty(text)).ToList();
                }

                // Final fallback to base implementation
                return base.GetColumnHeaders();
            }
            catch
            {
                return new List<string>();
            }
        }

        public override int GetDataRowCount()
        {
            try
            {
                // Try Kendo Grid specific rows first
                var kendoRows = Driver.FindElements(kendoGridRows);
                if (kendoRows.Count > 0)
                {
                    return kendoRows.Count;
                }

                // Fallback to base implementation
                return base.GetDataRowCount();
            }
            catch
            {
                return 0;
            }
        }

        public override string GetPageStatus()
        {
            try
            {
                // Try Kendo Pager info first
                var kendoPagerInfoElement = Driver.FindElements(kendoPagerInfo).FirstOrDefault();
                if (kendoPagerInfoElement != null)
                {
                    return kendoPagerInfoElement.Text;
                }

                // Fallback to base implementation
                return base.GetPageStatus();
            }
            catch
            {
                return "";
            }
        }

        public override bool VerifyDataTablePresent()
        {
            try
            {
                // Try Kendo Grid first, then fallback to base implementation
                return IsElementVisible(kendoGrid) || base.VerifyDataTablePresent();
            }
            catch
            {
                return false;
            }
        }

        public override bool VerifyPaginationControls()
        {
            try
            {
                // Try Kendo Pager first, then fallback to base implementation
                return IsElementVisible(kendoPager) || base.VerifyPaginationControls();
            }
            catch
            {
                return false;
            }
        }

        public override bool VerifyPageStatusDisplay()
        {
            try
            {
                // Try Kendo Pager info first, then fallback to base implementation
                return IsElementVisible(kendoPagerInfo) || base.VerifyPageStatusDisplay();
            }
            catch
            {
                return false;
            }
        }

        public override bool VerifyItemsPerPageSelector()
        {
            try
            {
                // Try Kendo specific selector first, then fallback to base implementation
                return IsElementVisible(kendoPagerItemsPerPage) || base.VerifyItemsPerPageSelector();
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Utility Methods

        public bool IsPageLoaded()
        {
            try
            {
                // Check for either the unique locator or the Kendo grid
                return IsElementVisible(UniqueLocator) || IsElementVisible(kendoGrid);
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
                // Fallback to waiting for Kendo grid
                Driver.WaitVisible(kendoGrid, TimeSpan.FromSeconds(15));
            }
        }

        #endregion
    }
}
