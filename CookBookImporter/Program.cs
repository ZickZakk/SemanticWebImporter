#region Using

using VDS.RDF.Writing;

#endregion

namespace CookBookImporter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var graph = RecipesImporter.ImportRecipesFrom("http://en.wikibooks.org/wiki/Category:Recipes_by_origin");

            //var graph = new OntologyGraph();

            //// Add Namespaces
            //graph.BaseUri = new Uri("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/cookbook/");
            //graph.NamespaceMap.AddNamespace("cookbook", UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/cookbook/"));
            //graph.NamespaceMap.AddNamespace("dcterms", UriFactory.Create("http://purl.org/dc/terms/"));
            //graph.NamespaceMap.AddNamespace("skos", UriFactory.Create("http://www.w3.org/2004/02/skos/core#"));

            //var recipeClass = graph.CreateOntologyClass(UriFactory.Create(graph.BaseUri + "Recipe"));

            //var recipe = recipeClass.CreateIndividual(UriFactory.Create("cookbook:" + "test"));

            //if (!recipe.Label.Any())
            //{

            //    recipe.AddLabel("test");
            //}

            //var categoryNode = graph.CreateIndividual(UriFactory.Create("cookbook:" + "cat"), UriFactory.Create("skos:Concept"));

            //if (!categoryNode.Label.Any())
            //{
            //    categoryNode.AddLabel("cat");
            //}

            //recipe = graph.CreateIndividual(UriFactory.Create("cookbook:" + "test"));

            //recipe.AddResourceProperty(UriFactory.Create("dcterms:subject"), categoryNode.Resource, true);

            var writer = new CompressingTurtleWriter();

            writer.Save(graph, "cookbook.ttl");
        }
    }
}