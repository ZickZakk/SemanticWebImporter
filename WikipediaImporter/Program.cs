#region Using

using VDS.RDF.Writing;

#endregion

namespace WikipediaImporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var graph = WikipediaImporter.ImportCountriesAndAdjectives("http://en.wikipedia.org/wiki/List_of_adjectival_and_demonymic_forms_for_countries_and_nations");

            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "adj.ttl");
        }
    }
}