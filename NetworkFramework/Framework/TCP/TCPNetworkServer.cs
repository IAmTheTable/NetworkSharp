using System;
using System.Net;
using System.Linq;
using System.Timers;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace NetworkSharp.Framework.TCP
{
    public class TCPNetworkServer
    {
        /// <summary>
        /// Fired when a client is connected to the TCP server.
        /// </summary>
        public event Action<TCPNetworkClient> OnClientConnected;
        /// <summary>
        /// Fired when a client sends data to the server.
        /// </summary>
        public event Action<TCPPacket> OnDataReceived;
        /// <summary>
        /// Fired when a client disconnects from the server.
        /// </summary>
        public event Action<int> OnClientDisconnected;
        /// <summary>
        /// Max amount of bytes that can be sent/Received in a single buffer.
        /// </summary>
        public int maxRecieveBuffer = 4096;
        /// <summary>
        /// A list of clients that are currently connected to the server.
        /// </summary>
        public Dictionary<int, TCPNetworkClient> ConnectedClients;

        private TcpListener tcpListener;
        private readonly int serverPort;

        /// <summary>
        /// Create a new TCPServer and start listening on the specified port.
        /// </summary>
        /// <param name="_port">The port you wish to listen on.</param>
        /// <returns>The TCPNetworkServer instance.</returns>
        public TCPNetworkServer(int _port)
        {
            ConnectedClients = new();
            serverPort = _port;

            try
            {
                // Debugging
                Logger.Log(Logger.Loglevel.Verbose, "[Server] Binding port...");
                tcpListener = new(IPAddress.Any, serverPort); // Create the server instance
                Logger.Log(Logger.Loglevel.Verbose, "[Server] Bound port.");
            }
            catch (SocketException e)
            {
                // Catch any exceptions and inform the user.
                Logger.Log(Logger.Loglevel.Error, $"[Server] There was an error while starting server at {tcpListener.LocalEndpoint}.\n{e.Message}");
            }
        }

        /// <summary>
        /// Begin listening for connections on port you specified.
        /// </summary>
        public void Listen()
        {
            new Thread(() =>
            {
                Logger.Log(Logger.Loglevel.Verbose, "[Server] Starting server...");
                // start the server
                tcpListener.Start();
                Logger.Log(Logger.Loglevel.Verbose, $"[Server] Server started at {tcpListener.LocalEndpoint}.");
                tcpListener.BeginAcceptTcpClient(TCPClientAcceptCallback, null);
                Thread.Sleep(-1);
            }).Start();
        }

        /// <summary>
        /// Send a packet to the target client.
        /// </summary>
        /// <param name="_clientIdx">The client index (in the client list) you wish to send the packet to.</param>
        /// <param name="_packet">The packet you wish to send.</param>
        public void SendPacket(int _clientIdx, TCPPacket _packet)
        {
            try
            {
                if (!ConnectedClients.ContainsKey(_clientIdx))
                {
                    Logger.Log(Logger.Loglevel.Error, $"Client id {_clientIdx} not connected or not found.");
                    return;
                }

                ConnectedClients.TryGetValue(_clientIdx, out TCPNetworkClient TargetClient);

                if (TargetClient.GetClient().Connected && TargetClient.GetClient().GetStream().CanWrite)
                    TargetClient.SendPacket(_packet);
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Loglevel.Error, $"There was an error while trying to send a packet to a client.\n{e.Message}");
            }
        }

        /// <summary>
        /// Send a packet to the target client.
        /// </summary>
        /// <param name="_client">The client you wish to send the packet to.</param>
        /// <param name="_packet">The packet you wish to send.</param>
        public void SendPacket(TCPNetworkClient _client, TCPPacket _packet)
        {
            try
            {
                if (_client.GetClient().Connected)
                {
                    _client.SendPacket(_packet);
                }
                else
                {
                    Logger.Log(Logger.Loglevel.Error, "There was an error while trying to send a packet to a client.\nClient not connected.");
                }
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Loglevel.Error, $"There was an error while trying to send a packet to a client.\n{e.Message}");
            }
        }
        private void TCPClientAcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Get the connected client
                TcpClient ConnectedClient = tcpListener.EndAcceptTcpClient(ar);
                tcpListener.BeginAcceptTcpClient(TCPClientAcceptCallback, null); // Start accepting clients
                var NewClient = new TCPNetworkClient(ConnectedClient);
                // Handle our call then send the event out 
                TcpServerEventHandler_OnClientConnected(NewClient);
                OnClientConnected(NewClient);
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Loglevel.Error, $"[Server] There was an error while a client tried connecting. \n-------------------------\n{e.Message}\n-------------------------");
            }
        }

        /// <summary>
        /// Called when a client is connected to the server.
        /// </summary>
        /// <param name="_client">The client instance that is used.</param>
        private void TcpServerEventHandler_OnClientConnected(TCPNetworkClient _client)
        {
            int ClientIndex = ConnectedClients.Count;
            ConnectedClients.Add(ClientIndex, _client);

            new Thread(() =>
            {
                TcpClient ConnectedClientBase = _client.GetClient();
                // while the connection is established
                while (GetState(ConnectedClientBase) == TcpState.Established)
                    Thread.Sleep(0);

                // Remove the client and call the function
                ConnectedClients.Remove(ClientIndex);
                OnClientDisconnected(ClientIndex);
            }).Start();

            // Handle the events, even though its on the client; the server will still handle it and it wont persist on both ends.
            _client.OnDataReceived += (arg) => OnDataReceived(arg);
            _client.StartRecieving();
        }

        //https://stackoverflow.com/questions/1387459/how-to-check-if-tcpclient-connection-is-closed
        /// <summary>
        /// Get a TCP Client state
        /// </summary>
        /// <param name="tcpClient">The client to get the state from</param>
        /// <returns>TcpState of the client.</returns>
        private TcpState GetState(TcpClient tcpClient)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }
    }
}
