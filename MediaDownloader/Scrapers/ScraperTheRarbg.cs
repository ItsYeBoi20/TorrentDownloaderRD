using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MediaDownloader.Scrapers
{
    internal class ScraperTheRarbg
    {
        public static async Task ScrapeTorrentsAsync(string searchText, string contentItem, string sortByItem, int websiteSearches, Action<TorrentInfo> updateCallback)
        {
            string SortByItemParsed = "";
            if (sortByItem == "Size Descending") { SortByItemParsed = "-s"; }
            else if (sortByItem == "Time Descending") { SortByItemParsed = "-a"; }
            else if (sortByItem == "Seeders Descending") { SortByItemParsed = "-se"; }

            string ContentItemParsed = "";
            if (contentItem == "Movies") { ContentItemParsed = "Movies"; }
            else if (contentItem == "TV") { ContentItemParsed = "TV"; }
            else if (contentItem == "Anime") { ContentItemParsed = "Anime"; }
            else if (contentItem == "Games") { ContentItemParsed = "Games"; }
            else if (contentItem == "XXX") { ContentItemParsed = "XXX"; }

            var tasks = new List<Task>();

            for (int i = 1; i <= websiteSearches; i++)
            {
                string url = "";

                if (ContentItemParsed != "" && SortByItemParsed != "")
                {
                    url = $"https://therarbg.com/get-posts/category:{ContentItemParsed}:keywords:{searchText.Replace(" ", "%20")}/?page={i}";
                }
                else if (ContentItemParsed != "" && SortByItemParsed == "")
                {
                    url = $"https://therarbg.com/get-posts/category:{ContentItemParsed}:keywords:{searchText.Replace(" ", "%20")}/?page={i}";
                }
                else if (ContentItemParsed == "" && SortByItemParsed != "")
                {
                    url = $"https://therarbg.com/get-posts/order:{SortByItemParsed}:keywords:{searchText.Replace(" ", "%20")}/?page={i}";
                }
                else if (ContentItemParsed == "" && SortByItemParsed == "")
                {
                    url = $"https://therarbg.com/get-posts/keywords:{searchText.Replace(" ", "%20")}/?page={i}";
                }

                tasks.Add(ProcessPageAsync(url, updateCallback));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task ProcessPageAsync(string url, Action<TorrentInfo> updateCallback)
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url);

            var rows = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'sortableTable2')]/tbody/tr");

            if (rows == null || rows.Count == 0)
            {
                Console.WriteLine("No rows found using the provided XPath.");
                return;
            }

            foreach (var row in rows)
            {
                var urlNode = row.SelectSingleNode(".//td[@class='cellName']/div/a");
                var sizeNode = row.SelectSingleNode(".//td[@class='sizeCell']");
                var seedersNode = row.SelectSingleNode(".//td[7]");
                var leechersNode = row.SelectSingleNode(".//td[8]");
                var dateNode = row.SelectSingleNode(".//td[4]/div");

                if (urlNode != null && sizeNode != null && seedersNode != null && leechersNode != null && dateNode != null)
                {
                    var torrent = ExtractTorrentInfoFromRow(urlNode, sizeNode, seedersNode, leechersNode, dateNode);
                    updateCallback?.Invoke(torrent);
                }
            }
        }


        private static TorrentInfo ExtractTorrentInfoFromRow(HtmlNode urlNode, HtmlNode sizeNode, HtmlNode seedersNode, HtmlNode leechersNode, HtmlNode dateNode)
        {
            string sizeText = sizeNode.InnerText.Trim();

            int seeders = 0;
            int leechers = 0;

            if (seedersNode != null)
            {
                string seedersText = seedersNode.InnerText.Trim().Replace(",", "");
                int.TryParse(seedersText, out seeders);
            }

            if (leechersNode != null)
            {
                string leechersText = leechersNode.InnerText.Trim().Replace(",", "");
                int.TryParse(leechersText, out leechers);
            }

            return new TorrentInfo
            {
                Url = "https://therarbg.com" + urlNode.GetAttributeValue("href", ""),
                Name = urlNode.InnerText.Trim(),
                Seeders = seeders,
                Leechers = leechers,
                Date = dateNode.InnerText.Trim(),
                Size = sizeText,
                SizeInBytes = ConvertToBytes(sizeText)
            };
        }

        public static long ConvertToBytes(string sizeText)
        {
            sizeText = sizeText.ToUpper().Replace(",", "").Trim();
            double size = double.Parse(System.Text.RegularExpressions.Regex.Match(sizeText, @"\d+(\.\d+)?").Value);

            if (sizeText.Contains("TB"))
                size *= Math.Pow(1024, 4);
            else if (sizeText.Contains("GB"))
                size *= Math.Pow(1024, 3);
            else if (sizeText.Contains("MB"))
                size *= Math.Pow(1024, 2);
            else if (sizeText.Contains("KB"))
                size *= 1024;

            return (long)size;
        }
    }
}
