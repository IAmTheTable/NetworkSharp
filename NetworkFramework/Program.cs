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
            server = UDPNetworkServer.Create(6000);

            server.Events.OnClientConnected += Events_OnClientConnected;
            server.Events.OnDataReceived += Events_OnDataReceived;

            client = new();
            client.Events.OnConnectToServer += Events_OnConnectToServer;
            client.Events.OnDataReceived += Events_OnDataReceived1;
            client.Connect("127.0.0.1", 6000);


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

            client.SendPacket(HelloPacket);
        }

        private static async Task Events_OnDataReceived(UDPPacket _arg, UDPNetworkClient _client)
        {
            UDPPacket packet = new (_arg.ToArray());

            Console.WriteLine($"[Server] Packet: {packet.ReadString()} New-len: {packet.GetLength() - packet.readPos}");
            await Task.Delay(0);
            server.SendPacket(_arg, _client.GetClientEndpoint());
        }
        private static async Task Events_OnClientConnected(UDPNetworkClient _arg)
        {
            Console.WriteLine($"[Server] Client connected {_arg.GetClientEndpoint()}");
            await Task.Delay(0);

            UDPPacket HelloPacket = new();
            HelloPacket.WriteString("Hello from server");

            server.SendPacket(HelloPacket, _arg.GetClientEndpoint());
        }
    }
}
