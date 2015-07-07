namespace RunAll
{
    using System;

    internal class Program
    {
        /// <summary>
        /// Runs all Importers consecutively
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            Console.WriteLine("Starte: Cookbook...");
            CookBookImporter.Program.Main(new string[0]);

            Console.WriteLine("Fertig: Cookbook...");
            Console.WriteLine("Starte: Wikipedia...");
            WikipediaImporter.Program.Main(new string[0]);

            Console.WriteLine("Fertig: Wikipedia...");
            Console.WriteLine("Starte: WineToMatch und Wine.com...");
            WineToMatchImporter.Program.Main(new string[0]);
        }
    }
}