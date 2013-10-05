using System.Collections.Generic;
using System.IO;

namespace Giparus.TeltonikaDriver.DataTypes
{
    public class AvlDataArray:ISelfParser
    {
        private List<AvlData> _data;

        public byte CodecId { get; private set; }
        public byte NumberOfData1 { get; private set; }
        public byte NumberOfData2 { get; private set; }

        public IEnumerable<AvlData> GetAvlData()
        {
            return _data;
        }

        void ISelfParser.ParseAndFill(BinaryReader reader)
        {
            this.CodecId = reader.ReadByte();
            this.NumberOfData1 = reader.ReadByte();

            _data = new List<AvlData>();

            for (var index = 0; index < this.NumberOfData1; index++)
            {
                var avlData = GenericParser.Parse<AvlData>(reader);
                _data.Add(avlData);
            }

            this.NumberOfData2 = reader.ReadByte();

            if (this.NumberOfData1 != this.NumberOfData2)
                throw new InvalidDataException("record counts are not equal");

            if(this.NumberOfData1!=_data.Count)
                throw new InvalidDataException("record counts are not equal");

            if (reader.BaseStream.Position != reader.BaseStream.Length)
                throw new InvalidDataException("unfinished avl array data");
        }

        private AvlDataArray() { }
    }
}
