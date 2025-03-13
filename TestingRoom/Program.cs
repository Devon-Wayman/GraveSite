using FinderScraper.Objects;
using CommonItems = FinderScraper.Commons;

namespace TestingRoom
{
    internal class Program
    {
        static int totalMemorials = 0;
        static readonly int testingCemeteryId = 2205898;

        static void Main(string[] args)
        {
            Cemetery tempCem = GetCemeteryInfo().GetAwaiter().GetResult();
            totalMemorials = tempCem.MemorialCount;

            Console.WriteLine($"Memorial Count: {tempCem.MemorialCount}");
            Console.WriteLine("\nPress Enter to proceed with memorial info scraping test...");
            Console.ReadLine();


            List<Memorial> memorials = RetrieveAllMemorials().GetAwaiter().GetResult();

            Console.WriteLine("All memorials retrieved successfully");

            if (memorials.Count > 0)
            {
                var firstMemorial = memorials[0];
                Console.WriteLine($"First Memorial Info: ID = {firstMemorial.MemorialId}, Name = {firstMemorial.FullName}, BirthDate = {firstMemorial.BirthDate}, DeathDate = {firstMemorial.DeathDate}");
            }
            else
            {
                Console.WriteLine("No memorials found.");
            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();

            Environment.Exit(0);
        }

        static async Task<Cemetery> GetCemeteryInfo()
        {
            string requestBody = CommonItems.CemeteryInfoRequestBody($"{testingCemeteryId}");

            if (string.IsNullOrEmpty(requestBody))
            {
                Console.WriteLine("Request body is empty");
                return null!;
            }

            string jsonResponse = await FinderScraper.Networking.Requests.PostAsync(CommonItems.FindAGraveApiUrl, requestBody);
            return FinderScraper.HelperFunctions.GetCemeteryInfo(jsonResponse);
        }


        static async Task<List<Memorial>> RetrieveAllMemorials()
        {
            List<Memorial> test = await FinderScraper.HelperFunctions.GetAllMemorials(testingCemeteryId, totalMemorials);
            Console.WriteLine($"Memorials found: {test.Count}");
            return test;
        }
    }
}
