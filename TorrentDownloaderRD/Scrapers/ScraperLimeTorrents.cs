using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaDownloader
{
    internal class ScraperLimeTorrents
    {
        public static async Task ScrapeTorrentsAsync(string searchText, string contentItem, string sortByItem, int websiteSearches, Action<TorrentInfo> updateCallback)
        {
            string SortByItemParsed = "";
            if (sortByItem == "Size Descending") { SortByItemParsed = "size"; }
            else if (sortByItem == "Time Descending") { SortByItemParsed = "date"; }
            else if (sortByItem == "Seeders Descending") { SortByItemParsed = "seeds"; }

            string ContentItemParsed = "";
            if (contentItem == "Movies") { ContentItemParsed = "movies"; }
            else if (contentItem == "TV") { ContentItemParsed = "tv"; }
            else if (contentItem == "Anime") { ContentItemParsed = "anime"; }
            else if (contentItem == "Games") { ContentItemParsed = "games"; }

            var tasks = new List<Task>();

            for (int i = 1; i <= websiteSearches; i++)
            {
                string url = "";

                if (ContentItemParsed != "" && SortByItemParsed != "")
                {
                    url = $"https://www.limetorrents.lol/search/{ContentItemParsed}/{searchText.Replace(" ", "-")}/{SortByItemParsed}/{i}/";
                }
                else if (ContentItemParsed != "" && SortByItemParsed == "")
                {
                    url = $"https://www.limetorrents.lol/search/{ContentItemParsed}/{searchText.Replace(" ", "-")}//{i}/";
                }
                else if (ContentItemParsed == "" && SortByItemParsed != "")
                {
                    url = $"https://www.limetorrents.lol/search/all/{searchText.Replace(" ", "-")}/{SortByItemParsed}/{i}/";
                }
                else if (ContentItemParsed == "" && SortByItemParsed == "")
                {
                    url = $"https://www.limetorrents.lol/search/all/{searchText.Replace(" ", "-")}//{i}/";
                }

                tasks.Add(ProcessPageAsync(url, updateCallback));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task ProcessPageAsync(string url, Action<TorrentInfo> updateCallback)
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url);
            var rows = htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'table')]//tr[not(contains(@class, 'header'))]");

            if (rows == null || rows.Count == 0) return;

            foreach (var row in rows)
            {
                var torrent = ExtractTorrentInfoFromRow(row);
                if (torrent != null)
                {
                    updateCallback?.Invoke(torrent);
                }
            }
        }

        private static TorrentInfo ExtractTorrentInfoFromRow(HtmlNode row)
        {
            try
            {
                var nameNode = row.SelectSingleNode(".//div[@class='tt-name']//a[2]/text()");
                var urlNode = row.SelectSingleNode(".//div[@class='tt-name']//a[2]");
                var sizeNode = row.SelectSingleNode(".//td[@class='tdnormal'][2]");
                var seedsNode = row.SelectSingleNode(".//td[@class='tdseed']");
                var leechersNode = row.SelectSingleNode(".//td[@class='tdleech']");
                var dateNode = row.SelectSingleNode(".//td[@class='tdnormal'][1]");

                if (urlNode == null || sizeNode == null || seedsNode == null || leechersNode == null || dateNode == null || nameNode == null)
                {
                    return null;
                }

                var baseUrl = "https://www.limetorrents.lol/";
                var relativeUrl = urlNode.GetAttributeValue("href", "").TrimStart('/');
                var torrentUrl = baseUrl + relativeUrl;

                string sizeText = sizeNode.InnerText.Trim();
                var sizeSpanNode = sizeNode.SelectSingleNode(".//span");
                if (sizeSpanNode != null)
                {
                    sizeNode.RemoveChild(sizeSpanNode);
                    sizeText = sizeNode.InnerText.Trim();
                }

                var name = nameNode.InnerText.Trim();

                return new TorrentInfo
                {
                    Url = torrentUrl,
                    Name = name,
                    Seeders = int.TryParse(seedsNode.InnerText.Trim().Replace(",", ""), out var seeds) ? seeds : 0,
                    Leechers = int.TryParse(leechersNode.InnerText.Trim().Replace(",", ""), out var leechers) ? leechers : 0,
                    Date = dateNode.InnerText.Trim(),
                    Size = sizeText,
                    SizeInBytes = ConvertToBytes(sizeText)
                };
            }
            catch (Exception ex)
            {
                //($"Error extracting torrent info: {ex.Message}");
                return null;
            }
        }

        public static long ConvertToBytes(string sizeText)
        {
            try
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
            catch (Exception ex)
            {
                //Console.WriteLine($"Error converting size to bytes: {ex.Message}");
                return 0;
            }
        }
    }
}
