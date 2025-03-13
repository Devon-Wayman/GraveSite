using System.Net;

namespace FinderScraper.Networking
{
    /// <summary>
    /// Sets up the network client that will be used to make varying network requests
    /// Does not contain any methods to make requests, only the client itself
    /// </summary>
    internal class FinderNetClient
    {
        static FinderNetClient? _instance;
        static readonly object _lock = new();
        public static FinderNetClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new FinderNetClient();
                    }
                }
                return _instance;
            }
        }
        public HttpClient HttpClient { get; private set; }
        readonly CookieContainer _cookieContainer;

        FinderNetClient()
        {   // initializes base http client with cookie container
            _cookieContainer = new CookieContainer();
            HttpClient = new HttpClient(new HttpClientHandler
            {
                CookieContainer = _cookieContainer
            })
            {
                BaseAddress = new Uri("https://www.findagrave.com")
            };

            HttpClient.DefaultRequestHeaders.Add("mv", "ios:1.22.3");
            HttpClient.DefaultRequestHeaders.Add("ak", "cb98d01e0ebd11e9abf812e7b2364c0a");
        }

        public CookieCollection GetCookies(Uri uri)
        {
            return _cookieContainer.GetCookies(uri);
        }


        /// <summary>
        /// Sends a GET request to the specified URL and returns the response as a string
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> GetJsonAsync(string url)
        {
            HttpResponseMessage response = await HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
