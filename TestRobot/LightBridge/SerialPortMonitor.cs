using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Timers;

namespace LightBridge
{
    static class SerialPortMonitor
    {
        public static event EventHandler<List<string>> PortsAdded;
        public static event EventHandler<List<string>> PortsRemoved;

        private static Timer _timer;
        private static List<string> _ports;

        public static double UpdateInterval
        {
            get { return _timer.Interval; }
            set
            {
                if (value > 0)
                {
                    _timer.Interval = value;
                    _timer.Start();
                }
                else
                {
                    _timer.Stop();
                }
            }
        }

        static SerialPortMonitor()
        {
            _ports = new List<string>();

            _timer = new Timer();
            _timer.Elapsed += OnTimer;

            Initialize();
        }

        public static void Initialize(double milliseconds)
        {
            Initialize();
            UpdateInterval = milliseconds;
        }

        public static void Initialize()
        {
            Update(false);
        }

        private static void OnTimer(object sender, ElapsedEventArgs e)
        {
            Update();
        }

        public static void Update(bool raiseEvents = true)
        {
            string[] newPorts = SerialPort.GetPortNames();

            var portsRemoved = _ports.Except(newPorts);
            var portsAdded = newPorts.Except(_ports);

            if (raiseEvents)
            {
                if (portsRemoved.Count() > 0)
                {
                    PortsRemoved?.Invoke(null, portsRemoved.ToList());
                }

                if (portsAdded.Count() > 0)
                {
                    PortsAdded?.Invoke(null, portsAdded.ToList());
                }
            }

            _ports = newPorts.ToList();
        }

        public static string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}
