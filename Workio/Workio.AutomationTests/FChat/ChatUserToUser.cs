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
    public class ChatUserToUser
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
        public void TheChatUserToUserTest()
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
            driver.FindElement(By.Id("global-search-bar")).Click();
            driver.FindElement(By.Id("global-search-bar")).SendKeys("Teste Automacao User 3");
            driver.FindElement(By.Id("global-search-bar")).SendKeys(Keys.Enter);
            driver.FindElement(By.XPath("//div[@id='user-tab-content']/ul/a/div/div/div/p")).Click();
            driver.FindElement(By.XPath("(.//*[normalize-space(text()) and normalize-space(.)='Teste Automacao User 3'])[1]/following::span[2]")).Click();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var toastTitleElement = By.ClassName("toast-title");
            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(toastTitleElement));

            var profilePictureElement = By.XPath("//img[@alt='Profile Picture']");
            wait.Until(ExpectedConditions.ElementToBeClickable(profilePictureElement)).Click();

            driver.FindElement(By.Id("user-logout-option")).Click();
            driver.FindElement(By.LinkText("Login")).Click();
            driver.FindElement(By.Id("Input_Email")).Click();
            driver.FindElement(By.Id("Input_Email")).Clear();
            driver.FindElement(By.Id("Input_Email")).SendKeys("testeautomacao3@mail.com");
            driver.FindElement(By.Id("Input_Password")).Click();
            driver.FindElement(By.Id("Input_Password")).Clear();
            driver.FindElement(By.Id("Input_Password")).SendKeys("!Teste123");
            driver.FindElement(By.Id("account")).Submit();
            driver.FindElement(By.XPath("//img[@alt='Profile Picture']")).Click();
            driver.FindElement(By.LinkText("Connections")).Click();
            driver.FindElement(By.Id("pending-tab")).Click();
            var wait3 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var acceptButton = By.LinkText("ACCEPT");
            wait3.Until(ExpectedConditions.ElementToBeClickable(acceptButton)).Click();
            driver.FindElement(By.Id("global-search-bar")).Click();
            driver.FindElement(By.Id("global-search-bar")).SendKeys("Teste Automacao User 2");
            driver.FindElement(By.Id("global-search-bar")).SendKeys(Keys.Enter);
            driver.FindElement(By.XPath("//div[@id='user-tab-content']/ul/a/div/div/div/p")).Click();
            driver.FindElement(By.XPath("(.//*[normalize-space(text()) and normalize-space(.)='Remove Connection'])[1]/following::span[1]")).Click();
            var wait4 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var message_to_send = By.Id("message_to_send");
            wait4.Until(ExpectedConditions.ElementToBeClickable(message_to_send)).Click();
            driver.FindElement(By.Id("message_to_send")).Clear();
            driver.FindElement(By.Id("message_to_send")).SendKeys("Hey, just checking in if Katalon can chat with you!");
            driver.FindElement(By.Id("message_to_send")).SendKeys(Keys.Enter);
            Thread.Sleep(5000);
            driver.FindElement(By.XPath("//img[@alt='Profile Picture']")).Click();
            driver.FindElement(By.Id("user-logout-option")).Click();
            driver.FindElement(By.LinkText("Login")).Click();
            driver.FindElement(By.Id("Input_Email")).Click();
            driver.FindElement(By.Id("Input_Email")).Clear();
            driver.FindElement(By.Id("Input_Email")).SendKeys("testeautomacao3@mail.com");
            driver.FindElement(By.Id("Input_Password")).Click();
            driver.FindElement(By.Id("Input_Password")).Clear();
            driver.FindElement(By.Id("Input_Password")).SendKeys("!Teste123");
            driver.FindElement(By.Id("account")).Submit();
            driver.FindElement(By.Id("global-search-bar")).SendKeys("Teste Automacao User 2");
            driver.FindElement(By.Id("global-search-bar")).SendKeys(Keys.Enter);
            driver.FindElement(By.XPath("//div[@id='user-tab-content']/ul/a/div/div/div/p")).Click();
            driver.FindElement(By.XPath("(.//*[normalize-space(text()) and normalize-space(.)='Remove Connection'])[1]/following::span[1]")).Click();
            var wait5 = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var message_to_send2 = By.Id("message_to_send");
            wait5.Until(ExpectedConditions.ElementToBeClickable(message_to_send2)).Click();
            driver.FindElement(By.Id("message_to_send")).Click();
            driver.FindElement(By.Id("message_to_send")).Clear();
            driver.FindElement(By.Id("message_to_send")).SendKeys("Hey I did receive your message!");
            driver.FindElement(By.Id("message_to_send")).SendKeys(Keys.Enter);
            Thread.Sleep(5000); //sleep so it displays the message instead of closing the test immediately
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
