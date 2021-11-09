using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
namespace NetworkFramework.Framework.Events
{
    public class TCPClientEventHandler
    {
        public event Func<TCPPacket, Task> OnDataRecieved;
        public void DataRecieved(TCPPacket _packet)
        {
            OnDataRecieved(_packet);
        }
    }
}
