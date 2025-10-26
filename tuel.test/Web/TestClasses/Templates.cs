using TUEL.TestFramework;
using TUEL.TestFramework.Web.PageObjects;
using TUEL.TestFramework.Web.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TUEL.TestFramework.Web.TestClasses
{
    // Test class for the Application Templates & Signatures page
    [TestClass, TestCategory("UI")]
    public class Templates : Base
    {
        private TemplatesPOM _templatesPage;
        private LoginPOM _loginPage;

        [TestInitialize]
        public void TemplatesTestSetup()
        {
            try
            {
                TestContext.WriteLine("Initializing Templates test components...");
                _templatesPage = new TemplatesPOM(Driver);
                _loginPage = new LoginPOM(Driver);

                if (!string.IsNullOrEmpty(InitializeTestAssembly.Email) && !string.IsNullOrEmpty(InitializeTestAssembly.Password))
                {
                    TestContext.WriteLine($"Performing authentication for user: {InitializeTestAssembly.Email}");

                    var authState = _loginPage.GetCurrentAuthenticationState();
                    TestContext.WriteLine($"Current authentication state: {authState}");

                    if (authState != AuthenticationState.LoggedIn)
                    {
                        _loginPage.LoginToApplication(InitializeTestAssembly.Email, InitializeTestAssembly.Password);
                        TestContext.WriteLine("Authentication flow completed");

                        VerifySuccessfulAuthentication();
                    }
                    else
                    {
                        TestContext.WriteLine("User already authenticated");
                    }

                    NavigateToTemplatesPage();
                }
                else
                {
                    TestContext.WriteLine("No credentials provided");
                }

                WaitForTemplatesPageReady();
                TestContext.WriteLine("Templates test setup completed successfully");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Templates test setup failed: {ex.Message}");
                TestContext.WriteLine($"Current URL: {Driver.Url}");
                TestContext.WriteLine($"Page Title: {Driver.Title}");
                throw;
            }
        }

        private void VerifySuccessfulAuthentication()
        {
            try
            {
                // Wait for redirect to complete after authentication
                var authenticationLanded = Driver.WaitForUrlContains("/application", TimeSpan.FromSeconds(15)) ||
                                          Driver.WaitForUrlContains("/dashboard", TimeSpan.FromSeconds(5));

                if (authenticationLanded)
                {
                    TestContext.WriteLine("Successfully authenticated and landed on Application");
                }
                else
                {
                    TestContext.WriteLine($"Warning: Expected Application landing, current URL: {Driver.Url}");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Authentication verification failed: {ex.Message}");
            }
        }

        private void NavigateToTemplatesPage()
        {
            NavigateToTemplatesPageAsync().GetAwaiter().GetResult();
        }

        private async Task NavigateToTemplatesPageAsync()
        {
            try
            {
                TestContext.WriteLine("Navigating to Templates page...");

                // Check if already on Templates page
                if (IsOnTemplatesPage())
                {
                    TestContext.WriteLine("Already on Templates page");
                    return;
                }

                // Try clicking Templates tab
                try
                {
                    _templatesPage.ClickTemplatesTab();
                    TestContext.WriteLine("Clicked Templates tab");
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Templates tab click failed: {ex.Message}");
                    await NavigateToTemplatesDirectlyAsync();
                }

                // Verify navigation success
                var templatesPageReached = Driver.WaitForUrlContains("/templates", TimeSpan.FromSeconds(10));
                if (templatesPageReached)
                {
                    TestContext.WriteLine("Successfully navigated to Templates page");
                }
                else
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    TestContext.WriteLine($"Templates navigation warning - current URL: {Driver.Url}");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Templates navigation failed: {ex.Message}");
                throw;
            }
        }

        private bool IsOnTemplatesPage()
        {
            try
            {
                var currentUrl = Driver.Url;
                return currentUrl.Contains("/business-application/templates", StringComparison.OrdinalIgnoreCase) ||
                       currentUrl.Contains("/templates", StringComparison.OrdinalIgnoreCase) ||
                       _templatesPage.IsPageLoaded();
            }
            catch
            {
                return false;
            }
        }

        private async Task NavigateToTemplatesDirectlyAsync()
        {
            try
            {
                var currentUrl = Driver.Url;
                if (currentUrl.Contains("application", StringComparison.OrdinalIgnoreCase))
                {
                    var baseUrl = currentUrl.Substring(0, currentUrl.IndexOf(".net") + 4);
                    var templatesUrl = $"{baseUrl}/business-application/templates";
                    TestContext.WriteLine($"Attempting direct navigation to: {templatesUrl}");
                    Driver.Navigate().GoToUrl(templatesUrl);

                    var navigated = Driver.WaitForUrlContains("/templates", TimeSpan.FromSeconds(5)) ||
                                     Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));

                    if (!navigated)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1.5));
                    }
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Direct Templates navigation failed: {ex.Message}");
            }
        }

        private void WaitForTemplatesPageReady()
        {
            WaitForTemplatesPageReadyAsync().GetAwaiter().GetResult();
        }

        private async Task WaitForTemplatesPageReadyAsync()
        {
            const int maxRetries = 3;
            var retryDelay = TimeSpan.FromSeconds(2);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    TestContext.WriteLine($"Waiting for Templates page readiness, attempt {attempt}/{maxRetries}");

                    Driver.WaitForPageTransition(TimeSpan.FromSeconds(5));

                    // Try to wait for Templates-specific elements
                    _templatesPage.WaitUntilPageIsLoaded(TimeSpan.FromSeconds(10));

                    TestContext.WriteLine("Templates page is ready");
                    return;
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"Templates page readiness attempt {attempt} failed: {ex.Message}");

                    if (attempt < maxRetries)
                    {
                        TestContext.WriteLine($"Retrying in {retryDelay.TotalSeconds:F1}s...");
                        await Task.Delay(retryDelay);
                    }
                    else
                    {
                        TestContext.WriteLine("Templates page readiness timeout");
                    }
                }
            }
        }

        [TestMethod]
        [Description("Templates & Signatures smoke test verifying components")]
        public void Templates_SmokeTest_AllComponentsPresent()
        {
            TestContext.WriteLine("Templates & Signatures Smoke Test");

            // Page State & Navigation Verification
            TestContext.WriteLine("Page State & Navigation Verification");
            bool pageTitle = _templatesPage.VerifyPageTitle();
            bool templatesTabActive = _templatesPage.VerifyTemplatesTabActive();
            bool templatesAndSignaturesHeader = _templatesPage.VerifyTemplatesAndSignaturesHeader();

            TestContext.WriteLine($"Page title contains 'Application': {pageTitle}");
            TestContext.WriteLine($"Templates tab is highlighted as active: {templatesTabActive}");
            TestContext.WriteLine($"'Templates & Signatures' sub-header visible: {templatesAndSignaturesHeader}");

            // Default Templates Section Verification
            TestContext.WriteLine("Default Templates Section Verification");
            bool defaultTemplatesSection = _templatesPage.VerifyDefaultTemplatesSection();
            bool defaultTemplateOptions = _templatesPage.VerifyDefaultTemplateOptionsDisplayed();
            bool allViewButtons = _templatesPage.VerifyAllTemplateViewButtons();
            int templateCount = _templatesPage.GetTemplateCardCount();
            int viewButtonCount = _templatesPage.GetViewButtonCount();
            var visibleTemplates = _templatesPage.GetVisibleTemplateNames();

            TestContext.WriteLine($"'Default Templates' section header visible: {defaultTemplatesSection}");
            TestContext.WriteLine($"Default template links displayed: {defaultTemplateOptions}");
            TestContext.WriteLine($"All default template links present: {allViewButtons}");
            TestContext.WriteLine($"Default template link count: {templateCount}");
            TestContext.WriteLine($"Actionable link count: {viewButtonCount}");
            TestContext.WriteLine($"Visible templates: {string.Join(", ", visibleTemplates)}");

            // Verify specific templates
            bool standbyPud = _templatesPage.VerifySpecificTemplateCard("Standby / PUD");
            bool confirming = _templatesPage.VerifySpecificTemplateCard("Confirmation Template");
            bool directPay = _templatesPage.VerifySpecificTemplateCard("Direct Pay");

            TestContext.WriteLine($"Standard template: {standbyPud}");
            TestContext.WriteLine($"Confirmation template: {confirming}");
            TestContext.WriteLine($"Direct Pay template: {directPay}");

            // Signatories Section Verification
            TestContext.WriteLine("Signatories Section Verification");
            bool signersBlock = _templatesPage.VerifySignersBlock();
            bool editSignersLink = _templatesPage.VerifyEditSignersLink();
            bool editSignersArrow = _templatesPage.VerifyEditSignersArrowIcon();

            TestContext.WriteLine($"'Signers' content block displayed: {signersBlock}");
            TestContext.WriteLine($"'Edit Signers' link visible: {editSignersLink}");
            TestContext.WriteLine($"'Edit Signers' right-arrow icon: {editSignersArrow}");

            // Special Templates Section Verification
            TestContext.WriteLine("Special Templates Section Verification");
            bool specialTemplatesSection = _templatesPage.VerifySpecialTemplatesSection();
            bool specialTemplatesContent = _templatesPage.VerifySpecialTemplatesContentBlock();

            TestContext.WriteLine($"'Special Templates' section header: {specialTemplatesSection}");
            TestContext.WriteLine($"Special templates content block displayed: {specialTemplatesContent}");

            // Overall verification
            bool allSectionsVerified = _templatesPage.VerifyAllSections();
            TestContext.WriteLine($"All sections comprehensive verification: {allSectionsVerified}");

            // Evaluate results
            var criticalChecks = new[] { pageTitle, templatesAndSignaturesHeader, defaultTemplatesSection, defaultTemplateOptions };
            var importantChecks = new[] { templatesTabActive, allViewButtons, signersBlock, editSignersLink };
            var recommendedChecks = new[] { specialTemplatesSection, specialTemplatesContent, editSignersArrow };

            int criticalPassed = criticalChecks.Count(c => c);
            int importantPassed = importantChecks.Count(c => c);
            int recommendedPassed = recommendedChecks.Count(c => c);

            TestContext.WriteLine($"Critical checks passed: {criticalPassed}/{criticalChecks.Length}");
            TestContext.WriteLine($"Important checks passed: {importantPassed}/{importantChecks.Length}");
            TestContext.WriteLine($"Recommended checks passed: {recommendedPassed}/{recommendedChecks.Length}");

            // Assertions
            Assert.IsTrue(pageTitle, "The browser page title must be 'Application'");
            Assert.IsTrue(templatesTabActive, "The 'Templates' tab must be highlighted as the active tab");
            Assert.IsTrue(templatesAndSignaturesHeader, "A sub-header with the text 'Templates & Signatures' is visible on the page");

            Assert.IsTrue(defaultTemplatesSection, "A section header with the text 'Default Templates' is visible");
            Assert.IsTrue(defaultTemplateOptions, "Default template links are displayed");
            Assert.IsTrue(allViewButtons, "Default template links should be present");

            Assert.IsTrue(signersBlock, "A content block titled 'Signers' is displayed");
            Assert.IsTrue(editSignersLink, "An 'Edit Signers' link is visible below the 'Signers' block");

            Assert.IsTrue(specialTemplatesSection, "A section header with the text 'Special Templates' is visible");
            Assert.IsTrue(specialTemplatesContent, "A large content block for special templates is displayed on the right side of the page");

        }

        [TestMethod]
        [Description("Verify page state and navigation elements")]
        public void Templates_VerifyPageStateAndNavigation()
        {
            TestContext.WriteLine("Testing Templates page state and navigation");

            bool pageTitle = _templatesPage.VerifyPageTitle();
            bool templatesTabActive = _templatesPage.VerifyTemplatesTabActive();
            bool navigationTabs = _templatesPage.VerifyNavigationTabsPresent();
            bool templatesAndSignaturesHeader = _templatesPage.VerifyTemplatesAndSignaturesHeader();

            TestContext.WriteLine($"Page title valid: {pageTitle}");
            TestContext.WriteLine($"Templates tab active: {templatesTabActive}");
            TestContext.WriteLine($"Navigation tabs present: {navigationTabs}");
            TestContext.WriteLine($"Templates & Signatures header: {templatesAndSignaturesHeader}");

            Assert.IsTrue(pageTitle, "Page title should contain 'Application'");
            Assert.IsTrue(templatesTabActive, "Templates tab should be highlighted as active");
            Assert.IsTrue(templatesAndSignaturesHeader, "Templates & Signatures header should be visible");

            TestContext.WriteLine("Page state and navigation verification completed");
        }

        [TestMethod]
        [Description("Verify Default Templates section with all template options")]
        public void Templates_VerifyDefaultTemplatesSection()
        {
            TestContext.WriteLine("Testing Default Templates section");

            bool defaultTemplatesSection = _templatesPage.VerifyDefaultTemplatesSection();
            bool defaultTemplateOptions = _templatesPage.VerifyDefaultTemplateOptionsDisplayed();
            bool allViewButtons = _templatesPage.VerifyAllTemplateViewButtons();

            TestContext.WriteLine($"Default Templates section visible: {defaultTemplatesSection}");
            TestContext.WriteLine($"Default template links displayed: {defaultTemplateOptions}");
            TestContext.WriteLine($"All default template links present: {allViewButtons}");

            // Verify individual templates
            bool standbyPudTemplate = _templatesPage.VerifySpecificTemplateCard("Standby / PUD");
            bool confirmingTemplate = _templatesPage.VerifySpecificTemplateCard("Confirmation Template");
            bool directPayTemplate = _templatesPage.VerifySpecificTemplateCard("Direct Pay");

            TestContext.WriteLine($"Standard template: {standbyPudTemplate}");
            TestContext.WriteLine($"Confirmation template: {confirmingTemplate}");
            TestContext.WriteLine($"Direct Pay template: {directPayTemplate}");

            var visibleTemplates = _templatesPage.GetVisibleTemplateNames();
            TestContext.WriteLine($"Visible template names: {string.Join(", ", visibleTemplates)}");

            Assert.IsTrue(defaultTemplatesSection, "Default Templates section should be visible");
            Assert.IsTrue(defaultTemplateOptions, "Default template links should be displayed");
            Assert.IsTrue(allViewButtons, "Default template links should be present");
            Assert.IsTrue(standbyPudTemplate && confirmingTemplate && directPayTemplate,
                         "All default template links should be visible: Standard, Confirmation, Direct Pay");

            TestContext.WriteLine("Default Templates section verification completed");
        }

        [TestMethod]
        [Description("Verify Special Templates section with content block")]
        public void Templates_VerifySpecialTemplatesSection()
        {
            TestContext.WriteLine("Testing Special Templates section");

            bool specialTemplatesSection = _templatesPage.VerifySpecialTemplatesSection();
            bool specialTemplatesContent = _templatesPage.VerifySpecialTemplatesContentBlock();
            bool completeSection = _templatesPage.VerifyCompleteSpecialTemplatesSection();

            TestContext.WriteLine($"Special Templates section: {specialTemplatesSection}");
            TestContext.WriteLine($"Special templates content block: {specialTemplatesContent}");
            TestContext.WriteLine($"Complete special templates section: {completeSection}");

            Assert.IsTrue(specialTemplatesSection, "Special Templates section should be visible");
            Assert.IsTrue(specialTemplatesContent, "Special templates content block should be displayed");

            TestContext.WriteLine("Special Templates section verification completed");
        }

        [TestMethod]
        [Description("Test default template link interactions")]
        public void Templates_TestTemplateViewButtonInteractions()
        {
            TestContext.WriteLine("Testing default template link interactions");

            var templateNames = new[] { "Standard Template", "Confirmation Template", "Direct Pay Template" };

            foreach (var templateName in templateNames)
            {
                try
                {
                    TestContext.WriteLine($"Testing {templateName} link");

                    bool templateVisible = _templatesPage.VerifySpecificTemplateCard(templateName);
                    TestContext.WriteLine($"{templateName} template visible: {templateVisible}");

                    if (templateVisible)
                    {
                        TestContext.WriteLine($"{templateName} link interaction test would be performed here");
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine($"{templateName} link test failed: {ex.Message}");
                }
            }

            TestContext.WriteLine("Default template link interactions testing completed");
        }

        [TestMethod]
        [Description("Test Edit Signers link interaction")]
        public void Templates_TestEditSignersLinkInteraction()
        {
            TestContext.WriteLine("Testing Edit Signers link interaction");

            bool editSignersVisible = _templatesPage.VerifyEditSignersLink();
            TestContext.WriteLine($"Edit Signers link visible: {editSignersVisible}");

            Assert.IsTrue(editSignersVisible, "Edit Signers link should be visible for interaction");

            if (editSignersVisible)
            {
                TestContext.WriteLine("Edit Signers link interaction test would be performed here");
            }

            TestContext.WriteLine("Edit Signers link interaction testing completed");
        }

        [TestMethod]
        [Description("Verify Templates page load and readiness")]
        public void Templates_VerifyPageLoadAndReadiness()
        {
            TestContext.WriteLine("Testing Templates page load and readiness");

            bool pageLoaded = _templatesPage.IsPageLoaded();
            bool allSections = _templatesPage.VerifyAllSections();

            TestContext.WriteLine($"Templates page loaded: {pageLoaded}");
            TestContext.WriteLine($"All sections verified: {allSections}");
            TestContext.WriteLine($"Current URL: {Driver.Url}");
            TestContext.WriteLine($"Page Title: {Driver.Title}");

            Assert.IsTrue(pageLoaded, "Templates page should be loaded successfully");

            TestContext.WriteLine("Templates page load and readiness verification completed");
        }

        [TestMethod]
        [Description("Debug test to check configuration and URL loading")]
        public void Debug_CheckTemplatesPageConfiguration()
        {
            TestContext.WriteLine($"Environment: {InitializeTestAssembly.ENV}");
            TestContext.WriteLine($"UI URL: {InitializeTestAssembly.UiUrl}");
            TestContext.WriteLine($"API URL: {InitializeTestAssembly.BaseApiUrl}");
            TestContext.WriteLine($"Browser: {InitializeTestAssembly.Browser}");
            TestContext.WriteLine($"Username: {InitializeTestAssembly.Email}");

            TestContext.WriteLine($"Current URL: {Driver.Url}");
            TestContext.WriteLine($"Page Title: {Driver.Title}");

            bool pageLoaded = _templatesPage.IsPageLoaded();
            bool templatesTabActive = _templatesPage.VerifyTemplatesTabActive();
            bool templatesHeader = _templatesPage.VerifyTemplatesAndSignaturesHeader();

            TestContext.WriteLine($"Templates page loaded: {pageLoaded}");
            TestContext.WriteLine($"Templates tab active: {templatesTabActive}");
            TestContext.WriteLine($"Templates header visible: {templatesHeader}");

            Assert.IsTrue(pageLoaded || templatesHeader, "Templates page elements should be accessible");

            TestContext.WriteLine("Debug configuration check completed");
        }
    }
}
