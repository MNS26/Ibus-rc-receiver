using System;
using System.Net;
using System.Net.Sockets;

namespace Ibus
{
    public class TCPIO : IOInterface
    {
        TcpListener listener;
        TcpClient tcp;

        public TCPIO(int port)
        {
            listener = new TcpListener(new IPEndPoint(IPAddress.IPv6Any, port));
            listener.Server.DualMode = true;
            listener.Start();
            listener.BeginAcceptTcpClient(TCPConnect, null);
        }

        private void TCPConnect(IAsyncResult ar)
        {
            try
            {
                if (tcp != null)
                {
                    
                    tcp.Close();
                }
            }
            catch
            {
            }
            tcp = listener.EndAcceptTcpClient(ar);
            listener.BeginAcceptTcpClient(TCPConnect, null);
        }

        public int Available()
        {
            if (tcp == null)
            {
                return 0;
            }
            return tcp.Available;
        }

        public void Read(byte[] buffer, int length)
        {
            try
            {
                int bytesToRead = length;
                while (bytesToRead > 0)
                {
                    int bytesRead = tcp.GetStream().Read(buffer, length - bytesToRead, length);
                    if (bytesRead == 0)
                    {
                        return;
                    }
                    bytesToRead -= bytesToRead;
                }
            }
            catch
            {
                tcp = null;
            }
        }

        public void Write(byte[] buffer, int length)
        {
            try
            {
                tcp.GetStream().Write(buffer, 0, length);
            }
            catch
            {
                tcp = null;
            }
        }
    }
}