#region Using

using System.IO;

using Common;

using VDS.RDF;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;

#endregion

namespace WineToMatchImporter
{
    public class Program
    {
        /// <summary>
        /// Starts the import of WineToMatch and Wine.com
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var graph = WineToMatchImporter.ImportFromWineToMatchAndWineDotCom();

            SaveHelper.SaveOnline(graph, "wineToMatch");

            // SaveHelper.SaveOffline(graph, "wineToMatch");
        }
    }
}