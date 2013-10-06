using System;
using System.IO;
using System.Linq;
using Giparus.TeltonikaDriver.DataTypes;

namespace Giparus.TeltonikaDriver.Tcp
{
    public class AvlTcpPacket:IPacket
    {
        private byte[] _data;
        private byte[] _crc;
        private byte[] _length;

        public int DataLength
        {
            get { return BitConverter.ToInt32(_length.Reverse().ToArray(), 0); }
        }

        public ushort Crc
        {
            get { return BitConverter.ToUInt16(_crc.Reverse().ToArray(), 0); }
        }

        public byte[] GetRawData()
        {
            return _data.ToArray(); //NOTE:clone the data
        }

        public BinaryReader GetRawDataReader()
        {
            return new BinaryReader(new MemoryStream(this.GetRawData()));
        }

        public T ParseAs<T>() where T : ISelfParser
        {
            return GenericParser.Parse<T>(this.GetRawDataReader());
        }

        public AvlDataArray GetAvlDataArray()
        {
            return this.ParseAs<AvlDataArray>();
        }

        public void CheckCrc()
        {
            var origCrc = BitConverter.ToUInt16(_crc.Reverse().ToArray(),0);
            var res = Crc16.ComputeChecksum(_data);

            if (res != origCrc)
                throw new InvalidDataException("crc mismatch");
        }

        void ISelfParser.ParseAndFill(BinaryReader reader)
        {
            var header = reader.ReadUInt32();
            if (header != 0)
                throw new InvalidDataException("header is not '0000'");

            _length = reader.ReadBytes(4);
            _data = reader.ReadBytes(this.DataLength);
            _crc = reader.ReadBytes(4);

            this.CheckCrc();
        }

        private AvlTcpPacket() { }
    }
}
