using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaDownloader.Scrapers
{
    internal class ScraperBitSearch
    {
        public static async Task ScrapeTorrentsAsync(string searchText, string contentItem, string sortByItem, int websiteSearches, Action<TorrentInfo> updateCallback)
        {
            string SortByItemParsed = "";
            if (sortByItem == "Size Descending") { SortByItemParsed = "size"; }
            else if (sortByItem == "Time Descending") { SortByItemParsed = "date"; }
            else if (sortByItem == "Seeders Descending") { SortByItemParsed = "seeders"; }

            string ContentItemParsed = "";
            if (contentItem == "Movies" || contentItem == "TV") { ContentItemParsed = "&category=1&subcat=2"; }
            else if (contentItem == "Games") { ContentItemParsed = "&category=6&subcat=1"; }

            var tasks = new List<Task>();

            for (int i = 1; i <= websiteSearches; i++)
            {
                string url = $"https://bitsearch.to/search?q={searchText.Replace(" ", "+")}&page={i}{ContentItemParsed}&sort={SortByItemParsed}";
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

            var rows = htmlDoc.DocumentNode.SelectNodes("/html/body/main/div[4]/div/div[3]/li");

            if (rows == null || rows.Count == 0) return;

            foreach (var row in rows)
            {
                var urlNode = row.SelectSingleNode("div[1]/div/h5/a");
                var sizeNode = row.SelectSingleNode("div[1]/div/div/div/div[2]");
                var seedersNode = row.SelectSingleNode("div[1]/div/div/div/div[3]");
                var leechersNode = row.SelectSingleNode("div[1]/div/div/div/div[4]");
                var dateNode = row.SelectSingleNode("div[1]/div/div/div/div[5]");

                if (urlNode != null && seedersNode != null && leechersNode != null && dateNode != null && sizeNode != null)
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
                var seedersText = seedersNode.SelectSingleNode("font").InnerText.Trim().Replace(",", "");
                int.TryParse(seedersText, out seeders);
            }

            if (leechersNode != null)
            {
                var leechersText = leechersNode.SelectSingleNode("font").InnerText.Trim().Replace(",", "");
                int.TryParse(leechersText, out leechers);
            }

            return new TorrentInfo
            {
                Url = "https://bitsearch.to" + urlNode.GetAttributeValue("href", ""),
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
