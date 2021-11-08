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
        public int maxBufferSize = 4096;

        private byte[] dataBuffer;

        private readonly TcpClient client;
        /// <summary>
        /// Constructs a new client class
        /// </summary>
        /// <param name="_client">TCPClient instance that is used.</param>
        public TCPNetworkClient(TcpClient _client)
        {
            client = _client;
            dataBuffer = new byte[maxBufferSize];
        }


        public void StartRecieving()
        {
            NetworkStream Stream = client.GetStream();

            Stream.BeginRead(dataBuffer, 0, maxBufferSize, OnTcpDataRecieved, null);
        }

        private void OnTcpDataRecieved(IAsyncResult ar)
        {
            int BytesRead = client.GetStream().EndRead(ar);
        }
    }
}
