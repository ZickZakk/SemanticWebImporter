#region Using

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Common;

using HtmlAgilityPack;

using Newtonsoft.Json.Linq;

using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;

#endregion

namespace WineToMatchImporter
{
    using System.Threading.Tasks;

    public static class WineToMatchImporter
    {
        private static string cookingTypeId;

        private static string winetypeId;

        private static string hasIdId;

        private static string matchesWineTypeId;

        private static string ingredientId;

        private static string hasIngredientId;

        private static string hasCookingTypeId;

        private static string hasCuisineId;

        private static string hasSelection;

        private static string combinationId;

        private static string cuisineId;

        private static OntologyGraph graph;

        /// <summary>
        /// Imports all Data From Wine To Match into an ontology graph. Starts import of Wine.com, too.
        /// </summary>
        /// <returns>Graph with imported data</returns>
        public static Graph ImportFromWineToMatchAndWineDotCom()
        {
            graph = new OntologyGraph();

            // Add Namespaces
            graph.BaseUri = UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/wineToMatch/#");
            graph.NamespaceMap.AddNamespace("wtm", UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/wineToMatch/#"));
            graph.NamespaceMap.AddNamespace("owl", UriFactory.Create("http://www.w3.org/2002/07/owl#"));

            graph.CreateOntologyResource(graph.BaseUri).AddType(UriFactory.Create(OntologyHelper.OwlOntology));

            cookingTypeId = graph.NamespaceMap.GetNamespaceUri("wtm") + "CookingType";
            winetypeId = graph.NamespaceMap.GetNamespaceUri("wtm") + "WineType";
            hasIdId = graph.NamespaceMap.GetNamespaceUri("wtm") + "hasId";
            matchesWineTypeId = graph.NamespaceMap.GetNamespaceUri("wtm") + "matchesWineType";
            ingredientId = graph.NamespaceMap.GetNamespaceUri("wtm") + "Ingredient";
            hasIngredientId = graph.NamespaceMap.GetNamespaceUri("wtm") + "hasIngredient";
            hasCookingTypeId = graph.NamespaceMap.GetNamespaceUri("wtm") + "hasCookingType";
            hasCuisineId = graph.NamespaceMap.GetNamespaceUri("wtm") + "hasCuisine";
            hasSelection = graph.NamespaceMap.GetNamespaceUri("wtm") + "hasSelectionId";
            combinationId = graph.NamespaceMap.GetNamespaceUri("wtm") + "Combination";
            cuisineId = graph.NamespaceMap.GetNamespaceUri("wtm") + "Cuisine";


            //Prepare Classes
            var wineType = graph.CreateOntologyClass(UriFactory.Create(winetypeId));
            var combination = graph.CreateOntologyClass(UriFactory.Create(combinationId));
            var cuisine = graph.CreateOntologyClass(UriFactory.Create(cuisineId));
            var cookingType = graph.CreateOntologyClass(UriFactory.Create(cookingTypeId));
            var ingredient = graph.CreateOntologyClass(UriFactory.Create(ingredientId));

            wineType.AddType(UriFactory.Create(OntologyHelper.OwlClass));
            combination.AddType(UriFactory.Create(OntologyHelper.OwlClass));
            cuisine.AddType(UriFactory.Create(OntologyHelper.OwlClass));
            cookingType.AddType(UriFactory.Create(OntologyHelper.OwlClass));
            ingredient.AddType(UriFactory.Create(OntologyHelper.OwlClass));

            CreateProperty(combination, cuisine, hasCuisineId);
            CreateProperty(combination, cookingType, hasCookingTypeId);
            CreateProperty(combination, ingredient, hasIngredientId);
            CreateProperty(combination, wineType, matchesWineTypeId);

            var idProperty = graph.CreateOntologyProperty(UriFactory.Create(hasIdId));
            idProperty.AddType(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
            idProperty.AddRange(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));

            var selectionProperty = graph.CreateOntologyProperty(UriFactory.Create(hasSelection));
            selectionProperty.AddType(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
            selectionProperty.AddRange(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            selectionProperty.AddDomain(wineType);

            var doc = new HtmlDocument();
            doc.LoadHtml(File.ReadAllText(Path.GetFullPath("Ressources/WineToMatch.html"), Encoding.UTF8));

            ImportWineTypes(doc);
            var cookingTypes = ImportFromHtmlAnchor(doc, "cooking", UriFactory.Create(cookingTypeId)).ToList();
            var cuisines = ImportFromHtmlAnchor(doc, "cuisine", UriFactory.Create(cuisineId)).ToList();
            var ingredients = ImportFromHtmlAnchor(doc, "main", UriFactory.Create(ingredientId)).ToList();

            ImportCombinations(ingredients, cuisines, cookingTypes);

            return graph;
        }

        /// <summary>
        /// Imports all combinations of given ingredients, cuisines and cookingTypes and their matching winetypes by using wineToMatch JSON-Api.
        /// </summary>
        /// <param name="ingredients">list of ingredients to include in combinations</param>
        /// <param name="cuisines">list of cuisines to include in combinations</param>
        /// <param name="cookingTypes">list of cookingTypes to include in combinations</param>
        private static void ImportCombinations(IList<Individual> ingredients, IList<Individual> cuisines, IList<Individual> cookingTypes)
        {
            int i = 0;

                Parallel.ForEach(
                    ingredients,
                    ingredient => Parallel.ForEach(
                        cuisines,
                        cuisine => Parallel.ForEach(
                            cookingTypes,
                            cookingType =>
                                {
                                    var values = new NameValueCollection();

                                    values["mainval"] =
                                        ingredient.GetLiteralProperty(hasIdId).First().Value;
                                    values["weightval"] = "0";
                                    values["flavorval"] = "NaN";
                                    values["cookingval"] =
                                        cookingType.GetLiteralProperty(UriFactory.Create(hasIdId))
                                            .First()
                                            .Value;
                                    values["cuisineval"] =
                                        cuisine.GetLiteralProperty(UriFactory.Create(hasIdId))
                                            .First()
                                            .Value;

                                    bool error;
                                    var json = new JObject();

                                    do
                                    {
                                        error = false;
                                        try
                                        {
                                            var response =
                                                new WebClient().UploadValues(
                                                    "http://www.winetomatch.com/libs/newwine.php",
                                                    values);

                                            var responseString = Encoding.Default.GetString(response);

                                            json = JObject.Parse(responseString);
                                        }
                                        catch (Exception)
                                        {
                                            error = true;
                                        }
                                    }
                                    while (error);
                                    

                                    lock (graph)
                                    {
                                        var combi = graph.CreateOntologyResource();
                                        combi.AddType(UriFactory.Create(combinationId));


                                        combi.AddResourceProperty(
                                            UriFactory.Create(hasIngredientId),
                                            ingredient.Resource,
                                            true);
                                        combi.AddResourceProperty(
                                            UriFactory.Create(hasCookingTypeId),
                                            cookingType.Resource,
                                            true);
                                        combi.AddResourceProperty(
                                            UriFactory.Create(hasCuisineId),
                                            cuisine.Resource,
                                            true);

                                        foreach (var result in
                                            json["items"].OrderByDescending(token => token["freq"])
                                                .Take(3))
                                        {
                                            var wineType =
                                                graph.CreateIndividual(
                                                    UriFactory.Create(
                                                        graph.NamespaceMap.GetNamespaceUri("wtm")
                                                        + string.Empty
                                                        + result["urlname"].ToString().ToRdfId()),
                                                    UriFactory.Create(winetypeId));

                                            combi.AddResourceProperty(
                                                UriFactory.Create(matchesWineTypeId),
                                                wineType.Resource,
                                                true);
                                        }
                                        i++;

                                        Console.WriteLine("Done #" + i + " : " + string.Join(" ", ingredient.Label.First(), cuisine.Label.First(), cookingType.Label.First()));
                                    }
                                })));
            }

        /// <summary>
        /// Creates an ObjectPropery with given name, domain and range in the result graph.
        /// </summary>
        /// <param name="domainClass">domain class of the property to create</param>
        /// <param name="rangeClass">range class of the property to create</param>
        /// <param name="name">name of the property to create</param>
        private static void CreateProperty(OntologyClass domainClass, OntologyClass rangeClass, string name)
        {
            var predicate = graph.CreateOntologyProperty(UriFactory.Create(name));
            predicate.AddType(UriFactory.Create(OntologyHelper.OwlObjectProperty));
            predicate.AddDomain(domainClass);
            predicate.AddRange(rangeClass);
        }

        /// <summary>
        /// Imports all WineTypes into the result graph. Starts import of Wines from Wine.com, too.
        /// </summary>
        /// <param name="doc">Html document to import from</param>
        private static void ImportWineTypes(HtmlDocument doc)
        {
            var wineTypeNodes = doc.GetElementbyId("li_container1").Descendants("a");

            var wineIdSet = new HashSet<string>();

            foreach (var wineTypeNode in wineTypeNodes)
            {
                var wineTypeId = wineTypeNode.Attributes["href"].Value.Split('/').Last();
                var wineTypeName = wineTypeNode.InnerText;

                var winetype = graph.CreateIndividual(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("wtm") + string.Empty + wineTypeId.ToRdfId()), UriFactory.Create(winetypeId));
                winetype.AddLabel(wineTypeName);

                var wineDoc = new HtmlDocument();
                var client = new WebClient();
                wineDoc.LoadHtml(client.DownloadString("http://www.winetomatch.com/wines/" + wineTypeId));

                var bottles = wineDoc.GetElementbyId("wine_links").Descendants("a").GroupBy(a => a.GetAttributeValue("href", string.Empty));
                var wineIds = bottles.Select(group => group.Key.Split(new[] { "%2f" }, StringSplitOptions.RemoveEmptyEntries).Reverse().ElementAt(1)).ToList();

                foreach (var wineId in wineIds)
                {
                    winetype.AddLiteralProperty(UriFactory.Create(hasSelection), graph.CreateLiteralNode(wineId), true);
                }

                wineIdSet.UnionWith(wineIds);
            }

            WineDotComImporter.ImportFromWineDotCom(wineIdSet);
        }

        /// <summary>
        /// Imports all descandant "a" of given anchor inside the given Html document to member of given class.
        /// </summary>
        /// <param name="doc">Html document to import from</param>
        /// <param name="anchorName">anchor name to look for descandant "a"</param>
        /// <param name="class">class of imported member</param>
        /// <returns>list with imported member of the class</returns>
        private static IEnumerable<Individual> ImportFromHtmlAnchor(HtmlDocument doc, string anchorName, Uri @class)
        {
            var nodes = doc.GetElementbyId(anchorName).Descendants("a");

            foreach (var node in nodes)
            {
                var id = node.Attributes["rel"].Value;
                var name = node.InnerText;

                var individual = graph.CreateIndividual(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("wtm") + string.Empty + name.ToRdfId()), @class);
                individual.AddLabel(name);
                individual.AddLiteralProperty(hasIdId, graph.CreateLiteralNode(id), true);

                yield return individual;
            }
        }
    }
}