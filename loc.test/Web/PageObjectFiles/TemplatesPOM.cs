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
    public class TemplatesPOM : BasePage
    {
        public TemplatesPOM(IWebDriver driver) : base(driver)
        {
        }

        // Primary page identifier Templates tab active state or the Templates & Signatures header
        protected override By UniqueLocator => By.XPath("//h2[contains(text(), 'Templates & Signatures')] | //a[contains(@class, 'summit-page-nav--active-link') and contains(text(), 'Templates')]");

        #region Page & Navigation Elements

        // Page title and main header
        private readonly By pageTitle = By.XPath("//h1[contains(text(), 'Business Application')] | //*[@class='summit-page-container-title-text' and contains(text(), 'Business Application')]");
        private readonly By templatesAndSignaturesHeader = By.XPath("//h2[contains(text(), 'Templates & Signatures')]");

        // Navigation tabs
        private new readonly By navigationTabs = By.XPath("//div[contains(@class, 'summit-page-nav')]//a[contains(@class, 'summit-page-nav--link')]");
        private new readonly By templatesTab = By.XPath("//a[contains(@class, 'summit-page-nav--link') and contains(text(), 'Templates')]");
        private readonly By templatesTabActive = By.XPath("//a[contains(@class, 'summit-page-nav--active-link') and contains(text(), 'Templates')]");

        #endregion

        #region Default Templates Section Elements

        // Default Templates section (updated to match summit card title)
        private readonly By defaultTemplatesSection = By.XPath("//h3[contains(text(), 'Default Templates')] | //div[contains(@class,'summit-card--title')][contains(., 'Default Templates')] | //div[contains(@class, 'summit-section-header-text-title') and contains(text(), 'Default Templates')]");
        private readonly By defaultTemplatesHeader = By.XPath("//div[contains(@class,'summit-card--title')][contains(., 'Default Templates')] | //h3[contains(text(), 'Default Templates')]");

        // Default Template links (replaced card/view buttons)
        private readonly By templateLinkStandbyPud = By.CssSelector("a.template-link[title='Standby / PUD'], a.template-link[href*='templates/standby-pud']");
        private readonly By templateLinkStandbyConfirming = By.CssSelector("a.template-link[title='Standby for Confirming LOCs'], a.template-link[href*='templates/standby-confirming']");
        private readonly By templateLinkDirectPay = By.CssSelector("a.template-link[title='Direct Pay'], a.template-link[href*='templates/direct-pay']");
        private readonly By allDefaultTemplateLinks = By.CssSelector(".left-container a.template-link");

        #endregion

        #region Business Document Signatories Section Elements

        // Business Document Signatories section
        private readonly By businessDocumentSignatoriesSection = By.XPath("//h3[contains(text(), 'Business Document Signatories')] | //div[contains(@class, 'summit-section-header-text-title') and contains(text(), 'Business Document Signatories')]");
        private readonly By businessDocumentSignatoriesHeader = By.XPath("//h3[contains(text(), 'Business Document Signatories')]");

        // Signers block
        private readonly By signersBlock = By.XPath("//summit-card | //div[contains(@class, 'summit-card')]");
        private readonly By signersTitle = By.XPath("//div[contains(@class, 'summit-card--title') and contains(text(), 'Signers')] | //*[contains(text(), 'Signers') and contains(@class, 'title')]");

        // Edit Signers link with arrow icon
        private readonly By editSignersLink = By.XPath("//button[contains(text(), 'Edit Signers')] | //a[contains(text(), 'Edit Signers')] | //*[contains(@class, 'summit-card--button')]");
        private readonly By editSignersArrowIcon = By.XPath("//button[contains(text(), 'Edit Signers')]//summit-icon | //button[contains(text(), 'Edit Signers')]//*[contains(@class, 'summit-card-button-icon')] | //*[contains(@src, 'arrow-right.svg')]");

        #endregion

        #region Special Letter Templates Section Elements

        // Special Letter Templates section
        private readonly By specialLetterTemplatesSection = By.XPath("//h3[contains(text(), 'Special Letter Templates')] | //div[contains(@class, 'summit-section-header-text-title') and contains(text(), 'Special Letter Templates')]");
        private readonly By specialLetterTemplatesHeader = By.XPath("//h3[contains(text(), 'Special Letter Templates')]");

        // Special templates content block
        private readonly By specialLetterTemplatesContentBlock = By.XPath("//summit-content-container | //div[contains(@class, 'summit-content-container')] | //div[contains(@class, 'special-letter-grid')]");
        private readonly By specialLetterTemplatesContent = By.XPath("//div[contains(@class, 'special-letter-grid')] | //div[contains(@class, 'summit-content-container--content')]");

        #endregion

        #region Core Verification Methods

        public override bool VerifyPageTitle()
        {
            try
            {
                var title = Driver.Title;
                var titleElement = IsElementVisible(pageTitle);
                return (!string.IsNullOrEmpty(title) && title.Contains("Business Application")) || titleElement;
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyTemplatesTabActive()
        {
            try
            {
                return IsElementVisible(templatesTabActive);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyTemplatesAndSignaturesHeader()
        {
            try
            {
                return IsElementVisible(templatesAndSignaturesHeader);
            }
            catch
            {
                return false;
            }
        }

        public override bool VerifyNavigationTabsPresent()
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

        #endregion

        #region Default Templates Section Verification Methods

        public bool VerifyDefaultTemplatesSection()
        {
            try
            {
                return IsElementVisible(defaultTemplatesSection) || IsElementVisible(defaultTemplatesHeader);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyDefaultTemplateOptionsDisplayed()
        {
            try
            {
                var standbyPud = IsElementVisible(templateLinkStandbyPud);
                var standbyConfirming = IsElementVisible(templateLinkStandbyConfirming);
                var directPay = IsElementVisible(templateLinkDirectPay);

                return standbyPud && standbyConfirming && directPay;
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyAllTemplateViewButtons()
        {
            try
            {
                var standbyPud = IsElementVisible(templateLinkStandbyPud);
                var standbyConfirming = IsElementVisible(templateLinkStandbyConfirming);
                var directPay = IsElementVisible(templateLinkDirectPay);
                return standbyPud && standbyConfirming && directPay;
            }
            catch
            {
                return false;
            }
        }

        public int GetTemplateCardCount()
        {
            try
            {
                var links = Driver.FindElements(allDefaultTemplateLinks);
                return links.Count;
            }
            catch
            {
                return 0;
            }
        }

        public int GetViewButtonCount()
        {
            try
            {
                var links = Driver.FindElements(allDefaultTemplateLinks);
                return links.Count;
            }
            catch
            {
                return 0;
            }
        }

        public bool VerifySpecificTemplateCard(string templateName)
        {
            try
            {
                var key = templateName.Trim().ToLowerInvariant();
                return key switch
                {
                    "standby / pud" or "standby/pud" or "standby-pud" => IsElementVisible(templateLinkStandbyPud),
                    "standby for confirming locs" or "standby-confirming" or "confirming" => IsElementVisible(templateLinkStandbyConfirming),
                    "direct pay" or "direct-pay" => IsElementVisible(templateLinkDirectPay),
                    _ => false
                };
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Business Document Signatories Section Verification Methods

        public bool VerifyBusinessDocumentSignatoriesSection()
        {
            try
            {
                return IsElementVisible(businessDocumentSignatoriesSection) || IsElementVisible(businessDocumentSignatoriesHeader);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifySignersBlock()
        {
            try
            {
                return IsElementVisible(signersBlock) && IsElementVisible(signersTitle);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyEditSignersLink()
        {
            try
            {
                return IsElementVisible(editSignersLink);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyEditSignersArrowIcon()
        {
            try
            {
                return IsElementVisible(editSignersArrowIcon);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyCompleteSignatoriesSection()
        {
            try
            {
                var section = VerifyLetterOfCreditSignatoriesSection();
                var signersBlock = VerifySignersBlock();
                var editLink = VerifyEditSignersLink();

                return section && signersBlock && editLink;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Special Letter Templates Section Verification Methods

        public bool VerifySpecialLetterTemplatesSection()
        {
            try
            {
                return IsElementVisible(specialLetterTemplatesSection) || IsElementVisible(specialLetterTemplatesHeader);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifySpecialLetterTemplatesContentBlock()
        {
            try
            {
                return IsElementVisible(specialLetterTemplatesContentBlock) || IsElementVisible(specialLetterTemplatesContent);
            }
            catch
            {
                return false;
            }
        }

        public bool VerifyCompleteSpecialTemplatesSection()
        {
            try
            {
                var section = VerifySpecialLetterTemplatesSection();
                var contentBlock = VerifySpecialLetterTemplatesContentBlock();

                return section && contentBlock;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Action Methods

        public void ClickTemplatesTab()
        {
            try
            {
                Click(templatesTab);
                Thread.Sleep(2000); // Wait for navigation
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click Templates tab: {ex.Message}");
            }
        }

        public void ClickTemplateViewButton(string templateName)
        {
            try
            {
                var key = templateName.Trim().ToLowerInvariant();
                By link = key switch
                {
                    "standby / pud" or "standby/pud" or "standby-pud" or "pud line-based" => templateLinkStandbyPud,
                    "standby for confirming locs" or "standby-confirming" or "confirming" or "pud transaction-based" => templateLinkStandbyConfirming,
                    "direct pay" or "direct-pay" => templateLinkDirectPay,
                    _ => throw new ArgumentException($"Unknown template name: {templateName}")
                };

                Click(link);
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click {templateName} View button: {ex.Message}");
            }
        }

        public void ClickEditSignersLink()
        {
            try
            {
                Click(editSignersLink);
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click Edit Signers link: {ex.Message}");
            }
        }

        #endregion

        #region Utility Methods

        public bool IsPageLoaded()
        {
            try
            {
                return VerifyTemplatesAndSignaturesHeader() &&
                       (VerifyTemplatesTabActive() || VerifyDefaultTemplatesSection());
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetVisibleTemplateNames()
        {
            try
            {
                var templateNames = new List<string>();
                if (IsElementVisible(templateLinkStandbyPud)) templateNames.Add("Standard Template");
                if (IsElementVisible(templateLinkStandbyConfirming)) templateNames.Add("Confirmation Template");
                if (IsElementVisible(templateLinkDirectPay)) templateNames.Add("Direct Pay Template");

                return templateNames;
            }
            catch
            {
                return new List<string>();
            }
        }

        public bool VerifyAllSections()
        {
            try
            {
                var pageState = VerifyPageTitle() && VerifyTemplatesTabActive() && VerifyTemplatesAndSignaturesHeader();
                var defaultTemplates = VerifyDefaultTemplatesSection() && VerifyDefaultTemplateOptionsDisplayed() && VerifyAllTemplateViewButtons();
                var signatories = VerifyCompleteSignatoriesSection();
                var specialTemplates = VerifyCompleteSpecialTemplatesSection();

                return pageState && defaultTemplates && signatories && specialTemplates;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
