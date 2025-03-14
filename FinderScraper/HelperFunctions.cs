using FinderScraper.Networking;
using FinderScraper.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Buffers;
using System.Text.Json;

namespace FinderScraper
{
    public class HelperFunctions
    {
        #region Common FindAGrave API Requests
        /// <summary>
        /// Returns all memorial info from a given cemetery ID
        /// </summary>
        /// <param name="cemeteryId">cemetery ID to scrape</param>
        /// <param name="totalMemorials">total number of memorials. required to generate request links</param>
        /// <returns></returns>
        public static async Task<List<Memorial>> GetAllMemorials(int cemeteryId, int totalMemorials)
        {
            Console.WriteLine($"Beginning memorial retrival. Total memorials: {totalMemorials}");
            List<string> urls = new();
            int currentSkip = 0;
            while (currentSkip < totalMemorials)
            {
                urls.Add($"https://www.findagrave.com/memorial/search?cemeteryId={cemeteryId}&orderby=r&skip={currentSkip}&limit=100");
                currentSkip += 100;
            }

            Console.WriteLine($"Total URLs: {urls.Count}");
            List<Memorial> collectedMemorialsList = new();
            using (SemaphoreSlim semaphore = new(3)) // Limit to 3 concurrent threads
            {
                // Create a list of tasks with limited concurrency
                IEnumerable<Task> tasks = urls.Select(async url =>
                {
                    await semaphore.WaitAsync();
                    byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(8192); // Rent a buffer from the pool
                    try
                    {
                        string response = await FinderNetClient.Instance.HttpClient.GetStringAsync(url).ConfigureAwait(false);
                        JObject jsonResponse = JObject.Parse(response);

                        // Check and add "memorials" to the list
                        if (jsonResponse.TryGetValue("memorials", out JToken? memorialsToken) && memorialsToken is JArray memorialsArray)
                        {
                            lock (collectedMemorialsList)
                            {
                                foreach (JObject memorial in memorialsArray.Cast<JObject>())
                                {
                                    collectedMemorialsList.Add(new Memorial
                                    {
                                        MemorialId = memorial["memorialId"]?.ToObject<int>() ?? 0,
                                        IsFamous = memorial["isFamous"]?.ToObject<bool>() ?? false,
                                        IsVeteran = memorial["isVeteran"]?.ToObject<bool>() ?? false,
                                        FirstName = memorial["firstName"]?.ToString(),
                                        MiddleName = memorial["middleName"]?.ToString(),
                                        LastName = memorial["lastName"]?.ToString(),
                                        FullName = memorial["fullName"]?.ToString(),
                                        BirthCityName = memorial["birthCityName"]?.ToString(),
                                        DeathCityName = memorial["deathCityName"]?.ToString(),
                                        BirthDate = memorial["birthDate"]?.ToString(),
                                        DeathDate = memorial["deathDate"]?.ToString(),
                                        Parents = memorial["Parents"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                                        Spouses = memorial["Spouses"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                                        Siblings = memorial["Siblings"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                                        Children = memorial["Children"]?.ToObject<string[]>() ?? Array.Empty<string>(),
                                        Location = memorial["location"]?.ToObject<float[]>() ?? Array.Empty<float>(),
                                        BirthYear = memorial["birthYear"]?.ToObject<int>() ?? 0,
                                        DeathYear = memorial["deathYear"]?.ToObject<int>() ?? 0,
                                        ApproxAge = memorial["deathYear"]?.ToObject<int>() - memorial["birthYear"]?.ToObject<int>() ?? 0,
                                        GoogleMapsLink = $"https://www.google.com/maps/place/{memorial["location"]?[1]},{memorial["location"]?[0]}",
                                    });
                                }
                            }
                        }
                        Thread.Sleep(1000);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(rentedBuffer); // Return the buffer to the pool
                        semaphore.Release();
                    }
                });
                await Task.WhenAll(tasks);
            }

            return collectedMemorialsList;
        }

        /// <summary>
        /// Find the total number of memorials in a JSON response from the FindAGrave API
        /// </summary>
        /// <param name="jsonContent">Raw JSON body to parse and search</param>
        /// <returns></returns>
        public static Cemetery GetCemeteryInfo(string jsonContent)
        {
            // TODO: Change this to take the full cemetery reply and make a Cemetery object. We will then be able to access the total memorials from the object.

            JToken? parsedJson = JsonConvert.DeserializeObject<JToken>(jsonContent);
            Cemetery cemetery = new Cemetery
            {
                CemeteryName = FindValueByKey(parsedJson!, "cemeteryName") ?? string.Empty,
                CemeteryId = FindValueByKey(parsedJson!, "id") ?? string.Empty,
                MemorialCount = int.Parse(FindValueByKey(parsedJson!, "memorialCount") ?? "0"),
                PhotoRequestCount = int.Parse(FindValueByKey(parsedJson!, "photoRequestCount") ?? "0"),
                PhotographedCount = int.Parse(FindValueByKey(parsedJson!, "photographedCount") ?? "0"),
                FamousCount = int.Parse(FindValueByKey(parsedJson!, "famousCount") ?? "0"),
                GpsCount = int.Parse(FindValueByKey(parsedJson!, "gpsCount") ?? "0"),
            };

            // set the gps percentage if the gps count is not 0
            if (cemetery.GpsCount != 0 || cemetery.MemorialCount != 0)
            {
                cemetery.PercentageWithGps = (int)Math.Round(cemetery.GpsCount * 100.0 / cemetery.MemorialCount);
            }
            return cemetery;
        }
        #endregion

        #region JSON Helpers
        /// <summary>
        /// Locate a value in a JSON object by key. This method is recursive and should 
        /// only be called by a public function within this class.
        /// </summary>
        /// <param name="token">token value to search for in the parsed json string</param>
        /// <param name="key">key to look for within json string</param>
        /// <returns></returns>
        static string? FindValueByKey(JToken token, string key)
        {
            // Check if the current token is an object
            if (token is JObject obj)
            {
                foreach (JProperty property in obj.Properties())
                {
                    if (property.Name == key)
                    {
                        return property.Value.ToString();
                    }

                    // Recursive call to check nested objects
                    string? value = FindValueByKey(property.Value, key);

                    if (value != null)
                    {
                        return value;
                    }
                }
            }
            // Check if the current token is an array
            else if (token is JArray array)
            {
                foreach (var item in array)
                {
                    string? value = FindValueByKey(item, key);
                    if (value != null)
                    {
                        return value;
                    }
                }
            }

            return null;
        }

        static readonly JsonSerializerOptions CachedJsonSerializerOptions = new()
        {
            WriteIndented = true
        };
        public static string PrettyPrintJson(string jsonString)
        {
            JsonElement jsonElement = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(jsonString);
            return System.Text.Json.JsonSerializer.Serialize(jsonElement, CachedJsonSerializerOptions);
        }
        #endregion
    }
}
