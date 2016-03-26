using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LightBridge
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //SerialPortMonitor.UpdateInterval = 1000;
            //Console.WriteLine("PORTS: ");
            //SerialPortMonitor.GetPorts().ToList().ForEach(p => Console.WriteLine(p));

            //LEDTest foo = new LEDTest();
        }
    }
}
