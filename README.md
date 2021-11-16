# NetworkSharp
A simple yet flexible networking framework for C#.

**I NEED TO DO DOCUMENTATION LATER**

**This was a project inspired by Tom Weiland's networking solution for unity**
I am very open to suggestions and improvements. (Thank You)


# How to use
Heres an example of how to use TCP Server/Client in a single project!

```cs
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
            server = new(8000);
            // subscribe to the events
            server.OnClientConnected += Server_OnClientConnected;
            server.OnDataReceived += Server_OnDataReceived;
            server.OnClientDisconnected += Server_OnClientDisconnected;
            // create our server, and listen in on port 8000
            server.Listen();

            // connect the client to the server
            client.Connect("127.0.0.1", 8000);
            // subscribe to the client events
            client.OnConnectToServer += Client_OnConnectToServer;
            client.OnDataReceived += Client_OnDataReceived;
        }

        private void Server_OnClientDisconnected(int obj) => Console.WriteLine("Client has disconnected");
        private void Server_OnDataReceived(TCPPacket arg) => Console.WriteLine($"[Server] {arg.ReadString()}");
        private void Client_OnDataReceived(TCPPacket arg) => Console.WriteLine($"[Client] {arg.ReadString()}");

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
            server.SendPacket(0, Packet);
        }
    }
}
```
