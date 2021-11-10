using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using NetworkFramework.Framework.TCP.Events;
namespace NetworkFramework.Framework.TCP
{
    public class TCPNetworkClient
    {
        public readonly TCPClientEventHandler Events;
        public int maxBufferSize = 4096;
        public EndPoint clientEndpoint;
        public EndPoint serverEndpoint;

        private readonly byte[] dataBuffer;
        private bool isReading = false;
        private readonly TcpClient client;
        private NetworkStream stream;
        /// <summary>
        /// Constructs a new client class
        /// </summary>
        /// <param name="_client">TCPClient instance that is used.</param>
        public TCPNetworkClient(TcpClient _client)
        {
            client = _client;
            dataBuffer = new byte[maxBufferSize];
            clientEndpoint = _client.Client.RemoteEndPoint;
            Events = new TCPClientEventHandler();
            stream = client.GetStream();
        }

        public TCPNetworkClient()
        {
            client = new TcpClient();
            dataBuffer = new byte[maxBufferSize];
            clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
            Events = new TCPClientEventHandler();
        }
        /// <summary>
        /// Connect to a server with a specified IP and Port.
        /// </summary>
        /// <param name="_ipAddress">IPAddress of the server you are connecting to.</param>
        /// <param name="_port">The port of the server you are connecting to.</param>
        public void Connect(string _ipAddress, uint _port)
        {
            if (!client.Connected)
                client.BeginConnect(_ipAddress, (int)_port, OnConnectedToServer, null);
            else
                Console.WriteLine("[Client] Already connected to the server.");

            serverEndpoint = new IPEndPoint(IPAddress.Parse(_ipAddress), (int)_port);
        }

        private async void OnConnectedToServer(IAsyncResult ar)
        {
            stream = client.GetStream();

            Events.OnConnectToServer += TcpClientEventHandler_OnConnectToServer;
            await Events.ConnectToServer(serverEndpoint);
        }

        private async Task TcpClientEventHandler_OnConnectToServer(EndPoint _endpoint)
        {
            Console.WriteLine("[Client] Connected.");
            await Task.Delay(0);
        }

        public void StartRecieving()
        {
            if (!isReading)
                if (stream.CanRead)
                {
                    isReading = true;
                    stream.BeginRead(dataBuffer, 0, maxBufferSize, OnTcpDataReceived, null);
                }
                else
                    Console.WriteLine("[Client] Failed to read stream: Cannot read the stream.");
            else
                Console.WriteLine("[Client] Already reading the stream.");
        }

        public void StopRecieving()
        {
            if (isReading)
                isReading = false;
            else
                Console.WriteLine("[Client] Not reading data.");
        }

        public void SendPacket(TCPPacket _packet)
        {
            if (client.Connected)
                if (stream.CanWrite)
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.GetLength(), OnFinishedWriting, null);
                else
                    Console.WriteLine("[Client] Failed to send packet: Cannot write to the stream.");
            else
                Console.WriteLine("[Client] Not connected to the server.");
        }

        private void OnFinishedWriting(IAsyncResult ar) => stream.EndWrite(ar);

        private async void OnTcpDataReceived(IAsyncResult ar)
        {
            int BytesRead = client.GetStream().EndRead(ar);

            if (!isReading)
                client.GetStream().BeginRead(dataBuffer, 0, maxBufferSize, OnTcpDataReceived, null);

            Events.OnDataReceived += TcpClientEventHandler_OnDataReceived;

            TCPPacket RecivedPacket = new(dataBuffer);

            int PacketSize = RecivedPacket.ReadInt();

            if (BytesRead != PacketSize)
            {
                Console.WriteLine("[Client] Packet size mismatch, dropping...");
                return;
            }

            await Events.DataReceived(RecivedPacket);
        }

        private async Task TcpClientEventHandler_OnDataReceived(TCPPacket arg)
        {
            await Task.Delay(0);
        }
    }
}
