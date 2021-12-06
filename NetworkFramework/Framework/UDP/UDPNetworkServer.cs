using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace NetworkSharp.Framework.UDP
{
    /// <summary>
    /// An easy to use class for a UDP server.
    /// </summary>
    public class UDPNetworkServer
    {
        /// <summary>
        /// Called when a client is connected
        /// </summary>
        public event Action<UDPNetworkClient> OnClientConnected;
        /// <summary>
        /// Event called when the server receives a packet, returns the packet received and the network client that sent that packet.
        /// </summary>
        public event Action<UDPPacket, UDPNetworkClient> OnDataReceived;
        /// <summary>
        /// The max amount of data that can be sent/Received in the buffer.
        /// </summary>
        public int MaxRecieveBuffer = 4096;
        /// <summary>
        /// The list of connected clients.
        /// </summary>
        public List<UDPNetworkClient> ConnectedClients { get; private set; } = new();
        /// <summary>
        /// The port in the network that the server is currently listening on.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// The primary socket the server uses to communicate with.
        /// </summary>
        private readonly Socket server;
        /// <summary>
        /// Create and start a new UDP server.
        /// </summary>
        /// <param name="_port">The port on the network you wish to listen on.</param>
        /// <returns>The new instance of the UDP Server.</returns>
        public UDPNetworkServer(int _port)
        {
            if (_port < 1 && _port > short.MaxValue)
                throw new Exception($"Value of '_port' cannot be greater than {short.MaxValue} and not less than 1.");
            try
            {
                _port = Port;
                // New Socket instance as server
                server = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                server.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                //--BEGIN-CONFIGURE-SETTINGS--\\
                server.MulticastLoopback = false;
                server.Blocking = false;
                // Im sure these aren't doing much on the front end(considering how simple this is meant to be) *added for precautionary actions*
                server.DontFragment = true;
                server.EnableBroadcast = false;
                //--END-CONFIGURE-SETTINGS--\\
            }
            catch (Exception e)
            {
                Logger.Log(Logger.Loglevel.Error, "[Server] Failed to start server\n", e.Message);
            }
        }
        
        /// <summary>
        /// Start listening for connections via the specified port.
        /// </summary>
        public void Listen()
        {
            server.Bind(new IPEndPoint(IPAddress.Any, Port)); // bind the server to the port
            Console.WriteLine($"[Server] Server started at {server.LocalEndPoint}");
            // Start receiving data.
            ReceiveMessage();
        }

        /// <summary>
        /// Recieve the next message from the client.
        /// </summary>
        private async void ReceiveMessage()
        {
            // Create variables that will be used later.
            byte[] DataBuffer = new byte[MaxRecieveBuffer];
            EndPoint ClientEP = new IPEndPoint(IPAddress.Any, 0);

            // Begin receiving data.
            SocketReceiveMessageFromResult MessageResult = await server.ReceiveMessageFromAsync(DataBuffer, SocketFlags.None, ClientEP);
            OnServerDataReceived(DataBuffer, MessageResult.RemoteEndPoint, MessageResult.ReceivedBytes); // Handle the message
        }

        /// <summary>
        /// Send a packet to a client.
        /// </summary>
        /// <param name="_packet">The packet you wish to send.</param>
        /// <param name="_destination">The IPEndPoint of the client you wish to send the packet to.</param>
        public async void SendPacket(UDPPacket _packet, IPEndPoint _destination)
        {
            // Convert the packet to a new instance, cause if I don't, then the original packet will get modified(idk why)
            _packet = new(_packet.ToArray());
            _packet.InsertLength();// write the packet length.

            // Send the packet to the destination
            await server.SendToAsync(_packet.ToArray(), SocketFlags.None, _destination);
            Logger.Log(Logger.Loglevel.Verbose, $"[Server] Sent packet: {_packet.GetLength()}");
        }

        /// <summary>
        /// Called when the server receives data.
        /// </summary>
        /// <param name="_dataReceived">The data the server receives (in a byte array)</param>
        /// <param name="_clientEP">The EndPoint of the client that sent the data.</param>
        /// <param name="_amountDataReceived">The amount of bytes the client sent.</param>
        private void OnServerDataReceived(byte[] _dataReceived, EndPoint _clientEP, int _amountDataReceived)
        {
            // Resize the array and begin listening again.
            ReceiveMessage();
            Array.Resize(ref _dataReceived, _amountDataReceived);

            Logger.Log(Logger.Loglevel.Verbose, $"[Server] Got data len {_amountDataReceived}");

            UDPNetworkClient Client; // Check if the client isn't connected to the server already and register them if they are.
            if (ConnectedClients.Exists(x => x.LocalEndpoint.ToString() == _clientEP.ToString()) == false)
            {
                Logger.Log(Logger.Loglevel.Verbose, "[Server] Client added to list");

                Client = new((IPEndPoint)server.LocalEndPoint, (IPEndPoint)_clientEP);
                ConnectedClients.Add(Client);

                OnClientConnected(Client);
            }
            else // If not, then get the client from the connected client list.
            {
                Client = ConnectedClients.Find(x => x.LocalEndpoint.ToString() == _clientEP.ToString());
                Logger.Log(Logger.Loglevel.Verbose, "[Server] Got client from list");
            }

            // Create a new packet from the data that we received.
            UDPPacket ReceivedPacket = new(_dataReceived);
            int PacketSize = ReceivedPacket.ReadInt();// Get the packet size

            // Check if the size is matching; I offset 4 because the first 4 bytes are **ALWAYS** allocated to the packet size, so we just ignore those.
            if (_amountDataReceived - 4 != PacketSize)
            {
                Logger.Log(Logger.Loglevel.Warn, "[Server] Packet size mismatch, dropping");
                return;
            }

            // Fire the event, so any subscribers can handle the packet.
            OnDataReceived(ReceivedPacket, Client);
        }
    }
}
