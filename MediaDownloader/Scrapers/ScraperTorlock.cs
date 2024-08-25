using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaDownloader
{
    internal class ScraperTorlock
    {
        public static async Task ScrapeTorrentsAsync(string searchText, string contentItem, string sortByItem, int websiteSearches, Action<TorrentInfo> updateCallback)
        {
            string SortByItemParsed = "";
            if (sortByItem == "Size Descending") { SortByItemParsed = "size"; }
            else if (sortByItem == "Seeders Descending") { SortByItemParsed = "seeds"; }

            string ContentItemParsed = "";
            if (contentItem == "Movies") { ContentItemParsed = "movies"; }
            else if (contentItem == "TV") { ContentItemParsed = "television"; }
            else if (contentItem == "Anime") { ContentItemParsed = "anime"; }
            else if (contentItem == "Games") { ContentItemParsed = "game"; }
            else if (contentItem == "XXX") { ContentItemParsed = "adult"; }

            var tasks = new List<Task>();

            for (int i = 1; i <= websiteSearches; i++)
            {
                string BaseUrl = "https://www.torlock2.com";
                string url = "";
                if (ContentItemParsed != "" && SortByItemParsed != "")
                {
                    url = $"{BaseUrl}/{ContentItemParsed}/torrents/{searchText.Replace(" ", "%20")}.html?sort={SortByItemParsed}&page={i}";
                }
                else if (ContentItemParsed != "" && SortByItemParsed == "")
                {
                    url = $"{BaseUrl}/{ContentItemParsed}/torrents/{searchText.Replace(" ", "%20")}.html?&page={i}";
                }
                else if (ContentItemParsed == "" && SortByItemParsed != "")
                {
                    url = $"{BaseUrl}/all/torrents/{searchText.Replace(" ", "%20")}.html?sort={SortByItemParsed}&page={i}";
                }
                else if (ContentItemParsed == "" && SortByItemParsed == "")
                {
                    url = $"{BaseUrl}/all/torrents/{searchText.Replace(" ", "%20")}.html?&page={i}";
                }

                tasks.Add(ProcessPageAsync(url, updateCallback));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task ProcessPageAsync(string url, Action<TorrentInfo> updateCallback)
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url);

            var tableNode = htmlDoc.DocumentNode.SelectSingleNode("/html/body/article/div[2]/table");

            if (tableNode == null)
            {
                return;
            }

            var rows = tableNode.SelectNodes(".//tr");

            if (rows == null || rows.Count == 0)
            {
                //Console.WriteLine("No rows found in the table.");
                return;
            }

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
                var nameNode = row.SelectSingleNode("./td[1]/div[1]/a");
                var sizeNode = row.SelectSingleNode("./td[3]");
                var seedsNode = row.SelectSingleNode("./td[4]");
                var leechersNode = row.SelectSingleNode("./td[5]");

                if (nameNode == null || sizeNode == null || seedsNode == null || leechersNode == null)
                {
                    //Console.WriteLine("Error: Missing expected node in row");
                    return null;
                }

                var baseUrl = "https://www.torlock2.com";
                var relativeUrl = nameNode.GetAttributeValue("href", "").TrimStart('/');
                var torrentUrl = $"{baseUrl}/{relativeUrl}";
                var torrentName = nameNode.InnerText.Trim();

                string sizeText = sizeNode.InnerText.Trim();

                return new TorrentInfo
                {
                    Url = torrentUrl,
                    Name = torrentName,
                    Seeders = int.TryParse(seedsNode.InnerText.Trim().Replace(",", ""), out var seeds) ? seeds : 0,
                    Leechers = int.TryParse(leechersNode.InnerText.Trim().Replace(",", ""), out var leechers) ? leechers : 0,
                    Size = sizeText,
                    SizeInBytes = ConvertToBytes(sizeText),
                };
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error extracting torrent info: {ex.Message}");
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
                //($"Error converting size to bytes: {ex.Message}");
                return 0;
            }
        }
    }
}
