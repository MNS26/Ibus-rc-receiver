#pragma warning disable CA1416
//Serial warnings for ios and android

using System.IO.Ports;

namespace Ibus
{
    public class SerialIO : IOInterface
    {
        SerialPort sp;

        public SerialIO(string serialPortName)
        {
            sp = new SerialPort(serialPortName, 115200, Parity.None, 8, StopBits.One);
            sp.Open();
        }

        public int Available()
        {
            return sp.BytesToRead;
        }

        public void Read(byte[] buffer, int length)
        {
            sp.Read(buffer, 0, length);
        }

        public void Write(byte[] buffer, int length)
        {
            sp.Write(buffer, 0, length);
        }
    }
}
