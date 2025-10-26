using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TUEL.TestFramework.Web.Support;

namespace TUEL.TestFramework.Web.PageObjects
{
    public class HealthCheckPOM : BasePage
    {
        public HealthCheckPOM(IWebDriver driver) : base(driver)
        {
        }

        // Primary page identifier
        protected override By UniqueLocator => By.CssSelector("h1.summit-page-container-title-text");

        #region Page & Header Elements
        private readonly By pageTitle = By.CssSelector("h1.summit-page-container-title-text");
        private readonly By systemHealthHeader = By.CssSelector(".summit-health--header-title");

        // Navigation Elements
        private readonly By healthCheckNavActive = By.CssSelector("a[href='/healthcheck'].active");
        #endregion

        #region Overall Health Status
        private readonly By overallHealthSection = By.CssSelector(".summit-health--header");
        private readonly By overallHealthStatus = By.CssSelector(".summit-health--header .summit-badge");
        #endregion

        #region System Components
        // Component accordions
        private readonly By allComponentAccordions = By.CssSelector("summit-accordion");

        // Component titles
        private readonly By componentTitles = By.CssSelector(".summit-accordion--title");

        // Component status badges
        private readonly By componentStatusBadges = By.CssSelector("summit-accordion .summit-badge");

        // Individual component elements within accordions
        private readonly By accordionTitle = By.CssSelector(".summit-accordion--title");
        private readonly By accordionContentWrapper = By.CssSelector(".summit-accordion--content-wrapper");
        private readonly By accordionStatusBadge = By.CssSelector(".summit-badge");
        private readonly By accordionChevronIcon = By.CssSelector("summit-icon");
        #endregion

        #region Expand/Collapse Functionality
        private readonly By expandCollapseButton = By.CssSelector("button[class='summit-pill-button summit-pill-button--link']");
        #endregion

        #region Expected Values from Real HTML
        // Only two status values exist in the real HTML
        private readonly string[] validHealthStatusTexts =
        [
            "Healthy", "Unhealthy"
        ];

        // Real component names from actual HTML
        private readonly string[] expectedComponentNames =
        [
            "Azure Blob Storage",
            "Domain API",
            "Account Domain Api",
            "Product Domain API",
            "Queues"
        ];
        #endregion

        #region Core Verification Methods

