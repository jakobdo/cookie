using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Configuration;

namespace cookie
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                if (appSettings.Count == 0)
                {
                    Console.WriteLine("App.config -> AppSettings er tom.");
                    Console.WriteLine("Tryk en vilkårlig tast for at lukke programmet");
                    Console.ReadKey();
                    return;
                }

                string sitemap = appSettings["sitemap"];
                string basedir = AppDomain.CurrentDomain.BaseDirectory;
                string output = string.Format(@"{0}\{1:yyyy-MM-dd}.csv", basedir, DateTime.Now);

                // Is chromedriver installed in correct path?
                if (!File.Exists(string.Format(@"{0}/chromedriver.exe", basedir)))
                {
                    Console.WriteLine(string.Format("Installer chromedriver.exe i denne mappe:\n{0}\nFilen kan findes her: H:\\Digitalisering\\Udvikling\\Cookie eller hentes herfra: http://chromedriver.storage.googleapis.com/2.14/chromedriver_win32.zip", basedir));
                    Console.WriteLine("Tryk en vilkårlig tast for at lukke programmet");
                    Console.ReadKey();
                    return;
                }

                XDocument doc = XDocument.Load(sitemap);
                Console.WriteLine("Sitemap loaded");
                XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
                List<string> urlList = doc.Root.Elements(ns + "url").Elements(ns + "loc").Select(x => (string)x).ToList();
                IWebDriver driver = new ChromeDriver(basedir);
                Console.WriteLine("Chromedriver loaded");
                int total = urlList.Count;
                int counter = 0;

                System.IO.StreamWriter file = new System.IO.StreamWriter(output);

                foreach (var item in urlList)
                {
                    driver.Navigate().GoToUrl(item.ToString());
                    foreach (OpenQA.Selenium.Cookie cookie in driver.Manage().Cookies.AllCookies)
                    {
                        file.WriteLine(string.Format("\"{0}\";\"{1}\";\"{2}\";\"{3}\"", item, cookie.Domain, cookie.Name, cookie.Value));
                    }
                    drawTextProgressBar(counter++, total);
                }
                file.Close();
                driver.Quit();
            }
            catch (ConfigurationErrorsException ex)
            {
                Console.WriteLine(string.Format("Fejl under læsning af app settings: {0}", ex.Message));
                Console.WriteLine("Tryk en vilkårlig tast for at lukke programmet");
                Console.ReadKey();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Ukendt fejl: {0}", ex.Message));
                Console.WriteLine("Tryk en vilkårlig tast for at lukke programmet");
                Console.ReadKey();
                return;
            }
        }

        private static void drawTextProgressBar(int progress, int total)
        {
            //draw empty progress bar
            Console.CursorLeft = 0;
            Console.Write("["); //start
            Console.CursorLeft = 32;
            Console.Write("]"); //end
            Console.CursorLeft = 1;
            float onechunk = 30.0f / total;

            //draw filled part
            int position = 1;
            for (int i = 0; i < onechunk * progress; i++)
            {
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw unfilled part
            for (int i = position; i <= 31; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            //draw totals
            Console.CursorLeft = 35;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(progress.ToString() + " of " + total.ToString() + "    "); //blanks at the end remove any excess
        }
    }
}
