using System;
using System.IO;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.ObjectModel;


namespace WebScraping
{
    class Program
    {
        static void Main()
        {
            Program program = new Program();
            program.Start();
        }

        public void Start()
        {
            var csv_path = @"C:\Users\gysem\source\repos\WebScraper\csv_files";

            Console.WriteLine("----- Options to scrape -----");
            Console.WriteLine("1 - Youtube");
            Console.WriteLine("2 - Indeed");
            Console.WriteLine("3 - Bol");
            Console.WriteLine("Your choice: ");

            var choice = Console.ReadLine();

            if (choice == "1")
            {
                YoutubeScraper(csv_path);
            }
            else if (choice == "2")
            {
                IndeedScraper(csv_path);
            }
            else if (choice == "3")
            {
                BolScraper(csv_path);
            }
            else
            {
                Program p = new Program();
                p.Start();
            }

        }

        public void YoutubeScraper(string csv_path)
        {
            // start chrome and go to youtube
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.youtube.com/");
            driver.Manage().Window.Maximize();
            var timeout = 10000; /* Maximum wait time of 10 seconds */
            var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));

            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            var agreeButton = driver.FindElement(By.XPath("//*[@id=\"content\"]/div[2]/div[5]/div[2]/ytd-button-renderer[2]/a"));
            agreeButton.Click();

            Console.WriteLine("Enter searchterm: ");
            var input = Console.ReadLine();

            // find the search bar and enter user input
            var search = driver.FindElement(By.Name("search_query"));
            search.SendKeys(input);
            search.SendKeys(Keys.Return);

            Thread.Sleep(1000);
            // activate the right filter (sort on upload date)
            var filterButton = driver.FindElement(By.XPath("//*[@id=\"filter-menu\"]/div/ytd-toggle-button-renderer/a"));
            filterButton.Click();
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            var sortButton = driver.FindElement(By.XPath("/html/body/ytd-app/div/ytd-page-manager/ytd-search/div[1]/ytd-two-column-search-results-renderer/div/ytd-section-list-renderer/div[1]/div[2]/ytd-search-sub-menu-renderer/div[1]/iron-collapse/div/ytd-search-filter-group-renderer[5]/ytd-search-filter-renderer[2]/a"));
            var link = sortButton.GetAttribute("href");
            Console.WriteLine(link);
            driver.Navigate().GoToUrl(link);

            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            Thread.Sleep(5000);
            /*var searchbar = driver.FindElement(By.Name("q"));
                        searchbar.SendKeys("webshop"); 
                        searchbar.Submit();*/
            By elem_video_link = By.CssSelector("ytd-video-renderer.style-scope.ytd-item-section-renderer");

            ReadOnlyCollection<IWebElement> videos = driver.FindElements(elem_video_link);
            Console.WriteLine("These are the 5 most recent videos: ");
            var vcount = 1;
            var item_yt = "VideoNO; Title; Uploader; Views; Release date; Link";
            foreach (IWebElement video in videos)
            {

                string str_title, str_views, str_rel, str_uploader, str_link;
                IWebElement elem_video_title = video.FindElement(By.CssSelector("#video-title"));
                str_title = elem_video_title.Text;

                IWebElement elem_video_views = video.FindElement(By.XPath(".//*[@id='metadata-line']/span[1]"));
                str_views = elem_video_views.Text;

                IWebElement elem_video_reldate = video.FindElement(By.XPath(".//*[@id='metadata-line']/span[2]"));
                str_rel = elem_video_reldate.Text;

                IWebElement elem_video_uploader = video.FindElement(By.XPath(".//div[1]/div[2]/ytd-channel-name/div/div/yt-formatted-string/a"));
                str_uploader = elem_video_uploader.Text;

                str_link = elem_video_title.GetAttribute("href");

                item_yt += "\n" + vcount + ";" + str_title + ";" + str_uploader + ";" + str_views
                    + ";" + str_rel + ";" + str_link;
                Console.WriteLine("\n");
                Console.WriteLine("******* Video " + vcount + " *******");
                Console.WriteLine("Video Title: " + str_title);
                Console.WriteLine("Uploader: " + str_uploader);
                Console.WriteLine("Video Views: " + str_views);
                Console.WriteLine("Video Release Date: " + str_rel);
                Console.WriteLine("Video Link: " + str_link);
                vcount++;
                if (vcount > 3) { break; }
            }

