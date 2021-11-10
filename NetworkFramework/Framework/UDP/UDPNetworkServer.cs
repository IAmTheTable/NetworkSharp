using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

using NetworkFramework.Framework.UDP.Events;

namespace NetworkFramework.Framework.UDP
{
    public class UDPNetworkServer
    {
        /// <summary>
        /// The max amount of data that can be sent/Received in a single buffer.
        /// </summary>
        public int maxRecieveBuffer = 4096;
        public List<UDPNetworkClient> ConnectedClients = new();
        public UDPServerEventHandler Events;

        private readonly Socket server;
        private UDPNetworkServer(uint _port)
        {
            try
            {
                Events = new UDPServerEventHandler();

                server = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); // New UdpClient instance as server
                server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                server.Bind(new IPEndPoint(IPAddress.Any, (int)_port));
                Console.WriteLine($"[Server] Server started at {server.LocalEndPoint}");

                ReceiveMessage();
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Loglevel.Error, "Failed to start server: ", e.Message);
            }
        }

        private async void ReceiveMessage()
        {
            byte[] DataBuffer = new byte[maxRecieveBuffer];
            EndPoint ClientEP = new IPEndPoint(IPAddress.Any, 0);

            SocketReceiveMessageFromResult MessageResult = await server.ReceiveMessageFromAsync(DataBuffer, SocketFlags.None, ClientEP);
            OnDataReceived(DataBuffer, MessageResult.RemoteEndPoint, MessageResult.ReceivedBytes);
        }

        public static UDPNetworkServer Create(uint _port) => new(_port);
        public void SendPacket(UDPPacket _packet, IPEndPoint _destination)
        {
            _packet = new(_packet.ToArray());
            _packet.InsertLength();

            server.SendToAsync(_packet.ToArray(), SocketFlags.None, _destination);
            Logger.Log(Logger.Loglevel.Verbose, $"[Server] Sent packet: {_packet.GetLength()}");
        }

        private async void OnDataReceived(byte[] _dataReceived, EndPoint _clientEP, int _amountDataReceived)
        {
            Array.Resize(ref _dataReceived, _amountDataReceived);
            ReceiveMessage();
            Logger.Log(Logger.Loglevel.Verbose, $"[Server] Got data len {_amountDataReceived}");
            UDPNetworkClient Client;
            if (ConnectedClients.Exists(x => x.GetClientEndpoint().ToString() == _clientEP.ToString()) == false)
            {
                Logger.Log(Logger.Loglevel.Verbose, "[Server] Client added to list");

                Client = new((IPEndPoint)server.LocalEndPoint, (IPEndPoint)_clientEP);
                ConnectedClients.Add(Client);
                await Events.ClientConnect(Client);
            }
            else
            {
                Client = ConnectedClients.Find(x => x.GetClientEndpoint().ToString() == _clientEP.ToString());
                Logger.Log(Logger.Loglevel.Verbose, "[Server] Got client from list");
            }

            UDPPacket ReceivedPacket = new(_dataReceived);
            int PacketSize = ReceivedPacket.ReadInt();

            if (_amountDataReceived - 4 != PacketSize)
            {
                Logger.Log(Logger.Loglevel.Warn, "[Server] Packet size mismatch, dropping");
                return;
            }

            await Events.DataReceived(ReceivedPacket, Client);

        }
    }
}
