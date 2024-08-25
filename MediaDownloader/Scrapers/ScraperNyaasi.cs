using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MediaDownloader
{
    internal class ScraperNyaasi
    {
        private const string BaseUrl = "https://nyaa.si";

        public static async Task ScrapeTorrentsAsync(string searchText, string contentItem, string sortByItem, int websiteSearches, Action<TorrentInfo> updateCallback)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            string SortByItemParsed = "";
            if (sortByItem == "Size Descending") { SortByItemParsed = "&s=size&o=desc"; }
            else if (sortByItem == "Size Ascending") { SortByItemParsed = "&s=size&o=asc"; }
            else if (sortByItem == "Time Descending") { SortByItemParsed = "&s=id&o=desc"; }
            else if (sortByItem == "Time Ascending") { SortByItemParsed = "&s=id&o=asc"; }
            else if (sortByItem == "Seeders Descending") { SortByItemParsed = "&s=seeders&o=desc"; }
            else if (sortByItem == "Seeders Ascending") { SortByItemParsed = "&s=seeders&o=asc"; }

            string ContentItemParsed = "";
            if (contentItem == "Movies") { ContentItemParsed = "4_0"; }
            else if (contentItem == "TV") { ContentItemParsed = "4_0"; }
            else if (contentItem == "Anime") { ContentItemParsed = "1_0"; }
            else if (contentItem == "Games") { ContentItemParsed = "6_2"; }

            var tasks = new List<Task>();

            for (int page = 1; page <= websiteSearches; page++)
            {
                string url = "";
                if (ContentItemParsed != "" && SortByItemParsed != "")
                {
                    url = $"{BaseUrl}/?f=0&c={ContentItemParsed}&q={searchText.Replace(" ", "+")}{SortByItemParsed}&p={page}";
                }
                else if (ContentItemParsed != "" && SortByItemParsed == "")
                {
                    url = $"{BaseUrl}/?f=0&c={ContentItemParsed}&q={searchText.Replace(" ", "+")}&p={page}";
                }
                else if (ContentItemParsed == "" && SortByItemParsed != "")
                {
                    url = $"{BaseUrl}/?f=0&c=0_0&q={searchText.Replace(" ", "+")}{SortByItemParsed}&p={page}";
                }
                else if (ContentItemParsed == "" && SortByItemParsed == "")
                {
                    url = $"{BaseUrl}/?f=0&c=0_0&q={searchText.Replace(" ", "+")}&p={page}";
                }

                tasks.Add(ProcessPageAsync(url, updateCallback));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task ProcessPageAsync(string url, Action<TorrentInfo> updateCallback)
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url);
            var rows = htmlDoc.DocumentNode.SelectNodes("//tr[@class='default' or @class='danger' or @class='success']");

            if (rows == null || rows.Count == 0) return;

            foreach (var row in rows)
            {
                var torrent = ExtractTorrentInfoFromRow(row);
                updateCallback?.Invoke(torrent);
            }
        }

        private static string GetCategory(string contentItem)
        {
            var categories = new Dictionary<string, string>
            {
                { "all", "0_0" },
                { "anime", "1_0" },
                { "books", "3_0" },
                { "music", "2_0" },
                { "pictures", "5_0" },
                { "software", "6_0" },
                { "tv", "4_0" },
                { "movies", "4_0" }
            };

            return categories.ContainsKey(contentItem) ? categories[contentItem] : "0_0";
        }

        private static TorrentInfo ExtractTorrentInfoFromRow(HtmlNode row)
        {
            var nameTdNode = row.SelectSingleNode("td[2]");
            var anchorNodes = nameTdNode.SelectNodes("a");
            HtmlNode nameNode = anchorNodes.Count > 1 ? anchorNodes[1] : anchorNodes[0];

            var sizeNode = row.SelectSingleNode("td[4]");
            var seedersNode = row.SelectSingleNode("td[6]");
            var leechersNode = row.SelectSingleNode("td[7]");

            string sizeText = ConvertSizeTextToDecimal(sizeNode.InnerText.Trim());

            return new TorrentInfo
            {
                Url = BaseUrl + nameNode.GetAttributeValue("href", ""),
                Name = nameNode.GetAttributeValue("title", nameNode.InnerText.Trim()),
                Size = sizeText,
                SizeInBytes = ConvertToBytes(sizeText),
                Seeders = int.Parse(seedersNode.InnerText.Trim()),
                Leechers = int.Parse(leechersNode.InnerText.Trim())
            };
        }

        private static string ConvertSizeTextToDecimal(string sizeText)
        {
            sizeText = sizeText.ToUpper();

            if (sizeText.Contains("TIB"))
                return sizeText.Replace("TIB", "TB");
            else if (sizeText.Contains("GIB"))
                return sizeText.Replace("GIB", "GB");
            else if (sizeText.Contains("MIB"))
                return sizeText.Replace("MIB", "MB");
            else if (sizeText.Contains("KIB"))
                return sizeText.Replace("KIB", "KB");

            return sizeText; 
        }

        public static long ConvertToBytes(string sizeText)
        {
            sizeText = sizeText.ToUpper().Replace(",", "").Trim();
            double size = double.Parse(System.Text.RegularExpressions.Regex.Match(sizeText, @"\d+(\.\d+)?").Value);

            if (sizeText.Contains("TIB"))  // TiB to TB conversion
                size *= Math.Pow(1024, 4) * (1000.0 / 1024.0);
            else if (sizeText.Contains("GIB"))  // GiB to GB conversion
                size *= Math.Pow(1024, 3) * (1000.0 / 1024.0);
            else if (sizeText.Contains("MIB"))  // MiB to MB conversion
                size *= Math.Pow(1024, 2) * (1000.0 / 1024.0);
            else if (sizeText.Contains("KIB"))  // KiB to KB conversion
                size *= 1024 * (1000.0 / 1024.0);
            else if (sizeText.Contains("TB"))  // TB (already in decimal, no conversion needed)
                size *= Math.Pow(1024, 4);
            else if (sizeText.Contains("GB"))  // GB (already in decimal, no conversion needed)
                size *= Math.Pow(1024, 3);
            else if (sizeText.Contains("MB"))  // MB (already in decimal, no conversion needed)
                size *= Math.Pow(1024, 2);
            else if (sizeText.Contains("KB"))  // KB (already in decimal, no conversion needed)
                size *= 1024;

            return (long)size;
        }
    }
}
