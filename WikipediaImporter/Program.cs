#region Using

using Common;

using VDS.RDF;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;

#endregion

namespace WikipediaImporter
{
    public class Program
    {
        /// <summary>
        /// Starts the import of Wikipedia and DBPedia
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var graph = WikipediaImporter.ImportCountriesAndAdjectives("http://en.wikipedia.org/wiki/List_of_adjectival_and_demonymic_forms_for_countries_and_nations");

            SaveHelper.SaveOnline(graph, "country-adjectives");

            //SaveHelper.SaveOffline(graph, "country-adjectives");
        }
    }
}