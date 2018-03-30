using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    public interface ISerialDeviceBuilder
    {
        string PortName { get; set; }

        uint BaudRate { get; set; }
    }

    public interface ISerialDevice 
    {
        string PortName { get; set; }

        uint BaudRate { get; set; }

        void Connect();

        void Connect(string PortName);
    }
}
