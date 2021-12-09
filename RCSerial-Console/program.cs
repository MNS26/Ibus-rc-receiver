#pragma warning disable CA1416
//Serial warnings for ios and android

using System;
using System.IO.Ports;
using System.Threading;

namespace Ibus
{
    class Program
    {
        private static long startupTime = DateTime.UtcNow.Ticks;
        private static IOInterface io;
        private static byte[] sendBuffer = new byte[64];
        public static void Main(string[] args)
        {
            SetupIO(args);

            //Set up sensors
            Sensor[] sensors = new Sensor[16];
            sensors[0] = null; //sensor 0 is the internal one on the RX
            sensors[1] = new Sensor(SensorType.GPS_ALT, GetVoltage);
            sensors[2] = new Sensor(SensorType.GPS_LAT, GetAltitude);
            sensors[3] = new Sensor(SensorType.GPS_LON,GetRPM);
            sensors[4] = new Sensor(SensorType.ARMED, GetRPM);
            sensors[5] = new Sensor(SensorType.FLIGHT_MODE,GetRPM); 



            Sender sender = new Sender(io);
            Handler handler = new Handler(MessageEvent, sensors, sender);
            Decoder decoder = new Decoder(handler);

            bool running = true;
            byte[] buffer = new byte[64];
            while (running)
            {
                int bytesAvailable = 0;
                while ((bytesAvailable = io.Available()) > 0)
                {
                    int bytesRead = bytesAvailable;
                    if (bytesRead > buffer.Length)
                    {
                        bytesRead = buffer.Length;
                    }
                    io.Read(buffer, bytesRead);
                    decoder.Decode(buffer, bytesRead);
                }
                //FileIO has run out of data, quit.
                if (io is FileIO)
                {
                    running = false;
                }
                Thread.Sleep(1);
            }
        }

        private static int GetVoltage()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            return (int)((currentTime - startupTime) / TimeSpan.TicksPerMillisecond) % 100;
        }

        private static int GetAltitude()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            return (int)((currentTime - startupTime) / TimeSpan.TicksPerMillisecond) * 100;
        }
        private static int GetRPM()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            return (int)((currentTime - startupTime) / TimeSpan.TicksPerMillisecond) * 100;
        }

        private static void MessageEvent(Message m)
        {
            //Console.WriteLine($"message {m.channels[0]}");
        }

        private static void SetupIO(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "file")
                {
                    io = new FileIO();
                }
                if (args[0] == "tcp")
                {
                    io = new TCPIO(5867);
                }
                if (args[0] == "serial")
                {
                    if (args.Length > 1)
                    {
                        io = new SerialIO(args[1]);
                    }
                    else
                    {
                        string[] serialPorts = SerialPort.GetPortNames();
                        if (serialPorts.Length == 1)
                        {
                            io = new SerialIO(serialPorts[0]);
                        }
                        else
                        {
                            foreach (string port in serialPorts)
                            {
                                Console.WriteLine($"Available serial ports: {port}");
                            }
                        }
                    }
                }
                if (args[0] == "udp")
                {
                    io = new UDPIO(5687);
                }
            }
            if (io == null)
            {
                io = new SerialIO("/dev/ttyUSB0");
            }
        }
    }
}
