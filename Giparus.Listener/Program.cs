using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Giparus.Core.Logger;
using Giparus.Core.Settings;

namespace Giparus.Listener
{
    public delegate void AmiDeviceConnectedEventHandler(string amiId);

    public class Program
    {
        #region Fields
        private bool _started;
        private readonly int _port;
        private TcpListener _listener;
        private readonly object _locker = new object();

        public Program(int port)
        {
            _port = port;
        }

        #endregion


        static void Main()
        {
            Console.WriteLine("Listening...");

            var program = new Program(2020);
            program.Listen();

            Console.WriteLine("Pree any key to exit");
            Console.ReadKey();
        }


        #region Listen & Handle
        public void Listen()
        {
            ThreadPool.QueueUserWorkItem(this.ListenToPort, null);
        }

        private void ListenToPort(object state)
        {
            if (_started) { GiparusLog.LogError("'ListenToPort' went wrong", "Duplicate listener", GiparusLog.Listener); }
            else _started = true;

            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();

                GiparusLog.Log("Listenning at: " + _listener.LocalEndpoint);
            }
            catch (Exception exception)
            {
                GiparusLog.LogError("'_listener.Start()' went wrong", exception.Message);
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
                    GiparusLog.Log(DateTime.Now + " @Client connected at " + client.Client.RemoteEndPoint);

                    //Create a thread to handle communication
                    var clientThread = new Thread(this.ClientHandler)
                    {
                        IsBackground = true,
                        Name = counter++.ToString(CultureInfo.InvariantCulture)
                    };
                    clientThread.Start(client);
                }
                catch (Exception exception) { GiparusLog.LogError("'_listener.AcceptTcpClient()' went wrong", exception.Message); }
            }
        }

        private void ClientHandler(object client)
        {
            var tcpClient = (TcpClient)client;
            //tcpClient.NoDelay = true;

            var stream = tcpClient.GetStream();
            var buffer = new byte[ListenerSettings.Instance.BufferSize];
            try
            {
                // Get first ResponseBase                    
                var length = stream.Read(buffer, 0, ListenerSettings.Instance.BufferSize);

                if (buffer[0] != 0 || buffer[1] != 0x0f) throw new InvalidDataException("Wrong IMEI");
                var imei = Encoding.ASCII.GetString(buffer, 0, length);
                Console.WriteLine(imei);

                // 1 as byte array response will determine if it would accept data from this module.
                var answer = BitConverter.GetBytes(1);
                stream.Write(answer, 0, answer.Length);

                buffer.Initialize();

                length = stream.Read(buffer, 0, ListenerSettings.Instance.BufferSize);

                var file = new byte[length];
                Array.Copy(buffer, file, length);
                File.WriteAllBytes("c:\\" + new Random().Next() + ".avldata", file);

                var state = State.StartReading;
                var index = 0;
                var avrDataArrayLenght = -1;
                var codeId = -1;
                var dataElements = -1;
                var crc = -1;
                byte[] data = null;

                while (index < length)
                {
                    switch (state)
                    {
                        case State.StartReading:
                            var starter = BitConverter.ToInt32(buffer, index);
                            if (starter != 0) throw new DataException(string.Format("Wrong data at '{0}'", index));
                            index += 4;

                            avrDataArrayLenght = BitConverter.ToInt32(buffer, index);
                            index += 4;

                            state = State.ReadDataArray;
                            break;

                        case State.ReadInfo:                          
                            var dataSize = length - index - 1; // Last 1 is for crc
                            data = new byte[dataSize];

                            Array.Copy(buffer, index, data, 0, dataSize);
                            state = State.ReadDataArray;

                            break;

                        case State.ReadDataArray:
                            codeId = buffer[index++];
                            dataElements = buffer[index++];

                            break;

                        case State.Crc:
                            crc = buffer[length - 1];
                            break;
                    }
                }

                //data = String.Concat(data, Encoding.ASCII.GetString(buffer, 0, length));

            }
            catch (Exception exception)
            {
                //a socket error has occured
                GiparusLog.LogError("'stream.Read()' went wrong", exception.Message);
            }
        }
        #endregion

        //public T Read<T>(byte[] buffer, ref int index) where T : struct
        //{
        //    BitConverter.to
        //    index += Marshal.SizeOf(typeof (T));
        //}
    }

    public enum State
    {
        StartReading,
        ReadInfo,
        ReadDataArray,
        Crc,
    }
}
