#region Using

using System;
using System.Linq;
using System.Net;

using HtmlAgilityPack;

using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

#endregion

namespace WikipediaImporter
{
    public static class WikipediaImporter
    {
        private static OntologyGraph graph;

        private const string Query = 
            @"CONSTRUCT 
            { 
              ?Country dbpedia-owl:populationTotal ?Population .
              ?Country rdfs:label ?Label 
            }
            WHERE 
            { 
              select distinct ?Country, Sum(?Population) AS ?Population, ?Label where 
                                                    { 
                                                      ?Country  rdf:type dbpedia-owl:Country; 
                                                                dbpedia-owl:populationTotal ?Population; 
                                                                dbpedia-owl:demonym ?Adj; 
                                                                rdfs:label ?Label 
                                                      FILTER(LANG(?Label) = '' || LANGMATCHES(LANG(?Label), 'en'))
                                                    }
                                                    Group By ?Label ?Country
            }";

        public static Graph ImportCountriesAndAdjectives(string url)
        {
            graph = new OntologyGraph();

            // Add Namespaces
            graph.BaseUri = new Uri("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/country-adjectives/#");
            graph.NamespaceMap.AddNamespace("adj", UriFactory.Create("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/country-adjectives/#"));
            graph.NamespaceMap.AddNamespace("dbpedia", UriFactory.Create("http://dbpedia.org/resource/"));
            graph.NamespaceMap.AddNamespace("dbpedia-owl", UriFactory.Create("http://dbpedia.org/ontology/"));
            graph.NamespaceMap.AddNamespace("owl", UriFactory.Create("http://www.w3.org/2002/07/owl#"));

            graph.CreateOntologyResource(graph.BaseUri).AddType(UriFactory.Create(OntologyHelper.OwlOntology));

            var predicate = graph.CreateOntologyProperty(UriFactory.Create("adj:hasAdjective"));
            predicate.AddType(UriFactory.Create(OntologyHelper.OwlDatatypeProperty));
            predicate.AddRange(UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeString));
            predicate.AddDomain(UriFactory.Create("dbpedia-owl:Country"));

            InsertCountries();
            InsertAdjectivesFrom(url);

            return graph;
        }

        private static void InsertCountries()
        {
            var endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
            var response = endpoint.QueryWithResultGraph(Query);

            graph.Merge(response);
        }

        private static void InsertAdjectivesFrom(string url)
        {
            var doc = new HtmlDocument();
            var client = new WebClient();
            doc.LoadHtml(client.DownloadString(url));

            var table = doc.GetElementbyId("mw-content-text").Descendants("tr").Select(n => n.Elements("td").Select(e => e.Descendants("a")).ToArray()).ToList();

            for (var i = 2; i < table.Count() - 1; i++)
            {
                var countryId = table[i][0].First().GetAttributeValue("href", string.Empty).Split('/').Last();

                if (countryId == string.Empty)
                {
                    continue;
                }

                var countryNode = graph.CreateOntologyResource(UriFactory.Create("dbpedia:" + countryId));

                foreach (var adjectiveNode in table[i].Count() > 1 ? table[i][1] : table[i - 1][1])
                {
                    var countryAdjective = adjectiveNode.InnerText;

                    countryNode.AddLiteralProperty(UriFactory.Create("adj:hasAdjective"), graph.CreateLiteralNode(countryAdjective), true);
                }
            }
        }
    }
}