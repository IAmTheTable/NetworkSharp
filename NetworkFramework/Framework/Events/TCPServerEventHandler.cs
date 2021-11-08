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
        public event Func<TcpClient, Task> OnClientConnected;
        public event Func<byte[], Task> OnDataReceived
        public void ClientConnect(TcpClient _client)
        {
            OnClientConnected(_client);
        }
    }
}
