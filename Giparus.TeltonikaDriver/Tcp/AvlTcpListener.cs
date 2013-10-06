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

        public event EventHandler<AvlTcpCommArgs> DeviceConnected;
        public event EventHandler<AvlTcpStatusArgs> StatusChanged;
        public event EventHandler<AvlTcpDataArgs> DataFetched;
        public event EventHandler<AvlTcpErrorArgs> ErrorOccured;

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
            if (_started) { throw new InvalidOperationException("duplicate listener"); }
            _started = true;

            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            this.OnStatusChanged(string.Empty, AvlTcpStatus.Started);

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
                    this.OnStatusChanged(null, AvlTcpStatus.Connected, string.Format("{0} @Client connected at {1}", DateTime.Now, client.Client.RemoteEndPoint));

                    //Create a thread to handle communication
                    var clientThread = new Thread(this.ClientHandler)
                    {
                        IsBackground = true,
                        Name = counter++.ToString(CultureInfo.InvariantCulture)
                    };
                    clientThread.Start(client);
                }
                catch (Exception exception) { this.OnErrorOccured(null, exception); }
            }
        }

        private void ClientHandler(object client)
        {
            var tcpClient = (TcpClient)client;
            //tcpClient.NoDelay = true;

            var stream = tcpClient.GetStream();

            var imei = this.ReadImei(stream);

            var commArgs = this.OnDeviceCnnected(imei);
            if (commArgs.CommunicationAccepted)
            {
                this.Accept(stream);
                this.OnStatusChanged(imei, AvlTcpStatus.Accepted);
            }
            else
            {
                this.Reject(stream);
                this.OnStatusChanged(imei, AvlTcpStatus.Rejected);
            }
            

            while (true)
            {
                try
                {
                    var memoryStream = this.ReadData(stream);
                    if (memoryStream.Length == 0)
                    {
                        this.OnStatusChanged(imei, AvlTcpStatus.Terminated);
                        break;
                    }

                    var packet = PacketParser.Parse<AvlTcpPacket>(memoryStream);
                    if (memoryStream.Position != memoryStream.Length)
                        throw new InvalidDataException("unfinished avl packet");

                    var avlData = packet.GetAvlDataArray();
                    this.OnDataFetched(imei, avlData);

                    this.ResponseReverse(avlData.NumberOfData1, stream);
                }
                catch (Exception exception)
                {
                    //TODO: write (int)0 as response to resend the packet

                    //a socket error has occured
                    this.OnErrorOccured(imei, exception);
                }
            }
        }

        protected AvlTcpCommArgs OnDeviceCnnected(string imei)
        {
            var commArgs = new AvlTcpCommArgs(imei);
            if (this.DeviceConnected != null)
                this.DeviceConnected(this, commArgs);

            return commArgs;
        }

        protected AvlTcpStatusArgs OnStatusChanged(string imei, AvlTcpStatus status, string extra = "")
        {
            var statusArgs = new AvlTcpStatusArgs(imei, status, extra);
            if (this.StatusChanged != null)
                this.StatusChanged(this, statusArgs);

            return statusArgs;
        }

        protected AvlTcpDataArgs OnDataFetched(string imei, AvlDataArray data)
        {
            var dataArgs = new AvlTcpDataArgs(imei, data);
            if (this.DataFetched != null)
                this.DataFetched(this, dataArgs);

            return dataArgs;
        }

        protected AvlTcpErrorArgs OnErrorOccured(string imei, Exception exception)
        {
            var errorArgs = new AvlTcpErrorArgs(imei, exception);
            if (this.ErrorOccured != null)
                this.ErrorOccured(this, errorArgs);

            return errorArgs;
        }

        #endregion

        private void Accept(NetworkStream stream)
        {
            // 1 as byte array response will determine if it would accept data from this module.
            this.Response(1, stream);
        }

        private void Reject(NetworkStream stream)
        {
            // 0 as byte array response will determine if it would reject data from this module.
            this.Response(0, stream);
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
