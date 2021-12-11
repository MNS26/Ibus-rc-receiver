//using AutopilotCommon;
using UnityEngine;
using Ibus;
using System;
using System.Threading;
using System.IO.Ports;
namespace RCSerial
{
    public class RCSerial : MonoBehaviour
    {
        Thread readThread;
        static AutopilotCommon.DataStore data = new AutopilotCommon.DataStore();
        Sensor[] sensors;
        Sender sender;
        Handler handler;
        Decoder decoder;
        private static long startupTime = DateTime.UtcNow.Ticks;
        IOInterface io;
        private static byte[] sendBuffer = new byte[64];
        byte[] buffer = new byte[64];
        bool running = true;
        private bool standalone = false;
        public void Start()
        {
            Log("Start");
            sensors  = new Sensor[16];
            sensors[0] = null;
            sensors[1] = new Sensor(SensorType.CELL,GetVoltage);
            io = new SerialIO(FindSerialPort());
            sender = new Sender(io);
            handler = new Handler(MessageEvent, sensors, sender);
            decoder = new Decoder(handler);
            DontDestroyOnLoad(this);
            try{
            data.serial = true;
            }
            catch
            {
                standalone = true;
            }
            sensors[1] = new Sensor(SensorType.CELL,GetVoltage);
            if (standalone == true){return;}
            readThread = new Thread(new ThreadStart(Loop));
            readThread.Start();
            //SetupIO(io);
        }
        public void Update(){}
        private void Loop()
        {
            while(running)
            {
                if(standalone == true){return;} 
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
            }
        }
        public void FixedUpdate()
        {
        }
        private static void MessageEvent(Message m)
        {
            //Console.WriteLine($"message {m.channels[0]}");
            data.RCchannels = m.channelsRaw;
        }
        public void OnDestroy()
        {
            running = false;
        }
        private string FindSerialPort()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length > 0)
            {
                //Return the last serial port as this is almost certainly the last one plugged in.
                return ports[ports.Length - 1];
            }
            return null;
        }
        private static int GetVoltage()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            return (int)((currentTime - startupTime) / TimeSpan.TicksPerMillisecond) % 100;
        }

        private static void SetupIO(IOInterface io)
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
                    Log($"Available serial ports: {port}");
                }
            }
        }
        //It's nice to identify in the log where things came from
        public static void Log(string text)
        {
            // Unity didn't like this
            // Debug.Log($"{Time.realtimeSinceStartup} [Autopilot] {text}");
            Debug.Log($"[RCSerial] {text}");
        }
    }
}