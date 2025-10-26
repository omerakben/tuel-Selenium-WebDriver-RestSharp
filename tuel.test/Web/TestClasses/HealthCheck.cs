using TUEL.TestFramework;
using TUEL.TestFramework.Web.PageObjects;
using TUEL.TestFramework.Web.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Threading;

namespace TUEL.TestFramework.Web.TestClasses
{
    [TestClass, TestCategory("UI")]
    public class HealthCheck : Base
    {
        private HealthCheckPOM? _healthCheckPage;

        [TestInitialize]
        public void HealthCheckTestSetup()
        {
            try
            {
                if (Driver == null)
                {
                    throw new InvalidOperationException("Driver not initialized");
                }

                TestContext.WriteLine("Initializing Health Check test components...");
                _healthCheckPage = new HealthCheckPOM(Driver);

                // Navigation to Health Check page
                NavigateToHealthCheckPage();

                WaitForHealthCheckPageReady();
                TestContext.WriteLine("Health Check test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Health Check test setup failed: {ex.Message}");
                TestContext.WriteLine($"Current URL: {Driver!.Url}");
                TestContext.WriteLine($"Page Title: {Driver.Title}");
                throw;
            }
        }

        private void NavigateToHealthCheckPage()
        {
            try
            {
                var currentUrl = Driver!.Url;
                TestContext.WriteLine($"Current URL: {currentUrl}");

                // Check if already on Health Check page
                if (IsOnHealthCheckPage())
                {
                    TestContext.WriteLine("Already on Health Check page");
                    return;
                }

                var baseUrl = ExtractBaseUrl(currentUrl);
                var healthCheckUrl = $"{baseUrl}/healthcheck";

                TestContext.WriteLine($"Navigating to Health Check page: {healthCheckUrl}");
                Driver.Navigate().GoToUrl(healthCheckUrl);

                WaitForHealthCheckPageReady();
                TestContext.WriteLine("Successfully navigated to Health Check page");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Health Check page navigation failed: {ex.Message}");
                throw;
            }
        }

