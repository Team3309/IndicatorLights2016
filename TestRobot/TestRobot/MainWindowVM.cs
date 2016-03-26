using NetworkTables;
using System;
using System.Collections.Generic;
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
        private NetworkTable _Table;

        // -- Red
        private bool _isRed;
        public bool IsRed
        {
            get { return _isRed; }
            set
            {
                if (_isRed == value) return;
                _isRed = value;
                NotifyPropertyChanged();
                _Table.PutBoolean("Red", value);
            }
        }

        // -- Blue
        private bool _isBlue;
        public bool IsBlue
        {
            get { return _isBlue; }
            set
            {
                if (_isBlue == value) return;
                _isBlue = value;
                NotifyPropertyChanged();
                _Table.PutBoolean("Blue", value);
            }
        }

        // -- Locked
        private bool _isLocked;
        public bool IsLocked
        {
            get { return _isLocked; }
            set
            {
                if (_isLocked == value) return;
                _isLocked = value;
                NotifyPropertyChanged();
                _Table.PutBoolean("Locked", value);
            }
        }

        private double _power;
        public double Power
        {
            get { return _power; }
            set
            {
                if (_power == value) return;
                _power = value;
                NotifyPropertyChanged();
                _Table.PutNumber("Power", value);
            }
        }

        public MainWindowVM()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            NetworkTable.SetServerMode();
            NetworkTable.Initialize();
            _Table = NetworkTable.GetTable("Status");
        }

    }
}
