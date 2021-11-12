using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace NetworkSharp.Framework.TCP
{
    public class TCPPacket
    {
        private readonly List<byte> buffer;
        private int readPos;
        public TCPPacket()
        {
            readPos = 0;
            buffer = new();
        }
        public TCPPacket(byte[] _data)
        {
            readPos = 0;
            buffer = _data.ToList();
        }
        #region Write Functions
        public void WriteByte(byte _data)
        {
            buffer.Add(_data);
        }
        public void WriteInt(int _data)
        {
            buffer.AddRange(BitConverter.GetBytes(_data));
        }
        public void WriteString(string _data)
        {
            buffer.AddRange(BitConverter.GetBytes(_data.Length));
            buffer.AddRange(Encoding.ASCII.GetBytes(_data));
        }
        public void WriteShort(short _data)
        {
            buffer.AddRange(BitConverter.GetBytes(_data));
        }
        #endregion

        #region Read Functions
        public byte ReadByte(bool increasePos = true)
        {
            if (buffer.Count > readPos)
            {
                byte value = buffer[readPos];

                if (increasePos)
                    readPos++;

                return value;
            }
            else
            {
                throw new Exception("Failed to read byte, I am already at the end of the stream.");
            }
        }
        public short ReadShort(bool increasePos = true)
        {
            if (buffer.Count > readPos)
            {
                short value = BitConverter.ToInt16(buffer.ToArray(), readPos);

                if (increasePos)
                    readPos += 2;

                return value;
            }
            else
            {
                throw new Exception("Failed to read short, I am already at the end of the stream.");
            }
        }
        public int ReadInt(bool increasePos = true)
        {
            if (buffer.Count > readPos)
            {
                int value = BitConverter.ToInt32(buffer.ToArray(), readPos);

                if (increasePos)
                    readPos += 4;

                return value;
            }
            else
            {
                throw new Exception("Failed to read int, I am already at the end of the stream.");
            }
        }
        public string ReadString(bool increasePos = true)
        {
            if (buffer.Count > readPos)
            {
                int StringLength = BitConverter.ToInt32(buffer.ToArray(), readPos);
                string value;

                if (increasePos)
                {
                    readPos += 4;
                    value = Encoding.ASCII.GetString(buffer.ToArray(), readPos, StringLength);
                    readPos += value.Length;
                }
                else
                    value = Encoding.ASCII.GetString(buffer.ToArray(), readPos + 4, StringLength);

                return value;
            }
            else
            {
                throw new Exception("Failed to read string, I am already at the end of the stream.");
            }
        }
        public byte[] ReadBytes(int Amount, bool increasePos = true)
        {
            if (buffer.Count > readPos)
            {
                if (increasePos)
                    readPos += Amount;

                return buffer.GetRange(readPos, Amount).ToArray();
            }
            else
            {
                throw new Exception("Failed to read byte array, I am already at the end of the stream");
            }
        }
        #endregion

        public byte[] ToArray()
        {
            // insert the packet length at the beginning
            buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count + 4));
            return buffer.ToArray();
        }
        public int GetLength() => buffer.Count;
    }
}
