using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Globalization;

namespace MediaDownloader.Scrapers
{
    internal class ScraperTorrentGalaxy
    {
        public static async Task ScrapeTorrentsAsync(string searchText, Action<TorrentInfo> updateCallback, int timeoutDelay, CancellationToken cancellationToken)
        {
            string filePath = "tgx24hdump.txt.gz";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Set the timeout for the download
                    using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(timeoutDelay));

                        // Try to download the file with the timeout and cancellation token
                        var response = await client.GetAsync("https://torrentgalaxy.to/cache/tgx24hdump.txt.gz", cts.Token);
                        response.EnsureSuccessStatusCode();

                        // Save the downloaded Gzip file to disk
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        using (var contentStream = await response.Content.ReadAsStreamAsync()) // Get the content stream
                        {
                            // Copy the content to the file stream, supporting cancellation
                            await contentStream.CopyToAsync(fileStream, 81920, cancellationToken); // Buffer size: 81920 bytes
                        }
                    }
                }

                string content = ReadGzFileContents(filePath);

                string[] lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                if (lines == null) return;

                foreach (var line in lines)
                {
                    if (cancellationToken.IsCancellationRequested) return;

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

                            updateCallback?.Invoke(torrentInfo);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Handle timeout or cancellation
                // Console.WriteLine("Task was canceled due to timeout or user request.");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                // Console.WriteLine($"Error scraping TorrentGalaxy: {ex.Message}");
            }
            finally
            {
                // Ensure the file is deleted if it exists, whether the process is successful or not
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }


        public static long ConvertToBytes(string sizeText)
        {
            try
            {
                sizeText = sizeText.ToUpper().Replace(",", "").Trim();
                double size = double.Parse(Regex.Match(sizeText, @"\d+(\.\d+)?").Value, CultureInfo.InvariantCulture);

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
