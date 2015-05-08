using System.Collections.Generic;

namespace WineToMatchExporter.Model.WineToMatch
{
    public class WineType
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public IList<int> MatchingWines { get; set; }

        public WineType()
        {
            MatchingWines = new List<int>();
        }

        public const string FileName = "wineTypes.json";
    }
}
