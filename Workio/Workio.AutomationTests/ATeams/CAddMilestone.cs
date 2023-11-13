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
    public class CAddMilestone
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
        public void TheAddMilestoneTest()
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
            driver.FindElement(By.LinkText("Teams")).Click();
            driver.FindElement(By.Id("team-input-search")).SendKeys("Edited Team Katalon Test");
            driver.FindElement(By.Id("team-input-search")).SendKeys(Keys.Enter);
            driver.FindElement(By.Id("myteams-tab")).Click();
            driver.FindElement(By.XPath("//div[@id='myteams-tab-content']/ul/a/div/div/div/div/h2")).Click();

            ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0,500);");

            var addMilestoneButton = driver.FindElement(By.Id("btn_AddMilestone"));
            addMilestoneButton.Click();

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var modalElement = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("milestoneModal")));

            // interact with the modal element here
            driver.FindElement(By.Id("Name")).Click();
            driver.FindElement(By.Id("Name")).Clear();
            driver.FindElement(By.Id("Name")).SendKeys("Teste de Automação");
            driver.FindElement(By.Id("Description")).Click();
            driver.FindElement(By.Id("Description")).Clear();
            driver.FindElement(By.Id("Description")).SendKeys("Automation Purposes");
            driver.FindElement(By.Id("submit-milestone")).Click();
            driver.FindElement(By.Id("StartDate")).Click();
            driver.FindElement(By.Id("StartDate")).Clear();
            driver.FindElement(By.Id("StartDate")).SendKeys(DateTime.Now.AddDays(1).ToString());
            driver.FindElement(By.Id("StartDate")).Click();
            driver.FindElement(By.Id("submit-milestone")).Click();
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
