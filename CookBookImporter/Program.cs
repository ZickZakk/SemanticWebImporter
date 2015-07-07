#region Using

using Common;

using VDS.RDF;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;

#endregion

namespace CookBookImporter
{
    public class Program
    {
        /// <summary>
        /// Starts the import of Wikimedia-CookBook
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var graph = RecipesImporter.ImportFrom("http://en.wikibooks.org/wiki/Category:Recipes_by_origin");

            SaveHelper.SaveOnline(graph, "cookbook");

            // SaveHelper.SaveOffline(graph, "cookbook");
        }
    }
}