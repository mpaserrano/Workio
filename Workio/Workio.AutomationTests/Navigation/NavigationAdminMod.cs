using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTests
{
    [TestClass]
    public class NavigationAdminMod
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
        public void TheNavigationAdminModTest()
        {
            driver.Navigate().GoToUrl("https://localhost:7212/");
            driver.FindElement(By.LinkText("Login")).Click();
            driver.FindElement(By.Id("Input_Email")).SendKeys("support@workio.space");
            driver.FindElement(By.Id("Input_Password")).Click();
            driver.FindElement(By.Id("Input_Password")).Clear();
            driver.FindElement(By.Id("Input_Password")).SendKeys("!Teste123");
            driver.FindElement(By.Id("account")).Submit();
            driver.FindElement(By.XPath("//img[@alt='Profile Picture']")).Click();
            driver.FindElement(By.XPath("//div[@id='navbarSupportedContent']/div/ul/li/a/strong")).Click();
            driver.FindElement(By.XPath("//img[@alt='Profile Picture']")).Click();
            driver.FindElement(By.LinkText("Connections")).Click();
            driver.FindElement(By.XPath("//img[@alt='Profile Picture']")).Click();
            driver.FindElement(By.LinkText("Settings")).Click();
            driver.FindElement(By.Id("email")).Click();
            driver.FindElement(By.Id("change-password")).Click();
            driver.FindElement(By.Id("external-login")).Click();
            driver.FindElement(By.LinkText("Admin Dashboard")).Click();
            driver.FindElement(By.Id("teams")).Click();
            driver.FindElement(By.Id("events")).Click();
            driver.FindElement(By.Id("reports")).Click();
            driver.FindElement(By.Id("logs")).Click();
            driver.FindElement(By.Id("statistics")).Click();
            driver.FindElement(By.LinkText("Events")).Click();
            driver.FindElement(By.LinkText("Create New")).Click();
            driver.FindElement(By.LinkText("Teams")).Click();
            driver.FindElement(By.LinkText("Create New")).Click();
            driver.FindElement(By.LinkText("CANCEL")).Click();
            driver.FindElement(By.LinkText("Home")).Click();
            driver.FindElement(By.Name("Name")).Clear();
            driver.FindElement(By.Name("Name")).SendKeys("teste");
            driver.FindElement(By.Name("Name")).Clear();
            driver.FindElement(By.Name("Name")).SendKeys("teste");
            driver.FindElement(By.Name("Name")).SendKeys(Keys.Enter);
            driver.FindElement(By.Id("team-tab")).Click();
            driver.FindElement(By.Id("event-tab")).Click();
            driver.FindElement(By.XPath("//img[@alt='Profile Picture']")).Click();
            driver.FindElement(By.Id("user-logout-option")).Click();
            driver.FindElement(By.LinkText("Login")).Click();
            driver.FindElement(By.Id("Input_Email")).Click();
            driver.FindElement(By.Id("Input_Email")).Clear();
            driver.FindElement(By.Id("Input_Email")).SendKeys("mod@gmail.com");
            driver.FindElement(By.Id("Input_Password")).Click();
            driver.FindElement(By.Id("Input_Password")).Clear();
            driver.FindElement(By.Id("Input_Password")).SendKeys("!Teste123");
            driver.FindElement(By.Id("account")).Submit();
            driver.FindElement(By.LinkText("Admin Dashboard")).Click();
            driver.FindElement(By.Id("teams")).Click();
            driver.FindElement(By.Id("statistics")).Click();
            driver.FindElement(By.XPath("//img[@alt='Profile Picture']")).Click();
            driver.FindElement(By.Id("user-logout-option")).Click();
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
