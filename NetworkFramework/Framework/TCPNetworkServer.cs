using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using System.IO;
using System.Net;
using System.Net.Sockets;
using NetworkFramework.Framework.Events;

namespace NetworkFramework.Framework
{
    public class TCPNetworkServer
    {
        public readonly TCPServerEventHandler tcpServerEventHandler;
        public List<TCPNetworkClient> ConnectedClients = new();

        private TcpListener tcpListener;
        private int serverPort;
        /// <summary>
        /// Initializes a new instance of a server to use.
        /// </summary>
        /// <param name="_port">Port to bind to</param>
        /// <returns></returns>
        private TCPNetworkServer(int _port)
        {
            serverPort = _port;
            tcpServerEventHandler = new TCPServerEventHandler();
            new Thread(() =>
            {
                try
                {
                    Console.WriteLine("Binding port...");
                    tcpListener = new(IPAddress.Any, serverPort);
                    Console.WriteLine("Bound port.");

                    Console.WriteLine("Starting server...");
                    tcpListener.Start();

                    Console.WriteLine($"Server started at {tcpListener.LocalEndpoint}");
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    // Catch any exceptions and inform the user.
                    Console.WriteLine($"There was an error while starting server at {tcpListener.LocalEndpoint}");
                }
                tcpServerEventHandler.OnClientConnected += TcpServerEventHandler_OnClientConnected;

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
                tcpListener.BeginAcceptTcpClient(TCPClientAcceptCallback, null);

                tcpServerEventHandler.ClientConnect(new TCPNetworkClient(ConnectedClient));
            }
            catch(Exception e)
            {
                Console.WriteLine("there was an error while a client tried connecting");
            }
        }
        public static TCPNetworkServer Create(int _port) => new(_port);

        private async Task TcpServerEventHandler_OnClientConnected(TCPNetworkClient _client)
        {
            ConnectedClients.Add(_client);
        }
    }
}
