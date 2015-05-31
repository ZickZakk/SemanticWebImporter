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

namespace WineToMatchImporter
{
    using Common;

    public static class WineToMatchImporter
    {
        private const string CookingTypeId = "wtm:CookingType";

        private const string WinetypeId = "wtm:WineType";

        private const string WineId = "wtm:Wine";

        private const string HasIdId = "wtm:hasId";

        private const string MatchesWineTypeId = "wtm:matchesWineType";

        private static OntologyGraph graph;

        private const string IngredientId = "wtm:Ingredient";

        private const string HasIngredientId = "wtm:hasIngredient";

        private const string HasCookingTypeId = "wtm:hasCookingType";

        private const string HasCuisineId = "wtm:hasCuisine";

        private const string HasSelection = "wtm:hasSelectionId";

        private const string CombinationId = "wtm:Combination";

        private const string CuisineId = "wtm:Cuisine";

        public static Graph ImportFromWineToMatch(string fileDestination)
        {
            graph = new OntologyGraph();

            // Add Namespaces
            graph.BaseUri = UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/wineToMatch/#");
            graph.NamespaceMap.AddNamespace("", UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/wineToMatch/#"));
            graph.NamespaceMap.AddNamespace("wtm", UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/wineToMatch/#"));
            graph.NamespaceMap.AddNamespace("owl", UriFactory.Create("http://www.w3.org/2002/07/owl#"));

            graph.CreateOntologyResource(graph.BaseUri).AddType(UriFactory.Create(OntologyHelper.OwlOntology));

            //Prepare Classes
            var wineType = graph.CreateOntologyClass(UriFactory.Create(WinetypeId));
            var combination = graph.CreateOntologyClass(UriFactory.Create(CombinationId));
            var cuisine = graph.CreateOntologyClass(UriFactory.Create(CuisineId));
            var cookingType = graph.CreateOntologyClass(UriFactory.Create(CookingTypeId));
            var ingredient = graph.CreateOntologyClass(UriFactory.Create(IngredientId));

            wineType.AddType(UriFactory.Create(OntologyHelper.OwlClass));
            combination.AddType(UriFactory.Create(OntologyHelper.OwlClass));
            cuisine.AddType(UriFactory.Create(OntologyHelper.OwlClass));
            cookingType.AddType(UriFactory.Create(OntologyHelper.OwlClass));
            ingredient.AddType(UriFactory.Create(OntologyHelper.OwlClass));

            CreateProperty(combination, cuisine, HasCuisineId, 1);
            CreateProperty(combination, cookingType, HasCookingTypeId, 1);
            CreateProperty(combination, ingredient, HasIngredientId, 1);
            CreateMaxProperty(combination, wineType, MatchesWineTypeId, 3);

            var idProperty = graph.CreateOntologyProperty(UriFactory.Create(HasIdId));
            idProperty.AddType(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
            idProperty.AddRange(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

            var selectionProperty = graph.CreateOntologyProperty(UriFactory.Create(HasSelection));
            selectionProperty.AddType(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
            selectionProperty.AddRange(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            selectionProperty.AddDomain(wineType);

            var doc = new HtmlDocument();
            doc.LoadHtml(File.ReadAllText(Path.GetFullPath("Ressources/WineToMatch.html"), Encoding.UTF8));

            ImportWineTypes(doc);
            var cookingTypes = Import(doc, "cooking", UriFactory.Create(CookingTypeId)).ToList();
            var cuisines = Import(doc, "cuisine", UriFactory.Create(CuisineId)).ToList();
            var ingredients = Import(doc, "main", UriFactory.Create(IngredientId)).ToList();

            ImportCombinations(ingredients, cuisines, cookingTypes);

            return graph;
        }

        private static void ImportCombinations(IList<Individual> ingredients, IList<Individual> cuisines, IList<Individual> cookingTypes)
        {
            using (var client = new WebClient())
            {
                foreach (var ingredient in ingredients.Take(2))
                {
                    foreach (var cuisine in cuisines.Take(2))
                    {
                        foreach (var cookingType in cookingTypes.Take(2))
                        {
                            var combi = graph.CreateOntologyResource();
                            combi.AddType(UriFactory.Create(CombinationId));
                            combi.AddResourceProperty(UriFactory.Create(HasIngredientId), ingredient.Resource, true);
                            combi.AddResourceProperty(UriFactory.Create(HasCookingTypeId), cookingType.Resource, true);
                            combi.AddResourceProperty(UriFactory.Create(HasCuisineId), cuisine.Resource, true);

                            var values = new NameValueCollection();

                            values["mainval"] = ingredient.GetLiteralProperty(HasIdId).First().Value;
                            values["weightval"] = "0";
                            values["flavorval"] = "NaN";
                            values["cookingval"] = cookingType.GetLiteralProperty(UriFactory.Create(HasIdId)).First().Value;
                            values["cuisineval"] = cuisine.GetLiteralProperty(UriFactory.Create(HasIdId)).First().Value;

                            var response = client.UploadValues("http://www.winetomatch.com/libs/newwine.php", values);

                            var responseString = Encoding.Default.GetString(response);

                            var json = JObject.Parse(responseString);

                            foreach (var result in json["items"].OrderByDescending(token => token["freq"]).Take(3))
                            {
                                var wineType = graph.CreateIndividual(UriFactory.Create("wtm:" + result["urlname"].ToString().ToRdfId()), UriFactory.Create(WinetypeId));

                                combi.AddResourceProperty(UriFactory.Create(MatchesWineTypeId), wineType.Resource, true);
                            }
                        }
                    }
                }
            }
        }

        private static void CreateMaxProperty(OntologyClass combination, OntologyClass cuisine, string name, int cardinality)
        {
            CreateCardinalProperty(combination, cuisine, name, cardinality, "owl:maxCardinality");
        }

        private static void CreateCardinalProperty(OntologyClass combination, OntologyClass cuisine, string name, int cardinality, string owlCardinality)
        {
            var predicate = graph.CreateOntologyProperty(UriFactory.Create(name));
            predicate.AddType(UriFactory.Create(OntologyHelper.OwlObjectProperty));
            predicate.AddDomain(combination);
            predicate.AddRange(cuisine);

            //var restriction = graph.CreateOntologyClass();
            //restriction.AddType(UriFactory.Create("owl:Restriction"));
            //restriction.AddResourceProperty(UriFactory.Create("owl:onProperty"), predicate.Resource, true);
            //restriction.AddLiteralProperty(UriFactory.Create(owlCardinality), graph.CreateLiteralNode(cardinality.ToString(), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger)), true);
        }

        private static void CreateProperty(OntologyClass combination, OntologyClass cuisine, string name, int cardinality)
        {
            CreateCardinalProperty(combination, cuisine, name, cardinality, "owl:cardinality");
        }

        private static void ImportWineTypes(HtmlDocument doc)
        {
            var wineTypeNodes = doc.GetElementbyId("li_container1").Descendants("a");

            var wineIdSet = new HashSet<string>();

            foreach (var wineTypeNode in wineTypeNodes.Take(2))
            {
                var wineTypeId = wineTypeNode.Attributes["href"].Value.Split('/').Last();
                var wineTypeName = wineTypeNode.InnerText;

                var winetype = graph.CreateIndividual(UriFactory.Create("wtm:" + wineTypeId.ToRdfId()), UriFactory.Create(WinetypeId));
                winetype.AddLabel(wineTypeName);

                var wineDoc = new HtmlDocument();
                var client = new WebClient();
                wineDoc.LoadHtml(client.DownloadString("http://www.winetomatch.com/wines/" + wineTypeId));

                var bottles = wineDoc.GetElementbyId("wine_links").Descendants("a").GroupBy(a => a.GetAttributeValue("href", string.Empty));
                var wineIds = bottles.Select(group => group.Key.Split(new[] { "%2f" }, StringSplitOptions.RemoveEmptyEntries).Reverse().ElementAt(1)).ToList();

                foreach (var wineId in wineIds)
                {
                    winetype.AddLiteralProperty(UriFactory.Create(HasSelection), graph.CreateLiteralNode(wineId), true);
                }

                wineIdSet.UnionWith(wineIds);
            }

            WineDotComImporter.ImportFromWineDotCom(wineIdSet);
        }

        private static IEnumerable<Individual> Import(HtmlDocument doc, string anchor, Uri @class)
        {
            var nodes = doc.GetElementbyId(anchor).Descendants("a");

            foreach (var node in nodes)
            {
                var id = node.Attributes["rel"].Value;
                var name = node.InnerText;

                var individual = graph.CreateIndividual(UriFactory.Create("wtm:" + name.ToRdfId()), @class);
                individual.AddLabel(name);
                individual.AddLiteralProperty(HasIdId, graph.CreateLiteralNode(id), true);

                yield return individual;
            }
        }
    }
}
