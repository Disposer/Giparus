using System.Xml;

namespace Giparus.Osm.Importer
{
    public interface INodeParser
    {
        void ParseAndFill(XmlReader xml);
    }
}
