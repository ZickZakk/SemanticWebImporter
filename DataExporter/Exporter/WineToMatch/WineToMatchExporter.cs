using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using HtmlAgilityPack;

using Newtonsoft.Json;

using WineToMatchExporter.Model.WineToMatch;

namespace WineToMatchExporter.Exporter.WineToMatch
{
    public class WineToMatchExporter
    {
        public static void ExportAllToJson(string fileDestination)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(File.ReadAllText(Path.GetFullPath("Ressources/WineToMatch.html"), Encoding.UTF8));

            ExportCookingTypeToJson(doc, fileDestination);
            ExportCuisineToJson(doc, fileDestination);
            ExportMainFlavorToJson(doc, fileDestination);

            ExportWine(doc, fileDestination);
        }

        private static void ExportWine(HtmlDocument doc, string fileDestination)
        {
            var wineTypeNodes = doc.GetElementbyId("li_container1").Descendants("a");

            var wineTypes = wineTypeNodes.Select(cookingTypeNode => new WineType { Id = cookingTypeNode.Attributes["href"].Value.Split('/').Last(), Name = cookingTypeNode.InnerText });

            string json = JsonConvert.SerializeObject(wineTypes, Formatting.Indented);

            File.WriteAllText(Path.Combine(fileDestination, WineType.FileName), json);
        }

        private static void ExportCookingTypeToJson(HtmlDocument doc, string fileDestination)
        {
            var cookingTypeNodes = doc.GetElementbyId("cooking").Descendants("a");

            var cookingTypes = cookingTypeNodes.Select(cookingTypeNode => new CookingType { Id = Convert.ToInt32(cookingTypeNode.Attributes["rel"].Value), Name = cookingTypeNode.InnerText });

            string json = JsonConvert.SerializeObject(cookingTypes, Formatting.Indented);

            File.WriteAllText(Path.Combine(fileDestination, CookingType.FileName), json);
        }

        private static void ExportCuisineToJson(HtmlDocument doc, string fileDestination)
        {
            var cuisineTypeNodes = doc.GetElementbyId("cuisine").Descendants("a");

            var cuisines = cuisineTypeNodes.Select(cookingTypeNode => new CookingType { Id = Convert.ToInt32(cookingTypeNode.Attributes["rel"].Value), Name = cookingTypeNode.InnerText });

            string json = JsonConvert.SerializeObject(cuisines, Formatting.Indented);

            File.WriteAllText(Path.Combine(fileDestination, Cuisine.FileName), json);
        }

        private static void ExportMainFlavorToJson(HtmlDocument doc, string fileDestination)
        {
            var mainFlavorNodes = doc.GetElementbyId("main").Descendants("a");

            var mainFlavors = mainFlavorNodes.Select(cookingTypeNode => new Cuisine { Id = Convert.ToInt32(cookingTypeNode.Attributes["rel"].Value), Name = cookingTypeNode.InnerText });

            string json = JsonConvert.SerializeObject(mainFlavors, Formatting.Indented);

            File.WriteAllText(Path.Combine(fileDestination, MainFlavor.FileName), json);
        }

        public class Container<T>
        {
            public List<T> Items { get; set; }

            public Container()
            {
                this.Items = new List<T>();
            }
        }

        public static void Main(string[] args)
        {
            ExportAllToJson(Directory.GetCurrentDirectory());
        }
    }
}
