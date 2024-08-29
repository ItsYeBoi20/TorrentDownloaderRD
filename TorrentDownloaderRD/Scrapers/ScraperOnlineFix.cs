using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MediaDownloader.Scrapers
{
    internal class ScraperOnlineFix
    {
        private const string SearchUrl = "https://hydralinks.cloud/sources/onlinefix.json";

        public static async Task ScrapeTorrentsAsync(string searchText, Action<TorrentInfo> updateCallback)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(SearchUrl);
                    var responseJson = JObject.Parse(response);
                    searchText = Uri.UnescapeDataString(searchText).ToLower();
                    var searchTerms = searchText.Split(' ');

                    foreach (var result in responseJson["downloads"])
                    {
                        string title = result["title"].ToString().ToLower();

                        if (MatchesSearchTerms(title, searchTerms))
                        {
                            string magnetLink = result["uris"][0].ToString();
                            string size = result["fileSize"].ToString();
                            string name = result["title"].ToString();

                            // Store the magnet link in the dictionary
                            Main.magnetLinksOnlineFix[name] = magnetLink;

                            var torrentInfo = new TorrentInfo
                            {
                                Name = name,
                                Url = "OnlineFix",
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
            catch (Exception ex)
            {
                // Console.WriteLine($"Error scraping OnlineFix Repacks: {ex.Message}");
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
                // Console.WriteLine($"Error converting size to bytes: {ex.Message}");
                return 0;
            }
        }
    }
}
