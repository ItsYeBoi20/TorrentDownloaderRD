using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MediaDownloader
{
    internal class Scraper1337x
    {
        public static async Task ScrapeTorrentsAsync(string searchText, string contentItem, string sortByItem, int websiteSearches, Action<TorrentInfo> updateCallback)
        {
            string SortByItemParsed = "";
            if (sortByItem == "Size Descending") { SortByItemParsed = "size/desc"; }
            else if (sortByItem == "Size Ascending") { SortByItemParsed = "size/asc"; }
            else if (sortByItem == "Time Descending") { SortByItemParsed = "time/desc"; }
            else if (sortByItem == "Time Ascending") { SortByItemParsed = "time/asc"; }
            else if (sortByItem == "Seeders Descending") { SortByItemParsed = "seeders/desc"; }
            else if (sortByItem == "Seeders Ascending") { SortByItemParsed = "seeders/asc"; }

            var tasks = new List<Task>();

            for (int i = 1; i <= websiteSearches; i++)
            {
                string url = "";

                if (contentItem != "" && SortByItemParsed != "")
                {
                    url = $"https://1337x.to/sort-category-search/{searchText.Replace(" ", "+")}/{contentItem}/{SortByItemParsed.ToLower()}/{i}/";
                }
                else if (contentItem != "" && SortByItemParsed == "")
                {
                    url = $"https://1337x.to/category-search/{searchText.Replace(" ", "+")}/{contentItem}/{i}/";
                }
                else if (contentItem == "" && SortByItemParsed != "")
                {
                    url = $"https://1337x.to/sort-search/{searchText.Replace(" ", "+")}/{SortByItemParsed.ToLower()}/{i}/";
                }
                else if (contentItem == "" && SortByItemParsed == "")
                {
                    url = $"https://1337x.to/search/{searchText.Replace(" ", "+")}/{i}/";
                }

                tasks.Add(ProcessPageAsync(url, updateCallback));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task ProcessPageAsync(string url, Action<TorrentInfo> updateCallback)
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url);
            var rows = htmlDoc.DocumentNode.SelectNodes("/html/body/main/div/div/div/div[2]/div[1]/table/tbody/tr");

            if (rows == null) return;

            foreach (var row in rows)
            {
                var torrent = ExtractTorrentInfoFromRow(row);
                updateCallback?.Invoke(torrent);
            }
        }

        private static TorrentInfo ExtractTorrentInfoFromRow(HtmlNode row)
        {
            var urlNode = row.SelectSingleNode("td[1]/a[2]");
            var seedersNode = row.SelectSingleNode("td[2]");
            var leechersNode = row.SelectSingleNode("td[3]");
            var dateNode = row.SelectSingleNode("td[4]");
            var sizeNode = row.SelectSingleNode("td[5]");

            string sizeText = sizeNode.InnerText.Trim();
            var sizeSpanNode = sizeNode.SelectSingleNode("span");

            if (sizeSpanNode != null)
            {
                sizeNode.RemoveChild(sizeSpanNode);
                sizeText = sizeNode.InnerText.Trim();
            }

            return new TorrentInfo
            {
                Url = "https://1337x.to" + urlNode.GetAttributeValue("href", ""),
                Name = urlNode.InnerText,
                Seeders = int.Parse(seedersNode.InnerText),
                Leechers = int.Parse(leechersNode.InnerText),
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
