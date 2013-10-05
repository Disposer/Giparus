using System.IO;

namespace Giparus.TeltonikaDriver
{
    public interface ISelfParser
    {
        void ParseAndFill(BinaryReader reader);
    }
}
