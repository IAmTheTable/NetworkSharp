using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;
using NetworkFramework.Framework.Events;

namespace NetworkFramework.Framework
{
    public class TCPNetworkClient
    {
        private readonly TcpClient client;
        /// <summary>
        /// Constructs a new client class
        /// </summary>
        /// <param name="_client">TCPClient instance that is used.</param>
        public TCPNetworkClient(TcpClient _client)
        {
            client = _client;
        }

    }
}
