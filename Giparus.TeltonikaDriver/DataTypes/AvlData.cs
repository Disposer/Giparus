using System.IO;

namespace Giparus.TeltonikaDriver.DataTypes
{
    public enum AvlDataPriority : byte{Low=0,High=1,Panic=2,Security=3};

    public class AvlData:ISelfParser
    {
        public AvlTimeStamp TimeStamp { get; private set; }
        public AvlDataPriority Priority{get;private set;}
        public GpsElement GpsElement { get; private set; }
        public IoElement IoElement { get; private set; }

        void ISelfParser.ParseAndFill(BinaryReader reader)
        {
            this.TimeStamp = GenericParser.Parse<AvlTimeStamp>(reader);
            this.Priority = (AvlDataPriority)reader.ReadByte();
            this.GpsElement = GenericParser.Parse<GpsElement>(reader);
            this.IoElement = GenericParser.Parse<IoElement>(reader);
        }

        private AvlData() { }
    }
}
