using System;
using System.Net;
using System.Threading;

namespace Giparus.VirtualAvl
{
    public class AvlVirtualProgram
    {
        public const int COUNT = 1;

        #region Fields
        private static int _port;
        private static IPAddress _server;
        #endregion

        static void Main()
        {
            Initialize();

            Console.WriteLine("VirtualAvl running...............");
            StartClient();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void Initialize()
        {
            _port = 2020;

            Console.Write("Enter ip: ");
            var ip = Console.ReadLine();
            if (!string.IsNullOrEmpty(ip)) _server = IPAddress.Parse(ip);
            else _server = IPAddress.Parse("127.0.0.1");
        }

        private static void StartClient()
        {
            Console.Write("Avl Devices: ");
            // ReSharper disable once AssignNullToNotNullAttribute
            var count = int.Parse(Console.ReadLine());
            for (var counter = 0; counter < count; counter++)
            {
                //Thread.Sleep(5);
                Console.WriteLine("Creating virtual avl: " + counter);
                var virtualAmi = new VirtualAvl(counter, _port, _server);
                var workerThread = new Thread(arg => virtualAmi.StartAmi())
                {
                    Name = "AvlVirtual" + counter,
                    IsBackground = true
                };
                workerThread.Start();
            }
        }
    }
}
