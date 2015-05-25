using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using HtmlAgilityPack;

using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Inference;
using VDS.RDF.Writing;

namespace CookBookImporter
{
    public static class RecipesImporter
    {
        private static OntologyGraph graph;

        private static int i = 10;

        public static Graph ImportRecipesFrom(string url)
        {
            graph = new OntologyGraph();

            // Add Namespaces
            graph.BaseUri = new Uri("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/cookbook/#");
            graph.NamespaceMap.AddNamespace("cookbook", UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/cookbook/#"));
            graph.NamespaceMap.AddNamespace("dcterms", UriFactory.Create("http://purl.org/dc/terms/"));
            graph.NamespaceMap.AddNamespace("owl", UriFactory.Create("http://www.w3.org/2002/07/owl#"));
            graph.NamespaceMap.AddNamespace("skos", UriFactory.Create("http://www.w3.org/2004/02/skos/core#"));

            graph.CreateOntologyClass(UriFactory.Create("cookbook:Recipe")).AddType(UriFactory.Create(OntologyHelper.OwlClass));

            InsertRecipesFrom(url);

            return graph;
        }

        private static void InsertRecipesFrom(string url)
        {
            var doc = new HtmlDocument();
            var client = new WebClient();
            doc.LoadHtml(client.DownloadString(url));

            if (doc.GetElementbyId("firstHeading").InnerText.StartsWith("Category:"))
            {
                HandleCategoryPage(doc);
            }
            else
            {
                HandleRecipePage(doc);
            }
        }

        private static void HandleRecipePage(HtmlDocument doc)
        {
            var categories = doc.GetElementbyId("catlinks").Descendants("a").ToList();

            if (categories.All(category => category.InnerText != "Recipes"))
            {
                // No recipe page
                return;
            }

            //i -= 1;

            //if (i == 0)
            //{
            //    var writer = new CompressingTurtleWriter();

            //    writer.Save(graph, "test.owl"); ;
            //}

            var recipeName = doc.GetElementbyId("firstHeading").InnerText.Split(':').Last();

            InsertRecipe(recipeName);

            foreach (var category in categories)
            {
                HandleCategoryFromRecipe(recipeName, category);
            }
        }

        private static void HandleCategoryFromRecipe(string recipeName, HtmlNode category)
        {
            var categoryName = category.InnerText;

            if (!categoryName.Contains("recipe"))
            {
                return;
            }

            InsertCategoryFromRecipe(categoryName, recipeName);

            var categoryUrl = category.GetAttributeValue("href", string.Empty);

            if (categoryUrl == string.Empty || categoryUrl.Contains("/w/"))
            {
                // Not valid category
                return;
            }

            var doc = new HtmlDocument();
            var client = new WebClient();
            doc.LoadHtml(client.DownloadString("http://en.wikibooks.org" + categoryUrl));

            InsertBroaderCategories(categoryName, doc);
        }

        private static void InsertBroaderCategories(string categoryName, HtmlDocument categoryPage)
        {
            if (!categoryName.Contains("recipes"))
            {
                return;
            }

            var categoryNode = graph.CreateIndividual(UriFactory.Create("cookbook:" + categoryName.Replace(' ', '_')), UriFactory.Create("skos:Concept"));

            var broaderCategories = categoryPage.GetElementbyId("catlinks").Descendants("a").Where(a => !a.InnerText.Contains("Categor")).ToList();

            foreach (var broaderCategory in broaderCategories)
            {
                var broaderCategoryName = broaderCategory.InnerText;

                var broaderCategoryNode = graph.CreateIndividual(UriFactory.Create("cookbook:" + broaderCategoryName.Replace(' ', '_')), UriFactory.Create("skos:Concept"));

                if (!broaderCategoryNode.Label.Any())
                {
                    broaderCategoryNode.AddLabel(broaderCategoryName);
                }

                categoryNode.AddResourceProperty(UriFactory.Create("skos:broader"), broaderCategoryNode.Resource, true);

                var broaderCategoryUrl = broaderCategory.GetAttributeValue("href", string.Empty);

                if (broaderCategoryUrl == string.Empty || broaderCategoryUrl.Contains("/w/"))
                {
                    // Not valid category
                    return;
                }

                var doc = new HtmlDocument();
                var client = new WebClient();
                doc.LoadHtml(client.DownloadString("http://en.wikibooks.org" + broaderCategoryUrl));

                InsertBroaderCategories(broaderCategoryName, doc);
            }
        }

        private static void InsertCategoryFromRecipe(string categoryName, string recipeName)
        {
            var categoryNode = graph.CreateIndividual(UriFactory.Create("cookbook:" + categoryName.Replace(' ', '_')), UriFactory.Create("skos:Concept"));

            if (!categoryNode.Label.Any())
            {
                categoryNode.AddLabel(categoryName);
            }

            var recipeNode = graph.CreateIndividual(UriFactory.Create("cookbook:" + recipeName.Replace(' ', '_')));

            recipeNode.AddResourceProperty(UriFactory.Create("dcterms:subject"), categoryNode.Resource, true);
        }

        private static void InsertRecipe(string recipeName)
        {
            var recipe = graph.CreateIndividual(UriFactory.Create("cookbook:" + recipeName.Replace(' ', '_')), UriFactory.Create("cookbook:Recipe"));

            if (!recipe.Label.Any())
            {

                recipe.AddLabel(recipeName);
            }
        }

        private static void HandleCategoryPage(HtmlDocument doc)
        {
            var subCategories = doc.GetElementbyId("mw-subcategories") == null ? new List<HtmlNode>() : doc.GetElementbyId("mw-subcategories").Descendants("a");

            foreach (var subCategory in subCategories)
            {
                var url = subCategory.GetAttributeValue("href", string.Empty);

                if (url == string.Empty)
                {
                    continue;
                }

                InsertRecipesFrom("http://en.wikibooks.org" + url);
            }

            var recipes = doc.GetElementbyId("mw-pages") == null ? new List<HtmlNode>() : doc.GetElementbyId("mw-pages").Descendants("a");

            foreach (var recipe in recipes)
            {
                var url = recipe.GetAttributeValue("href", string.Empty);

                if (url == string.Empty)
                {
                    continue;
                }

                InsertRecipesFrom("http://en.wikibooks.org" + url);
            }
        }
    }
}