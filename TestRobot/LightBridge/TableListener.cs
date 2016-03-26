using NetworkTables.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetworkTables;

namespace LightBridge
{
    public class TableValueChanged : EventArgs
    {
        public string Key { get; private set; }
        public object Value { get; private set; }

        public TableValueChanged(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    class TableListener : ITableListener
    {
        public event EventHandler<TableValueChanged> OnValueChanged;

        public void ValueChanged(ITable source, string key, object value, NotifyFlags flags)
        {
            OnValueChanged?.Invoke(this, new TableValueChanged(key, value));
        }
    }
}

