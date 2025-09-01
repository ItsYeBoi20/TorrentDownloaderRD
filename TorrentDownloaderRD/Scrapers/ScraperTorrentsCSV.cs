using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MediaDownloader.Scrapers
{
    internal class ScraperTorrentsCSV
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task ScrapeTorrentsAsync(string searchText, int numberOfResults, Action<TorrentInfo> updateCallback, int timeoutDelay, CancellationToken cancellationToken)
        {
            string apiUrl = $"https://torrents-csv.com/service/search?q={searchText}&size={numberOfResults}";

            try
            {
                var response = await LoadFromWebWithTimeoutAsync(apiUrl, TimeSpan.FromSeconds(timeoutDelay), cancellationToken);

                if (response == null)
                {
                    return;
                }

                var json = JObject.Parse(response);

                foreach (var torrent in json["torrents"])
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var torrentInfo = ExtractTorrentInfoFromJson(torrent);

                    Main.magnetLinksCSV[torrentInfo.Name] = torrentInfo.Magnet;

                    updateCallback?.Invoke(torrentInfo);
                }
            }
            catch (TaskCanceledException)
            {
                // Handle task cancellation due to timeout or cancellation token
                // Console.WriteLine("Task was canceled.");
            }
            catch (Exception ex)
            {
                // Handle other errors
                // Console.WriteLine($"Error scraping Torrents CSV: {ex.Message}");
            }
        }

        private static async Task<string> LoadFromWebWithTimeoutAsync(string url, TimeSpan timeout, CancellationToken cancellationToken)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                cts.CancelAfter(timeout);
                try
                {
                    var response = await httpClient.GetAsync(url, cts.Token);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    return null;
                }
                catch (OperationCanceledException)
                {
                    return null;
                }
            }
        }

        private static TorrentInfo ExtractTorrentInfoFromJson(JToken torrent)
        {
            var name = torrent["name"]?.ToString();
            var sizeBytes = torrent["size_bytes"]?.ToObject<long>() ?? 0;
            var seeders = torrent["seeders"]?.ToObject<int>() ?? 0;
            var leechers = torrent["leechers"]?.ToObject<int>() ?? 0;
            var infohash = torrent["infohash"]?.ToString();

            string magnetUrl = $"magnet:?xt=urn:btih:{infohash}&dn={Uri.EscapeDataString(name)}";

            return new TorrentInfo
            {
                Name = name,
                Size = ConvertBytesToReadableSize(sizeBytes),
                SizeInBytes = sizeBytes,
                Seeders = seeders,
                Leechers = leechers,
                Url = $"https://torrents-csv.com/torrent/{infohash}",
                Magnet = magnetUrl
            };
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
