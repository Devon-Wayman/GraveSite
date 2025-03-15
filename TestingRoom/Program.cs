using FinderScraper.Objects;
using ShellProgressBar;
using CommonItems = FinderScraper.Commons;

namespace TestingRoom
{
    internal class Program
    {
        static string cemeteryName = string.Empty;
        //static int totalMemorials = 0;
        static int cemeteryId = 0;

        static void Main(string[] args)
        {
            #region Handle Arguments
            if (args.Length == 0)
            {
                Console.WriteLine("No URL provided.");
                return;
            }

            string url = args[0];
            string[] segments = url.Split('/');

            if (segments.Length < 2)
            {
                Console.WriteLine("Invalid URL format.");
                return;
            }

            cemeteryName = segments[^1];
            cemeteryId = int.Parse(segments[^2]);

            Console.WriteLine($"Cemetery Name: {cemeteryName}");
            Console.WriteLine($"Cemetery ID: {cemeteryId}");
            #endregion

            Cemetery tempCem = GetCemeteryInfo().GetAwaiter().GetResult();

            Console.WriteLine($"Memorial Count: {tempCem.MemorialCount}");

            Console.WriteLine("Retrieving all memorial data...");

            // TODO: Implement ShellProgressBar to capture update messages and progress to update the user
            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                BackgroundColor = ConsoleColor.DarkGray,
                ProgressCharacter = '─',
                DisplayTimeInRealTime = true
            };

            // Initialize the total memorials list
            List<Memorial> memorials = new();

            int memorialsProcessed = 0;
            using (var progressBar = new ProgressBar(tempCem.MemorialCount, "Retrieving memorials...", options))
            {
                FinderScraper.HelperFunctions.MemorialProcessed += memorial =>
                {
                    memorialsProcessed++;
                    progressBar.Tick(memorialsProcessed, $"Processing memorial {memorialsProcessed} of {tempCem.MemorialCount}...");
                };

                memorials = RetrieveAllMemorials(tempCem.MemorialCount).GetAwaiter().GetResult();
            }

            // all original
            // List<Memorial> memorials = RetrieveAllMemorials(tempCem.MemorialCount).GetAwaiter().GetResult();
            Console.WriteLine("SUCCESS");

            Console.WriteLine("Writing data to xlsx file...");
            FinderScraper.Reporting.ReportsManager.InitReports(memorials, cemeteryName);
            Console.WriteLine("SUCCESS");

            Console.WriteLine("Press Enter to exit...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        static async Task<Cemetery> GetCemeteryInfo()
        {
            string requestBody = CommonItems.CemeteryInfoRequestBody(cemeteryId);

            if (string.IsNullOrEmpty(requestBody))
            {
                Console.WriteLine("Request body is empty");
                return null!;
            }

            string jsonResponse = await FinderScraper.Networking.Requests.PostAsync(CommonItems.FindAGraveApiUrl, requestBody);
            return FinderScraper.HelperFunctions.GetCemeteryInfo(jsonResponse);
        }


        static async Task<List<Memorial>> RetrieveAllMemorials(int memCount)
        {
            List<Memorial> test = await FinderScraper.HelperFunctions.GetAllMemorials(cemeteryId, memCount);
            return test;
        }
    }
}
