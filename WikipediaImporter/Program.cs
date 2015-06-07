#region Using

using VDS.RDF;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;

#endregion

namespace WikipediaImporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var graph = WikipediaImporter.ImportCountriesAndAdjectives("http://en.wikipedia.org/wiki/List_of_adjectival_and_demonymic_forms_for_countries_and_nations");

            SaveOnline(graph);

            //SaveOffline(graph);
        }

        private static void SaveOnline(Graph graph)
        {
            var server = new StardogServer("http://141.57.9.24:5820/", "gjenschmischek", "asd123");

            var database = server.GetStore("country-adjectives");

            database.DeleteGraph(graph.BaseUri);
            database.SaveGraph(graph);
        }

        private static void SaveOffline(Graph graph)
        {
            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "country-adjectives.ttl");
        }
    }
}