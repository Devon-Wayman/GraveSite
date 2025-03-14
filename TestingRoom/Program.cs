using FinderScraper.Objects;
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
            //totalMemorials = tempCem.MemorialCount;

            Console.WriteLine($"Memorial Count: {tempCem.MemorialCount}");

            Console.WriteLine("Retrieving all memorial data...");
            List<Memorial> memorials = RetrieveAllMemorials(tempCem.MemorialCount).GetAwaiter().GetResult();
            Console.WriteLine("SUCCESS");

            Console.WriteLine("Writing data to xlsx file...");
            FinderScraper.Reporting.WorkbookManager.SaveToExcel(memorials, cemeteryName);
            Console.WriteLine("SUCCESS");

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
            Console.WriteLine($"Memorials found: {test.Count}");
            return test;
        }
    }
}
