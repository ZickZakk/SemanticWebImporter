#region Using

using System.IO;

using VDS.RDF.Writing;

#endregion

namespace WineToMatchImporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var graph = WineToMatchImporter.ImportFromWineToMatch(Directory.GetCurrentDirectory());

            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "wtm.ttl");
        }
    }
}