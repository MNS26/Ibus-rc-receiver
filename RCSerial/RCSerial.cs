using AutopilotCommon;
using FSControl;
using Modules;
using UnityEngine;
using Ibus;
using System;
using System.IO.Ports;
namespace RCSerial
{
    public class RCSerial : MonoBehaviour
    {
        static DataStore data = new DataStore();
        static Sensor[] sensors = new Sensor[16];
        static Sender sender = new Sender(io);
        static Handler handler = new Handler(MessageEvent, sensors, sender);
        Decoder decoder = new Decoder(handler);
        private static long startupTime = DateTime.UtcNow.Ticks;
        private static IOInterface io;
        private static byte[] sendBuffer = new byte[64];
        byte[] buffer = new byte[64];
        private bool standalone = false;
        public void Start()
        {
            DontDestroyOnLoad(this);
            Log("Start");
            try{
            data.serial = true;
            }
            catch
            {
                standalone = true;
            }
            sensors[1] = new Sensor(SensorType.CELL,GetVoltage);
            if (standalone == true){return;}
            SetupIO();
        }
        public void Update()
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
        public void FixedUpdate()
        {
        }
        private static void MessageEvent(Message m)
        {
            //Console.WriteLine($"message {m.channels[0]}");
            data.RCchannels = m.channelsRaw;
        }
        private static int GetVoltage()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            return (int)((currentTime - startupTime) / TimeSpan.TicksPerMillisecond) % 100;
        }

        private static void SetupIO()
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