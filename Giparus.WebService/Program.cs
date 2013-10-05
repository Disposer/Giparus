using System;
using System.Net.Sockets;

namespace Giparus.WebService
{
    public class Program
    {
        static void Main()
        {
        }


        #region Listen & Handle
        public void Listen()
        {
            ThreadPool.QueueUserWorkItem(this.ListenToPort, null);
        }

        private void ListenToPort(object state)
        {
            if (_started) { ElemonLog.LogError("'ListenToPort' went wrong", "Duplicate listener", ElemonLog.Listener); }
            else _started = true;

            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();

                ElemonLog.Log("Listenning at: " + _listener.LocalEndpoint);
            }
            catch (Exception exception)
            {
                ElemonLog.LogError("'_listener.Start()' went wrong", exception.Message);
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
                    ElemonLog.Log("Client connected at " + client.Client.RemoteEndPoint);

                    //Create a thread to handle communication
                    var clientThread = new Thread(this.ClientHandler)
                    {
                        IsBackground = true,
                        Name = counter++.ToString(CultureInfo.InvariantCulture)
                    };
                    clientThread.Start(client);
                }
                catch (Exception exception) { ElemonLog.LogError("'_listener.AcceptTcpClient()' went wrong", exception.Message); }
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
                stream.Read(buffer, 0, ListenerSettings.Instance.BufferSize);

                var firstResponseString = Encoding.ASCII.GetString(buffer, 0, 10);
                var amiId = firstResponseString.Replace("\0", string.Empty);

                if (!this.IsValidId(amiId))
                {
                    ElemonLog.LogError("'Listener.ClientHandler()' went wrong", "Invalid amiId:" + amiId);
                    return;
                }
                ElemonLog.Log(amiId + " is connected.", ElemonLog.Listener, LogType.Success);

                if (this.AmiDeviceConnectedHandler == null) return;

                this.AddDeviceTcpClient(amiId, tcpClient);
                this.AmiDeviceConnectedHandler(amiId);
            }
            catch (Exception exception)
            {
                //a socket error has occured
                ElemonLog.LogError("'stream.Read()' went wrong", exception.Message);
            }
        }
        #endregion
    }
}
