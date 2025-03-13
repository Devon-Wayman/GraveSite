using System.Text;

namespace FinderScraper.Networking
{
    public class Requests
    {
        /// <summary>
        /// Sends a GET request to the specified URL and returns the response as a string
        /// </summary>
        /// <param name="url">Url to send request to</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url)
        {
            HttpResponseMessage response = await FinderNetClient.Instance.HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string jsonString = await response.Content.ReadAsStringAsync();
            return HelperFunctions.PrettyPrintJson(jsonString);
        }

        /// <summary>
        /// Sends a POST request to the specified URL with the specified JSON content and returns the response as a string
        /// </summary>
        /// <param name="url">Url to send request to</param>
        /// <param name="jsonContent">JSON body data to pass in request (optional)</param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string url, string jsonContent)
        {
            HttpContent content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await FinderNetClient.Instance.HttpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            string jsonString = await response.Content.ReadAsStringAsync();
            return HelperFunctions.PrettyPrintJson(jsonString);
        }
    }
}
