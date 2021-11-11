using System;
using System.Threading.Tasks;

namespace NetworkFramework.Framework.UDP.Events
{
    public class UDPServerEventHandler
    {
        public UDPServerEventHandler()
        {
            OnClientConnected += (UDPNetworkClient) => new Task(() => { });
            OnDataReceived += (UDPPacket, UDPNetworkClient) => new Task(() => { });
        }
        /// <summary>
        /// Called when a client is connected
        /// </summary>
        public event Func<UDPNetworkClient, Task> OnClientConnected;
        /// <summary>
        /// Event called when teh server receives a packet, returns the packet received and the network client that sent that packet.
        /// </summary>
        public event Func<UDPPacket, UDPNetworkClient, Task> OnDataReceived;
        public async Task ClientConnect(UDPNetworkClient _client) => await OnClientConnected(_client);
        public async Task DataReceived(UDPPacket _packet, UDPNetworkClient _client) => await OnDataReceived(_packet, _client);
    }
}