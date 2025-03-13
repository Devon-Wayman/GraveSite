namespace FinderScraper
{
    public class Commons
    {
        public static readonly string FindAGraveApiUrl = "https://www.findagrave.com/orc/graphql";

        #region Api request bodies
        static readonly string MemorialCountTemplate = $@"
        {{
          ""operationName"": ""Cemetery"",
          ""query"": ""query Cemetery($cemeteryID: [ID!], $memorialSearchSearch: MemorialSearchInput) {{ cemeteries(ids: $cemeteryID) {{ __typename total cemeteries {{ __typename id phone memorialCount photoRequestCount photographedCount acceptingPhotos url isFavorited isPhotoVolunteer famousCount myCount gpsCount acceptingBurials acceptingPhotoRequests names {{ __typename name abbreviation language }} description {{ __typename value }} coordinates {{ __typename lat lon }} physicalAddress {{ __typename postalCode street }} locations {{ __typename city county state country }} transcriptionCount {{ __typename public private }} dateCreated photos {{ __typename photos {{ __typename path id caption dateCreated sortOrder type contributor {{ __typename id publicName }} }} }} }} }} memorialSearch(search: $memorialSearchSearch) {{ __typename memorials {{ __typename id name {{ __typename first middle last }} photos {{ __typename photos {{ __typename path }} }} }} }} nearbyCemeteries(ids: $cemeteryID) {{ __typename names {{ __typename name }} locations {{ __typename city county state country }} id memorialCount gpsCount photographedCount photoRequestCount coordinates {{ __typename lat lon }} photos(size: 1) {{ __typename photos {{ __typename path }} }} }} }}"",
          ""variables"": {{
            ""cemeteryID"": [
              ""CEM_ID_HERE""
            ],
            ""memorialSearchSearch"": {{
              ""cemeteryIds"": [
                ""CEM_ID_HERE""
              ],
              ""from"": 0,
              ""isFamous"": true,
              ""size"": 10
            }}
          }}
        }}";
        #endregion


        public static string CemeteryInfoRequestBody(string cemeteryId)
        {
            if (string.IsNullOrEmpty(cemeteryId)) return string.Empty;

            return MemorialCountTemplate.Replace("CEM_ID_HERE", cemeteryId);
        }
    }
}
