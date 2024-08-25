using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MediaDownloader.Scrapers
{
    internal class ScraperPirateBay
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task ScrapeTorrentsAsync(string searchText, int numberOfResults, string contentItem, Action<TorrentInfo> updateCallback)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            string ContentItemParsed = "";
            if (contentItem == "Movies") { ContentItemParsed = "&cat=201"; }
            else if (contentItem == "TV") { ContentItemParsed = "&cat=205"; }
            else if (contentItem == "Games") { ContentItemParsed = "&cat=401"; }
            else if (contentItem == "XXX") { ContentItemParsed = "&cat=501"; }

            int resultsFetched = 0;
            int page = 0;

            while (resultsFetched < numberOfResults)
            {
                string BaseUrl = "https://apibay.org";
                string url = "";
                if (ContentItemParsed != "")
                {
                    url = $"{BaseUrl}/q.php?q={searchText.Replace(" ", "+")}{ContentItemParsed}";
                }
                else if (ContentItemParsed == "")
                {
                    url = $"{BaseUrl}/q.php?q={searchText.Replace(" ", "+")}";
                }

                var response = await httpClient.GetStringAsync(url);
                var torrents = JArray.Parse(response);

                if (torrents.Count == 0) break;

                foreach (var torrent in torrents)
                {
                    if (resultsFetched >= numberOfResults) break;

                    if (torrent["id"] == null || torrent["info_hash"] == null) continue;

                    var name = torrent["name"].ToString();
                    var magnetLink = $"magnet:?xt=urn:btih:{torrent["info_hash"]}";

                    // Store the magnet link in the dictionary using the torrent name as the key
                    Main.magnetLinksPirate[name] = magnetLink;

                    var torrentInfo = new TorrentInfo
                    {
                        Name = name,
                        Size = ConvertBytesToReadableSize(long.Parse(torrent["size"].ToString())),
                        Seeders = int.Parse(torrent["seeders"].ToString()),
                        Leechers = int.Parse(torrent["leechers"].ToString()),
                        Url = $"https://thepiratebay.org/description.php?id={torrent["id"]}",
                        Magnet = magnetLink
                    };

                    updateCallback?.Invoke(torrentInfo);
                    resultsFetched++;
                }

                page++;
            }
        }

        private static string ConvertBytesToReadableSize(long sizeBytes)
        {
            double size = sizeBytes;
            string[] sizeUnits = { "B", "KB", "MB", "GB", "TB" };
            int unitIndex = 0;

            while (size >= 1024 && unitIndex < sizeUnits.Length - 1)
            {
                unitIndex++;
                size /= 1024;
            }

            return $"{size:F2} {sizeUnits[unitIndex]}";
        }
    }
}
