using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.Interfaces
{
    public interface ISerialDeviceFactory
    {
        string PortName { get; set; }
    }

    public interface ISerialDevice 
    {
        string PortName { get; }

        void Connect();
    }
}
