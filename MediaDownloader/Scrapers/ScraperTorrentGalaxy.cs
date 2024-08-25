using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;

namespace MediaDownloader.Scrapers
{
    internal class ScraperTorrentGalaxy
    {
        public static async Task ScrapeTorrentsAsync(string searchText, Action<TorrentInfo> updateCallback)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync("https://torrentgalaxy.to/cache/tgx24hdump.txt.gz");
                    response.EnsureSuccessStatusCode();

                    using (var fileStream = new FileStream("tgx24hdump.txt.gz", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fileStream);
                    }
                }

                string content = ReadGzFileContents("tgx24hdump.txt.gz");

                string[] lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    if (line.ToLower().Contains(searchText.ToLower()))
                    {
                        string[] parts = line.Split('|');
                        if (parts.Length >= 5)
                        {
                            string name = parts[1];
                            string category = parts[2];
                            string torrentUrl = parts[3];
                            string downloadUrl = parts[4];

                            string magnetLink = "magnet:?xt=urn:btih:" + parts[0];
                            Main.magnetLinksGalaxy[name] = magnetLink;

                            var torrentInfo = new TorrentInfo
                            {
                                Name = name,
                                Url = torrentUrl,
                                Size = "0 KB", // Size is not provided in the file
                                Seeders = 0, // Seeders count is not provided in the file
                                Leechers = 0, // Leechers count is not provided in the file
                                SizeInBytes = 0 // Size in bytes is not provided in the file
                            };

                            updateCallback(torrentInfo);
                        }
                    }
                }

                File.Delete("tgx24hdump.txt.gz");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Error scraping TorrentGalaxy: {ex.Message}");
            }
        }


        public static long ConvertToBytes(string sizeText)
        {
            try
            {
                sizeText = sizeText.ToUpper().Replace(",", "").Trim();
                double size = double.Parse(Regex.Match(sizeText, @"\d+(\.\d+)?").Value);

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
                //Console.WriteLine($"Error converting size to bytes: {ex.Message}");
                return 0;
            }
        }

        public static string ReadGzFileContents(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
            using (StreamReader reader = new StreamReader(gzipStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
