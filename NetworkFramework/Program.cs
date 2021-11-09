using NetworkFramework.Framework;
using System;
using System.Threading.Tasks;

namespace NetworkFramework
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TCPNetworkServer server = TCPNetworkServer.Create(6000);
            server.tcpServerEventHandler.OnClientConnected += TcpServerEventHandler_OnClientConnected;
            await System.Threading.Tasks.Task.Delay(2000);
            TCPNetworkClient client = new TCPNetworkClient();
            client.Connect("127.0.0.1", 6000);
            await Task.Delay(-1);
        }

        private static async System.Threading.Tasks.Task TcpServerEventHandler_OnClientConnected(TCPNetworkClient arg)
        {
            Console.WriteLine($"Client connected: {arg.clientEndpoint}");
        }
    }
}
