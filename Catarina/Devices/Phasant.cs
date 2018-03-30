using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catarina.Interfaces;

namespace Catarina.Devices
{

    class BPhasantBuilder : Interfaces.IDeviceBuilder, Interfaces.ISerialDeviceBuilder
    {
        public string Type => "БМ Фазан";

        public IRadarModule Build()
        {
            var p = new Phasant();
            p.PortName = PortName;
            p.BaudRate = BaudRate;
            p.Type = Type;
            return p;
        }

        public string PortName { get; set; }

        public uint BaudRate { get; set; }

        public string SettingsStirng => PortName;
    }

    class BOctopustBuilder : Interfaces.IDeviceBuilder, Interfaces.ISerialDeviceBuilder
    {
        public string Type => "БМ Осьминог";

        public IRadarModule Build()
        {
            var p = new Phasant();
            p.PortName = PortName;
            p.BaudRate = BaudRate;
            p.Type = Type;
            return p;
        }

        public string PortName { get; set; }

        public uint BaudRate { get; set; }

        public string SettingsStirng => PortName;
    }

    class BOctopusMBuilder : Interfaces.IDeviceBuilder, Interfaces.ISerialDeviceBuilder
    {
        public string Type => "БМ Осьминог-M";

        public IRadarModule Build()
        {
            var p = new Phasant();
            p.PortName = PortName;
            p.BaudRate = BaudRate;
            p.Type = Type;
            return p;
        }

        public string PortName { get; set; }

        public uint BaudRate { get; set; }

        public string SettingsStirng => PortName;
    }

    public class Phasant : Interfaces.ISerialDevice, IRadarModule
    {
        Olvia.Devices.pheasant.Device _dev = new Olvia.Devices.pheasant.Device();

        public string PortName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public uint BaudRate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Serial => "00000";

        public string Type { get; set; } = "БМ Фазан, БМ Осьминог, БМ Осьминог-M";

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
