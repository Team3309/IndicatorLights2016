using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NetworkTables;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows.Data;
using System.Windows;

namespace LightBridge.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private SerialPort _Port;
        private NetworkTable _Table;
        private TableListener _TableListener;
        private ConnectionListener _ConnectionListener;

        public bool IsRed { get; set; }
        public bool IsBlue { get; set; }
        public bool IsLocked { get; set; }
        public double Power { get; set; }
        public string RobotIP { get; set; } = Properties.Settings.Default.RobotIP;
        public ObservableCollection<string> ComPorts { get; set; }
        public string SelectedComPort { get; set; }
        public string ConnectionStatus { get; set; } = "Not Connected";

        // commands
        public RelayCommand RefreshComPortsCommand { get; private set; }
        public RelayCommand ConnectToArduinoCommand { get; private set; }
        public RelayCommand ConnectToRobotCommand { get; private set; }

        // -- Brightness
        private int _Brightness = Properties.Settings.Default.Brightness;
        public int Brightness
        {
            get { return _Brightness; }
            set
            {
                if (value == _Brightness) return;
                _Brightness = value;
                UpdateBrightness();
            }
        }

        private bool _ConnectedToRobot = false;
        public bool ConnectedToRobot
        {
            get { return _ConnectedToRobot; }
            set
            {
                if (value == _ConnectedToRobot) return;
                _ConnectedToRobot = value;
                if (value)
                {
                    ConnectionStatus = "Connected";
                    Send("connect", 1);
                }
                else
                {
                    ConnectionStatus = "Not Connected";
                    Send("connect", 0);
                }
            }
        }

        public MainViewModel()
        {
            // -- Objects
            ComPorts = new ObservableCollection<string>();

            // -- Commands
            RefreshComPortsCommand = new RelayCommand(RefreshComPorts);
            ConnectToArduinoCommand = new RelayCommand(ConnectToArduino);
            ConnectToRobotCommand = new RelayCommand(ConnectToRobot);

            // -- Stop here if in the designer!
            if (!IsInDesignMode)
            {
                NonDesignerCtor();
            }
        }

        private void NonDesignerCtor()
        {
            // Table listener because NetworkTable doesn't have events (this is dumb; Need to write a wrapper that just processes events)
            _TableListener = new TableListener();
            _TableListener.OnValueChanged += OnTableValueChanged;
            _ConnectionListener = new ConnectionListener();
            _ConnectionListener.OnConnected += (s, e) => { ConnectedToRobot = true; };
            _ConnectionListener.OnDisconnected += (s, e) => { ConnectedToRobot = false; };

            // Connect to arduino
            RefreshComPorts();

            // Connect to the server
            NetworkTable.SetClientMode();
            ConnectToRobot();
        }

        public override void Cleanup()
        {
            Properties.Settings.Default.RobotIP = RobotIP;
            Properties.Settings.Default.Brightness = Brightness;
            Properties.Settings.Default.Save();
            base.Cleanup();
        }

        private void ConnectToArduino()
        {
            if ((_Port != null) && (_Port.IsOpen))
            {
                _Port.Close();
            }

            if (!string.IsNullOrEmpty(SelectedComPort))
            {
                _Port = new SerialPort(SelectedComPort, 9600);
                try
                {
                    _Port.Open();
                }
                catch
                {
                    MessageBox.Show("Error connecting to com port " + SelectedComPort);
                    RefreshComPorts();
                }

                if ((_Table == null) || (!_Table.IsConnected))
                {
                    Send("connect", 0);
                }

                UpdateBrightness();
            }
        }

        private void RefreshComPorts()
        {
            string lastPort = SelectedComPort;

            ComPorts.Clear();
            SerialPort.GetPortNames().Distinct().ToList().ForEach(ComPorts.Add);
            if (ComPorts.Contains(lastPort))
            {
                SelectedComPort = lastPort;
            }

            if ((_Port == null) || (!_Port.IsOpen))
            {
                if (ComPorts.Count() == 1)
                {
                    SelectedComPort = ComPorts.First();
                    ConnectToArduino();
                }
            }
        }

        private void ConnectToRobot()
        {
            // Disconnect
            ConnectedToRobot = false;
            NetworkTable.Shutdown();

            // Connect
            NetworkTable.SetIPAddress(RobotIP);
            NetworkTable.Initialize();
            _Table = NetworkTable.GetTable("Status");
            _Table.AddConnectionListener(_ConnectionListener, true);
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

        private void UpdateBrightness()
        {
            Send("bright", Brightness);
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
                Send("pwr", Math.Max(0, Math.Min((int)(Power / 100 * 255), 255)));
            }
        }
    }

}