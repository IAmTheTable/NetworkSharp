using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkSharp.Framework.UDP
{
    /// <summary>
    /// An easy to use UDP Client for sending and receiving data.
    /// </summary>
    public class UDPNetworkClient
    {
        /// <summary>
        /// Called when the client is connected to the server.
        /// </summary>
        public event Func<Task> OnConnectToServer;
        /// <summary>
        /// Called when the client Received data.
        /// </summary>
        public event Func<UDPPacket, Task> OnDataReceived;
        /// <summary>
        /// The max amount of bytes the client can receive/send at once.
        /// </summary>
        public int MaxBufferSize = 4096;
        /// <summary>
        /// The endpoint of the server.
        /// </summary>
        public IPEndPoint ServerEndpoint { get; private set; }
        /// <summary>
        /// The endpoint of the client
        /// </summary>
        public IPEndPoint LocalEndpoint { get; private set; }
        /// <summary>
        /// The socket used
        /// </summary>
        private readonly Socket client;
        /// <summary>
        /// Is the client already reading data from the network?
        /// </summary>
        private bool isReading;
        /// <summary>
        /// Create a new UDP Client instance(used by the server).
        /// </summary>
        /// <param name="_client"></param>
        public UDPNetworkClient(IPEndPoint _serverEp, IPEndPoint _localEp = null)
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
            LocalEndpoint = _localEp ?? new IPEndPoint(IPAddress.Any, 0);
            ServerEndpoint = _serverEp;
        }

        /// <summary>
        /// Create a new UDP Client instance.
        /// </summary>
        public UDPNetworkClient()
        {
            OnConnectToServer += () => new Task(() => { });
            OnDataReceived += (UDPPacket) => new Task(() => { });
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
        }

        /// <summary>
        /// Connect to a UDP server
        /// </summary>
        /// <param name="_ipAddress">IP of the server</param>
        /// <param name="_port">Port of the server</param>
        public async void Connect(string _ipAddress, uint _port)
        {
            try
            {
                if (ServerEndpoint == null)
                {
                    ServerEndpoint = new(IPAddress.Parse(_ipAddress), (int)_port);
                    client.MulticastLoopback = false;
                    client.Blocking = false;
                    client.EnableBroadcast = false;
                    client.DontFragment = true;
                    await client.ConnectAsync(ServerEndpoint);
                    await OnConnectToServer();
                }
                else
                    Logger.Log(Logger.Loglevel.Warn, "[Client] Client already connected");
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Loglevel.Error, $"[Client] Failed to connect to server: {e.Message}");
            }
        }
        /// <summary>
        /// Start receiving data from the server.
        /// </summary>
        public void StartReceiving()
        {
            if (ServerEndpoint == null)
                Logger.Log(Logger.Loglevel.Warn, "[Client] Failed to read data: Client not connected to the server.");
            else
            {
                if (!isReading)
                {
                    Logger.Log(Logger.Loglevel.Verbose, "[Client] Pre read data");
                    isReading = true;
                    if (ServerEndpoint == null)
                        Logger.Log(Logger.Loglevel.Error, "[Client] Failed to read data: Client not connected to the server.");
                    ReceiveMessage();
                }
                else
                    Logger.Log(Logger.Loglevel.Warn, "[Client] Already reading data.");
            }
        }
        /// <summary>
        /// Stop receiving data from the server.
        /// </summary>
        public void StopReceiving()
        {
            if (isReading)
                isReading = false;
            else
                Logger.Log(Logger.Loglevel.Warn, "[Client] Not reading data.");
        }
        /// <summary>
        /// Send a packet to the server.
        /// </summary>
        /// <param name="_packet">The packet of data you wish to send.</param>
        public async void SendPacket(UDPPacket _packet)
        {
            Console.WriteLine("Sent packet");
            _packet = new(_packet.ToArray());
            _packet.InsertLength();

            Logger.Log(Logger.Loglevel.Verbose, $"[Client] Sending packet: {_packet.GetLength()}");
            //client.SendToAsync(_packet.ToArray(),SocketFlags.None, ServerEndpoint);
            await client.SendAsync(_packet.ToArray(), SocketFlags.None);
        }

        /// <summary>
        /// Get the network socket object used.
        /// </summary>
        /// <returns>The socket this client uses.</returns>
        public Socket GetClient() => client;
        /// <summary>
        /// Locally get the message.
        /// </summary>
        private async void ReceiveMessage()
        {
            byte[] DataBuffer = new byte[MaxBufferSize];
            EndPoint ServerEP = ServerEndpoint;

            SocketReceiveMessageFromResult MessageResult = await client.ReceiveMessageFromAsync(DataBuffer, SocketFlags.None, ServerEP);
            OnClientDataReceived(DataBuffer, MessageResult.RemoteEndPoint, MessageResult.ReceivedBytes);
        }

        /// <summary>
        /// Called when the client receives data.
        /// </summary>
        /// <param name="_dataReceived">Bytes received</param>
        /// <param name="_clientEP">Endpoint of the data source</param>
        /// <param name="_amountDataReceived">The amount of data received</param>
        private async void OnClientDataReceived(byte[] _dataReceived, EndPoint _sourceEP, int _amountDataReceived)
        {
            // Remove 4 because the first 4 bytes are allocated to the packet size, the packet size doesnt take itself into account.
            _amountDataReceived -= 4;

            if (_amountDataReceived < 4)
            {
                Logger.Log(Logger.Loglevel.Warn, "Data received event called but no data is present...");
            }
            if (_sourceEP != ServerEndpoint)
            {
                Logger.Log(Logger.Loglevel.Warn, "Got data that isnt from server?");
                return;
            }

            Array.Resize(ref _dataReceived, _amountDataReceived + 4);

            UDPPacket ReceivedPacket = new(_dataReceived);
            int PacketSize = ReceivedPacket.ReadInt();

            if (_amountDataReceived != PacketSize)
            {
                Logger.Log(Logger.Loglevel.Warn, "[Client] Packet size mismatch, dropping...");
                return;
            }

            await OnDataReceived(ReceivedPacket);

            if (isReading) // Check if we need to read data or not
                ReceiveMessage();
            else
                return;
        }
    }
}
