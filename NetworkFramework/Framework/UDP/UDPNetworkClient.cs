using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;


using NetworkFramework.Framework.UDP.Events;
using System.Threading.Tasks;

namespace NetworkFramework.Framework.UDP
{
    public class UDPNetworkClient
    {
        public readonly int maxBufferSize = 4096;
        public readonly UDPClientEventHandler Events;
        public readonly Socket client;

        private IPEndPoint ServerEndpoint;
        private readonly IPEndPoint LocalEndpoint;

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

            Events = new UDPClientEventHandler();
        }
        /// <summary>
        /// Create a new UDP Client instance.
        /// </summary>
        /// <param name="_port">The port the client binds to(not server port).</param>
        public UDPNetworkClient()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);

            Events = new UDPClientEventHandler();
        }
        public IPEndPoint GetServerEndpoint() => ServerEndpoint;
        public IPEndPoint GetClientEndpoint() => LocalEndpoint;
        public async void Connect(string _ipAddress, uint _port)
        {
            try
            {
                if (ServerEndpoint == null)
                {
                    ServerEndpoint = new(IPAddress.Parse(_ipAddress), (int)_port);
                    await Events.ClientConnect();
                }
                else
                    Logger.Log(Logger.Loglevel.Warn, "[Client] Client already connected");
            }
            catch(Exception e)
            {
                Logger.Log(Logger.Loglevel.Error, $"[Client] Failed to connect to server: {e.Message}");
            }
        }
        public async void StartReceiving()
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
                    await client.ConnectAsync(ServerEndpoint);
                    ReceiveMessage();
                }
                else
                    Logger.Log(Logger.Loglevel.Warn, "[Client] Already reading data.");
            }
        }
        private async void ReceiveMessage()
        {
            byte[] DataBuffer = new byte[maxBufferSize];
            EndPoint ServerEP = ServerEndpoint;
            
            SocketReceiveMessageFromResult MessageResult = await client.ReceiveMessageFromAsync(DataBuffer, SocketFlags.None, ServerEP);
            OnDataReceived(DataBuffer, MessageResult.RemoteEndPoint, MessageResult.ReceivedBytes);
        }

        public void StopReceiving()
        {
            if (isReading)
                isReading = false;
            else
                Logger.Log(Logger.Loglevel.Warn, "[Client] Not reading data.");
        }

        public void SendPacket(UDPPacket _packet)
        {
            _packet = new(_packet.ToArray());
            _packet.InsertLength();

            Logger.Log(Logger.Loglevel.Verbose, $"[Client] Sending packet: {_packet.GetLength()}");
            client.BeginSendTo(_packet.ToArray(), 0, _packet.GetLength(), SocketFlags.None, ServerEndpoint, null, null);
        }

        private async void OnDataReceived(byte[] _dataReceived, EndPoint _clientEP, int _amountDataReceived)
        {
            if (isReading) // Check if we need to read data or not
                ReceiveMessage();
            else
                return;

            Array.Resize(ref _dataReceived, _amountDataReceived);

            UDPPacket ReceivedPacket = new(_dataReceived);
            int PacketSize = ReceivedPacket.ReadInt();

            if (_amountDataReceived - 4 != PacketSize)
            {
                Logger.Log(Logger.Loglevel.Warn, "[Client] Packet size mismatch, dropping...");
                return;
            }

            await Events.DataReceived(ReceivedPacket);
        }
    }
}
