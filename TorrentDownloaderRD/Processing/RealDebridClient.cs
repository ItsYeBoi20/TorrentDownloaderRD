using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using static MediaDownloader.Data;

namespace RealDebridAPI
{
    public class RealDebridClient : IDisposable
    {
        private string accessAPI = "";
        private const string BaseUrl = "https://api.real-debrid.com/rest/1.0/";
        private readonly HttpClient _httpClient;

        public RealDebridClient(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));
            }
            else
            {
                accessAPI = accessToken;
            }


            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        /// Checks if the user is a premium user.
        /// </summary>
        public async Task<bool> IsPremiumUserAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("user");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    // Handle the 401 Unauthorized response specifically
                    return false;
                }

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(jsonString);

                // Check if the user type is "premium" and premium seconds are greater than 0
                return user.Type == "premium" && user.Premium > 0;
            }
            catch (HttpRequestException ex)
            {
                // Log or handle the exception as needed
                // For now, we'll assume that if there's an HttpRequestException, the API key might be invalid.
                return false;
            }
        }

        /// <summary>
        /// Adds a magnet link to Real-Debrid and returns the associated torrent ID.
        /// </summary>
        /// <param name="magnetLink">The magnet link to add.</param>
        /// <returns>The torrent ID.</returns>
        public async Task<string> AddMagnetLinkAsync(string magnetLink)
        {
            if (string.IsNullOrWhiteSpace(magnetLink))
                throw new ArgumentException("Magnet link cannot be null or empty.", nameof(magnetLink));

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("magnet", magnetLink)
            });

            var response = await _httpClient.PostAsync("torrents/addMagnet", content);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Torrent>(jsonString);

            return result.Id;
        }

        /// <summary>
        /// Retrieves information about a specific torrent, including file details.
        /// </summary>
        /// <param name="torrentId">The ID of the torrent.</param>
        /// <returns>A TorrentInfo object containing details about the torrent.</returns>
        public async Task<TorrentInfo> GetTorrentInfoAsync(string torrentId)
        {
            if (string.IsNullOrWhiteSpace(torrentId))
                throw new ArgumentException("Torrent ID cannot be null or empty.", nameof(torrentId));

            var response = await _httpClient.GetAsync($"torrents/info/{torrentId}");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<TorrentInfo>(jsonString);

            return result;
        }

        /// <summary>
        /// Selects specific files within a torrent for download.
        /// </summary>
        /// <param name="torrentId">The ID of the torrent.</param>
        /// <param name="fileIds">An array of file IDs to select.</param>
        /// <returns>True if the operation was successful.</returns>
        public async Task<bool> SelectFilesAsync(string torrentId, string[] fileIds)
        {
            if (string.IsNullOrWhiteSpace(torrentId))
                throw new ArgumentException("Torrent ID cannot be null or empty.", nameof(torrentId));

            if (fileIds == null || fileIds.Length == 0)
                throw new ArgumentException("File IDs array cannot be null or empty.", nameof(fileIds));

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("files", string.Join(",", fileIds))
            });

            var response = await _httpClient.PostAsync($"torrents/selectFiles/{torrentId}", content);
            response.EnsureSuccessStatusCode();

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieves the list of downloadable links for the selected files in a torrent.
        /// Note: This method waits until the torrent is ready.
        /// </summary>
        /// <param name="torrentId">The ID of the torrent.</param>
        /// <param name="pollIntervalSeconds">Interval in seconds to poll the torrent status. Default is 5 seconds.</param>
        /// <param name="timeoutMinutes">Maximum time in minutes to wait for the torrent to be ready. Default is 10 minutes.</param>
        /// <returns>A list of direct download links.</returns>
        public async Task<List<DownloadLinkInfo>> GetDownloadLinksAsync(string torrentId, string passedTorrentID)
        {
            if (string.IsNullOrWhiteSpace(torrentId))
                throw new ArgumentException("Torrent ID cannot be null or empty.", nameof(torrentId));

            var downloadLinks = new List<DownloadLinkInfo>();

            // Fetch torrent information
            var torrentInfo = await GetTorrentInfoAsync(torrentId);
            ///Console.WriteLine($"Torrent status: {torrentInfo.Status}");

            // Only process if the torrent has been downloaded
            if (torrentInfo.Status == "downloaded")
            {
                // Track used links to avoid duplicates
                var usedLinks = new HashSet<string>();

                Dictionary<string, string> backupDownloadLinks = null; // Dictionary to store file path and link

                string latestTorrentId = "";
                if (passedTorrentID == null)
                {
                    latestTorrentId = await GetLatestTorrentIdAsync();
                }
                else
                {
                    latestTorrentId = passedTorrentID;  //added
                }
                // string latestTorrentId = await GetLatestTorrentIdAsync();
                backupDownloadLinks = await GetDownloadLinksFromIDAsync(latestTorrentId);

                // Iterate over the torrent files
                foreach (var file in torrentInfo.Files)
                {
                    if (file.Selected)
                    {
                        //Console.WriteLine($"Warning: File {file.Path} has an empty or null link. Trying alternative method.");

                        // Fetch the latest torrent ID and download links with file names
                        if (latestTorrentId != null)
                        {
                            if (backupDownloadLinks != null)
                            {
                                foreach (var kvp in backupDownloadLinks)
                                {
                                    ///Console.WriteLine($"File: {kvp.Key}, Download Link: {kvp.Value}");

                                    // Use the same logic for adding the backup link to the downloadLinks list
                                    if (!string.IsNullOrWhiteSpace(kvp.Value) && !usedLinks.Contains(kvp.Value))
                                    {
                                        downloadLinks.Add(new DownloadLinkInfo
                                        {
                                            FileId = null,  // Assuming the file ID isn't available here, but you can adjust this if needed
                                            FileName = kvp.Key,
                                            DownloadLink = kvp.Value
                                        });
                                        usedLinks.Add(kvp.Value); // Mark this link as used
                                    }
                                    else
                                    {
                                        ///Console.WriteLine($"Duplicate or invalid backup link found for file {kvp.Key}. Skipping.");
                                    }
                                }
                            }
                            else
                            {
                                //Console.WriteLine("No download links found.");
                            }
                        }
                        else
                        {
                            //Console.WriteLine("No torrent ID found.");
                        }
                    }
                }

                // Return the download links, even if some were missed (you may decide to change this behavior)
                return downloadLinks;
            }

            // If no valid links are found, throw an exception
            throw new InvalidOperationException("Failed to retrieve download links. The torrent might not be fully downloaded or processed, or no files were selected.");
        }

        /// <summary>
        /// Retrieves download links for a given torrent ID from the Real-Debrid API.
        /// Note: The files can be sorted by episode number, and only selected files are considered.
        /// </summary>
        /// <param name="ID">The ID of the torrent for which to retrieve download links.</param>
        /// <returns>A dictionary with file paths as keys and their corresponding download links as values.</returns>
        public async Task<Dictionary<string, string>> GetDownloadLinksFromIDAsync(string ID)
        {
            string apiUrl = "https://api.real-debrid.com/rest/1.0/torrents/info/" + ID;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessAPI);

                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var torrentInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TorrentInfo>(responseBody);

                        var downloadLinks = new Dictionary<string, string>();

                        // Sort the files by episode number {OPTIONAL}
                        var sortedFiles = torrentInfo.Files.OrderBy(file => ExtractEpisodeNumber(file.Path)).ToList();

                        int linkIndex = 0;
                        foreach (var file in torrentInfo.Files) // Use sortedFiles if you want to sort by episode number
                        {
                            if (file.Selected && linkIndex < torrentInfo.Links.Length)
                            {
                                downloadLinks[file.Path] = torrentInfo.Links[linkIndex];
                                linkIndex++;
                            }
                        }

                        // Helper method to extract episode number from file path
                        int ExtractEpisodeNumber(string filePath)
                        {
                            var match = Regex.Match(filePath, @"S\d+E(\d+)");
                            return match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
                        }

                        return downloadLinks;
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error Code: {response.StatusCode} {errorResponse}");

                        //Console.WriteLine($"Error: Unable to retrieve torrents. Status Code: {response.StatusCode}");
                        //Console.WriteLine($"Error Details: {errorResponse}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);

                    //Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            return null; // If there's an error or no download links are found
        }

        /// <summary>
        /// Retrieves the latest torrent ID added from the Real-Debrid API.
        /// Note: Assumes the latest torrent is the first one returned in the list.
        /// </summary>
        /// <returns>The ID of the latest torrent or null if none are found.</returns>
        public async Task<string> GetLatestTorrentIdAsync()
        {
            string apiUrl = "https://api.real-debrid.com/rest/1.0/torrents";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessAPI);

                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var torrents = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Torrent>>(responseBody);

                        // Assuming the latest torrent is the first one in the list
                        var latestTorrent = torrents?.FirstOrDefault();
                        if (latestTorrent != null)
                        {
                            return latestTorrent.Id;
                        }
                    }
                    else
                    {

                        string errorResponse = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error Code: {response.StatusCode} {errorResponse}");

                        //Console.WriteLine($"Error: Unable to retrieve torrents. Status Code: {response.StatusCode}");
                        //Console.WriteLine($"Error Details: {errorResponse}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);

                    //Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            return null; // If there's an error or no torrents are found
        }

        /// <summary>
        /// Retrieves all torrent IDs added from the Real-Debrid API.
        /// </summary>
        /// <returns>A List of IDs from all torrent or null if none are found.</returns>
        public async Task<List<Torrent>> GetAllTorrentIdAsync()
        {
            string apiUrl = "https://api.real-debrid.com/rest/1.0/torrents";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessAPI);

                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var torrents = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Torrent>>(responseBody);

                        return torrents; // Return the list of torrents
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error Code: {response.StatusCode} {errorResponse}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception: " + ex.Message);
                }
            }

            return null; // If there's an error or no torrents are found
        }

        /// <summary>
        /// Deletes a torrent from Real-Debrid by its ID.
        /// Note: This operation is irreversible.
        /// </summary>
        /// <param name="torrentId">The ID of the torrent to delete.</param>
        /// <exception cref="Exception">Thrown if the deletion fails.</exception>
        public async Task DeleteTorrentAsync(string torrentId)
        {
            var requestUri = $"https://api.real-debrid.com/rest/1.0/torrents/delete/{torrentId}";
            var response = await _httpClient.DeleteAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to delete torrent: {response.ReasonPhrase}");
            }
        }

        /// <summary>
        /// Unrestricts a given link and returns a direct downloadable link.
        /// </summary>
        /// <param name="link">The link to unrestrict.</param>
        /// <returns>A direct download link.</returns>
        public async Task<string> UnrestrictLinkAsync(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
                throw new ArgumentException("Link cannot be null or empty.", nameof(link));

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("link", link)
            });

            var response = await _httpClient.PostAsync("unrestrict/link", content);

            // Log detailed information in case of failure
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Error during unrestricting link: {response.StatusCode} - {errorContent}");
                //Console.WriteLine($"Error during unrestricting link: {response.StatusCode} - {errorContent}");
                throw new Exception($"Failed to unrestrict link. Status Code: {response.StatusCode}, Response: {errorContent}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UnrestrictLinkResponse>(jsonString);

            return result.Download;
        }

        /// <summary>
        /// Unrestricts a given link and returns a direct downloadable link.
        /// </summary>
        /// <param name="link">The link to unrestrict.</param>
        /// <returns>A direct download link.</returns>
        public async Task<string> GetFileName(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
                throw new ArgumentException("Link cannot be null or empty.", nameof(link));

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("link", link)
            });

            var response = await _httpClient.PostAsync("unrestrict/link", content);

            // Log detailed information in case of failure
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                MessageBox.Show($"Error during unrestricting link: {response.StatusCode} - {errorContent}");
                //Console.WriteLine($"Error during unrestricting link: {response.StatusCode} - {errorContent}");
                throw new Exception($"Failed to unrestrict link. Status Code: {response.StatusCode}, Response: {errorContent}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<UnrestrictLinkResponse>(jsonString);

            return result.FileName;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #region Data Models

        public class Torrent
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("filename")]
            public string Filename { get; set; }
        }

        private class AddMagnetResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public class TorrentInfo
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("filename")]
            public string FileName { get; set; }

            [JsonProperty("hash")]
            public string Hash { get; set; }

            [JsonProperty("bytes")]
            public long Bytes { get; set; }

            [JsonProperty("original_bytes")]
            public long OriginalBytes { get; set; }

            [JsonProperty("host")]
            public string Host { get; set; }

            [JsonProperty("split")]
            public int Split { get; set; }

            [JsonProperty("progress")]
            public double Progress { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("added")]
            public string Added { get; set; }

            [JsonProperty("files")]
            public TorrentFile[] Files { get; set; }

            [JsonProperty("links")]
            public string[] Links { get; set; }

            [JsonProperty("ended")]
            public string Ended { get; set; }

            [JsonProperty("speed")]
            public long Speed { get; set; }

            [JsonProperty("seeders")]
            public int Seeders { get; set; }

            [JsonProperty("comments")]
            public string Comments { get; set; }

            [JsonProperty("private")]
            public int Private { get; set; }

            [JsonProperty("folder")]
            public bool Folder { get; set; }

            public List<string> backupLinks { get; set; }
        }

        public class TorrentFile
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("bytes")]
            public long Bytes { get; set; }

            [JsonProperty("selected")]
            public bool Selected { get; set; }

            [JsonProperty("link")]
            public string Link { get; set; }
        }

        private class UnrestrictLinkResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("filename")]
            public string FileName { get; set; }

            [JsonProperty("mimeType")]
            public string MimeType { get; set; }

            [JsonProperty("filesize")]
            public long FileSize { get; set; }

            [JsonProperty("link")]
            public string Link { get; set; }

            [JsonProperty("host")]
            public string Host { get; set; }

            [JsonProperty("chunks")]
            public int Chunks { get; set; }

            [JsonProperty("crc")]
            public string Crc { get; set; }

            [JsonProperty("download")]
            public string Download { get; set; }

            [JsonProperty("streamable")]
            public int Streamable { get; set; }

            [JsonProperty("generated")]
            public int Generated { get; set; }
        }

        public class DownloadLinkInfo
        {
            public string FileId { get; set; }
            public string FileName { get; set; }
            public string DownloadLink { get; set; }
        }

        public class User
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public int Points { get; set; }
            public string Locale { get; set; }
            public string Avatar { get; set; }
            public string Type { get; set; }
            public int Premium { get; set; }
            public string Expiration { get; set; }
        }

        #endregion
    }
}
