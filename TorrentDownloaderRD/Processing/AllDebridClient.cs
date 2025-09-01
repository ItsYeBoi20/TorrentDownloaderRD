using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static RealDebridAPI.RealDebridClient;

namespace TorrentDownloaderRD.Processing
{
    internal class AllDebridClient
    {
        private string accessAPI = "";
        private const string BaseUrl = "https://api.alldebrid.com/v4.1/";
        private readonly HttpClient _httpClient;

        public AllDebridClient(string accessToken)
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
                    return false; // API key is invalid
                }

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonString);

                if (apiResponse?.Status == "success" && apiResponse.Data?.User != null)
                {
                    var user = apiResponse.Data.User;
                    long premiumUntilUnix;

                    if (!long.TryParse(user.PremiumUntil, out premiumUntilUnix))
                    {
                        return false;
                    }

                    return user.IsPremium && premiumUntilUnix > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }

                return false;
            }
            catch (HttpRequestException)
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves all saved torrent links (magnets).
        /// </summary>
        public async Task<List<Magnet>> GetAllTorrentsAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("magnet/status", new FormUrlEncodedContent(new Dictionary<string, string>()));
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<MagnetStatusResponse>(jsonString);

                if (apiResponse?.Status == "success" && apiResponse.Data?.Magnets != null)
                {
                    return apiResponse.Data.Magnets;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching torrents: {ex.Message}");
            }

            return new List<Magnet>();
        }

        /// <summary>
        /// Retrieves the status of a specific magnet by its ID.
        /// </summary>
        /// <param name="magnetId">The ID of the magnet to retrieve status for.</param>
        /// <returns>A task that represents the asynchronous operation. Returns a Magnet object if found, otherwise null.</returns>
        public async Task<Magnet> GetStatusByIDAsync(long magnetId)
        {
            var parameters = new Dictionary<string, string>
            {
                { "id", magnetId.ToString() }
            };

            try
            {
                var response = await _httpClient.PostAsync("magnet/status", new FormUrlEncodedContent(parameters));
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<MagnetStatusResponse>(jsonString);

                if (apiResponse?.Status == "success" && apiResponse.Data?.Magnets != null && apiResponse.Data.Magnets.Count > 0)
                {
                    return apiResponse.Data.Magnets.FirstOrDefault(m => m.Id == magnetId);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error getting magnet status: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Retrieves file information for the given list of magnet IDs.
        /// </summary>
        public async Task<List<MagnetFileResult>> GetMagnetFilesAsync(List<long> magnetIds)
        {
            var parameters = new Dictionary<string, string>();
            foreach (var id in magnetIds)
            {
                parameters.Add("id[]", id.ToString());
            }

            try
            {
                var response = await _httpClient.PostAsync("magnet/files", new FormUrlEncodedContent(parameters));
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<MagnetFilesResponse>(jsonString);
                return result?.Data?.Magnets;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching magnet files: {ex.Message}");
            }

            return new List<MagnetFileResult>();
        }

        public async Task<List<(string Name, string Link, long Size)>> GetDownloadableFilesAsync(long magnetId)
        {
            var result = new List<(string Name, string Link, long Size)>();
            var response = await GetMagnetFilesAsync(new List<long> { magnetId });

            if (response == null || response.Count == 0) return result;

            foreach (var magnet in response)
            {
                if (magnet.Files != null)
                {
                    TraverseFiles(magnet.Files, result);
                }
            }

            return result;
        }

        private void TraverseFiles(List<FileEntry> entries, List<(string Name, string Link, long Size)> result)
        {
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.L) && entry.S.HasValue)
                {
                    result.Add((entry.N, entry.L, entry.S.Value));
                }

                if (entry.E != null && entry.E.Count > 0)
                {
                    TraverseFiles(entry.E, result);
                }
            }
        }

        public async Task<UploadMagnetResult> UploadMagnetAsync(string magnet)
        {
            var parameters = new Dictionary<string, string>
            {
                { "magnets[]", magnet }
            };

            try
            {
                var response = await _httpClient.PostAsync("magnet/upload", new FormUrlEncodedContent(parameters));
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<UploadMagnetsResponse>(jsonString);

                if (apiResponse != null
                    && apiResponse.Status == "success"
                    && apiResponse.Data != null
                    && apiResponse.Data.Magnets != null
                    && apiResponse.Data.Magnets.Count > 0)
                {
                    return apiResponse.Data.Magnets[0];
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error uploading magnet: {ex.Message}");
            }

            return null;
        }

        public async Task<bool> DeleteMagnetAsync(long magnetId)
        {
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "id", magnetId.ToString() }
                };

                var response = await _httpClient.PostAsync("magnet/delete", new FormUrlEncodedContent(parameters));
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(jsonString);

                return apiResponse?.Status == "success";
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error deleting magnet: {ex.Message}");
                return false;
            }
        }
        
        public async Task<LinkUnlockResult> UnlockLinkAsync(string link, string password = null)
        {
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "link", link },
                    { "password", password ?? string.Empty }
                };

                var response = await _httpClient.PostAsync("link/unlock", new FormUrlEncodedContent(parameters));
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<LinkUnlockResponse>(jsonString);

                if (apiResponse?.Status == "success" && apiResponse.Data != null)
                {
                    return new LinkUnlockResult
                    {
                        Link = apiResponse.Data.Link,
                        Host = apiResponse.Data.Host,
                        Filename = apiResponse.Data.Filename,
                        Filesize = apiResponse.Data.Filesize,
                        Id = apiResponse.Data.Id,
                        HostDomain = apiResponse.Data.HostDomain
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error unlocking link: {ex.Message}");
            }

            return null;
        }

        public class SingleOrArrayConverter<T> : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(List<T>));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JToken token = JToken.Load(reader);
                if (token is JArray)
                {
                    return token.ToObject<List<T>>();
                }

                return new List<T> { token.ToObject<T>() };
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        public class ApiResponse
        {
            public string Status { get; set; }
            public DataWrapper Data { get; set; }
        }

        public class DataWrapper
        {
            public User User { get; set; }
            public Message Message { get; set; }
        }
        public class Message
        {
            public string MessageText { get; set; }
        }

        public class User
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public bool IsPremium { get; set; }
            public bool IsTrial { get; set; }
            public string PremiumUntil { get; set; }
        }

        public class MagnetStatusResponse
        {
            public string Status { get; set; }
            public MagnetData Data { get; set; }
        }

        public class MagnetData
        {
            [JsonProperty("magnets")]
            [JsonConverter(typeof(SingleOrArrayConverter<Magnet>))]
            public List<Magnet> Magnets { get; set; }
        }

        public class Magnet
        {
            public long Id { get; set; }
            public string Filename { get; set; }
            public long Size { get; set; }
            public string Status { get; set; }
            public int StatusCode { get; set; }
            public long? Downloaded { get; set; }
            public long? Uploaded { get; set; }
            public int? Seeders { get; set; }
            public long? DownloadSpeed { get; set; }
            public long? UploadSpeed { get; set; }
            public long UploadDate { get; set; }
            public long? CompletionDate { get; set; }
        }

        public class MagnetFilesResponse
        {
            public string Status { get; set; }
            public MagnetFilesData Data { get; set; }
        }

        public class MagnetFilesData
        {
            public List<MagnetFileResult> Magnets { get; set; }
        }

        public class MagnetFileResult
        {
            public string Id { get; set; }
            public List<FileEntry> Files { get; set; }
            public ErrorInfo Error { get; set; }
        }

        public class FileEntry
        {
            public string N { get; set; }
            public long? S { get; set; }
            public string L { get; set; }
            public List<FileEntry> E { get; set; }
        }

        public class ErrorInfo
        {
            public string Code { get; set; }
            public string Message { get; set; }
        }

        public class UploadMagnetsResponse
        {
            public string Status { get; set; }
            public UploadMagnetsData Data { get; set; }
        }

        public class UploadMagnetsData
        {
            public List<UploadMagnetResult> Magnets { get; set; }
        }

        public class UploadMagnetResult
        {
            public string Magnet { get; set; }
            public string Hash { get; set; }
            public string Name { get; set; }
            public long? Size { get; set; }
            public bool? Ready { get; set; }
            public long? Id { get; set; }
            public ErrorInfo Error { get; set; }
        }

        public class LinkUnlockResponse
        {
            public string Status { get; set; }
            public LinkUnlockData Data { get; set; }
        }

        public class LinkUnlockData
        {
            public string Link { get; set; }
            public string Host { get; set; }
            public string Filename { get; set; }
            public long? Filesize { get; set; }
            public string Id { get; set; }
            public string HostDomain { get; set; }
        }

        public class LinkUnlockResult
        {
            public string Link { get; set; }
            public string Host { get; set; }
            public string Filename { get; set; }
            public long? Filesize { get; set; }
            public string Id { get; set; }
            public string HostDomain { get; set; }
        }
    }
}