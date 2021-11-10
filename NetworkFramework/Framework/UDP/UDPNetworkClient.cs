using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using System.Net;
using System.Net.Sockets;


using NetworkFramework.Framework.UDP.Events;

namespace NetworkFramework.Framework.UDP
{
    public class UDPNetworkClient
    {
        public readonly UDPClientEventHandler Events;
        public readonly UdpClient client;

        private IPEndPoint Endpoint;

        private bool isReading;
        public UDPNetworkClient(UdpClient _client)
        {
            Endpoint = (IPEndPoint)_client.Client.LocalEndPoint;
            
            client = _client;
            /*client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(Endpoint);*/

            Events = new UDPClientEventHandler();

            Events.OnConnectToServer += async () => { };
            Events.OnDataReceived += async (UDPPacket) => { };
        }
        /// <summary>
        /// Create a new UDP Client instance.
        /// </summary>
        /// <param name="_port">The port the client binds to(not server port).</param>
        public UDPNetworkClient(int _port)
        {
            client = new UdpClient();

            Events = new UDPClientEventHandler();
            Events.OnConnectToServer += async () => { };
            Events.OnDataReceived += async (UDPPacket) => { };
        }
        public IPEndPoint GetEndpoint() => Endpoint;
        public async void Connect(string _ipAddress, uint _port)
        {
            if (!client.Client.Connected)
            {
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                client.Client.Bind(new IPEndPoint(IPAddress.Any, (int)_port));

                Endpoint = new IPEndPoint(IPAddress.Parse(_ipAddress), (int)_port);
                client.Connect(_ipAddress, (int)_port);
                await Events.ClientConnect();
            }
            else
                Console.WriteLine("[Client] Client already connected");
        }
        public void StartReceiving()
        {
            if (!client.Client.Connected)
                Console.WriteLine("[Client] Failed to read data: Client not connected to the server.");
            else
            {
                if (!isReading)
                {
                    Console.WriteLine("[Client] Pre read data");
                    isReading = true;
                    if (!client.Client.Connected)
                        Console.WriteLine("[Client] Failed to read data: Client not connected to the server.");
                    client.BeginReceive(OnDataReceived, null);
                }
                else
                    Console.WriteLine("[Client] Already reading data.");
            }
        }
        public void StopReceiving()
        {
            if (isReading)
                isReading = false;
            else
                Console.WriteLine("[Client] Not reading data.");
        }

        public void SendPacket(UDPPacket _packet)
        {
            _packet = new(_packet.ToArray());
            _packet.InsertLength();

            Console.WriteLine($"[Client] Sending packet: {_packet.GetLength()}");
            client.BeginSend(_packet.ToArray(), _packet.GetLength(), null, null);
        }

        private async void OnDataReceived(IAsyncResult ar)
        {
            Console.WriteLine("Started receiving");
            byte[] DataReceived = client.EndReceive(ar, ref Endpoint);

            UDPPacket ReceivedPacket = new(DataReceived);
            int PacketSize = ReceivedPacket.ReadInt();

            if (DataReceived.Length - 4!= PacketSize)
            {
                Console.WriteLine("[Client] Packet size mismatch, dropping...");
                return;
            }

            await Events.DataReceived(ReceivedPacket);
        }
    }
}
