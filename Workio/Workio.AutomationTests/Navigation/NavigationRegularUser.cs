using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace SeleniumTests
{
    [TestClass]
    public class NavigationRegularUser
    {
        private static IWebDriver driver;
        private StringBuilder verificationErrors;
        private static string baseURL;
        private bool acceptNextAlert = true;
        private string webAppUrl;

        [ClassInitialize]
        public static void InitializeClass(TestContext testContext)
        {
            driver = new ChromeDriver();
            baseURL = "https://www.google.com/";
            driver.Manage().Window.Maximize();
        }
        
        [ClassCleanup]
        public static void CleanupClass()
        {
            try
            {
                //driver.Quit();// quit does not close the window
                driver.Close();
                driver.Dispose();
            }
            catch (Exception)
            {
                // Ignore errors if unable to close the browser
            }
        }
        
        [TestInitialize]
        public void InitializeTest()
        {
            verificationErrors = new StringBuilder();
        }
        
        [TestCleanup]
        public void CleanupTest()
        {
            Assert.AreEqual("", verificationErrors.ToString());
        }
        
        [TestMethod]
        public void TheNavigationRegularUserTest()
        {
            driver.Navigate().GoToUrl("https://localhost:7212/");
            driver.FindElement(By.LinkText("Login")).Click();
            driver.FindElement(By.Id("Input_Email")).Click();
            driver.FindElement(By.Id("Input_Email")).Clear();
            driver.FindElement(By.Id("Input_Email")).SendKeys("testeautomacao2@mail.com");
            driver.FindElement(By.Id("Input_Password")).Click();
            driver.FindElement(By.Id("Input_Password")).Clear();
            driver.FindElement(By.Id("Input_Password")).SendKeys("!Teste123");
            driver.FindElement(By.Id("account")).Submit();
            driver.FindElement(By.XPath("//img[@alt='Profile Picture']")).Click();
            driver.FindElement(By.XPath("//div[@id='navbarSupportedContent']/div/ul/li/a/strong")).Click();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var navbarDropdownMenuLink = By.Id("navbarDropdownMenuLink");
            wait.Until(ExpectedConditions.ElementToBeClickable(navbarDropdownMenuLink)).Click();
            driver.FindElement(By.Id("navbarDropdownMenuLink")).Click();
            driver.FindElement(By.LinkText("Apply to be a certified entity")).Click();
            driver.FindElement(By.CssSelector("svg.svg-inline--fa.fa-bell > path")).Click();
            driver.FindElement(By.Id("showRead")).Click();
            driver.FindElement(By.Id("showUnread")).Click();
            driver.FindElement(By.LinkText("Events")).Click();
            driver.FindElement(By.Id("joined-events-tab")).Click();
            driver.FindElement(By.Id("recommended-events-tab")).Click();
            driver.FindElement(By.Id("all-events-tab")).Click();
            driver.FindElement(By.LinkText("Teams")).Click();
            driver.FindElement(By.Id("myteams-tab")).Click();
            driver.FindElement(By.Id("invites-tab")).Click();
            driver.FindElement(By.Id("recommended-tab")).Click();
            driver.FindElement(By.Id("allteams-tab")).Click();
            driver.FindElement(By.LinkText("Privacy")).Click();
            driver.FindElement(By.LinkText("Home")).Click();
            driver.FindElement(By.Name("Name")).Click();
            driver.FindElement(By.Name("Name")).Clear();
            driver.FindElement(By.Name("Name")).SendKeys("teste");
            driver.FindElement(By.Name("Name")).Clear();
            driver.FindElement(By.Name("Name")).SendKeys("teste");
            driver.FindElement(By.Name("Name")).SendKeys(Keys.Enter);
            driver.FindElement(By.XPath("//div[@id='user-tab-content']/ul/a/div/div/div/p")).Click();
            driver.FindElement(By.Name("Name")).Click();
            driver.FindElement(By.Name("Name")).Clear();
            driver.FindElement(By.Name("Name")).SendKeys("teste");
            driver.FindElement(By.Name("Name")).Clear();
            driver.FindElement(By.Name("Name")).SendKeys("Edited Team Katalon Test");
            driver.FindElement(By.Name("Name")).SendKeys(Keys.Enter);
            driver.FindElement(By.Id("team-tab")).Click();
            driver.FindElement(By.Id("status-open")).Click();
            driver.FindElement(By.Id("status-open")).Click();
            driver.FindElement(By.Id("status-closed")).Click();
            driver.FindElement(By.Id("status-closed")).Click();
            driver.FindElement(By.Id("event-tab")).Click();
        }
        private bool IsElementPresent(By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        
        private bool IsAlertPresent()
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }
        
        private string CloseAlertAndGetItsText() {
            try {
                IAlert alert = driver.SwitchTo().Alert();
                string alertText = alert.Text;
                if (acceptNextAlert) {
                    alert.Accept();
                } else {
                    alert.Dismiss();
                }
                return alertText;
            } finally {
                acceptNextAlert = true;
            }
        }
    }
}
