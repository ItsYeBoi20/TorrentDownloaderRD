using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaDownloader
{
    internal class ScraperYourBittorent
    {
        public static async Task ScrapeTorrentsAsync(string searchText, string contentItem, string sortByItem, int websiteSearches, Action<TorrentInfo> updateCallback)
        {
            string contentItemParsed = "";
            if (contentItem == "Movies") { contentItemParsed = "&c=1"; }
            else if (contentItem == "TV") { contentItemParsed = "&c=3"; }
            else if (contentItem == "Anime") { contentItemParsed = "&c=6"; }
            else if (contentItem == "Games") { contentItemParsed = "&c=4"; }
            else if (contentItem == "XXX") { contentItemParsed = "&c=7"; }

            var tasks = new List<Task>();

            for (int i = 1; i <= websiteSearches; i++)
            {
                string url = $"https://yourbittorrent.com/?q={searchText.Replace(" ", "+")}&page={i}{contentItemParsed}";
                tasks.Add(ProcessPageAsync(url, updateCallback));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task ProcessPageAsync(string url, Action<TorrentInfo> updateCallback)
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url);

            var rows = htmlDoc.DocumentNode.SelectNodes("/html/body/div/div[2]/table/tbody/tr");

            if (rows == null) return;

            foreach (var row in rows)
            {
                var torrent = ExtractTorrentInfoFromRow(row);
                updateCallback?.Invoke(torrent);
            }
        }

        private static TorrentInfo ExtractTorrentInfoFromRow(HtmlNode row)
        {
            var urlNode = row.SelectSingleNode("td[2]/a");
            var sizeNode = row.SelectSingleNode("td[3]");
            var dateNode = row.SelectSingleNode("td[4]");
            var seedersNode = row.SelectSingleNode("td[5]");
            var leechersNode = row.SelectSingleNode("td[6]");

            string sizeText = sizeNode.InnerText.Trim();

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
                Url = "https://yourbittorrent.com" + urlNode.GetAttributeValue("href", ""),
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
