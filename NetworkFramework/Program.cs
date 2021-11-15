using System;
using NetworkSharp.Framework;
using NetworkSharp.Framework.TCP;

namespace YourProject
{
    class Program
    {
        // create our server and client
        TCPNetworkServer server;
        TCPNetworkClient client = new();

        static void Main(string[] args) => new Program().ServerStart();

        void ServerStart()
        {
            Logger.ErrorLogging = true;
            // create our server, and listen in on port 8000
            server = TCPNetworkServer.Create(8000);
            // subscribe to the events
            server.OnClientConnected += Server_OnClientConnected;
            server.OnDataReceived += Server_OnDataReceived;

            // connect the client to the server
            client.Connect("127.0.0.1", 8000);
            // subscribe to the client events
            client.OnConnectToServer += Client_OnConnectToServer;
            client.OnDataReceived += Client_OnDataReceived;
        }

        /// <summary>
        /// When the client receives data
        /// </summary>
        /// <param name="arg"></param>
        private void Server_OnDataReceived(TCPPacket arg)
        {
            Console.WriteLine($"[Server] {arg.ReadString()}");
        }

        private void Client_OnDataReceived(TCPPacket arg)
        {
            Console.WriteLine($"[Client] {arg.ReadString()}");

            //client = null;
        }

        private void Client_OnConnectToServer(System.Net.EndPoint arg)
        {
            Console.WriteLine($"[Client] Connected to server at EP : {arg}");
            client.StartRecieving();

            TCPPacket Packet = new();
            Packet.WriteString("Hello from client");

            client.SendPacket(Packet);
        }

        private void Server_OnClientConnected(TCPNetworkClient arg)
        {
            Console.WriteLine($"[Server] Client connected at {arg.clientEndpoint}");
            TCPPacket Packet = new();
            Packet.WriteString("Welcome to the server!");

            /*new System.Threading.Thread(() =>
            {
                while (true)
                {
                    TCPPacket Packet = new();
                    Packet.WriteString("Welcome to the server!");

                    arg.SendPacket(Packet);
                }

            }).Start();*/
            server.SendPacket(0, Packet);
            client = null;
            for (int i = 0; i < 50; i++)
            {

                server.SendPacket(0, Packet);
            }
        }
    }
}
