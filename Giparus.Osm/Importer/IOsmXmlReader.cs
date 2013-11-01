namespace Giparus.Osm.Importer
{
    public interface IOsmXmlReader
    {
        void Open(string connectionstring);
        void Read();
        void ReadAndInsertInBulk();
        void ReadAndInsertAndCheck();
    }
}
