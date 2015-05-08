using System.Collections.Generic;

namespace WineToMatchExporter.Model.WineToMatch
{
    public class Cuisine
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public const string FileName = "cuisines.json";
    }
}
