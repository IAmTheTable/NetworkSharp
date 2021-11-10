using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkFramework.Framework.UDP.Events
{
    public class UDPClientEventHandler
    {
        /// <summary>
        /// Called when the client is connected to the server.
        /// </summary>
        public event Func<Task> OnConnectToServer;
        /// <summary>
        /// Called when the client Received data.
        /// </summary>
        public event Func<UDPPacket, Task> OnDataReceived;
        public async Task ClientConnect() => await OnConnectToServer();
        public async Task DataReceived(UDPPacket _packet) => await OnDataReceived(_packet);
    }
}
