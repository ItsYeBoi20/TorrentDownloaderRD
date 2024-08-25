using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MediaDownloader.Scrapers
{
    internal class ScraperTorrentsCSV
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public static async Task ScrapeTorrentsAsync(string searchText, int numberOfResults, Action<TorrentInfo> updateCallback)
        {
            string apiUrl = $"https://torrents-csv.com/service/search?q={searchText}&size={numberOfResults}";
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync(apiUrl);
                var json = JObject.Parse(response);

                foreach (var torrent in json["torrents"])
                {
                    var name = torrent["name"].ToString();
                    var magnetLink = $"magnet:?xt=urn:btih:{torrent["infohash"]}";

                    Main.magnetLinksCSV[name] = magnetLink;

                    var torrentInfo = new TorrentInfo
                    {
                        Name = name,
                        Size = ConvertBytesToReadableSize(long.Parse(torrent["size_bytes"].ToString())).ToString(),
                        Seeders = int.Parse(torrent["seeders"].ToString()),
                        Leechers = int.Parse(torrent["leechers"].ToString()),
                        Url = $"https://torrents-csv.com/torrent/{torrent["rowid"]}",
                        Magnet = magnetLink
                    };

                    updateCallback?.Invoke(torrentInfo);
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

            return $"{size:F2} {sizeUnits[unitIndex]}";
        }
    }
}
