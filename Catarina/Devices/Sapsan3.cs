using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catarina.Interfaces;

namespace Catarina.Devices
{
    public class Sapsan3Builder : Interfaces.IImitatorBuilder, ISerialDeviceBuilder
    {
        public string Type => "Сапсан 3";

        public string PortName { get; set; }

        public uint BaudRate { set; get; }

        public string InfoString => this.ToString();

        public IImitator Build() { return new Sapsan3(); }

        public override string ToString()
        {
            return String.Format("{0} ({1})", Type, PortName);
        }
    }

    public class Sapsan3 : Interfaces.IImitator, Interfaces.ISerialDevice
    {
        public string PortName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public uint BaudRate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Serial => "000001";

        public string Type => "Сапсан 3";

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Connect(string PortName)
        {
            throw new NotImplementedException();
        }
    }
}
