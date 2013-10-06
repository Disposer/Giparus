using System;

namespace Giparus.TeltonikaDriver.Tcp
{
    public enum AvlTcpStatus { Started,Connected, Accepted, Rejected, Terminated };

    public class AvlTcpStatusArgs:EventArgs
    {
        public string Imei { get; private set; }
        public AvlTcpStatus Status { get; private set; }
        public string Extra { get; private set; }

        internal AvlTcpStatusArgs(string imei, AvlTcpStatus status, string extra="")
        {
            this.Imei = imei;
            this.Status = status;
            this.Extra = extra;
        }
    }
}
