using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        /// Max amount of bytes that can be sent/Received in a single buffer.
        /// </summary>
        public int maxRecieveBuffer = 4096;
        /// <summary>
        /// A list of clients that are currently connected to the server.
        /// </summary>
        public Dictionary<int, TCPNetworkClient> ConnectedClients = new();

        private TcpListener tcpListener;
        private readonly int serverPort;
        /// <summary>
        /// Initializes a new instance of a server to use.
        /// </summary>
        /// <param name="_port">Port to bind to</param>
        /// <returns></returns>
        private TCPNetworkServer(int _port)
        {
            serverPort = _port;
            new Thread(() =>
            {
                try
                {
                    // Debugging
                    Logger.Log(Logger.Loglevel.Verbose, "[Server] Binding port...");
                    tcpListener = new(IPAddress.Any, serverPort); // Create the server instance
                    Logger.Log(Logger.Loglevel.Verbose, "[Server] Bound port.");
                    Logger.Log(Logger.Loglevel.Verbose, "[Server] Starting server...");
                    // Init the event handler
                    OnClientConnected += TcpServerEventHandler_OnClientConnected;
                    // start the server
                    tcpListener.Start();
                    Logger.Log(Logger.Loglevel.Verbose, $"[Server] Server started at {tcpListener.LocalEndpoint}.");
                }
                catch (SocketException e)
                {
                    // Catch any exceptions and inform the user.
                    Logger.Log(Logger.Loglevel.Error, $"[Server] There was an error while starting server at {tcpListener.LocalEndpoint}.\n{e.Message}");
                }

                tcpListener.BeginAcceptTcpClient(TCPClientAcceptCallback, null);
                Thread.Sleep(-1);

            }).Start();
        }

        private void TCPClientAcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Get the connected client
                TcpClient ConnectedClient = tcpListener.EndAcceptTcpClient(ar);
                tcpListener.BeginAcceptTcpClient(TCPClientAcceptCallback, null); // Start accepting clients
                // Fire the client connected event
                OnClientConnected(new TCPNetworkClient(ConnectedClient));
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Loglevel.Error, $"[Server] There was an error while a client tried connecting. \n-------------------------\n{e.Message}\n-------------------------");
            }
        }

        /// <summary>
        /// Create a new TCPServer and start listening on the specified port.
        /// </summary>
        /// <param name="_port">The port you wish to listen on.</param>
        /// <returns>The TCPNetworkServer instance.</returns>
        public static TCPNetworkServer Create(int _port) => new(_port);

        /// <summary>
        /// Called when a client is connected to the server.
        /// </summary>
        /// <param name="_client">The client instance that is used.</param>
        private void TcpServerEventHandler_OnClientConnected(TCPNetworkClient _client)
        {
            ConnectedClients.Add(ConnectedClients.Count, _client);

            // Handle the events, even though its on the client; the server will still handle it and it wont persist on both ends.
            _client.OnDataReceived += TcpClientEventHandler_OnDataReceived;
            _client.StartRecieving();
        }

        /// <summary>
        /// When the server recieves data.
        /// </summary>
        /// <param name="arg">The data that is Received</param>
        /// <returns>None</returns>
        private void TcpClientEventHandler_OnDataReceived(TCPPacket arg) => OnDataReceived(arg);

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

                TCPNetworkClient TargetClient = ConnectedClients[_clientIdx];

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
                if(_client.GetClient().Connected)
                {
                    _client.SendPacket(_packet);
                }
                else
                {
                    Logger.Log(Logger.Loglevel.Error, "There was an error while trying to send a packet to a client.\nClient not connected.");
                }
            }
            catch(Exception e)
            {
                Logger.Log(Logger.Loglevel.Error, $"There was an error while trying to send a packet to a client.\n{e.Message}");
            }
        }
    }
}
