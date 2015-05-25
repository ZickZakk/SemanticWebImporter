using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using HtmlAgilityPack;

using Newtonsoft.Json.Linq;

using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace WineToMatchImporter
{
    public static class WineDotComImporter
    {
        private const string WineId = "wdc:Wine";

        private const string HasColorId = "wdc:hasColor";

        private const string OriginNameId = "wdc:originName";

        private static OntologyGraph graph;

        public static void ImportFromWineDotCom(IEnumerable<string> wineIds)
        {
            graph = new OntologyGraph();

            // Add Namespaces
            graph.BaseUri = UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/wineDotCom/#");
            graph.NamespaceMap.AddNamespace("wdc", UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/wineDotCom/#"));
            graph.NamespaceMap.AddNamespace("owl", UriFactory.Create("http://www.w3.org/2002/07/owl#"));

            //Prepare Classes
            var wine = graph.CreateOntologyClass(UriFactory.Create(WineId));

            wine.AddType(UriFactory.Create(OntologyHelper.OwlClass));

            var colorProperty = graph.CreateOntologyProperty(UriFactory.Create(HasColorId));
            colorProperty.AddType(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
            colorProperty.AddRange(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            colorProperty.AddDomain(wine);

            var originProperty = graph.CreateOntologyProperty(UriFactory.Create(OriginNameId));
            originProperty.AddType(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
            originProperty.AddRange(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            originProperty.AddDomain(wine);

            ImportWines(wineIds);


            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "test2.owl");
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

                var jsonString = script.InnerText.Split('=').Last().TrimEnd(';','\n');

                var json = JObject.Parse(jsonString);

                var wineNameId = json["OmnitureProps"].Value<string>("Url").Split('/').ElementAt(4); 
                var wineOrigin = json["OmnitureProps"].Value<string>("Region").Split(',').Last().Trim().Split('-').First().Trim();

                var wineNode = graph.CreateIndividual(UriFactory.Create("wdc:" + wineNameId), UriFactory.Create(WineId));
                wineNode.AddLabel(wineName);
                wineNode.AddLiteralProperty(UriFactory.Create(HasColorId), graph.CreateLiteralNode(wineColor), true);
                wineNode.AddLiteralProperty(UriFactory.Create(OriginNameId), graph.CreateLiteralNode(wineOrigin), true);
            }
        }
    }
}
