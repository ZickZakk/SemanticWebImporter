using System;

using VDS.RDF;
using VDS.RDF.Ontology;
using VDS.RDF.Query;

namespace DbPediaImporter
{
    public static class DbPediaImporter
    {
        private const string Query = @"CONSTRUCT 
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

        private static OntologyGraph graph;

        public static Graph ImportCountries()
        {
            graph = new OntologyGraph();

            // Add Namespaces
            graph.BaseUri = new Uri("http://www.imn.htwk-leipzig.de/gjenschm/ontologies/countries/#");
            graph.NamespaceMap.AddNamespace("dbpedia", UriFactory.Create("http://dbpedia.org/resource/"));
            graph.NamespaceMap.AddNamespace("dbpedia-owl", UriFactory.Create("http://dbpedia.org/ontology/"));

            graph.CreateOntologyResource(graph.BaseUri).AddType(UriFactory.Create(OntologyHelper.OwlOntology));

            var endpoint = new SparqlRemoteEndpoint(new Uri("http://dbpedia.org/sparql"), "http://dbpedia.org");
            var response = endpoint.QueryWithResultGraph(Query);

            graph.Merge(response);

            return graph;
        }
    }
}