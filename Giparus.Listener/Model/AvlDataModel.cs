using System;
using Giparus.TeltonikaDriver.DataTypes;
using ServiceStack.DataAnnotations;

namespace Giparus.Listener.Model
{
    public class AvlDataModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Iemi { get; set; }
        public DateTime Time { get; set; }

        public int Altitude { get; set; }
        public int Angle { get; set; }
        public GpsPosition Latitude { get; set; }
        public GpsPosition Longitude { get; set; }
        public byte SatelliteCount { get; set; }
        public uint Speed { get; set; }
        public int Priority { get; set; }
    }
}
