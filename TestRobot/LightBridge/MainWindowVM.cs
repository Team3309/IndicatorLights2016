using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NetworkTables;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace LightBridge
{
    class MainWindowVM : ViewModelBase
    {
        private SerialPort _Port;
        private NetworkTable _Table;
        private TableListener _TableListener;

        public bool IsRed { get; set; }
        public bool IsBlue { get; set; }
        public bool IsLocked { get; set; }
        public double Power { get; set; }
        public ObservableCollection<string> _ComPorts { get; set; }
        //public string SelectedComPort

        // commands
        public RelayCommand ConnectToArduinoCommand { get; private set; }

        // -- Robot IP
        private string _RobotIP = "localhost";
        public string RobotIP
        {
            get { return _RobotIP; }
            set
            {
                if (value == _RobotIP) return;
                _RobotIP = value;
                UpdateNetworkTableConnection();
            }
        }

        // -- Brightness
        private int _Brightness = 256;
        public int Brightness
        {
            get { return _Brightness; }
            set
            {
                if (value == _Brightness) return;
                _Brightness = value;
                
                // more flooding issues...
                Send("bright", value);
            }
        }

        public MainWindowVM()
        {
            // Cbjects
            _ComPorts = new ObservableCollection<string>();

            // Commands
            ConnectToArduinoCommand = new RelayCommand(ConnectToArduino);

            if (IsInDesignMode) return;
            //if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            //    return;

            //// Monitor the serial ports
            //SerialPortMonitor.Initialize(1000);
            //SerialPortMonitor.PortsAdded += (s, e) => e.ForEach(p => _ComPorts.Add(p));
            //SerialPortMonitor.PortsRemoved += (s, e) => e.ForEach(p => _ComPorts.Remove(p));

            // Table listener because NetworkTable doesn't have events (this is dumb; Need to write a wrapper that just processes events)
            _TableListener = new TableListener();
            _TableListener.OnValueChanged += OnTableValueChanged;

            // Connect to the server
            NetworkTable.SetClientMode();
            UpdateNetworkTableConnection();

            // Connect to arduino
            SerialPort.GetPortNames().Distinct().ToList().ForEach(_ComPorts.Add);
            if (_ComPorts.Count() == 1)
            { 
                _Port = new SerialPort(_ComPorts.First(), 9600);
                _Port.Open();

                Brightness = 32;
            }
        }

        private void ConnectToArduino()
        {

        }

        private void UpdateNetworkTableConnection()
        {
            NetworkTable.Shutdown();
            NetworkTable.SetIPAddress(RobotIP);
            NetworkTable.Initialize();
            _Table = NetworkTable.GetTable("Status");
            _Table.AddTableListener(_TableListener, true);
        }

        void Send(string msg, int value)
        {
            string message = $"{msg} {value}";
            if ((_Port != null) && (_Port.IsOpen))
            {
                _Port.Write(message + "\r");
                Console.WriteLine($"Sending: {message}");
            }
            else
            {
                Console.WriteLine($"message '{message}' failed, port not open.");
            }

        }

        private void OnTableValueChanged(object sender, TableValueChanged e)
        {
            if (e.Key == "Red")
            {
                IsRed = (bool)e.Value;
                Send("red", IsRed ? 1 : 0);
            }
            else if (e.Key == "Blue")
            {
                IsBlue = (bool)e.Value;
                Send("blu", IsBlue ? 1 : 0);
            }
            else if (e.Key == "Locked")
            {
                IsLocked = (bool)e.Value;
                Send("lck", IsLocked ? 1 : 0);
            }
            else if (e.Key == "Power")
            {
                Power = (double)e.Value;
                Send("pwr", (int)(Power / 100 * 255));
            }
        }
    }
}
