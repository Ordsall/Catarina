using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.ViewModel
{
    public class SerialDeviceSetupModel : ViewModel.ViewModelBase, Interfaces.ISetupModel
    {
        public string Header { get; set; } = null;

        public Interfaces.ISerialDeviceFactory Device { get; }

        public SerialDeviceSetupModel(Interfaces.ISerialDeviceFactory device , string header)
        {
            Device = device;
            Header = header;
        }

        public string SerialPortSelected
        {
            get => Device.PortName;
            set { Device.PortName = value; OnPropertyChanged(nameof(SerialPortSelected));  }
        }

        public ObservableCollection<string> AvaibleSerialPortList { get; set; } = 
            new ObservableCollection<string>(System.IO.Ports.SerialPort.GetPortNames().ToList<string>());


        public ObservableCollection<int> BaudRates = new ObservableCollection<int>()
        {
            115200,
            57600,
            38400,
            9600
        };


    }
}
