using System;

namespace Giparus.TeltonikaDriver.Tcp
{
    public class AvlTcpErrorArgs:EventArgs
    {
        public string Imei { get; private set; }
        public Exception Exception { get; private set; }

        public AvlTcpErrorArgs(string imei,Exception exception)
        {
            this.Imei = imei;
            this.Exception = exception;
        }
    }
}
