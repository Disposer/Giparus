using System;
using System.Data;
using System.Linq;
using Giparus.Listener.Model;
using Giparus.TeltonikaDriver.DataTypes;
using Giparus.TeltonikaDriver.Tcp;
using ServiceStack.OrmLite;

namespace Giparus.Listener
{
    public delegate void AmiDeviceConnectedEventHandler(string amiId);

    public class Program
    {
        private static IDbConnection _database;

        static void Main()
        {
            Console.WriteLine("Initializeing Database.");
            const string conectionString = "Initial Catalog=ADB;Data Source=Hecarim\\MainServer;Integrated Security=true;";
            var factory = new OrmLiteConnectionFactory(conectionString, SqlServerDialect.Provider);
            _database = factory.OpenDbConnection();

            Console.WriteLine("Creating Tables");
            _database.CreateTable(false, typeof(AvlDataModel));

            Console.WriteLine("Conection established");

            Console.WriteLine("Listening...");
            var listener = new AvlTcpListener(2020);

            listener.DataFetched += listener_DataFetched;
            listener.DeviceConnected += listener_DeviceConnected;
            listener.ErrorOccured += listener_ErrorOccured;
            listener.StatusChanged += listener_StatusChanged;

            listener.Listen();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }


        static void listener_StatusChanged(object sender, AvlTcpStatusArgs e)
        {
            Console.WriteLine("Imei: {0}, Status: {1}, Info: {2}", e.Imei, e.Status, e.Extra);
        }

        static void listener_ErrorOccured(object sender, AvlTcpErrorArgs e)
        {
            Console.WriteLine("Error: {0}", e.Exception);
        }

        static void listener_DeviceConnected(object sender, AvlTcpCommArgs e)
        {
            e.Accept(); //TODO:filtering here

            Console.WriteLine("Imei: {0}, Accepted: {1}", e.Imei, e.CommunicationAccepted);
        }

        static void listener_DataFetched(object sender, AvlTcpDataArgs e)
        {
            Console.WriteLine("Fetched Data from '{0}' at '{1}':", e.Imei, e.FetchedTimeStamp);
            foreach (var datium in e.Data.GetAvlData())
            {
                Console.WriteLine("Adding data to db: {0}", datium);

                var avl = new AvlDataModel
                {
                    Id = Guid.NewGuid(),
                    Iemi = e.Imei,
                    Time = datium.TimeStamp.Time,
                    Altitude = datium.GpsElement.Altitude,
                    Angle = datium.GpsElement.Angle,
                    Latitude = datium.GpsElement.Latitude,
                    Longitude = datium.GpsElement.Longitude,
                    SatelliteCount = datium.GpsElement.SatelliteCount,
                    Speed = datium.GpsElement.Speed,
                    Priority = (int)datium.Priority,
                };

                var items = _database.Where<AvlDataModel>(c => (c.Iemi == avl.Iemi) && (c.Time == avl.Time));
                if (items.Any()) continue;
                _database.Insert(avl);
            }
        }
    }
}