        public override bool VerifyPageTitle()
        {
            try
            {
                // Check the page heading
                var pageElement = Driver.FindElement(pageTitle);
                var pageText = pageElement.Text?.Trim() ?? "";
                return pageText.Contains("System Health", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifySystemHealthHeader()
        {
            try
            {
                return IsElementVisible(systemHealthHeader);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyHealthCheckNavActive()
        {
            try
            {
                return IsElementVisible(healthCheckNavActive);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyHealthCheckNavHighlighted()
        {
            try
            {
                return IsElementVisible(healthCheckNavActive);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Overall Health Status Verification

        public bool VerifyOverallHealthSection()
        {
            try
            {
                return IsElementVisible(overallHealthSection);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyOverallHealthStatus()
        {
            try
            {
                return IsElementVisible(overallHealthStatus);
            }
            catch
            {
                return false;
            }
        }

        public string GetOverallHealthStatus()
        {
            try
            {
                var statusElement = Driver.FindElement(overallHealthStatus);
                return statusElement.Text?.Trim() ?? "";
            }
            catch
            {
                return "";
            }
        }

        public bool IsValidHealthStatus(string statusText)
        {
            if (string.IsNullOrEmpty(statusText)) return false;

            return validHealthStatusTexts.Any(valid =>
                statusText.Contains(valid, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region System Components Verification

        public bool VerifyAllSystemComponentsPresent()
        {
            try
            {
                var componentElements = Driver.FindElements(componentTitles);
                return componentElements.Count >= 5;
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyComponentHealthStatuses()
        {
            try
            {
                var statusElements = Driver.FindElements(componentStatusBadges);
                return statusElements.Count >= 5;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetComponentNames()
        {
            try
            {
                var componentNames = new List<string>();
                var titleElements = Driver.FindElements(componentTitles);

                foreach (var element in titleElements)
                {
                    var text = element.Text?.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        componentNames.Add(text);
                    }
                }

                return componentNames;
            }
            catch
            {
                return new List<string>();
            }
        }

        public Dictionary<string, string> GetComponentStatuses()
        {
            try
            {
                var statuses = new Dictionary<string, string>();
                var accordionElements = Driver.FindElements(allComponentAccordions);

                foreach (var accordion in accordionElements)
                {
                    try
                    {
                        var titleElement = accordion.FindElement(accordionTitle);
                        var statusElement = accordion.FindElement(accordionStatusBadge);

                        var componentName = titleElement.Text?.Trim() ?? "";
                        var statusText = statusElement.Text?.Trim() ?? "";

                        if (!string.IsNullOrEmpty(componentName))
                        {
                            statuses[componentName] = statusText;
                        }
                    }
                    catch
                    {
                    }
                }

                return statuses;
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        public bool VerifyValidHealthStatusesDisplayed()
        {
            try
            {
                var componentStatuses = GetComponentStatuses();
                int validStatuses = 0;

                foreach (var status in componentStatuses.Values)
                {
                    if (IsValidHealthStatus(status))
                    {
                        validStatuses++;
                    }
                }

                return validStatuses >= Math.Max(1, componentStatuses.Count * 0.8);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Expand/Collapse Functionality Verification

        public bool VerifyExpandCollapseToggle()
        {
            try
            {
                return IsElementVisible(expandCollapseButton);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyExpandAllButton()
        {
            try
            {
                var buttonElement = Driver.FindElement(expandCollapseButton);
                var buttonText = buttonElement.Text?.Trim() ?? "";
                return buttonText.Contains("Expand all", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyCollapseAllButton()
        {
            try
            {
                var buttonElement = Driver.FindElement(expandCollapseButton);
                var buttonText = buttonElement.Text?.Trim() ?? "";
                return buttonText.Contains("Collapse all", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public string GetExpandCollapseButtonText()
        {
            try
            {
                var buttonElement = Driver.FindElement(expandCollapseButton);
                return buttonElement.Text?.Trim() ?? "";
            }
            catch
            {
                return "";
            }
        }

        #endregion

        #region Component Details Verification

        public Dictionary<string, string> GetComponentDetails()
        {
            try
            {
                var details = new Dictionary<string, string>();
                var accordionElements = Driver.FindElements(allComponentAccordions);

                foreach (var accordion in accordionElements)
                {
                    try
                    {
                        var titleElement = accordion.FindElement(accordionTitle);
                        var detailElement = accordion.FindElement(accordionContentWrapper);

                        var componentName = titleElement.Text?.Trim() ?? "";
                        var detailText = detailElement.Text?.Trim() ?? "";

                        if (!string.IsNullOrEmpty(componentName))
                        {
                            details[componentName] = detailText;
                        }
                    }
                    catch
                    {
                    }
                }

                return details;
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        #endregion

        #region Action Methods

        public void ClickExpandCollapseButton()
        {
            try
            {
                if (IsElementVisible(expandCollapseButton))
                {
                    Click(expandCollapseButton);
                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(3));
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click expand/collapse button: {ex.Message}");
            }
        }

        public void ClickExpandAllButton()
        {
            try
            {
                if (VerifyExpandAllButton())
                {
                    Click(expandCollapseButton);
                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(3));
                }
                else
                {
                    throw new InvalidOperationException("Expand All button is not available");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click Expand All button: {ex.Message}");
            }
        }

        public void ClickCollapseAllButton()
        {
            try
            {
                if (VerifyCollapseAllButton())
                {
                    Click(expandCollapseButton);
                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(3));
                }
                else
                {
                    throw new InvalidOperationException("Collapse All button is not available");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click Collapse All button: {ex.Message}");
            }
        }

        public void ExpandComponent(string componentName)
        {
            try
            {
                var accordionElements = Driver.FindElements(allComponentAccordions);

                foreach (var accordion in accordionElements)
                {
                    try
                    {
                        var titleElement = accordion.FindElement(accordionTitle);
                        var titleText = titleElement.Text?.Trim() ?? "";

                        if (titleText.Equals(componentName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Find the chevron icon and click it to expand
                            var chevronIcon = accordion.FindElement(accordionChevronIcon);
                            chevronIcon.Click();
                            Driver.WaitForPageTransition(TimeSpan.FromSeconds(2));
                            return;
                        }
                    }
                    catch
                    {
                    }
                }

                throw new InvalidOperationException($"Component '{componentName}' not found");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to expand component '{componentName}': {ex.Message}");
            }
        }

        #endregion

        #region Missing Methods Needed by Test

        public bool VerifyComponentDetailsContainMeaningfulContent()
        {
            try
            {
                var details = GetComponentDetails();
                int meaningfulDetails = 0;

                foreach (var detail in details.Values)
                {
                    if (!string.IsNullOrEmpty(detail) && detail.Length > 10)
                    {
                        meaningfulDetails++;
                    }
                }

                return meaningfulDetails >= Math.Max(1, details.Count * 0.6);
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<string, bool> AnalyzeComponentDetailQuality()
        {
            try
            {
                var details = GetComponentDetails();
                var qualityAnalysis = new Dictionary<string, bool>();

                foreach (var componentName in expectedComponentNames)
                {
                    bool hasQualityDetail = false;
                    if (details.ContainsKey(componentName))
                    {
                        var detail = details[componentName];
                        hasQualityDetail = !string.IsNullOrEmpty(detail) && detail.Length > 10;
                    }
                    qualityAnalysis[componentName] = hasQualityDetail;
                }

                return qualityAnalysis;
            }
            catch
            {
                return new Dictionary<string, bool>();
            }
        }

        public string GetHealthScenarioClassification()
        {
            try
            {
                var statuses = GetComponentStatuses();
                int healthyCount = 0;
                int unhealthyCount = 0;

                foreach (var status in statuses.Values)
                {
                    if (status.Equals("Healthy", StringComparison.OrdinalIgnoreCase))
                        healthyCount++;
                    else if (status.Equals("Unhealthy", StringComparison.OrdinalIgnoreCase))
                        unhealthyCount++;
                }

                if (healthyCount > 0 && unhealthyCount == 0) return "All Healthy";
                if (unhealthyCount > 0 && healthyCount == 0) return "All Unhealthy";
                if (healthyCount > 0 && unhealthyCount > 0) return "Mixed Health Status";

                return "Unknown Status";
            }
            catch
            {
                return "Classification Error";
            }
        }

        public Dictionary<string, int> GetHealthStatusSummary()
        {
            try
            {
                var statuses = GetComponentStatuses();
                var summary = new Dictionary<string, int>
                {
                    { "Healthy", 0 },
                    { "Unhealthy", 0 },
                    { "Unknown", 0 }
                };

                foreach (var status in statuses.Values)
                {
                    if (status.Equals("Healthy", StringComparison.OrdinalIgnoreCase))
                        summary["Healthy"]++;
                    else if (status.Equals("Unhealthy", StringComparison.OrdinalIgnoreCase))
                        summary["Unhealthy"]++;
                    else
                        summary["Unknown"]++;
                }

                return summary;
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }

        public Dictionary<string, string> GetComponentHealthStatusTypes()
        {
            try
            {
                return GetComponentStatuses();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        #endregion

        #region Utility Methods

        public bool IsPageLoaded()
        {
            try
            {
                return VerifySystemHealthHeader() || VerifyAllSystemComponentsPresent();
            }
            catch
            {
                return false;
            }
        }

        public int GetVisibleComponentCount()
        {
            try
            {
                return GetComponentNames().Count;
            }
            catch
            {
                return 0;
            }
        }

        public bool WaitForHealthDataReady(int timeoutSeconds = 10)
        {
            for (int attempt = 1; attempt <= 2; attempt++)
            {
                try
                {
                    var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));

                    bool success = wait.Until(driver =>
                    {
                        var accordions = driver.FindElements(allComponentAccordions);
                        if (accordions.Count < 1) return false;

                        int validStatuses = accordions.Count(accordion =>
                        {
                            try
                            {
                                var statusBadge = accordion.FindElement(accordionStatusBadge);
                                return IsValidHealthStatus(statusBadge.Text.Trim());
                            }
                            catch
                            {
                                return false;
                            }
                        });

                        return validStatuses >= 1;
                    });

                    if (success) return true;
                }
                catch
                {
                    // Try refresh on first attempt
                }

                // Retry with refresh
                if (attempt == 1)
                {
                    try
                    {
                        Driver.Navigate().Refresh();
                        Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
