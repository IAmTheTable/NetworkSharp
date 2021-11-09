using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkFramework.Framework.Events
{
    public class TCPServerEventHandler
    {
        /// <summary>
        /// Fired when a client is connected to the TCP server.
        /// </summary>
        public event Func<TCPNetworkClient, Task> OnClientConnected;
        public event Func<TCPPacket, Task> OnDataReceived;
        public void ClientConnect(TCPNetworkClient _client)
        {
            OnClientConnected(_client);
        }
    }
}
