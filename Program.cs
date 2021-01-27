using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Challenge
{
    class Program
    {
        private static IWebDriver driver;
        private static readonly string creds = "2222";
        private static readonly string challengeUrl = "https://challenge.flinks.io";
        private static void Main(string[] args)
        {
            driver = new FirefoxDriver(@"C:\Program Files (x86)\Google\Chrome\Application");
            driver.Url = challengeUrl;

            // click on Start
            var startChallenge = driver.FindElement(By.LinkText("START"));
            startChallenge.Click();
            int counter = 0;

            for (int i = 1; i<6; i++)
            {
                string cachedUrl = driver.Url;
                for(int k=0; k<9; k++)
                {
                    // Log in
                    var userName = driver.FindElement(By.Name("username"));
                    userName.SendKeys(creds);

                    var password = driver.FindElement(By.Name("password"));
                    password.SendKeys(creds);

                    var signInButton = driver.FindElement(By.XPath("//button[contains(text(), 'Sign In')]"));
                    signInButton.Click();

                    var cookies = driver.Manage().Cookies.AllCookies;

                    var random = new Random();
                    System.Threading.Thread.Sleep(random.Next(500, 6000));

                    // Send this to the server -- it looks like some kind of 
                    // a detector to react to mouse movements
                    // Calling it should fake user activity
                    SendMmToSever(cookies, driver.Url);

                    // Display count
                    counter++;
                    Console.WriteLine("{0} Attempt {1} : {2}", driver.Url, counter, DateTime.Now);

                    // Display token
                    var returnedText = driver.FindElement(By.XPath("//b[contains(text(), '==')]"));
                    Console.WriteLine(returnedText.Text);

                    // Go back to restart the process
                    driver.Navigate().GoToUrl(driver.Url);
                }

                // This is an attempt to wait out the detector, but it does not work
                Console.WriteLine("Waiting {0} minutes...", i*10);
                System.Threading.Thread.Sleep(i*60000);
                Console.WriteLine("Resume");
            }
        }

        private static void SendMmToSever(ReadOnlyCollection<OpenQA.Selenium.Cookie> cookies, string referrer)
        {
            Task.Factory.StartNew(() =>
            {
                var handler = new HttpClientHandler();
                var cookieContainer = new CookieContainer();
                foreach (var cookie in cookies)
                {
                    var netCookie = new System.Net.Cookie(cookie.Name, cookie.Value);
                    netCookie.Domain = cookie.Domain;          
                    cookieContainer.Add(netCookie);
                }

                handler.CookieContainer = cookieContainer;

                HttpClient client = new HttpClient(handler);
                client.BaseAddress = new Uri("https://challenge.flinks.io");
                client.DefaultRequestHeaders.Referrer = new Uri(referrer);

                client.GetAsync("/mm");
            });
        }
    }
}
