using System.IO;

using VDS.RDF.Writing;

namespace WineToMatchImporter
{
    public class Programm
    {
        public static void Main(string[] args)
        {
            var graph = WineToMatchImporter.ImportFromWineToMatch(Directory.GetCurrentDirectory());

            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "test.owl");
        } 
    }
}