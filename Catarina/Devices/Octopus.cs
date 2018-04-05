using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catarina.Interfaces;

namespace Catarina.Devices
{
    class OctopusBuilder : Interfaces.IDeviceBuilder, Interfaces.ISerialDeviceBuilder
    {
        public string Type => "ДТ Осьминог";

        public string PortName { get; set; }

        public uint BaudRate { get; set; }

        public string SettingsStirng => PortName;

        public IRadarModule Build()
        {
            var p = new Octopus(this);
            p.PortName = PortName;
            p.BaudRate = BaudRate;
            return p;
        }
    }

    class Octopus : Interfaces.IRadarModule, ISerialDevice
    {
        public Octopus(OctopusBuilder builder) => DeviceType = builder;
        public string Serial => "0000";
        public OctopusBuilder DeviceType { get; private set; }
        public string Type => DeviceType.Type;

        public string PortName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public uint BaudRate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Connect(string PortName)
        {
            throw new NotImplementedException();
        }

        public void GetData()
        {
            throw new NotImplementedException();
        }
    }
}
