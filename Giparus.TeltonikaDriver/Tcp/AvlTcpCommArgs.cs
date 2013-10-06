using System;

namespace Giparus.TeltonikaDriver.Tcp
{
    public class AvlTcpCommArgs:EventArgs
    {
        public string Imei { get; private set; }
        public bool CommunicationAccepted { get; set; }

        public void Accept()
        {
            this.CommunicationAccepted = true;
        }

        public void Reject()
        {
            this.CommunicationAccepted = false;
        }

        internal AvlTcpCommArgs(string imei)
        {
            this.Imei = imei;
            this.CommunicationAccepted = true;
        }
    }
}
