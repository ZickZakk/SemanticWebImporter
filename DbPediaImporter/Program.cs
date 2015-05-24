using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VDS.RDF.Writing;

namespace DbPediaImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            var graph = DbPediaImporter.ImportCountriesAndAdjectives("http://en.wikipedia.org/wiki/List_of_adjectival_and_demonymic_forms_for_countries_and_nations");

            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "test.owl");
        }
    }
}
