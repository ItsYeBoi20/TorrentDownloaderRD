using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaDownloader.Scrapers
{
    internal class ScraperTorrentDownload
    {
        public static async Task ScrapeTorrentsAsync(string searchText, string contentItem, string sortByItem, int websiteSearches, Action<TorrentInfo> updateCallback)
        {
            string SortByItemParsed = "";
            if (sortByItem == "Size Descending") { SortByItemParsed = "searchs"; }
            else if (sortByItem == "Time Descending") { SortByItemParsed = "searchd"; }
            else if (sortByItem == "Seeders Descending") { SortByItemParsed = "search"; }

            var tasks = new List<Task>();

            for (int i = 1; i <= websiteSearches; i++)
            {
                string url = "";

                if (SortByItemParsed != "")
                {
                    url = $"https://www.torrentdownload.info/{SortByItemParsed}?q={searchText.Replace(" ", "+")}&p={i}";
                }
                else if (SortByItemParsed == "")
                {
                    url = $"https://www.torrentdownload.info/search?q={searchText.Replace(" ", "+")}&p={i}";
                }

                tasks.Add(ProcessPageAsync(url, updateCallback));
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during scraping: {ex.Message}");
                throw;
            }
        }

        private static async Task ProcessPageAsync(string url, Action<TorrentInfo> updateCallback)
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url);

            var rows = htmlDoc.DocumentNode.SelectNodes("/html/body/div/table[2]/tr");

            if (rows == null || rows.Count == 0) return;

            foreach (var row in rows)
            {
                var urlNode = row.SelectSingleNode("td[1]/div[1]/a");
                var seedersNode = row.SelectSingleNode("td[4]");
                var leechersNode = row.SelectSingleNode("td[5]");
                var dateNode = row.SelectSingleNode("td[2]");
                var sizeNode = row.SelectSingleNode("td[3]");

                if (urlNode != null && seedersNode != null && leechersNode != null && dateNode != null && sizeNode != null)
                {
                    var torrent = ExtractTorrentInfoFromRow(urlNode, seedersNode, leechersNode, dateNode, sizeNode);
                    updateCallback?.Invoke(torrent);
                }
            }
        }

        private static TorrentInfo ExtractTorrentInfoFromRow(HtmlNode urlNode, HtmlNode seedersNode, HtmlNode leechersNode, HtmlNode dateNode, HtmlNode sizeNode)
        {
            string sizeText = sizeNode.InnerText.Trim();
            var sizeSpanNode = sizeNode.SelectSingleNode("span");

            if (sizeSpanNode != null)
            {
                sizeNode.RemoveChild(sizeSpanNode);
                sizeText = sizeNode.InnerText.Trim();
            }

            int seeders = 0;
            int leechers = 0;

            if (seedersNode != null)
            {
                int.TryParse(seedersNode.InnerText.Trim().Replace(",", ""), out seeders);
            }

            if (leechersNode != null)
            {
                int.TryParse(leechersNode.InnerText.Trim().Replace(",", ""), out leechers);
            }

            return new TorrentInfo
            {
                Url = "https://www.torrentdownload.info" + urlNode.GetAttributeValue("href", ""),
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
