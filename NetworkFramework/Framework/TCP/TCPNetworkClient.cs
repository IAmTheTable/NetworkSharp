using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetworkSharp.Framework.TCP
{
    public class TCPNetworkClient
    {
        /// <summary>
        /// Called when the client Receives data from the server.
        /// </summary>
        public event Action<TCPPacket> OnDataReceived;
        /// <summary>
        /// Called when the client connects to the server.
        /// </summary>
        public event Action<EndPoint> OnConnectToServer;
        /// <summary>
        /// The max amount of bytes the client can receive at once.
        /// </summary>
        public int MaxBufferSize = 4096;
        /// <summary>
        /// The endpoint of the local client;
        /// </summary>
        public readonly EndPoint clientEndpoint;
        /// <summary>
        /// The endpoint of the server.
        /// </summary>
        public EndPoint ServerEndpoint { get; private set; }

        private readonly byte[] dataBuffer; // Temp data buffer
        private bool isReading = false; // client read state
        private readonly TcpClient client; // actual tcp client
        private NetworkStream stream; // stream of our client

        /// <summary>
        /// Constructs a new client class
        /// </summary>
        /// <param name="_client">TCPClient instance that is used.</param>
        public TCPNetworkClient(TcpClient _client)
        {
            client = _client;
            dataBuffer = new byte[MaxBufferSize];
            clientEndpoint = _client.Client.RemoteEndPoint;
            stream = client.GetStream();
        }

        /// <summary>
        /// Create a new Client to use for sending/receiving data.
        /// </summary>
        public TCPNetworkClient()
        {
            client = new TcpClient();
            dataBuffer = new byte[MaxBufferSize];
            clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
        }
        ~TCPNetworkClient()
        {
            Logger.Log(Logger.Loglevel.Verbose, "[Client] Disposed");
            client.Close();
            ServerEndpoint = null;
            stream = null;
            client.Dispose();
        }

        /// <summary>
        /// Connect to a server with a specified IP and Port.
        /// </summary>
        /// <param name="_ipAddress">IPAddress of the server you are connecting to.</param>
        /// <param name="_port">The port of the server you are connecting to.</param>
        public void Connect(string _ipAddress, uint _port)
        {
            if (!client.Connected)
                client.BeginConnect(_ipAddress, (int)_port, OnConnectedToServer, null);
            else
                Console.WriteLine("[Client] Already connected to the server.");

            ServerEndpoint = new IPEndPoint(IPAddress.Parse(_ipAddress), (int)_port);
        }

        /// <summary>
        /// Called when the client is connected to the server.
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnectedToServer(IAsyncResult ar)
        {
            stream = client.GetStream();

            OnConnectToServer(ServerEndpoint);
        }

        /// <summary>
        /// Start listening for data from the server.
        /// </summary>
        public void StartRecieving()
        {
            try
            {
                if (!isReading) // check if we're already reading data from the server
                    if (stream.CanRead) // check if we can read
                    {
                        isReading = true;
                        stream.BeginRead(dataBuffer, 0, MaxBufferSize, OnTcpDataReceived, null);
                    }
                    else
                        Logger.Log(Logger.Loglevel.Warn, "[Client] Failed to read stream: Cannot read the stream.");
                else
                    Logger.Log(Logger.Loglevel.Warn, "[Client] Already reading the stream.");
            }
            catch (Exception e) { Logger.Log(Logger.Loglevel.Error, $"There was an error while attempting to read data from the server\n {e.Message}"); }
        }

        /// <summary>
        /// Gets the raw TCPClient class used.
        /// </summary>
        /// <returns>The tcp client used.</returns>
        public TcpClient GetClient() => client;

        /// <summary>
        /// Stop reading data from the server.
        /// </summary>
        public void StopRecieving()
        {
            if (isReading)
                isReading = false;
            else
                Logger.Log(Logger.Loglevel.Warn, "[Client] Not reading data.");
        }

        /// <summary>
        /// Send a packet to the server.
        /// </summary>
        /// <param name="_packet">The packet you wish to send.</param>
        public void SendPacket(TCPPacket _packet)
        {
            _packet.WriteLength();
            try
            {
                if (client.Connected) // check if we're connected to the server
                    if (stream.CanWrite) // check if we can write
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.GetLength(), OnFinishedWriting, null); // write the data
                    else
                        Console.WriteLine("[Client] Failed to send packet: Cannot write to the stream.");
                else
                    Console.WriteLine("[Client] Not connected to the server.");

            }
            catch (Exception e) { Logger.Log(Logger.Loglevel.Error, $"[Client] There was an error while trying to send a TCP Packet,\n {e.Message}"); }
        }

        /// <summary>
        /// Called when the client finished writing to the stream.
        /// </summary>
        /// <param name="ar"></param>
        private void OnFinishedWriting(IAsyncResult ar) => stream.EndWrite(ar);

        /// <summary>
        /// Called when the client receives data.
        /// </summary>
        /// <param name="ar"></param>
        private void OnTcpDataReceived(IAsyncResult ar)
        {
            // Get the bytes that we read
            int BytesRead = client.GetStream().EndRead(ar);


            // Create the packet instance from the data we got
            TCPPacket RecivedPacket = new(dataBuffer);
            int PacketSize = RecivedPacket.ReadInt(); // get the packet size.

            // Check if the packet size matches with the amount of data we got

            // if the amount of bytes we received is less than the packet size, we need to keep listening.
            if (BytesRead < PacketSize)
            {
                // still reading data
                while (BytesRead < PacketSize)
                {
                    // Store the data temporarily
                    byte[] TempBuffer = new byte[PacketSize - BytesRead];
                    stream.BeginRead(TempBuffer, 0, TempBuffer.Length,
                    (_ar) =>
                    {
                        // The amount of bytes read.
                        int AmountRead = stream.EndRead(_ar);
                        BytesRead += AmountRead;//increase the packet size
                    }, null);

                    RecivedPacket.WriteBytes(TempBuffer);
                    TempBuffer = null; // flush the bytes, because it would cause memory leak.
                }
            }

            if (BytesRead - 4 > PacketSize)
            {
                Console.WriteLine("[Client] Packet size mismatch, dropping...");
                return;
            }

            // Fire the event, for handlers to handle the data.
            OnDataReceived(RecivedPacket);

            // Check if we need to read data
            if (!isReading)
                client.GetStream().BeginRead(dataBuffer, 0, MaxBufferSize, OnTcpDataReceived, null);
        }
    }
}
