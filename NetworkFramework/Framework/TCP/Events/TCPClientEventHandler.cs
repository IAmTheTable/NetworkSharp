using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
namespace NetworkFramework.Framework.TCP.Events
{
    public class TCPClientEventHandler
    {
        /// <summary>
        /// Called when the client recieves data from the server.
        /// </summary>
        public event Func<TCPPacket, Task> OnDataReceived;
        /// <summary>
        /// Called when the client connects to the server.
        /// </summary>
        public event Func<EndPoint, Task> OnConnectToServer;
        public async Task DataReceived(TCPPacket _packet) => await OnDataReceived(_packet);
        public async Task ConnectToServer(EndPoint _endpoint) => await OnConnectToServer (_endpoint);
    }
}
