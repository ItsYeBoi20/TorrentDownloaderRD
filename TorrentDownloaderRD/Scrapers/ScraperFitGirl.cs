using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MediaDownloader.Scrapers
{
    internal class ScraperFitGirl
    {
        private const string SearchUrl = "https://hydralinks.cloud/sources/fitgirl.json";

        private static readonly HttpClient client = new HttpClient(new HttpClientHandler
        {
            MaxConnectionsPerServer = 10,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public static async Task ScrapeTorrentsAsync(string searchText, Action<TorrentInfo> updateCallback, int timeoutDelay, CancellationToken cancellationToken)
        {
            try
            {
                // Set a request-specific timeout using the cancellation token
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(timeoutDelay));
                    cancellationToken = cts.Token;

                    // Make the request
                    var response = await client.GetAsync(SearchUrl, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        return;
                    }

                    // Read the response content
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var responseJson = JObject.Parse(responseContent);
                    searchText = Uri.UnescapeDataString(searchText).ToLower();
                    var searchTerms = searchText.Split(' ');

                    foreach (var result in responseJson["downloads"])
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        string title = result["title"].ToString().ToLower();

                        if (MatchesSearchTerms(title, searchTerms))
                        {
                            string magnetLink = result["uris"][0].ToString();
                            string size = result["fileSize"].ToString();
                            string name = result["title"].ToString();

                            // Store the magnet link in the dictionary
                            Main.magnetLinksFitGirl[name] = magnetLink;

                            var torrentInfo = new TorrentInfo
                            {
                                Name = name,
                                Url = "FitGirl",
                                Size = size,
                                Seeders = 0, // Seeders are not provided
                                Leechers = 0, // Leechers are not provided
                                SizeInBytes = ConvertToBytes(size)
                            };

                            updateCallback(torrentInfo);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Handle task cancellation due to timeout or cancellation token
                // Console.WriteLine("Task was canceled.");
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"Error scraping torrents: {ex.Message}");
            }
        }


        private static bool MatchesSearchTerms(string title, string[] searchTerms)
        {
            foreach (var term in searchTerms)
            {
                if (!title.Contains(term))
                {
                    return false;
                }
            }
            return true;
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
                // Console.WriteLine($"Error converting size to bytes: {ex.Message}");
                return 0;
            }
        }
    }
}
