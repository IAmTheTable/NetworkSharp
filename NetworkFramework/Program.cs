using NetworkFramework.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

using NetworkFramework.Framework.TCP;
using NetworkFramework.Framework.UDP;
using System.Net.Sockets;
using System.Text;

namespace NetworkFramework
{
    class Program
    {
        private static UDPNetworkServer server;
        private static UDPNetworkClient client;
        static async Task Main()
        {
            bool server2 = true;
            if (server2)
            {
                server = UDPNetworkServer.Create(6000);

                server.Events.OnClientConnected += Events_OnClientConnected;
                server.Events.OnDataReceived += Events_OnDataReceived;
            }
            else
            {
                client = new(6000);
                client.Events.OnConnectToServer += Events_OnConnectToServer;
                client.Events.OnDataReceived += Events_OnDataReceived1;
                client.Connect("192.168.0.200", 6000);
            }


            await Task.Delay(-1);
        }

        private static async Task Events_OnDataReceived1(UDPPacket arg)
        {
            Console.WriteLine($"[Client] Packet: {arg.ReadString()}");
            await Task.Delay(0);
        }

        private static async Task Events_OnConnectToServer()
        {
            Console.WriteLine("[Client] Connected to server");
            await Task.Delay(0);
            client.StartReceiving();

            UDPPacket HelloPacket = new();
            HelloPacket.WriteString("Hello from client");

            while (true)
            {
                client.SendPacket(HelloPacket);
                await Task.Delay(2000);
                client.SendPacket(HelloPacket);
            }
        }

        private static async Task Events_OnDataReceived(UDPPacket _arg, UDPNetworkClient _client)
        {
            UDPPacket packet = _arg;

            Console.WriteLine($"[Server] Packet: {packet.ReadString()} New-len: {packet.GetLength() - packet.readPos}");
            await Task.Delay(0);
            server.SendPacket(_arg, _client.GetEndpoint());
        }
        private static async Task Events_OnClientConnected(UDPNetworkClient _arg)
        {
            Console.WriteLine($"[Server] Client connected {_arg.GetEndpoint()}");
            await Task.Delay(0);

            UDPPacket HelloPacket = new();
            HelloPacket.WriteString("Hello from server");

            server.SendPacket(HelloPacket, _arg.GetEndpoint());
        }
    }
}
