namespace Giparus.Osm.Test
{
    public class Program
    {
        private static void Main()
        {
            var test = new OsmTest();

            // Read OSM file and put it in mongo
            // test.TestReadOsmXmlFileAndInsertWithUpdate();

            // Fill SQL server form mongo
            //test.TestFillSqlFromMongo();

            test.RunSomeQueries();
        }
    }
}
