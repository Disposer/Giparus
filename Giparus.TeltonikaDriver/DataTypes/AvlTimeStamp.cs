using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Giparus.TeltonikaDriver.DataTypes
{
    public class AvlTimeStamp:ISelfParser
    {
        private byte[] _timeStamp;

        void ISelfParser.ParseAndFill(BinaryReader reader)
        {
            _timeStamp = reader.ReadBytes(8);
        }

        public DateTime Time
        {
            get
            {
                var milliSeconds = BitConverter.ToUInt64(_timeStamp.Reverse().ToArray(), 0);
                var dateTime=new DateTime(1970,1,1,0,0,0,0).AddMilliseconds(milliSeconds);
                return dateTime;
            }
        }

        public override string ToString()
        {
            return this.Time.ToString(CultureInfo.InvariantCulture);
        }

        public static implicit operator DateTime(AvlTimeStamp timestamp)
        {
            return timestamp.Time;
        }

        private AvlTimeStamp() { }
    }
}
