using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Giparus.VirtualAvl
{
    public class VirtualAmi
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


        public VirtualAmi(int id, int port, IPAddress server)
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
                var buffer = Encoding.ASCII.GetBytes("Hello");
                try { stream.Write(buffer, 0, buffer.Length); }
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
                    var listenerCoomand = Encoding.ASCII.GetString(readBuffer, 0, readCounts);
                    Console.WriteLine(listenerCoomand);
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
