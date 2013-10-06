using System;
using Giparus.TeltonikaDriver.Tcp;

namespace Giparus.Listener
{
    public delegate void AmiDeviceConnectedEventHandler(string amiId);

    public class Program
    {
        static void Main()
        {
            Console.WriteLine("Listening...");

            var listener = new AvlTcpListener(2020);

            listener.DataFetched += listener_DataFetched;
            listener.DeviceConnected += listener_DeviceConnected;
            listener.ErrorOccured += listener_ErrorOccured;
            listener.StatusChanged += listener_StatusChanged;

            listener.Listen();

            Console.WriteLine("Pree any key to exit");
            Console.ReadKey();
        }

        static void listener_StatusChanged(object sender, AvlTcpStatusArgs e)
        {
            Console.WriteLine("Imei: {0}, Status: {1}, Info: {2}",e.Imei,e.Status,e.Extra);
        }

        static void listener_ErrorOccured(object sender, AvlTcpErrorArgs e)
        {
            Console.WriteLine("Error: {0}", e.Exception);
        }

        static void listener_DeviceConnected(object sender, AvlTcpCommArgs e)
        {
            e.Accept(); //TODO:filtering here

            Console.WriteLine("Imei: {0}, Status:{1}", e.Imei, e.CommunicationAccepted);
        }

        static void listener_DataFetched(object sender, AvlTcpDataArgs e)
        {
            Console.WriteLine("Fetched Data from '{0}' at '{1}':", e.Imei, e.FetchedTimeStamp);
            foreach (var datum in e.Data.GetAvlData())
                Console.WriteLine(datum);
        }
    }
}
