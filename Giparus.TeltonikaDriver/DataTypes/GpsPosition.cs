using System;

namespace Giparus.TeltonikaDriver.DataTypes
{
    public enum CoordinationSide { North, South, East, West }
    public enum CoordinationMode{Latitude,Longitude}

    public class GpsPosition
    {
        public CoordinationMode Mode { get; private set; }
        public CoordinationSide Side { get; private set; }
        public int Value { get; private set; }
        public decimal Standard{get;private set;}
        public int Degrees { get; private set; }
        public int Minutes { get; private set; }
        public int Seconds { get; private set; }

        public GpsPosition(int value,CoordinationMode mode)
        {
            this.Mode=mode;
            this.Value = value;

            this.Side = this.Mode==CoordinationMode.Longitude
                ? this.Value > 0 ? CoordinationSide.North : CoordinationSide.South
                : this.Value > 0 ? CoordinationSide.East : CoordinationSide.West;

            this.Standard = Math.Abs((decimal)this.Value) / 10000000;

            this.Seconds = (int)Math.Round(this.Standard * 3600);
            this.Degrees = this.Seconds / 3600;
            this.Seconds = Math.Abs(this.Seconds % 3600);
            this.Minutes = this.Seconds / 60;
            this.Seconds %= 60;
        }

        public override string ToString()
        {
            return string.Format("{0} ° {1}", this.Standard, this.Side);
        }
    }
}
