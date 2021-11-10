using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace NetworkFramework.Framework.UDP
{
    public class UDPPacket
    {
        public int readPos { get; private set; }
        private readonly List<byte> buffer;
        public UDPPacket()
        {
            readPos = 0;
            buffer = new();
        }
        public UDPPacket(byte[] _data)
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
            WriteInt(_data.Length);
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
            string value = "";
            try
            {
                if (buffer.Count > readPos)
                {
                    int StringLength = ReadInt();
                    byte[] data = buffer.ToArray();
                    value = Encoding.ASCII.GetString(data, readPos, StringLength);

                    if (increasePos)
                        readPos += value.Length;
                    else
                        readPos -= 4;

                    return value;
                }
                else
                {
                    throw new Exception("Failed to read string, I am already at the end of the stream.");
                }
            }
            catch (Exception) { return value; }
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
        public void InsertLength() => buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
        public byte[] ToArray() => buffer.GetRange(readPos, GetLength() - readPos).ToArray();
        public int GetLength() => buffer.Count;
    }
}
