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

        private readonly uint serverPort;
        private readonly UdpClient server;
        private UDPNetworkServer(uint _port)
        {
            serverPort = _port;

            Events = new UDPServerEventHandler();
            Events.OnClientConnected += async (UDPNetworkClient) => { };
            Events.OnDataReceived += async (UDPPacket, UDPNetworkClient) => { };

            server = new UdpClient();
            server.ExclusiveAddressUse = false;
            server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            server.Client.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), (int)_port ));

            Console.WriteLine($"[Server] Server started at {server.Client.LocalEndPoint}");

            server.BeginReceive(OnDataReceived, null);
        }
        public static UDPNetworkServer Create(uint _port) => new(_port);
        public void SendPacket(UDPPacket _packet, IPEndPoint _destination)
        {
            server.BeginSend(_packet.ToArray(), _packet.GetLength(), _destination, OnDataSent, null);

            Console.WriteLine("[Server] Sent packet");
        }

        // TO DO: SEND PACKET AS TCP AND REQUEST AS UDP

        private void OnDataSent(IAsyncResult ar)
        {
            int BytesSent = server.EndSend(ar);
            Console.WriteLine($"[Server] Bytes sent: {BytesSent}");
            //server.Close();
        }

        private async void OnDataReceived(IAsyncResult ar)
        {
            IPEndPoint ClientAddress = new(IPAddress.Any, 0);
            byte[] DataReceived = server.EndReceive(ar, ref ClientAddress);
            server.BeginReceive(OnDataReceived, null);
            Console.WriteLine($"[Server] Got data len {DataReceived.Length}");
            UDPNetworkClient Client;
            if (ConnectedClients.Exists(x => x.GetEndpoint().ToString() == ClientAddress.ToString()) == false)
            {
                Console.WriteLine("[Server] Client added to list");

                UdpClient ConClient = new UdpClient();
                ConClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                ConClient.Client.Bind(new IPEndPoint(IPAddress.Any, 6001 + ConnectedClients.Count + 5));

                Client = new(ConClient);
                ConnectedClients.Add(Client);
                await Events.ClientConnect(Client);
            }
            else
            {
                Client = ConnectedClients.Find(x => x.GetEndpoint().ToString() == ClientAddress.ToString());
                Console.WriteLine("[Server] Got client from list");
            }

            UDPPacket ReceivedPacket = new(DataReceived);
            int PacketSize = ReceivedPacket.ReadInt();

            if (DataReceived.Length - 4 != PacketSize)
            {
                Console.WriteLine("[Server] Packet size mismatch, dropping");
                return;
            }

            await Events.DataReceived(ReceivedPacket, Client);
            
        }
    }
}
