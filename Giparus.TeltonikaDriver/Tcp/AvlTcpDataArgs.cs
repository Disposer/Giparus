using System;
using Giparus.TeltonikaDriver.DataTypes;

namespace Giparus.TeltonikaDriver.Tcp
{
    public class AvlTcpDataArgs:EventArgs
    {
        public string Imei { get; private set; }
        public AvlDataArray Data { get; private set; }
        public DateTime FetchedTimeStamp { get; private set; }

        public AvlTcpDataArgs(string imei, AvlDataArray data)
        {
            this.Imei = imei;
            this.Data = data;
            this.FetchedTimeStamp = DateTime.Now;
        }
    }
}
