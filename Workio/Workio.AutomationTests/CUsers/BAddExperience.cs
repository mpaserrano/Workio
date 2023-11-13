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
    public class BAddExperience
    {
        private static IWebDriver driver;
        private StringBuilder verificationErrors;
        private static string baseURL;
        private bool acceptNextAlert = true;
        
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
        public void TheBAddExperienceTest()
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
            driver.FindElement(By.CssSelector("div.col-9 > div.card > div.card-body > div.row > div.col-2 > div.justify-content-end > a > svg.svg-inline--fa.fa-plus.fa-xl > path")).Click();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var modalElement = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("experienceForm")));

            driver.FindElement(By.Id("WorkTitle")).Clear();
            driver.FindElement(By.Id("WorkTitle")).SendKeys("Katalon Recorder");
            driver.FindElement(By.Id("Company")).Click();
            driver.FindElement(By.Id("Company")).Clear();
            driver.FindElement(By.Id("Company")).SendKeys("IPS");
            driver.FindElement(By.Id("Description")).Click();
            driver.FindElement(By.Id("Description")).Clear();
            driver.FindElement(By.Id("Description")).SendKeys("For Workio Project");
            driver.FindElement(By.Id("StartDate")).Click();
            driver.FindElement(By.Id("StartDate")).Click();
            driver.FindElement(By.Id("StartDate")).Clear();
            driver.FindElement(By.Id("StartDate")).SendKeys("2023-04-04");
            driver.FindElement(By.Id("EndDate")).Click();
            driver.FindElement(By.Id("EndDate")).Clear();
            driver.FindElement(By.Id("EndDate")).SendKeys("2023-04-04");
            driver.FindElement(By.Id("submit-experience")).Click();
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
