using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;

namespace LightBridge
{
    class LEDTest : IDisposable
    {
        private SerialPort _port;
        private readonly int NumLEDS = 8;
        private int led = 0;
        private int value = 0;

        public LEDTest()
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports.Length == 1)
            {
                _port = new SerialPort(ports[0], 250000);
                _port.Open();
            }

            Send("ca");
            System.Threading.Thread.Sleep(10);
            //Send("SetBright 16");

            Timer t = new Timer(10);
            t.Elapsed += OnTimer;
            t.Start();
        }

        private void Send(string str)
        {
            if ((_port != null) && (_port.IsOpen))
            {
                Console.WriteLine(str);
                _port.Write(str + "\r");
            }
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            Send($"s {led} {value} 0 {value}");
            
            value += 16;
            if (value > 255)
            {   
                value = 0;
                led += 1;
                if (led > NumLEDS - 1)
                {
                    System.Threading.Thread.Sleep(10);
                    Send("ca");
                    led = 0;
                }
            }
        }

        public void Dispose()
        {
            if (_port != null)
            {
                _port.Close();
            }
        }
    }
}
