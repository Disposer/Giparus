using System.IO;

namespace Giparus.TeltonikaDriver
{
   public class PacketParser
    {
        public static T Parse<T>(BinaryReader reader)where T:IPacket
        {
            return GenericParser.Parse<T>(reader);
        }

        public static T Parse<T>(byte[] rawData) where T : IPacket
        {
            var stream = new MemoryStream(rawData);
            return Parse<T>(stream);
        }

        public static T Parse<T>(Stream stream) where T : IPacket
        {
            var reader = new BinaryReader(stream);
            return GenericParser.Parse<T>(reader);
        }
    }
}
