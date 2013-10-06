using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Giparus.TeltonikaDriver.DataTypes;

namespace Giparus.TeltonikaDriver.Tcp
{
    public class AvlTcpListener
    {
        private bool _started;
        private readonly int _port;
        private TcpListener _listener;
        private readonly object _locker = new object();


        #region .ctor
        public AvlTcpListener(int port)
        {
            _port = port;
        }
        #endregion


        #region Listen & Handle
        public void Listen()
        {
            ThreadPool.QueueUserWorkItem(this.ListenToPort, null);
        }

        private void ListenToPort(object state)
        {
            if (_started) { Console.WriteLine("'ListenToPort' went wrong: {0}", "Duplicate listener"); }
            else _started = true;

            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();

                Console.WriteLine("Listenning at: " + _listener.LocalEndpoint);
            }
            catch (Exception exception)
            {
                Console.WriteLine("'_listener.Start()' went wrong: {0}", exception.Message);
                throw;
            }

            var counter = 0;
            while (true) // For now, flag is always true
            {
                try
                {
                    if (!_listener.Pending())
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    TcpClient client;
                    lock (_locker) { client = _listener.AcceptTcpClient(); }
                    Console.WriteLine(DateTime.Now + " @Client connected at {0}", client.Client.RemoteEndPoint);

                    //Create a thread to handle communication
                    var clientThread = new Thread(this.ClientHandler)
                    {
                        IsBackground = true,
                        Name = counter++.ToString(CultureInfo.InvariantCulture)
                    };
                    clientThread.Start(client);
                }
                catch (Exception exception) { Console.WriteLine("'_listener.AcceptTcpClient()' went wrong" + exception.Message); }
            }
        }

        private void ClientHandler(object client)
        {
            var tcpClient = (TcpClient)client;
            //tcpClient.NoDelay = true;

            var stream = tcpClient.GetStream();

            var imei = this.ReadImei(stream);
            Console.WriteLine(imei);

            this.Accept(stream);
            while (true)
            {
                try
                {
                    var memoryStream = this.ReadData(stream);
                    if (memoryStream.Length == 0)
                    {
                        Console.WriteLine("Comm. Terminated");
                        break;
                    }

                    var packet = PacketParser.Parse<AvlTcpPacket>(memoryStream);
                    if (memoryStream.Position != memoryStream.Length)
                        throw new InvalidDataException("unfinished avl packet");

                    var avlData = packet.GetAvlDataArray().GetAvlData();

                    var avlDataArray = avlData as AvlData[] ?? avlData.ToArray();
                    foreach (var data in avlDataArray)
                        Console.WriteLine(data.ToString());

                    this.ResponseReverse(avlDataArray.Count(), stream);

                    Console.WriteLine("Response with Count:'{0}' sent", avlDataArray.Count());
                    Console.WriteLine(new string('-', 79));
                    //var file = new byte[length];
                    //Array.Copy(buffer, file, length);
                    //File.WriteAllBytes("c:\\" + new Random().Next() + ".avldata", file);

                }
                catch (Exception exception)
                {
                    //TODO: write (int)0 as response to resend the packet

                    //a socket error has occured
                    Console.WriteLine("'stream.Read()' went wrong" + exception.Message);
                }
            }
        }       
        #endregion

        private void Accept(NetworkStream stream)
        {
            // 1 as byte array response will determine if it would accept data from this module.
            this.Response(1, stream);
        }

        private void Response(int count, NetworkStream stream)
        {
            var answer = BitConverter.GetBytes(count).ToArray();
            stream.Write(answer, 0, answer.Length);
        }

        private void ResponseReverse(int count, NetworkStream stream)
        {
            var answer = BitConverter.GetBytes(count).Reverse().ToArray();
            stream.Write(answer, 0, answer.Length);
        }

        private void Reject(NetworkStream stream)
        {
            // 0 as byte array response will determine if it would reject data from this module.
            this.Response(0, stream);
        }

        private string ReadImei(NetworkStream stream)
        {
            var buffer = new byte[128]; //IMEI buffer
            var length = stream.Read(buffer, 0, buffer.Length);

            if (buffer[0] != 0 || buffer[1] != 0x0f) throw new InvalidDataException("Wrong IMEI");
            var imei = Encoding.ASCII.GetString(buffer, 2, length - 2);
            return imei;
        }

        private MemoryStream ReadData(NetworkStream stream)
        {
            var buffer = new byte[4096];

            var memoryStream = new MemoryStream();

            while (true)
            {
                var length = stream.Read(buffer, 0, buffer.Length);
                if (length == 0) break;
                memoryStream.Write(buffer, 0, length);
                if (!stream.DataAvailable) break;
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
