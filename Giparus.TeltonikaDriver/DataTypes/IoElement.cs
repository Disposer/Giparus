using System.Collections.Generic;
using System.IO;

namespace Giparus.TeltonikaDriver.DataTypes
{
   public class IoElement:ISelfParser
    {
       private List<IoBlock> _blocks;

       public byte EventId { get; private set; }
       public byte TotalIoCount { get; private set; }

       void ISelfParser.ParseAndFill(BinaryReader reader)
       {
           this.EventId = reader.ReadByte();
           this.TotalIoCount = reader.ReadByte();

           _blocks = new List<IoBlock>
                {
                 new IoBlock(reader,IoType.OneByte),
                 new IoBlock(reader, IoType.TwoBytes),
                 new IoBlock(reader,IoType.FourBytes),
                 new IoBlock(reader,IoType.EightBytes)
                };
       }

        public IEnumerable<IoBlock> GetIoBlocks()
        {
            return _blocks;
        }

        private IoElement() { }
    }
}
