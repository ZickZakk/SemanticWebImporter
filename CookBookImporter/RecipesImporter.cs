#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using Common;

using HtmlAgilityPack;

using VDS.RDF;
using VDS.RDF.Ontology;

#endregion

namespace CookBookImporter
{
    public static class RecipesImporter
    {
        private static OntologyGraph graph;

        /// <summary>
        /// Imports Recipes and Categories starting at given URL
        /// </summary>
        /// <param name="url">URL to start importing from</param>
        /// <returns>Graph with imported data</returns>
        public static Graph ImportFrom(string url)
        {
            graph = new OntologyGraph();

            // Add Namespaces
            graph.BaseUri = new Uri("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/cookbook/#");
            graph.NamespaceMap.AddNamespace("cookbook", UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/cookbook/#"));
            graph.NamespaceMap.AddNamespace("dcterms", UriFactory.Create("http://purl.org/dc/terms/"));
            graph.NamespaceMap.AddNamespace("owl", UriFactory.Create("http://www.w3.org/2002/07/owl#"));
            graph.NamespaceMap.AddNamespace("skos", UriFactory.Create("http://www.w3.org/2004/02/skos/core#"));

            graph.CreateOntologyResource(graph.BaseUri).AddType(UriFactory.Create(OntologyHelper.OwlOntology));

            graph.CreateOntologyClass(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("cookbook") + "Recipe")).AddType(UriFactory.Create(OntologyHelper.OwlClass));

            InsertFrom(url);

            return graph;
        }

        /// <summary>
        /// Inserts recipes or categories from given URL
        /// </summary>
        /// <param name="url">URL to insert from</param>
        private static void InsertFrom(string url)
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

        /// <summary>
        /// Handles a recipe page for inserting a recipe and its categories.
        /// </summary>
        /// <param name="doc">Document to be handled, representing a recipe page</param>
        private static void HandleRecipePage(HtmlDocument doc)
        {
            var categories = doc.GetElementbyId("catlinks").Descendants("a").ToList();

            if (categories.All(category => category.InnerText != "Recipes"))
            {
                // No recipe page
                return;
            }

            var recipeName = doc.GetElementbyId("firstHeading").InnerText.Split(':').Last();

            InsertRecipe(recipeName);

            foreach (var category in categories)
            {
                HandleCategoryFromRecipe(recipeName, category);
            }
        }

        /// <summary>
        /// Handles a category of a recipe found in recipe page.
        /// </summary>
        /// <param name="recipeName">name of the recipe of the category</param>
        /// <param name="categoryNode">HTML node of the category</param>
        private static void HandleCategoryFromRecipe(string recipeName, HtmlNode categoryNode)
        {
            var categoryName = categoryNode.InnerText;

            if (!categoryName.Contains("recipe"))
            {
                return;
            }

            InsertCategoryFromRecipe(categoryName, recipeName);

            var categoryUrl = categoryNode.GetAttributeValue("href", string.Empty);

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

        /// <summary>
        /// Inserts broader categories to a category rekursivly.
        /// </summary>
        /// <param name="categoryName">current category name</param>
        /// <param name="categoryPage">category page, containing the broader categories</param>
        private static void InsertBroaderCategories(string categoryName, HtmlDocument categoryPage)
        {
            // recipes category is broadest category - stop here
            if (!categoryName.Contains("recipes"))
            {
                return;
            }

            var categoryNode = graph.CreateIndividual(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("cookbook") + categoryName.ToRdfId()), UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("skos") + "Concept"));

            var broaderCategories = categoryPage.GetElementbyId("catlinks").Descendants("a").Where(a => !a.InnerText.Contains("Categor")).ToList();

            foreach (var broaderCategory in broaderCategories)
            {
                var broaderCategoryName = broaderCategory.InnerText;

                var broaderCategoryNode = graph.CreateIndividual(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("cookbook") + broaderCategoryName.ToRdfId()), UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("skos") + "Concept"));

                if (!broaderCategoryNode.Label.Any())
                {
                    broaderCategoryNode.AddLabel(broaderCategoryName);
                }

                categoryNode.AddResourceProperty(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("skos") + "broader"), broaderCategoryNode.Resource, true);

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

        /// <summary>
        /// Inserts a category of an recipe to the result graph
        /// </summary>
        /// <param name="categoryName">name of the category</param>
        /// <param name="recipeName">name of the recipe</param>
        private static void InsertCategoryFromRecipe(string categoryName, string recipeName)
        {
            var categoryNode = graph.CreateIndividual(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("cookbook") + categoryName.ToRdfId()), UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("skos") + "Concept"));

            if (!categoryNode.Label.Any())
            {
                categoryNode.AddLabel(categoryName);
            }

            var recipeNode = graph.CreateIndividual(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("cookbook") + recipeName.ToRdfId()));

            recipeNode.AddResourceProperty(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("dcterms") + "subject"), categoryNode.Resource, true);
        }

        /// <summary>
        /// Inserts a recipe to the result graph
        /// </summary>
        /// <param name="recipeName">name of the recipe</param>
        private static void InsertRecipe(string recipeName)
        {
            var recipe = graph.CreateIndividual(UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("cookbook") + recipeName.ToRdfId()), UriFactory.Create(graph.NamespaceMap.GetNamespaceUri("cookbook") + "Recipe"));

            if (!recipe.Label.Any())
            {
                recipe.AddLabel(recipeName);
            }
        }

        /// <summary>
        /// Handles a category page containing recipes and subcategories.
        /// </summary>
        /// <param name="doc">Document to be handled, containing recipes and subcategories</param>
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

                InsertFrom("http://en.wikibooks.org" + url);
            }

            var recipes = doc.GetElementbyId("mw-pages") == null ? new List<HtmlNode>() : doc.GetElementbyId("mw-pages").Descendants("a").Where(link => link.InnerText.StartsWith("Cookbook:"));

            foreach (var recipe in recipes)
            {
                var url = recipe.GetAttributeValue("href", string.Empty);

                if (url == string.Empty)
                {
                    continue;
                }

                InsertFrom("http://en.wikibooks.org" + url);
            }
        }
    }
}