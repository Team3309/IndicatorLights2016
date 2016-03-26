using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LightBridge
{
    class QueuedSerialPort : SerialPort
    {
        private int _Interval { get; set; }
        private Timer _Timer;
        private Stopwatch _Stopwatch;
        private Queue<string> _Queue;

        public QueuedSerialPort(string portName, int baudRate, int interval)
            : base(portName, baudRate)
        {
            _Interval = interval;

            _Stopwatch = new Stopwatch();

            _Queue = new Queue<string>();

            _Timer = new Timer(interval);
            _Timer.Elapsed += OnTimer;
        }

        private void OnTimer(object sender, ElapsedEventArgs e)
        {
            if (_Queue.Count > 0)
            {
                DoWrite(_Queue.Dequeue());
            }

            if (_Queue.Count == 0)
            {
                _Timer.Stop();
            }
        }

        private void DoWrite(string text)
        {
            Console.WriteLine(">>sending: " + text);
            base.Write(text);
            _Stopwatch.Restart();
        }

        new public void Write(string text)
        {
            if ((_Queue.Count == 0) && (_Stopwatch.ElapsedMilliseconds > _Interval))
            {
                DoWrite(text);
            }
            else
            {
                Console.Write(">>adding to queue: " + text);
                _Queue.Enqueue(text);
                if (!_Timer.Enabled)
                {
                    _Timer.Start();
                }
            }
        }
    }
}
