namespace RunAll
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CookBookImporter.Program.Main(new string[0]);
            DbPediaImporter.Program.Main(new string[0]);
            WikipediaImporter.Program.Main(new string[0]);
            WineToMatchImporter.Program.Main(new string[0]);
        }
    }
}