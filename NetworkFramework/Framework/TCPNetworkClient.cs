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
    public class TCPNetworkClient
    {
        public readonly TCPClientEventHandler tcpClientEventHandler;
        public int maxBufferSize = 4096;
        public EndPoint clientEndpoint;

        private byte[] dataBuffer;

        private readonly TcpClient client;
        /// <summary>
        /// Constructs a new client class
        /// </summary>
        /// <param name="_client">TCPClient instance that is used.</param>
        public TCPNetworkClient(TcpClient _client)
        {
            client = _client;
            dataBuffer = new byte[maxBufferSize];
            clientEndpoint = _client.Client.RemoteEndPoint;
            tcpClientEventHandler = new TCPClientEventHandler();
        }

        public TCPNetworkClient()
        {
            client = new TcpClient();
            dataBuffer = new byte[maxBufferSize];
            clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
            tcpClientEventHandler = new TCPClientEventHandler();
        }

        public void Connect(string _ipAddress, int _port)
        {
            if(!client.Connected)
            {
                client.BeginConnect(_ipAddress, _port, OnConnectedToServer, null);
            }
        }

        private void OnConnectedToServer(IAsyncResult ar)
        {
            tcpClientEventHandler.OnConnectToServer += TcpClientEventHandler_OnConnectToServer;
            tcpClientEventHandler.ConnectToServer();
        }

        private async Task TcpClientEventHandler_OnConnectToServer()
        {
            Console.WriteLine("Connected");
        }

        public void StartRecieving()
        {
            NetworkStream Stream = client.GetStream();

            Stream.BeginRead(dataBuffer, 0, maxBufferSize, OnTcpDataRecieved, null);
        }

        private void OnTcpDataRecieved(IAsyncResult ar)
        {
            int BytesRead = client.GetStream().EndRead(ar);
            client.GetStream().BeginRead(dataBuffer, 0, maxBufferSize, OnTcpDataRecieved, null);

            tcpClientEventHandler.DataRecieved(new TCPPacket(dataBuffer));
        }
    }
}
