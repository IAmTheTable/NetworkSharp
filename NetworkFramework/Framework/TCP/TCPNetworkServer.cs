using System;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

using NetworkFramework.Framework.TCP.Events;
namespace NetworkFramework.Framework.TCP
{
    public class TCPNetworkServer
    {
        /// <summary>
        /// Max amount of bytes that can be sent/Received in a single buffer.
        /// </summary>
        public int maxRecieveBuffer = 4096;
        /// <summary>
        /// The class for handling events.
        /// </summary>
        public readonly TCPServerEventHandler Events;
        /// <summary>
        /// A list of clients that are currently connected to the server.
        /// </summary>
        public List<TCPNetworkClient> ConnectedClients = new();

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

            // Init the event handler
            Events = new TCPServerEventHandler();
            Events.OnClientConnected += TcpServerEventHandler_OnClientConnected;

            new Thread(() =>
            {
                try
                {
                    Console.WriteLine("[Server] Binding port...");
                    tcpListener = new(IPAddress.Any, serverPort);
                    Console.WriteLine("[Server] Bound port.");

                    Console.WriteLine("[Server] Starting server...");
                    tcpListener.Start();

                    Console.WriteLine($"[Server] Server started at {tcpListener.LocalEndpoint}.");
                }
                catch (SocketException e)
                {
                    // Catch any exceptions and inform the user.
                    Console.WriteLine($"[Server] There was an error while starting server at {tcpListener.LocalEndpoint}.\n-------------------------\n{e.Message}\n-------------------------");
                }
                
                tcpListener.BeginAcceptTcpClient(TCPClientAcceptCallback, null);
                Thread.Sleep(-1);

            }).Start();
        }

        private async void TCPClientAcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Get the connected client
                TcpClient ConnectedClient = tcpListener.EndAcceptTcpClient(ar);
                tcpListener.BeginAcceptTcpClient(TCPClientAcceptCallback, null);

                await Events.ClientConnect(new TCPNetworkClient(ConnectedClient));
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Server] There was an error while a client tried connecting. \n-------------------------\n{e.Message}\n-------------------------");
            }
        }

        public static TCPNetworkServer Create(int _port) => new(_port);

        private async Task TcpServerEventHandler_OnClientConnected(TCPNetworkClient _client)
        {
            ConnectedClients.Add(_client);
            await Task.Delay(0);

            // Handle the events, even though its on the client; the server will still handle it and it wont persist on both ends.
            _client.Events.OnDataReceived += TcpClientEventHandler_OnDataReceived;
            _client.StartRecieving();
        }
        /// <summary>
        /// When the server recieves data.
        /// </summary>
        /// <param name="arg">The data that is Received</param>
        /// <returns>None</returns>
        private async Task TcpClientEventHandler_OnDataReceived(TCPPacket arg) => await Events.DataReceived(arg);
    }
}
