namespace FinderScraper.Objects
{
    public class Cemetery
    {
        public string? CemeteryName { get; set; }
        public string? CemeteryId { get; set; }
        public int MemorialCount { get; set; }
        public int PhotoRequestCount { get; set; }
        public int PhotographedCount { get; set; }
        public int FamousCount { get; set; }
        public int GpsCount { get; set; }
        public int PercentageWithGps { get; set; }

        public Cemetery(string? cemeteryName = null, string? cemeteryId = null, int memorialCount = 0, int photoRequestCount = 0, int photographedCount = 0, int famousCount = 0, int gpsCount = 0)
        {
            CemeteryName = cemeteryName; // this will be grabbed from the end of the URL
            CemeteryId = cemeteryId;
            MemorialCount = memorialCount;
            PhotoRequestCount = photoRequestCount;
            PhotographedCount = photographedCount;
            FamousCount = famousCount;
            GpsCount = gpsCount;
            PercentageWithGps = 0;
        }
    }
}
