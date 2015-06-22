using VDS.RDF.Writing;

namespace Reasoning
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using VDS.RDF;
    using VDS.RDF.Ontology;
    using VDS.RDF.Storage;
    using VDS.RDF.Storage.Management;

    class Program
    {
        static IList<string> dataBases = new List<string> { "wineToMatch", "wineDotCom", "cookbook", "country-adjectives" };

        private static string baseUri = "http://www.imn.htwk-leipzig.de/gjenschm/ontologies/";

        static void Main(string[] args)
        {
            var server = new StardogServer("http://141.57.9.23:5820/", "gjenschmischek", "asd123");
            var connector = new StardogConnector(
                "http://141.57.9.23:5820/",
                "internationalWineAndFood",
                "gjenschmischek",
                "asd123") { Timeout = int.MaxValue };

            MergeDatabases(server, connector);

            Reasoning(connector);
        }

        private static void MergeDatabases(StardogServer server, IStorageProvider store)
        {
            Console.WriteLine("Starte: DB Merge ...");

            var writer = new CompressingTurtleWriter();
            
            foreach (var databaseName in dataBases)
            {
                var database = server.GetStore(databaseName);

                var graphUri = baseUri + databaseName + "/#";

                var graph = new Graph();

                database.LoadGraph(graph, graphUri);

                writer.Save(graph, databaseName + ".ttl");

                store.DeleteGraph(graphUri);

                store.SaveGraph(graph);
            }

            var iwfGraph = new OntologyGraph
                               {
                                   BaseUri =
                                       new Uri(
                                       "http://www.imn.htwk-leipzig.de/gjenschm/ontologies/internationalWineAndFood/#")
                               };

            iwfGraph.CreateOntologyResource(iwfGraph.BaseUri).AddType(UriFactory.Create(OntologyHelper.OwlOntology));


            store.DeleteGraph(iwfGraph.BaseUri);

            store.SaveGraph(iwfGraph);

            Console.WriteLine("Fertig: DB Merge ...");
        }

        private static void Reasoning(StardogConnector store)
        {
            Console.WriteLine("Starte: Reasoning ...");

            foreach (var file in Directory.EnumerateFiles("Queries\\Reasoning\\"))
            {
                Console.WriteLine("Starte: " + file + " ...");

                var query = File.ReadAllText(file);
                store.Query(query);
            }

            Console.WriteLine("Fertig: Reasoning ...");
        }
    }
}
