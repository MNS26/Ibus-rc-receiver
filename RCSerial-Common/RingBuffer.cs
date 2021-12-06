using System;
using System.IO;
using System.Collections.Generic;

namespace Ibus
{
    public class RingBuffer
    {
        public byte[] buffer = new byte[512];
        int readPos;
        int writePos;

        public int Available
        {
            get
            {
                return (buffer.Length + writePos - readPos) % buffer.Length;
            }
        }

        public int WriteAvailable
        {
            get
            {
                return (buffer.Length + readPos - writePos - 1) % buffer.Length;
            }
        }

        public void Write(byte[] bytes, int offset, int length)
        {
            if (length > WriteAvailable)
            {
                throw new ArgumentOutOfRangeException("Buffer overflow");
            }
            int firstWrite = buffer.Length - writePos;
            if (firstWrite > length)
            {
                firstWrite = length;
            }
            int secondWrite = length - firstWrite;
            try
            {
                Array.Copy(bytes, offset, buffer, writePos, firstWrite);
            }
            catch
            {
                Console.WriteLine($"bytes,offset:{offset}, buffer,writePos:{writePos},firstWrite:{firstWrite}");
            }
            writePos += firstWrite;
            if (writePos == buffer.Length)
            {
                writePos = 0;
            }
            if (secondWrite > 0)
            {
                Array.Copy(bytes, offset + firstWrite, buffer, writePos, secondWrite);
                writePos = secondWrite;
            }
        }

        public void Read(byte[] dest, int offset, int length)
        {
            if (length > Available)
            {
                throw new ArgumentOutOfRangeException("Buffer underrun");
            }
            int firstRead = buffer.Length - readPos;
            if (firstRead > length)
            {
                firstRead = length;
            }else
            if (0>firstRead)
            {
                firstRead = 1; 
            }
            int secondRead = length - firstRead;
            //Array.Copy(buffer, readPos, dest, offset, firstRead);
            try
            {
                Array.Copy(buffer, readPos, dest, offset, firstRead);
            }
            catch
            {
                Console.WriteLine($"buffer,readPos:{readPos}, dest,offset:{offset},firtsRead:{firstRead}");
            }
            readPos += firstRead;
            if (readPos == buffer.Length)
            {
                readPos = 0;
            }
            if (secondRead > 0)
            {
                Array.Copy(buffer, readPos, dest, firstRead + offset, secondRead);
            }
        }

        public byte ReadByte()
        {
            byte returnValue = buffer[readPos];
            readPos++;
            if (readPos == buffer.Length)
            {
                readPos = 0;
            }
            return returnValue;
        }
    }
}