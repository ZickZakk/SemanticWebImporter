using System;
using System.Linq;
using System.Net;

using HtmlAgilityPack;

using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace DbPediaImporter
{
    public static class DbPediaImporter
    {
        private const string Query = @"select distinct ?Country, Sum(?Population), ?Label where 
                                        { 
                                          ?Country  rdf:type dbpedia-owl:Country; 
                                                    dbpedia-owl:populationTotal ?Population; 
                                                    dbpedia-owl:demonym ?Adj; 
                                                    rdfs:label ?Label 
                                          FILTER(LANG(?Label) = '' || LANGMATCHES(LANG(?Label), 'en'))
                                        }
                                        Group By ?Label ?Country";

        private static OntologyGraph graph;

        public static Graph ImportCountries()
        {
            graph = new OntologyGraph();

            // Add Namespaces
            graph.BaseUri = new Uri("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/countries/#");
            graph.NamespaceMap.AddNamespace("dbpedia", UriFactory.Create("http://dbpedia.org/resource/"));
            graph.NamespaceMap.AddNamespace("dbpedia-owl", UriFactory.Create("http://dbpedia.org/ontology/"));
            graph.NamespaceMap.AddNamespace("owl", UriFactory.Create("http://www.w3.org/2002/07/owl#"));

            //var endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");

            ////Make a SELECT query against the Endpoint
            //SparqlResultSet results = endpoint.QueryWithResultSet(Query);

            //Define a remote endpoint
            //Use the DBPedia SPARQL endpoint with the default Graph set to DBPedia
            SparqlRemoteEndpoint endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
            //Make a SELECT query against the Endpoint
            var results = endpoint.QueryRaw("SELECT DISTINCT ?Concept WHERE {[] a ?Concept}");



            return graph;
        }

        private static void InsertAdjectivesFrom(string url)
        {
            var doc = new HtmlDocument();
            var client = new WebClient();
            doc.LoadHtml(client.DownloadString(url));

            var table = doc.GetElementbyId("mw-content-text").Descendants("tr").Select(n => n.Elements("td").Select(e => e.Descendants("a")).ToArray()).ToList();

            for (int i = 2; i < table.Count() - 1; i++)
            {
                var countryId = table[i][0].First().GetAttributeValue("href", string.Empty).Split('/').Last();
                var countryName = table[i][0].First().GetAttributeValue("title", string.Empty);

                if (countryId == string.Empty)
                {
                    continue;
                }

                var countryNode = graph.CreateIndividual(UriFactory.Create("adj:" + countryId), UriFactory.Create("dbpedia-owl:Country"));
                countryNode.AddLabel(countryName);
                
                foreach (var adjectiveNode in table[i].Count() > 1 ? table[i][1] : table[i - 1][1])
                {
                    var countryAdjective = adjectiveNode.InnerText;

                    countryNode.AddLiteralProperty(UriFactory.Create("adj:hasAdjective"), graph.CreateLiteralNode(countryAdjective), true);
                }
            }
        }
    }
}