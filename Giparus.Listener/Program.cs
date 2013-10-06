using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using Giparus.TeltonikaDriver.Tcp;
using Giparus.TeltonikaDriver;

namespace Giparus.Listener
{
    public delegate void AmiDeviceConnectedEventHandler(string amiId);

    public class Program
    {
        static void Main()
        {
            Console.WriteLine("Listening...");

            var listener = new AvlTcpListener(2020);
            listener.Listen();

            Console.WriteLine("Pree any key to exit");
            Console.ReadKey();
        }
    }
}
