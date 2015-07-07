using VDS.RDF;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;

namespace Common
{
    /// <summary>
    /// Providing Methods for saving an RDF graph.
    /// </summary>
    public class SaveHelper
    {
        /// <summary>
        /// Saves an RDF graph in the stardog triple store
        /// </summary>
        /// <param name="graph">graph to be saved</param>
        /// <param name="name">name of the data store, where the data should be saved in</param>
        public static void SaveOnline(Graph graph, string name)
        {
            var server = new StardogServer("http://141.57.9.24:5820/", "gjenschmischek", "asd123");

            var database = server.GetStore(name);

            database.DeleteGraph(graph.BaseUri);
            database.SaveGraph(graph);
        }

        /// <summary>
        /// Saves an RDF graph on the local disk as a turtle file
        /// </summary>
        /// <param name="graph">graph to be saved</param>
        /// <param name="name">name of the turtle file</param>
        public static void SaveOffline(Graph graph, string name)
        {
            var writer = new CompressingTurtleWriter();

            writer.Save(graph, name + ".ttl");
        } 
    }
}