using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace NetworkSharp.Framework.UDP
{
    /// <summary>
    /// A packet of data(in bytes) that is either sent or recieved.
    /// </summary>
    public class UDPPacket
    {
        /// <summary>
        /// The index of the current read position in the packet.
        /// </summary>
        public int ReadPos { get; private set; }
        private readonly List<byte> buffer;
        /// <summary>
        /// Create a new UDPPacket to use.
        /// </summary>
        public UDPPacket()
        {
            ReadPos = 0;
            buffer = new();
        }
        /// <summary>
        /// Create a UDPPacket from existing data.
        /// </summary>
        /// <param name="_data"></param>
        public UDPPacket(byte[] _data)
        {
            ReadPos = 0;
            buffer = _data.ToList();
        }

        #region Write Functions
        /// <summary>
        /// Write a single byte to the packet.
        /// </summary>
        /// <param name="_data">The byte you wish to write.</param>
        public void WriteByte(byte _data)
        {
            buffer.Add(_data);
        }

        /// <summary>
        /// Write an integer to the packet.
        /// </summary>
        /// <param name="_data">The integer you wish to write.</param>
        public void WriteInt(int _data)
        {
            buffer.AddRange(BitConverter.GetBytes(_data));
        }

        /// <summary>
        /// Write a string to the packet.
        /// </summary>
        /// <param name="_data">The string you wish to write.</param>
        public void WriteString(string _data)
        {
            WriteInt(_data.Length);
            buffer.AddRange(Encoding.ASCII.GetBytes(_data));
        }

        /// <summary>
        /// Write an Int16(short) to the packet.
        /// </summary>
        /// <param name="_data">The Int16(short) you wish to write.</param>
        public void WriteShort(short _data)
        {
            buffer.AddRange(BitConverter.GetBytes(_data));
        }
        #endregion

        #region Read Functions
        /// <summary>
        /// Read a single byte from the packet.
        /// </summary>
        /// <param name="increasePos">False if you wish to be able to read the next value, otherwise the next Read call will read on.</param>
        /// <returns>The value of the byte you read.</returns>
        public byte ReadByte(bool increasePos = true)
        {
            if (buffer.Count > ReadPos)
            {
                byte value = buffer[ReadPos];

                if (increasePos)
                    ReadPos++;

                return value;
            }
            else
            {
                throw new Exception("Failed to read byte, I am already at the end of the stream.");
            }
        }

        /// <summary>
        /// Read a single Int16(short) from the packet.
        /// </summary>
        /// <param name="increasePos">False if you wish to be able to read the next value, otherwise the next Read call will read on.</param>
        /// <returns>The value of the Int16(short) you read.</returns>
        public short ReadShort(bool increasePos = true)
        {
            if (buffer.Count > ReadPos)
            {
                short value = BitConverter.ToInt16(buffer.ToArray(), ReadPos);

                if (increasePos)
                    ReadPos += 2;

                return value;
            }
            else
            {
                throw new Exception("Failed to read short, I am already at the end of the stream.");
            }
        }

        /// <summary>
        /// Read a single int from the packet.
        /// </summary>
        /// <param name="increasePos">False if you wish to be able to read the next value, otherwise the next Read call will read on.</param>
        /// <returns>The value of the int you read.</returns>
        public int ReadInt(bool increasePos = true)
        {
            if (buffer.Count > ReadPos)
            {
                int value = BitConverter.ToInt32(buffer.ToArray(), ReadPos);

                if (increasePos)
                    ReadPos += 4;

                return value;
            }
            else
            {
                throw new Exception("Failed to read int, I am already at the end of the stream.");
            }
        }

        /// <summary>
        /// Read a string from the packet.
        /// </summary>
        /// <param name="increasePos">False if you wish to be able to read the next value, otherwise the next Read call will read on.</param>
        /// <returns>The value of the string you read.</returns>
        public string ReadString(bool increasePos = true)
        {
            string value = "";
            try
            {
                if (buffer.Count > ReadPos)
                {
                    int StringLength = ReadInt();
                    byte[] data = buffer.ToArray();
                    value = Encoding.ASCII.GetString(data, ReadPos, StringLength);

                    if (increasePos)
                        ReadPos += value.Length;
                    else
                        ReadPos -= 4;

                    return value;
                }
                else
                {
                    throw new Exception("Failed to read string, I am already at the end of the stream.");
                }
            }
            catch (Exception) { return value; }
        }

        /// <summary>
        /// Read an array of bytes from the packet.
        /// </summary>
        /// <param name="increasePos">False if you wish to be able to read the next value, otherwise the next Read call will read on.</param>
        /// <param name="Amount">Amount of bytes you wish to read.</param>
        /// <returns>The array of bytes you read.</returns>
        public byte[] ReadBytes(int Amount, bool increasePos = true)
        {
            if (buffer.Count > ReadPos)
            {
                if (increasePos)
                    ReadPos += Amount;

                return buffer.GetRange(ReadPos, Amount).ToArray();
            }
            else
            {
                throw new Exception("Failed to read byte array, I am already at the end of the stream");
            }
        }
        #endregion
        /// <summary>
        /// Write the packet length. (excluding the addition of this insertion)
        /// </summary>
        public void InsertLength() => buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count));
        /// <summary>
        /// Convert the packet to a byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray() => buffer.GetRange(ReadPos, GetLength() - ReadPos).ToArray();
        /// <summary>
        /// Get how many bytes there are in the packet.
        /// </summary>
        /// <returns></returns>
        public int GetLength() => buffer.Count;
    }
}
