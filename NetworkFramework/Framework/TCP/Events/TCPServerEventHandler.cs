using System;
using System.Threading.Tasks;

namespace NetworkFramework.Framework.TCP.Events
{
    public class TCPServerEventHandler
    {
        /// <summary>
        /// Fired when a client is connected to the TCP server.
        /// </summary>
        public event Func<TCPNetworkClient, Task> OnClientConnected;
        /// <summary>
        /// Fired when a client sends data to the server.
        /// </summary>
        public event Func<TCPPacket, Task> OnDataReceived;
        public async Task ClientConnect(TCPNetworkClient _client) => await OnClientConnected(_client);
        public async Task DataReceived(TCPPacket _packet) => await OnDataReceived(_packet);
    }
}
