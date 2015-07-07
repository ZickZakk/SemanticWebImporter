# Semantic Web Projekt HTWK 2015

## Allgemein
- **Autor:** Georg Jenschmischek
- **Thema:** International Wine and Food

## Importer
- 'Wikimedia Cookbook'-Daten werden im Projekt 'CookBookImporter' importiert
- 'Wine To Match'- und 'Wine.com'-Daten werden im Projekt 'WineToMatchImporter' importiert
 - Grund: Es werden nur Weine aus 'Wine.com' importiert, die vorher bei 'Wine To Match' gefunden wurden
- 'Wikipedia'- und 'DBPedia'-Daten werden im Projekt 'WikipediaImporter' importiert
 - Die 'DBPedia'-SPARQL Anfrage liegt im 'Common'-Projekt im Pfad 'Queries/Import'
- Das Projekt 'RunAll' startet alle Importer, es können aber auch alle Importer einzeln aus ihren Projekten heraus gestartet werden
- Aktuell werden alle importierten Daten als Triple direkt in den StarDog-Triple-Store gespeichert, es ist aber auch möglich, die Daten als Turtle-Datei lokal zu speichern

## Reasoning
- Die importierten Daten werden im Projekt 'Reasoning' verknüpft
- Zunächst werden alle importierten Daten zu einem Graph zusammengefasst
- Anschließend werden alle verknüpfenden SPARQL-Anfragen aus dem 'Common'-Projekt im Pfad 'Queries/Reasoning' geladen und auf dem entstandenen Graphen ausgeführt

## Beantwortung der Recherchefragestellungen
- Um die Recherchefragestellungen zu beantworten wurden SPARQL-Anfragen auf dem verknüpften Triple-Store ausgeführt
- Alle Fragen und dazugehörige SPARQL-Anfragen sind im 'Common'-Projekt im Pfad 'Queries/Questions' abgelegt