            Csv(item_yt, csv_path);
            Quit(driver);

        }


        public void IndeedScraper(string csv_path)
        {
            // start chrome and go to indeed
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://be.indeed.com/");
            driver.Manage().Window.Maximize();
            var timeout = 10000; /* Maximum wait time of 10 seconds */
            var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            Console.WriteLine("Enter searchterm: ");
            var input = Console.ReadLine();

            // find the search bar and search what the user inputs
            var search = driver.FindElement(By.Name("q"));
            search.SendKeys(input);
            search.SendKeys(Keys.Return);
            driver.Manage().Window.Maximize();
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            var sortByDate = driver.FindElement(By.XPath("//*[@id=\"resultsCol\"]/div[3]/div[4]/div[1]/span[2]/a"));
            sortByDate.Click();
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            // een FindElement om de dialog uit te lokken
            var uitlokken = driver.FindElement(By.XPath("//*[@id='mosaic-provider-jobcards']/a/div[1]/div/div/div/table[1]/tbody/tr/td/div[1]/h2/span"));
            Thread.Sleep(1000);
            driver.FindElement(By.CssSelector("button.popover-x-button-close")).Click();
            Thread.Sleep(750);
            string[] age_to_scrape = new string[] { "Just posted", "Today", "1 day ago", "2 days ago", "3 days ago" };


            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));


            By elem_job_link = By.XPath("//*[@id='mosaic-provider-jobcards']/a");
            var item_indeed = "Title; Location; Company; Age of post; Link";
            ReadOnlyCollection<IWebElement> jobs = driver.FindElements(elem_job_link);
            foreach (IWebElement job in jobs)
            {
                string str_title = "", str_company = "", str_location = "", str_link = "";
                string[] rows = job.FindElement(By.XPath(".//div[1]/div/div/div/table[2]/tbody/tr[2]/td/div/span")).Text.Split('\n');
                if (rows.Length == 2)
                {
                    if (age_to_scrape.Contains(rows[1]))
                    {
                        IWebElement elem_job_title = job.FindElement(By.XPath(".//div[1]/div/div/div/table[1]/tbody/tr/td/div[1]/h2/span"));
                        str_title = elem_job_title.Text;

                        IWebElement elem_job_location = job.FindElement(By.XPath(".//div[1]/div/div/div/table[1]/tbody/tr/td/div[2]/pre/div"));
                        str_location = elem_job_location.Text;

                        IWebElement elem_job_company = job.FindElement(By.XPath(".//div[1]/div/div/div/table[1]/tbody/tr/td/div[2]/pre/span"));
                        str_company = elem_job_company.Text;

                        IWebElement elem_job_url = job.FindElement(By.XPath("."));
                        str_link = elem_job_url.GetAttribute("href");

                        item_indeed += "\n" + str_title + ";" + str_location + ";" + str_company + ";" + rows[1]
                        + ";" + str_link;
                        Console.WriteLine("\n\n");
                        Console.WriteLine("Job Title: " + str_title);
                        Console.WriteLine("Location: " + str_location);
                        Console.WriteLine("This job was posted by " + str_company + " '" + rows[1] + "'");
                        Console.WriteLine("----------");
                        Console.WriteLine("Job Link: " + str_link);
                    }
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollTo(0, 500);");
                }
            }
            Csv(item_indeed, csv_path);
            Quit(driver);
        }


        public void BolScraper(string csv_path)
        {
            // start chrome and go to Zalando
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("https://www.bol.com/be/nl/");
            driver.Manage().Window.Maximize();
            var timeout = 10000; /* Maximum wait time of 10 seconds */
            var wait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeout));
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            var acceptButton = driver.FindElement(By.XPath("//*[@id=\"modalWindow\"]/div[2]/div[2]/wsp-consent-modal/div[2]/button[1]"));
            acceptButton.Click();

            Console.WriteLine("Enter searchterm: ");
            var input = Console.ReadLine();

            // find the search bar and search what the user inputs
            var search = driver.FindElement(By.Name("searchtext"));
            search.SendKeys(input);
            search.SendKeys(Keys.Return);
            driver.Manage().Window.Maximize();
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

            // Sort
            Sort(driver);
            Thread.Sleep(2000);
            wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
            By elem_product_link = By.XPath("//*[@id=\"js_items_content\"]/li");
            var item_bol = "Title; Creator; Specs; Prijs; Link";
            ReadOnlyCollection<IWebElement> products = driver.FindElements(elem_product_link);
            foreach (IWebElement product in products)
            {
                string str_title, str_creator, str_prijs, str_link;
                IWebElement elem_product_title = product.FindElement(By.XPath(".//div[2]/div/div[1]/a"));
                str_title = elem_product_title.Text;

                str_link = elem_product_title.GetAttribute("href");

                IWebElement elem_product_creator = product.FindElement(By.XPath(".//div[2]/div/ul[1]/li/a"));
                str_creator = elem_product_creator.Text;

                By elem_product_specs = By.XPath(".//li[1]/div[2]/div/ul[2]/li");
                ReadOnlyCollection<IWebElement> specList = driver.FindElements(elem_product_specs);
                var specs = "";
                foreach (IWebElement spec in specList)
                {
                    string str_spec;
                    IWebElement elem_spec = spec.FindElement(By.XPath(".//span"));
                    str_spec = elem_spec.Text;
                    specs += str_spec + " | ";
                }

                string[] rows = product.FindElement(By.XPath(".//div[2]/wsp-buy-block/div[1]/section/div[1]/div/span")).Text.Split('\n');

                str_prijs = rows[0];

                item_bol += "\n" + str_title + ";" + str_creator + ";" + specs
                    + ";" + str_prijs.Substring(0, str_prijs.Length - 1) + ";" + str_link;
                Console.WriteLine("\n");
                Console.WriteLine("Title: " + str_title);
                Console.WriteLine("Creator: " + str_creator);
                Console.WriteLine("Specs: " + specs);
                Console.WriteLine("Prijs: " + str_prijs + " Euro");
                Console.WriteLine("Video Link: " + str_link);
            }
            Csv(item_bol, csv_path);
            Quit(driver);
        }

        private void Csv(string item, string path)
        {
            var fileName = "";

            // show the items in console
            Console.WriteLine("\n");

            // ask a name for the csv file
            Console.WriteLine("Scraping Data has succeeded!");
            Console.WriteLine("How do you want to name the file to store the scraped data in? ");
            fileName = Console.ReadLine() + ".csv";

            File.WriteAllText(Path.Combine(path + @"\" + fileName), item);

            // give info about the file's whereabouts
            Console.WriteLine("\n" + fileName + " is saved to the folder: " + path);
        }


        private void Sort(IWebDriver driver)
        {
            // filter
            var filter = "";
            Console.WriteLine("\nSorteer op: ");
            Console.WriteLine("1 - Relevantie");
            Console.WriteLine("2 - Prijs laag-hoog");
            Console.WriteLine("3 - Prijs hoog-laag");
            filter = Console.ReadLine();
            if (filter == "1")
            {
                var filterButton = driver.FindElement(By.XPath("//*[@id=\"sort\"]/option[1]"));
                filterButton.Click();
            }
            else if (filter == "2")
            {
                var filterButton = driver.FindElement(By.XPath("//*[@id=\"sort\"]/option[3]"));
                filterButton.Click();
            }
            else if (filter == "3")
            {
                var filterButton = driver.FindElement(By.XPath("//*[@id=\"sort\"]/option[4]"));
                filterButton.Click();
            }
            else
            {
                Sort(driver);
            }
        }



        private void Quit(IWebDriver driver)
        {
            Console.WriteLine("\nPress enter to quit.");
            if (Console.ReadLine() == "") { driver.Quit(); }
            Start();
        }
    }
}