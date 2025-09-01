using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static RealDebridAPI.RealDebridClient;

namespace MediaDownloader
{
    internal class ScraperKickAssTorrents
    {
        public static string ChromeUserAgent => Environment.OSVersion.Platform == PlatformID.Unix ?
            "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36" :
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36";

        public static async Task ScrapeTorrentsAsync(string searchText, string contentItem, string sortByItem, int websiteSearches,
            Action<TorrentInfo> updateCallback, int timeoutDelay, CancellationToken cancellationToken)
        {
            string SortByItemParsed = "";
            if (sortByItem == "Size Descending") { SortByItemParsed = "size&sort=desc"; }
            else if (sortByItem == "Size Ascending") { SortByItemParsed = "size&sort=asc"; }
            else if (sortByItem == "Time Descending") { SortByItemParsed = "time&sort=desc"; }
            else if (sortByItem == "Time Ascending") { SortByItemParsed = "time&sort=asc"; }
            else if (sortByItem == "Seeders Descending") { SortByItemParsed = "seeders&sort=desc"; }
            else if (sortByItem == "Seeders Ascending") { SortByItemParsed = "seeders&sort=asc"; }

            string ContentItemParsed = "";
            if (contentItem == "Movies") { ContentItemParsed = "movies"; }
            else if (contentItem == "TV") { ContentItemParsed = "tv"; }
            else if (contentItem == "Anime") { ContentItemParsed = "anime"; }
            else if (contentItem == "Games") { ContentItemParsed = "games"; }
            else if (contentItem == "XXX") { ContentItemParsed = "xxx"; }

            var tasks = new List<Task>();

            for (int i = 1; i <= websiteSearches; i++)
            {
                string url = "";

                if (contentItem != "" && SortByItemParsed != "")
                {
                    url = $"https://kickasstorrents.to/search/{searchText.Replace(" ", "%20")}/category/{ContentItemParsed}/{i}/?sortby={SortByItemParsed}";
                }
                else if (contentItem != "" && SortByItemParsed == "")
                {
                    url = $"https://kickasstorrents.to/search/{searchText.Replace(" ", "%20")}/category/{ContentItemParsed}/{i}/";
                }
                else if (contentItem == "" && SortByItemParsed != "")
                {
                    url = $"https://kickasstorrents.to/search/{searchText.Replace(" ", "%20")}/{i}/?sortby={SortByItemParsed}";
                }
                else if (contentItem == "" && SortByItemParsed == "")
                {
                    url = $"https://kickasstorrents.to/search/{searchText.Replace(" ", "%20")}/{i}/";
                }

                tasks.Add(ProcessPageAsync(url, updateCallback, timeoutDelay, cancellationToken));
                await Task.Delay(500);                
            }

            await Task.WhenAll(tasks);
        }

        private static async Task ProcessPageAsync(string url, Action<TorrentInfo> updateCallback, int timeoutDelay, CancellationToken cancellationToken)
        {
            var htmlDoc = await LoadFromWebWithTimeoutAsync(url, TimeSpan.FromSeconds(timeoutDelay), cancellationToken);

            var rows = htmlDoc.DocumentNode.SelectNodes("//table[@class='data frontPageWidget']//tr[not(contains(@class, 'firstr'))]");

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


        private static async Task<HtmlDocument> LoadFromWebWithTimeoutAsync(string url, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(timeout);
                try
                {
                    var httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("User-Agent", ChromeUserAgent);
                    var response = await httpClient.GetStringAsync(url);
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(response);
                    return htmlDoc;
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }
        }

        private static TorrentInfo ExtractTorrentInfoFromRow(HtmlNode row)
        {
            var nameAndUrlNode = row.SelectSingleNode(".//div[@class='torrentname']//a[@class='cellMainLink']");
            if (nameAndUrlNode == null) return null; 

            string relativeUrl = nameAndUrlNode.GetAttributeValue("href", "");
            string fullUrl = "https://kickasstorrents.to" + relativeUrl;
            string name = nameAndUrlNode.InnerText.Trim();

            var sizeNode = row.SelectSingleNode(".//td[@class='nobr center']");
            var seedersNode = row.SelectSingleNode(".//td[contains(@class, 'green center')]");
            var leechersNode = row.SelectSingleNode(".//td[contains(@class, 'red lasttd center')]");

            string sizeText = sizeNode?.InnerText.Trim() ?? "Unknown";
            int seeders = int.TryParse(seedersNode?.InnerText.Trim(), out int seed) ? seed : 0;
            int leechers = int.TryParse(leechersNode?.InnerText.Trim(), out int leech) ? leech : 0;

            return new TorrentInfo
            {
                Url = fullUrl,
                Name = name,
                Seeders = seeders,
                Leechers = leechers,
                Size = sizeText,
                SizeInBytes = ConvertToBytes(sizeText)
            };
        }



        public static long ConvertToBytes(string sizeText)
        {
            sizeText = sizeText.ToUpper().Replace(",", "").Trim();
            double size = double.Parse(System.Text.RegularExpressions.Regex.Match(sizeText, @"\d+(\.\d+)?").Value, CultureInfo.InvariantCulture);

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
