using System;
using System.IO;
using System.Linq;

namespace Giparus.TeltonikaDriver.DataTypes
{
    public class GpsElement:ISelfParser
    {
        private byte[] _longitude;
        private byte[] _latitude;
        private byte[] _altitude;
        private byte[] _angle;
        private byte _satellites;
        private byte[] _speed;

        public byte SatelliteCount
        {
            get { return _satellites; }
        }
        public uint Speed
        {
            get { return BitConverter.ToUInt16(_speed.Reverse().ToArray(),0); }
        }
        public GpsPosition Latitude { get; private set; }
        public GpsPosition Longitude { get; private set; }
        public int Altitude
        {
            get { return BitConverter.ToInt16(_altitude.Reverse().ToArray(), 0); }
        }
        public int Angle
        {
            get { return BitConverter.ToInt16(_angle.Reverse().ToArray(), 0); }
        }

        void ISelfParser.ParseAndFill(BinaryReader reader)
        {
            _longitude = reader.ReadBytes(4);
            _latitude = reader.ReadBytes(4);
            _altitude = reader.ReadBytes(2);
            _angle = reader.ReadBytes(2);
            _satellites = reader.ReadByte();
            _speed = reader.ReadBytes(2);

            this.Latitude = new GpsPosition(BitConverter.ToInt32(_latitude.Reverse().ToArray(), 0), CoordinationMode.Latitude);
            this.Longitude = new GpsPosition(BitConverter.ToInt32(_longitude.Reverse().ToArray(), 0), CoordinationMode.Longitude);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", this.Latitude, this.Longitude);
        }

        private GpsElement() { }
    }
}
