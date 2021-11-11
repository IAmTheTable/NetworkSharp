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

            server.Events.OnClientConnected += Server_ClientConnected;
            server.Events.OnDataReceived += Server_DataReceived;

            client = new();
            client.Events.OnConnectToServer += Client_ConnectToServer;
            client.Events.OnDataReceived += Client_DataReceived;

            client.Connect("127.0.0.1", 6000);

            await Task.Delay(-1);
        }
        private static async Task Client_DataReceived(UDPPacket arg)
        {
            Console.WriteLine($"[Client] CPacket: {arg.ReadString()}");
            await Task.Delay(0);

            UDPPacket CPack = new();
            CPack.WriteString($"I AM A CLIENT. {DateTime.Now.Millisecond}");
            client.SendPacket(CPack);
        }
        private static async Task Client_ConnectToServer()
        {
            Console.WriteLine("[Client] Connected to server");
            await Task.Delay(0);
            client.StartReceiving();

            UDPPacket HelloPacket = new();
            HelloPacket.WriteString("Hello from client XD");

            client.SendPacket(HelloPacket);
        }
        private static async Task Server_DataReceived(UDPPacket _arg, UDPNetworkClient _client)
        {
            UDPPacket packet = new (_arg.ToArray());

            Console.WriteLine($"[Server] Packet: {packet.ReadString()}");
            await Task.Delay(0);

            UDPPacket skid = new UDPPacket();
            skid.WriteString($"I AM A SERVER. {DateTime.Now.Millisecond}");

            server.SendPacket(skid, _client.GetClientEndpoint());
        }
        private static async Task Server_ClientConnected(UDPNetworkClient _arg)
        {
            Console.WriteLine($"[Server] Client connected {_arg.GetClientEndpoint()}");
            await Task.Delay(0);

            UDPPacket HelloPacket = new();
            HelloPacket.WriteString("Hello from server");

            server.SendPacket(HelloPacket, _arg.GetClientEndpoint());
        }
    }
}