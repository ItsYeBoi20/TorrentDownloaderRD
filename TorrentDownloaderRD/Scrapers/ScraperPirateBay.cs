using System;
using System.Drawing;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace MediaDownloader.Scrapers
{
    internal class ScraperPirateBay
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task ScrapeTorrentsAsync(string searchText, int numberOfResults, string contentItem, Action<TorrentInfo> updateCallback, int timeoutDelay, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(searchText)) return;

            string ContentItemParsed = "";
            if (contentItem == "Movies") { ContentItemParsed = "&cat=201"; }
            else if (contentItem == "TV") { ContentItemParsed = "&cat=205"; }
            else if (contentItem == "Games") { ContentItemParsed = "&cat=401"; }
            else if (contentItem == "XXX") { ContentItemParsed = "&cat=501"; }

            string BaseUrl = "https://apibay.org";
            string url = string.IsNullOrEmpty(ContentItemParsed) ?
                $"{BaseUrl}/q.php?q={searchText.Replace(" ", "+")}" :
                $"{BaseUrl}/q.php?q={searchText.Replace(" ", "+")}{ContentItemParsed}";

            try
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    // Set timeout
                    cts.CancelAfter(TimeSpan.FromSeconds(timeoutDelay));

                    // Make the request with a cancellation token
                    var response = await httpClient.GetAsync(url, cts.Token);
                    response.EnsureSuccessStatusCode();

                    var responseString = await response.Content.ReadAsStringAsync();
                    var torrents = JArray.Parse(responseString);

                    foreach (var torrent in torrents)
                    {
                        if (cancellationToken.IsCancellationRequested) return; // Check if cancellation was requested

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
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Handle cancellation or timeout
                // Console.WriteLine("Task was canceled due to timeout or user request.");
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                // Console.WriteLine($"Error scraping Pirate Bay: {ex.Message}");
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

            //return $"{size:F2} {sizeUnits[unitIndex]}";

            return string.Format(CultureInfo.InvariantCulture, "{0:F2} {1}", size, sizeUnits[unitIndex]);
        }
    }
}
