using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Giparus.VirtualAvl
{
    public class VirtualAvl
    {
        #region Fields
        private readonly int _port;
        private readonly IPAddress _server;
        private readonly static object Locker = new object();
        private readonly TcpClient _client;
        #endregion


        #region Properties
        public int DeviceId { get; private set; }
        #endregion


        public VirtualAvl(int id, int port, IPAddress server)
        {
            _port = port;
            _server = server;
            _client = new TcpClient();

            this.DeviceId = id;
        }


        public void StartAmi()
        {
            while (true)
            {
                try
                {
                    lock (Locker)
                        if (!_client.Connected)
                        {
                            _client.Connect(_server, _port);
                        }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }

                var stream = _client.GetStream();

                var imei = "123456789012345";
                var imeiBuffer = Encoding.ASCII.GetBytes(imei);
                var memoryStream = new MemoryStream();
                memoryStream.WriteByte(0);
                memoryStream.WriteByte(15);
                memoryStream.Write(imeiBuffer, 0, imeiBuffer.Length);

                try { stream.Write(memoryStream.ToArray(), 0,(int)memoryStream.Length); }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }

                // Read command after a connection has been established
                var readBuffer = new byte[1024];
                try
                {
                    var readCounts = stream.Read(readBuffer, 0, readBuffer.Length);
                    var listenerResponse = Encoding.ASCII.GetString(readBuffer, 0, readCounts);

                    //if (listenerResponse == "0001")
                    {
                        var path0 = @"..\..\..\SampleData\321666841.avldata";
                        var path1 = @"..\..\..\SampleData\400600440.avldata";
                        var path2 = @"..\..\..\SampleData\1399292567.avldata"; 

                        var gpsData = File.ReadAllBytes(path0);

                        stream.Write(gpsData, 0, gpsData.Length);
                    }
                    //else if (listenerResponse == "0000")
                    {
                    }

                    Thread.Sleep(1 * 60 * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }

            // ReSharper disable once FunctionNeverReturns
        }
    }
}
