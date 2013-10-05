using System.Collections.Generic;
using System.IO;

namespace Giparus.TeltonikaDriver.DataTypes
{
    public class IoBlock
    {
        private readonly List<IoPair> _list = new List<IoPair>();

        public IoType Type { get; private set; }
        public byte Count { get; private set; }

        private void ReadIoPair(BinaryReader reader)
        {
            var id = reader.ReadByte();
            var value = reader.ReadBytes((int)this.Type);

            var pair = new IoPair(id, value);
            _list.Add(pair);
        }

        public IEnumerable<IoPair> GetIoPairs()
        {
            return _list;
        }

        internal IoBlock(BinaryReader reader, IoType type)
        {
            this.Count = reader.ReadByte();
            this.Type = type;

            for (var index = 0; index < this.Count; index++)
                this.ReadIoPair(reader);
        }
    }
}
