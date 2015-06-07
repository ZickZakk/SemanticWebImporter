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
            var graph = DbPediaImporter.ImportCountries();

            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "cookbook.ttl");
        }
    }
}
