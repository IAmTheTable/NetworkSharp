using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;
using NetworkFramework.Framework.Events;

namespace NetworkFramework.Framework
{
    public class TCPNetworkServer
    {
        public readonly TCPServerEventHandler tcpServerEventHandler;

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
                throw new Exception(e.Message);
            }

            tcpServerEventHandler.OnClientConnected += TcpServerEventHandler_OnClientConnected;

            tcpListener.BeginAcceptTcpClient(TCPClientAcceptCallback, null);
        }

        private void TCPClientAcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Get the connected client
                TcpClient ConnectedClient = tcpListener.EndAcceptTcpClient(ar);
                tcpServerEventHandler.ClientConnect(ConnectedClient);
            }
            catch(Exception e)
            {

            }
        }

        public static TCPNetworkServer Create(int _port)
        {

            return new(_port);
        }

        private static Task TcpServerEventHandler_OnClientConnected(TcpClient arg)
        {
            throw new NotImplementedException();
        }
    }
}
