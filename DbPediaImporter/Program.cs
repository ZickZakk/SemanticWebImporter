#region Using

using VDS.RDF.Writing;

#endregion

namespace DbPediaImporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var graph = DbPediaImporter.ImportCountries();

            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "cookbook.ttl");
        }
    }
}