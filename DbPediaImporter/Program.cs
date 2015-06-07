#region Using

using VDS.RDF;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;

#endregion

namespace DbPediaImporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // var graph = DbPediaImporter.ImportCountries();

            // SaveOnline(graph);

            // SaveOffline(graph);
        }

        private static void SaveOnline(Graph graph)
        {
            var server = new StardogServer("http://141.57.9.24:5820/", "gjenschmischek", "***");

            var database = server.GetStore("countries");

            database.DeleteGraph(graph.BaseUri);
            database.SaveGraph(graph);
        }

        private static void SaveOffline(Graph graph)
        {
            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "countries.ttl");
        }
    }
}