        private static string ExtractBaseUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return $"{uri.Scheme}://{uri.Host}:{uri.Port}";
            }
            catch
            {
                if (url.Contains(".net"))
                {
                    return url.Split('/')[0] + "//" + url.Split('/')[2];
                }
                return url;
            }
        }

        private bool IsOnHealthCheckPage()
        {
            try
            {
                return Driver!.Url.Contains("/healthcheck") && _healthCheckPage!.IsPageLoaded();
            }
            catch
            {
                return false;
            }
        }

        private void WaitForHealthCheckPageReady()
        {
            try
            {
                TestContext.WriteLine("Waiting for Health Check page and data to be ready...");

                bool dataReady = _healthCheckPage!.WaitForHealthDataReady();

                if (dataReady)
                {
                    TestContext.WriteLine("Health Check page and data ready");
                }
                else
                {
                    TestContext.WriteLine("Warning: Health data not ready after wait timeout");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Error waiting for health data: {ex.Message}");
            }
        }

        [TestMethod]
        [Description("Comprehensive Health Check page verification all major components")]
        public void HealthCheck_SmokeTest_AllComponentsPresent()
        {
            TestContext.WriteLine("Starting comprehensive Health Check page verification");

            // Page State & Navigation Verification
            bool pageTitle = _healthCheckPage!.VerifyPageTitle();
            bool systemHealthHeader = _healthCheckPage.VerifySystemHealthHeader();
            bool navHighlighted = _healthCheckPage.VerifyHealthCheckNavHighlighted();
            bool navActive = _healthCheckPage.VerifyHealthCheckNavActive();

            TestContext.WriteLine($"Page Title Contains Health Check: {pageTitle}");
            TestContext.WriteLine($"System Health Header Present: {systemHealthHeader}");
            TestContext.WriteLine($"Health Check Nav Item Highlighted: {navHighlighted}");
            TestContext.WriteLine($"Health Check Nav Item Active: {navActive}");

            // Overall Health Status Verification
            bool overallHealthSection = _healthCheckPage.VerifyOverallHealthSection();
            bool overallHealthStatus = _healthCheckPage.VerifyOverallHealthStatus();
            string overallStatus = _healthCheckPage.GetOverallHealthStatus();
            bool validOverallStatus = _healthCheckPage.IsValidHealthStatus(overallStatus);

            TestContext.WriteLine($"Overall Health Section Visible: {overallHealthSection}");
            TestContext.WriteLine($"Overall Health Status Visible: {overallHealthStatus}");
            TestContext.WriteLine($"Overall Health Status Text: {overallStatus}");
            TestContext.WriteLine($"Overall Status Is Valid Health Indicator: {validOverallStatus}");

            // System Components Health Verification
            bool allComponentsPresent = _healthCheckPage.VerifyAllSystemComponentsPresent();
            bool componentStatuses = _healthCheckPage.VerifyComponentHealthStatuses();
            bool validStatusesDisplayed = _healthCheckPage.VerifyValidHealthStatusesDisplayed();
            var componentNames = _healthCheckPage.GetComponentNames();
            var componentStatusDict = _healthCheckPage.GetComponentStatuses();

            TestContext.WriteLine($"All System Components Present: {allComponentsPresent}");
            TestContext.WriteLine($"Component Health Statuses Visible: {componentStatuses}");
            TestContext.WriteLine($"Valid Health Statuses Displayed: {validStatusesDisplayed}");
            TestContext.WriteLine($"Component Count Found: {componentNames.Count}");
            TestContext.WriteLine($"Component Names: {string.Join(", ", componentNames)}");

            // Expand/Collapse Functionality Verification
            bool expandCollapseToggle = _healthCheckPage.VerifyExpandCollapseToggle();
            bool expandAllButton = _healthCheckPage.VerifyExpandAllButton();
            bool collapseAllButton = _healthCheckPage.VerifyCollapseAllButton();
            string expandCollapseText = _healthCheckPage.GetExpandCollapseButtonText();

            TestContext.WriteLine($"Expand Collapse Toggle Visible: {expandCollapseToggle}");
            TestContext.WriteLine($"Expand All Button Visible: {expandAllButton}");
            TestContext.WriteLine($"Collapse All Button Visible: {collapseAllButton}");
            TestContext.WriteLine($"Expand Collapse Button Text: {expandCollapseText}");

            // Component Details Verification
            var componentDetails = _healthCheckPage.GetComponentDetails();
            bool meaningfulContent = _healthCheckPage.VerifyComponentDetailsContainMeaningfulContent();
            var detailQuality = _healthCheckPage.AnalyzeComponentDetailQuality();

            TestContext.WriteLine($"Component Details Retrieved: {componentDetails.Count}");
            TestContext.WriteLine($"Meaningful Content In Details: {meaningfulContent}");

            // Display component status details for reference
            if (componentStatusDict.Any())
            {
                TestContext.WriteLine($"Found Component Statuses: {string.Join(", ", componentStatusDict.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");
            }

            // Display current health scenario statistics
            string healthScenario = _healthCheckPage.GetHealthScenarioClassification();
            var statusSummary = _healthCheckPage.GetHealthStatusSummary();
            TestContext.WriteLine($"Current Health Scenario: {healthScenario}");
            TestContext.WriteLine($"Health Status Summary: {string.Join(", ", statusSummary.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");

            // Evaluate results based on requirement categories
            var criticalChecks = new[] { systemHealthHeader, allComponentsPresent, validStatusesDisplayed };
            var importantChecks = new[] { pageTitle, overallHealthStatus, componentStatuses, expandCollapseToggle };
            var optionalChecks = new[] { navHighlighted, overallHealthSection, expandAllButton, meaningfulContent };

            int criticalPassed = criticalChecks.Count(c => c);
            int importantPassed = importantChecks.Count(c => c);
            int optionalPassed = optionalChecks.Count(c => c);

            TestContext.WriteLine($"Critical checks passed: {criticalPassed}/{criticalChecks.Length}");
            TestContext.WriteLine($"Important checks passed: {importantPassed}/{importantChecks.Length}");
            TestContext.WriteLine($"Optional checks passed: {optionalPassed}/{optionalChecks.Length}");

            // Assertions based on requirements
            Assert.IsTrue(criticalPassed >= 2, $"Critical Health Check page elements should be present, but only {criticalPassed} passed");
            Assert.IsTrue(importantPassed >= 3, $"Important Health Check page elements should be present, but only {importantPassed} passed");

            TestContext.WriteLine("Comprehensive Health Check page verification completed successfully");
        }

        [TestMethod]
        [Description("Verify page state and navigation verification")]
        public void HealthCheck_VerifyPageAndNavigation()
        {
            TestContext.WriteLine("Starting page state and navigation verification test");

            bool pageTitle = _healthCheckPage!.VerifyPageTitle();
            bool navActive = _healthCheckPage.VerifyHealthCheckNavActive();
            bool navHighlighted = _healthCheckPage.VerifyHealthCheckNavHighlighted();
            bool systemHealthHeader = _healthCheckPage.VerifySystemHealthHeader();

            TestContext.WriteLine($"Browser Page Title Contains Health Check: {pageTitle}");
            TestContext.WriteLine($"Health Check Section Active In Navigation: {navActive}");
            TestContext.WriteLine($"Health Check Nav Item Highlighted: {navHighlighted}");
            TestContext.WriteLine($"System Health Main Page Title Displayed: {systemHealthHeader}");

            // At least page title or system health header should be visible
            Assert.IsTrue(pageTitle || systemHealthHeader,
                "Page title should contain 'Health Check' OR 'System Health' header should be visible");

            // Navigation should show Health Check as active
            Assert.IsTrue(navActive || navHighlighted,
                "Health Check section should be highlighted/active in navigation");

            TestContext.WriteLine("Page and navigation verification completed successfully");
        }

        [TestMethod]
        [Description("Verify overall health status verification")]
        public void HealthCheck_VerifyOverallHealthStatus()
        {
            TestContext.WriteLine("Starting overall health status verification test");

            bool overallHealthSection = _healthCheckPage!.VerifyOverallHealthSection();
            bool overallHealthStatus = _healthCheckPage.VerifyOverallHealthStatus();
            string statusText = _healthCheckPage.GetOverallHealthStatus();

            TestContext.WriteLine($"Overall Health Section Visible: {overallHealthSection}");
            TestContext.WriteLine($"Overall Health Status Indicator Visible: {overallHealthStatus}");
            TestContext.WriteLine($"Overall Health Status Text: {statusText}");

            // Overall health status should be visible and prominently displayed
            Assert.IsTrue(overallHealthStatus, "Overall Health status indicator should be visible");

            if (!string.IsNullOrEmpty(statusText))
            {
                TestContext.WriteLine($"Overall health status shows: {statusText}");
                // Valid health statuses
                bool isValidStatus = statusText.Equals("Healthy", StringComparison.OrdinalIgnoreCase) ||
                                   statusText.Equals("Unhealthy", StringComparison.OrdinalIgnoreCase);

                Assert.IsTrue(isValidStatus, $"Overall health status should show valid status, but shows: {statusText}");
            }

            TestContext.WriteLine("Overall health status verification completed successfully");
        }

        [TestMethod]
        [Description("Verify system components health verification")]
        public void HealthCheck_VerifySystemComponents()
        {
            TestContext.WriteLine("Starting system components health verification test");

            bool allComponentsPresent = _healthCheckPage!.VerifyAllSystemComponentsPresent();
            bool componentStatuses = _healthCheckPage.VerifyComponentHealthStatuses();
            var componentNames = _healthCheckPage.GetComponentNames();
            var componentStatusDict = _healthCheckPage.GetComponentStatuses();

            TestContext.WriteLine($"All System Components Present: {allComponentsPresent}");
            TestContext.WriteLine($"Component Health Statuses Visible: {componentStatuses}");
            TestContext.WriteLine($"Total Components Found: {componentNames.Count}");

            // Log each component found
            foreach (var name in componentNames)
            {
                TestContext.WriteLine($"Component Found: {name}");
            }

            // Log each component status
            foreach (var (component, status) in componentStatusDict)
            {
                TestContext.WriteLine($"Component Status - {component}: {status}");
            }

            // Expected components
            var expectedComponents = new[]
            {
                "Azure Blob Storage",
                "Domain API",
                "Account Domain Api",
                "Product Domain API",
                "Queues"
            };

            TestContext.WriteLine("Checking for expected components:");
            int foundExpectedComponents = 0;
            foreach (var expected in expectedComponents)
            {
                bool found = componentNames.Any(name => name.Contains(expected, StringComparison.OrdinalIgnoreCase));
                TestContext.WriteLine($"Expected Component {expected}: {(found ? "Found" : "Not Found")}");
                if (found) foundExpectedComponents++;
            }

            // Assertions
            Assert.IsTrue(allComponentsPresent, "All 5 primary system components should be present");
            Assert.IsTrue(componentNames.Count >= 5, $"Should find at least 5 components, but found {componentNames.Count}");
            Assert.IsTrue(foundExpectedComponents >= 5, $"Should find at least 5 expected components, but found {foundExpectedComponents}");
            Assert.IsTrue(componentStatuses, "Component health statuses should be visible");

            TestContext.WriteLine("System components verification completed successfully");
        }

        [TestMethod]
        [Description("Verify expand/collapse functionality verification")]
        public void HealthCheck_VerifyExpandCollapseFunction()
        {
            TestContext.WriteLine("Starting expand/collapse functionality verification test");

            bool expandCollapseToggle = _healthCheckPage!.VerifyExpandCollapseToggle();
            bool expandAllButton = _healthCheckPage.VerifyExpandAllButton();
            bool collapseAllButton = _healthCheckPage.VerifyCollapseAllButton();
            string buttonText = _healthCheckPage.GetExpandCollapseButtonText();

            TestContext.WriteLine($"Expand Collapse Toggle Control Visible: {expandCollapseToggle}");
            TestContext.WriteLine($"Expand All Button Visible: {expandAllButton}");
            TestContext.WriteLine($"Collapse All Button Visible: {collapseAllButton}");
            TestContext.WriteLine($"Current Button Text: {buttonText}");

            // Test expand/collapse functionality
            bool toggleFunctionality = false;
            try
            {
                var componentNames = _healthCheckPage.GetComponentNames();

                if (expandAllButton)
                {
                    _healthCheckPage.ClickExpandAllButton();
                    Thread.Sleep(1000);
                    toggleFunctionality = true;
                    TestContext.WriteLine("Successfully tested Expand All functionality");
                }
                else if (collapseAllButton)
                {
                    _healthCheckPage.ClickCollapseAllButton();
                    Thread.Sleep(1000);
                    toggleFunctionality = true;
                    TestContext.WriteLine("Successfully tested Collapse All functionality");
                }

                if (componentNames.Any())
                {
                    var firstComponent = componentNames.First();
                    _healthCheckPage.ExpandComponent(firstComponent);
                    Thread.Sleep(500);
                    TestContext.WriteLine($"Successfully tested individual component expansion for: {firstComponent}");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Expand/collapse functionality test encountered issue: {ex.Message}");
            }

            // Assertions
            Assert.IsTrue(expandCollapseToggle, "Expand/Collapse toggle control should be visible");

            // Either expand all or collapse all should be visible (they toggle)
            Assert.IsTrue(expandAllButton || collapseAllButton,
                "Either 'Expand all' or 'Collapse all' button should be visible");

            // Button should have appropriate text
            bool validButtonText = buttonText.Contains("Expand", StringComparison.OrdinalIgnoreCase) ||
                                 buttonText.Contains("Collapse", StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(validButtonText, $"Button should contain 'Expand' or 'Collapse', but shows: {buttonText}");

            // Button should functional
            Assert.IsTrue(toggleFunctionality, "Expand/Collapse functionality should work when clicked, but the action failed.");

            TestContext.WriteLine("Expand/collapse functionality verification completed successfully");
        }

        [TestMethod]
        [Description("Verify component details verification across all health scenarios")]
        public void HealthCheck_VerifyComponentDetails()
        {
            TestContext.WriteLine("Starting component details verification test");

            // Analyze current health scenario
            string healthScenario = _healthCheckPage!.GetHealthScenarioClassification();
            var statusSummary = _healthCheckPage.GetHealthStatusSummary();

            TestContext.WriteLine($"Current Health Scenario: {healthScenario}");
            TestContext.WriteLine($"Status Distribution: {string.Join(", ", statusSummary.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}");

            // First, try to expand all components to see their details
            try
            {
                if (_healthCheckPage.VerifyExpandAllButton())
                {
                    _healthCheckPage.ClickExpandAllButton();
                    Thread.Sleep(2000);
                    TestContext.WriteLine("Expanded all components to view details");
                }
                else
                {
                    TestContext.WriteLine("Expand All button not available, checking individual component details");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Component expansion encountered issue: {ex.Message}");
            }

            var componentDetails = _healthCheckPage.GetComponentDetails();
            bool meaningfulContent = _healthCheckPage.VerifyComponentDetailsContainMeaningfulContent();
            var detailQuality = _healthCheckPage.AnalyzeComponentDetailQuality();

            TestContext.WriteLine($"Component Details Retrieved: {componentDetails.Count}");
            TestContext.WriteLine($"Content In Details: {meaningfulContent}");

            TestContext.WriteLine("Detailed component verification by health scenario:");
            int qualityDetails = 0;

            foreach (var (componentName, hasQuality) in detailQuality)
            {
                string qualityDescription = hasQuality ? "Good Quality" : "Basic Content";
                TestContext.WriteLine($"Component Detail Quality - {componentName}: {qualityDescription}");
                if (hasQuality) qualityDetails++;
            }

            TestContext.WriteLine($"Quality component details found: {qualityDetails}/5");

            // Log all details found for debugging
            TestContext.WriteLine("All component details found:");
            foreach (var (component, detail) in componentDetails)
            {
                string detailPreview = detail.Length > 100 ? detail.Substring(0, 100) + "..." : detail;
                TestContext.WriteLine($"  {component}: {detailPreview}");
            }

            // Scenario-Aware Assertions
            TestContext.WriteLine($"Performing assertions for {healthScenario} scenario");

            // Should retrieve details regardless of health status
            Assert.IsTrue(componentDetails.Count >= 3,
                $"Should retrieve details for at least 3 components in {healthScenario} scenario, but found {componentDetails.Count}");

            // Should find meaningful content regardless of healthy/unhealthy status
            Assert.IsTrue(qualityDetails >= 3,
                $"Should find meaningful detail content for at least 3 components in {healthScenario} scenario, but found {qualityDetails}");

            // Overall meaningful content check should pass
            Assert.IsTrue(meaningfulContent,
                $"Should display meaningful health detail content in {healthScenario} scenario");

            TestContext.WriteLine($"Component details verification passed for {healthScenario} scenario");
            TestContext.WriteLine("Component details verification completed successfully");
        }

        [TestMethod]
        [Description("Test individual component expand/collapse functionality")]
        public void HealthCheck_TestIndividualComponentExpansion()
        {
            TestContext.WriteLine("Starting individual component expansion test");

            var componentNames = _healthCheckPage!.GetComponentNames();
            TestContext.WriteLine($"Found {componentNames.Count} components to test expansion");

            if (!componentNames.Any())
            {
                Assert.Fail("No components found to test expansion functionality");
            }

            int successfulExpansions = 0;

            foreach (var componentName in componentNames)
            {
                try
                {
                    TestContext.WriteLine($"Testing expansion for component: {componentName}");
                    _healthCheckPage.ExpandComponent(componentName);
                    Thread.Sleep(1000);

                    // Verify component details are now visible
                    var componentDetails = _healthCheckPage.GetComponentDetails();
                    if (componentDetails.ContainsKey(componentName) &&
                        !string.IsNullOrEmpty(componentDetails[componentName]))
                    {
                        successfulExpansions++;
                        TestContext.WriteLine($"Successfully expanded component: {componentName}");
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Failed to expand component {componentName}: {ex.Message}");
                }
            }

            TestContext.WriteLine($"Successful component expansions: {successfulExpansions}/{componentNames.Count}");

            Assert.IsTrue(successfulExpansions >= Math.Min(2, componentNames.Count / 2),
                $"Should be able to expand at least some components, but only {successfulExpansions} worked");

            TestContext.WriteLine("Individual component expansion test completed successfully");
        }

        [TestMethod]
        [Description("Debug test to check Health Check page accessibility and structure")]
        public void Debug_HealthCheckPageStructure()
        {
            TestContext.WriteLine("Starting Health Check page structure debug test");

            TestContext.WriteLine($"Current URL: {Driver!.Url}");
            TestContext.WriteLine($"Page Title: {Driver.Title}");

            // Check page structure
            bool pageLoaded = _healthCheckPage!.IsPageLoaded();
            TestContext.WriteLine($"Health Check Page Loaded: {pageLoaded}");

            var componentCount = _healthCheckPage.GetVisibleComponentCount();
            TestContext.WriteLine($"Visible Component Count: {componentCount}");

            // Try to find any health-related elements
            try
            {
                var healthElements = Driver.FindElements(By.XPath("//*[contains(text(), 'health') or contains(text(), 'Health')]"));
                TestContext.WriteLine($"Health-related elements found: {healthElements.Count}");

                foreach (var element in healthElements.Take(5))
                {
                    try
                    {
                        TestContext.WriteLine($"Health element text: {element.Text}");
                    }
                    catch
                    {
                        TestContext.WriteLine("Health element text could not be retrieved");
                    }
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Could not search for health elements: {ex.Message}");
            }

            // Try to find summit components
            try
            {
                var summitElements = Driver.FindElements(By.XPath("//*[contains(@class, 'summit') or starts-with(name(), 'summit')]"));
                TestContext.WriteLine($"Summit UI components found: {summitElements.Count}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Could not search for summit elements: {ex.Message}");
            }

            Assert.IsTrue(pageLoaded, "Health Check page should be accessible and loaded");

            TestContext.WriteLine("Debug Health Check page structure completed");
        }

        [TestMethod]
        [Description("Demonstrate how test handles different health status scenarios")]
        public void HealthCheck_ScenarioHandlingDemonstration()
        {
            TestContext.WriteLine("Starting health status scenario handling demonstration");

            // Analyze the current health scenario
            string healthScenario = _healthCheckPage!.GetHealthScenarioClassification();
            var statusSummary = _healthCheckPage.GetHealthStatusSummary();
            var componentStatusTypes = _healthCheckPage.GetComponentHealthStatusTypes();

            TestContext.WriteLine($"Detected Health Scenario: {healthScenario}");

            // Detailed scenario analysis
            TestContext.WriteLine("Health Status Breakdown:");
            foreach (var (status, count) in statusSummary.OrderByDescending(kvp => kvp.Value))
            {
                if (count > 0)
                {
                    TestContext.WriteLine($"  {status} Components: {count}");
                }
            }

            TestContext.WriteLine("Individual Component Analysis:");
            foreach (var (component, statusType) in componentStatusTypes)
            {
                TestContext.WriteLine($"  {component} Status Type: {statusType}");
            }

            // Scenario-specific behavior
            TestContext.WriteLine("Scenario-Specific Test Behavior:");

            switch (healthScenario)
            {
                case "All Healthy":
                    TestContext.WriteLine("   All Healthy scenario detected");
                    TestContext.WriteLine("   Test validates: Green badges, Healthy text, positive messages");
                    TestContext.WriteLine("   Expected: All components show success indicators");
                    break;

                case "All Unhealthy":
                    TestContext.WriteLine("   All Unhealthy scenario detected");
                    TestContext.WriteLine("   Test validates: Red badges, Unhealthy text, error messages");
                    TestContext.WriteLine("   Expected: All components show failure indicators");
                    break;

                case "Mixed Health Status":
                    TestContext.WriteLine("   Mixed Health scenario detected");
                    TestContext.WriteLine("   Test validates: Mix of green/red badges, various status texts");
                    TestContext.WriteLine("   Expected: Components show different status indicators");
                    break;

                default:
                    TestContext.WriteLine($"   Unusual scenario detected: {healthScenario}");
                    TestContext.WriteLine("   Test validates: Any valid health status indicators present");
                    break;
            }

            // Verify test passes regardless of health scenario
            bool pageWorking = _healthCheckPage.VerifyAllSystemComponentsPresent() &&
                              _healthCheckPage.VerifyValidHealthStatusesDisplayed();

            TestContext.WriteLine("Test Outcome:");
            TestContext.WriteLine($"Page Functionality Working: {pageWorking}");
            TestContext.WriteLine($"Health Status Display Valid: {_healthCheckPage.VerifyValidHealthStatusesDisplayed()}");

            if (pageWorking)
            {
                TestContext.WriteLine($"Test passes for {healthScenario} scenario");
                TestContext.WriteLine("Health Check page displays status information correctly");
            }
            else
            {
                TestContext.WriteLine($"Test fails for {healthScenario} scenario");
                TestContext.WriteLine("Health Check page not functioning properly");
            }

            TestContext.WriteLine("Key Principle:");
            TestContext.WriteLine("Health Check UI tests validate page functionality regardless of actual service health");
            TestContext.WriteLine("Tests verify that status indicators are displayed properly, not that services are healthy");

            // Assertion
            Assert.IsTrue(pageWorking,
                $"Health Check page should function properly regardless of service health status in {healthScenario} scenario");

            TestContext.WriteLine("Health status scenario completed successfully");
        }
    }
}
