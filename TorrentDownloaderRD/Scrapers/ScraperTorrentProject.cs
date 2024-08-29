using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MediaDownloader.Scrapers
{
    internal class ScraperTorrentProject
    {
        public static async Task ScrapeTorrentsAsync(string searchText, int websiteSearches, Action<TorrentInfo> updateCallback)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < websiteSearches; i++)
            {
                string url = $"https://torrentproject.cc/?t={searchText.Replace(" ", "+")}&p={i}";
                tasks.Add(ProcessPageAsync(url, updateCallback));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task ProcessPageAsync(string url, Action<TorrentInfo> updateCallback)
        {
            var htmlDoc = await new HtmlWeb().LoadFromWebAsync(url);
            var rows = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"similarfiles\"]/div");

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
            var urlNode = row.SelectSingleNode(".//span[1]/a");
            var seedersNode = row.SelectSingleNode(".//span[2]");
            var leechersNode = row.SelectSingleNode(".//span[3]");
            var sizeNode = row.SelectSingleNode(".//span[5]");

            if (urlNode == null || seedersNode == null || leechersNode == null || sizeNode == null)
            {
                return null;
            }

            string sizeText = sizeNode.InnerText.Trim();
            var sizeSpanNode = sizeNode.SelectSingleNode("span");

            if (sizeSpanNode != null)
            {
                sizeNode.RemoveChild(sizeSpanNode);
                sizeText = sizeNode.InnerText.Trim();
            }

            int seeders = 0;
            int leechers = 0;

            if (!int.TryParse(seedersNode.InnerText.Trim(), out seeders))
            {
                seeders = 0;
            }

            if (!int.TryParse(leechersNode.InnerText.Trim(), out leechers))
            {
                leechers = 0;
            }

            return new TorrentInfo
            {
                Url = "https://torrentproject.cc" + urlNode.GetAttributeValue("href", ""),
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

        public async Task<string> DownloadTorrentAsync(string info)
        {
            var html = await RetrieveUrlAsync(info);
            var match = Regex.Match(html, "href=['\"].*?(magnet.+?)['\"]");
            if (match.Success && match.Groups.Count > 0)
            {
                var magnet = Uri.UnescapeDataString(match.Groups[1].Value);
                return magnet;
            }
            return null;
        }


        private async Task<string> RetrieveUrlAsync(string url)
        {
            using (var client = new HttpClient())
            {
                int retries = 3;
                int delay = 1000;

                for (int i = 0; i < retries; i++)
                {
                    try
                    {
                        var response = await client.GetStringAsync(url);
                        return response;
                    }
                    catch (Exception ex)
                    {
                        if (i == retries - 1)
                        {
                            throw;
                        }
                        await Task.Delay(delay);
                    }
                }
            }
            return null;
        }


        public string DownloadTorrent(string info)
        {
            var html = RetrieveUrl(info);
            var match = Regex.Match(html, "href=['\"].*?(magnet.+?)['\"]");
            if (match.Success && match.Groups.Count > 0)
            {
                var magnet = Uri.UnescapeDataString(match.Groups[1].Value);
                return magnet;
            }
            return null;
        }

        private string RetrieveUrl(string url)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetStringAsync(url).Result;
                return response;
            }
        }
    }
}
