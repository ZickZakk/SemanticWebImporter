#region Using

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Common;

using HtmlAgilityPack;

using Newtonsoft.Json.Linq;

using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Management;
using VDS.RDF.Writing;

#endregion

namespace WineToMatchImporter
{
    public static class WineDotComImporter
    {
        private static string wineId;

        private static string hasColorId;

        private static string originNameId;

        private static string hasWineIdId;

        private static OntologyGraph graph;

        public static void ImportFromWineDotCom(IEnumerable<string> wineIds)
        {
            graph = new OntologyGraph();

            // Add Namespaces
            graph.BaseUri = UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/wineDotCom/#");
            graph.NamespaceMap.AddNamespace("wdc", UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/wineDotCom/#"));
            graph.NamespaceMap.AddNamespace("owl", UriFactory.Create("http://www.w3.org/2002/07/owl#"));

            graph.CreateOntologyResource(graph.BaseUri).AddType(UriFactory.Create(OntologyHelper.OwlOntology));

            wineId = graph.NamespaceMap.GetNamespaceUri("wdc") + "Wine";
            hasColorId = graph.NamespaceMap.GetNamespaceUri("wdc") + "hasColor";
            hasWineIdId = graph.NamespaceMap.GetNamespaceUri("wdc") + "hasWineId";
            originNameId = graph.NamespaceMap.GetNamespaceUri("wdc") + "originName";

            // Prepare Classes
            var wine = graph.CreateOntologyClass(UriFactory.Create(wineId));

            wine.AddType(UriFactory.Create(OntologyHelper.OwlClass));

            var colorProperty = graph.CreateOntologyProperty(UriFactory.Create(hasColorId));
            colorProperty.AddType(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
            colorProperty.AddRange(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            colorProperty.AddDomain(wine);

            var originProperty = graph.CreateOntologyProperty(UriFactory.Create(originNameId));
            originProperty.AddType(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
            originProperty.AddRange(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            originProperty.AddDomain(wine);

            var hasWineIdProperty = graph.CreateOntologyProperty(UriFactory.Create(hasWineIdId));
            hasWineIdProperty.AddType(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
            hasWineIdProperty.AddRange(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            hasWineIdProperty.AddDomain(wine);

            ImportWines(wineIds);

            SaveOnline();

            SaveOffline();
        }

        private static void SaveOnline()
        {
            var server = new StardogServer("http://141.57.9.24:5820/", "gjenschmischek", "asd123");

            var database = server.GetStore("wineDotCom");

            database.DeleteGraph(graph.BaseUri);
            database.SaveGraph(graph);
        }

        private static void SaveOffline()
        {
            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "wineDotCom.ttl");
        }

        private static void ImportWines(IEnumerable<string> wineIds)
        {
            foreach (var wineId in wineIds)
            {
                var wineDoc = new HtmlDocument();
                var client = new WebClient();
                client.Encoding = Encoding.UTF8;
                wineDoc.LoadHtml(client.DownloadString("http://www.wine.com/v6/wine/" + wineId + "/Detail.aspx"));

                var wineName = wineDoc.DocumentNode.Descendants("title").First().InnerText;
                var wineColor = WebUtility.HtmlDecode(wineDoc.DocumentNode.Descendants("span").Last(span => span.Attributes.Contains("class") && span.Attributes["class"].Value.Contains("offscreen")).InnerText).Split('&').Last().Trim();
                if (!wineColor.EndsWith("wine"))
                {
                    wineColor += " wine";
                }

                var script = wineDoc.DocumentNode.Descendants("script").First();

                var jsonString = script.InnerText.Split('=').Last().TrimEnd(';', '\n');

                var json = JObject.Parse(jsonString);

                if (json["OmnitureProps"].Value<string>("Url").Contains("/gift/"))
                {
                    continue;
                }

                var wineNameId = json["OmnitureProps"].Value<string>("Url").Split('/').ElementAt(4);
                var wineOrigin = json["OmnitureProps"].Value<string>("Region").Split(',').Last().Trim().Split('-').First().Trim();

                var wineNode = graph.CreateIndividual(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("wdc") + wineNameId.ToRdfId()), UriFactory.Create(WineDotComImporter.wineId));
                wineNode.AddLabel(wineName);
                wineNode.AddLiteralProperty(UriFactory.Create(hasColorId), graph.CreateLiteralNode(wineColor), true);
                wineNode.AddLiteralProperty(UriFactory.Create(originNameId), graph.CreateLiteralNode(wineOrigin), true);
                wineNode.AddLiteralProperty(UriFactory.Create(hasWineIdId), graph.CreateLiteralNode(wineId), true);
            }
        }
    }
}