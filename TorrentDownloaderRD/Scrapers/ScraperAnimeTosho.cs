using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MediaDownloader
{
    internal class ScraperAnimeTosho
    {
        public static async Task ScrapeTorrentsAsync(string searchText, string contentItem, string sortByItem, int websiteSearches, Action<TorrentInfo> updateCallback)
        {
            string SortByItemParsed = "";
            if (sortByItem == "Size Descending") { SortByItemParsed = "size-d"; }
            else if (sortByItem == "Size Ascending") { SortByItemParsed = "size-a"; }
            else if (sortByItem == "Time Descending") { SortByItemParsed = "date-d"; }
            else if (sortByItem == "Time Ascending") { SortByItemParsed = "date-a"; }

            var tasks = new List<Task>();

            for (int i = 1; i <= websiteSearches; i++)
            {
                string url = "";

                if (SortByItemParsed != "")
                {
                    url = $"https://animetosho.org/search?q={searchText.Replace(" ", "+")}&order={SortByItemParsed}&page={i}";
                }
                else if (SortByItemParsed == "")
                {
                    url = $"https://animetosho.org/search?q={searchText.Replace(" ", "+")}&page={i}";
                }

                tasks.Add(ProcessPageAsync(url, updateCallback));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task ProcessPageAsync(string url, Action<TorrentInfo> updateCallback)
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url);
            var rows = htmlDoc.DocumentNode.SelectNodes("//*[@id='content']/div[5]/div[contains(@class, 'home_list_entry')]");

            if (rows == null) return;

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
            var urlNode = row.SelectSingleNode(".//div[@class='link']/a");
            var seedersLeechersNode = row.SelectSingleNode(".//span[@title and contains(@title, 'Seeders')]");
            var sizeNode = row.SelectSingleNode(".//div[@class='size']");
            if (urlNode == null || seedersLeechersNode == null || sizeNode == null)
            {
                return null;
            }

            string sizeText = sizeNode.InnerText.Trim();
            string seedersLeechersText = seedersLeechersNode.GetAttributeValue("title", "");
            var seedersLeechers = seedersLeechersText.Split(new[] { "Seeders: ", " / Leechers: " }, StringSplitOptions.RemoveEmptyEntries);

            int seeders = 0;
            int leechers = 0;

            if (seedersLeechers.Length > 0 && !int.TryParse(seedersLeechers[0].Trim(), out seeders))
            {
                seeders = 0;
            }

            if (seedersLeechers.Length > 1 && !int.TryParse(seedersLeechers[1].Trim(), out leechers))
            {
                leechers = 0;
            }

            return new TorrentInfo
            {
                Url = urlNode.GetAttributeValue("href", ""),
                Name = urlNode.InnerText.Trim(),
                Seeders = seeders,
                Leechers = leechers,
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
