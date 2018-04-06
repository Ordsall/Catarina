using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catarina.Interfaces;
using Newtonsoft.Json;

namespace Catarina.Devices
{
    [JsonObject(MemberSerialization.OptIn)]
    class BPhasantFactory : Interfaces.IDeviceFactory, Interfaces.ISerialDeviceFactory
    {
        public override string Type => "БМ Фазан";

        [JsonProperty()]
        public string PortName { get; set; }

        public override string DeviceInfo => String.Format("{0} ({1})", Type, PortName);

        public override IRadarModule Build()
        {
            var p = new Phasant(this);
            return p;
        }

    }

    [JsonObject(MemberSerialization.OptIn)]
    class BOctopustFactory : Interfaces.IDeviceFactory, Interfaces.ISerialDeviceFactory
    {
        public override string Type => "БМ Осьминог";

        [JsonProperty()]
        public string PortName { get; set; }

        public override string DeviceInfo => String.Format("{0} ({1})", Type, PortName);

        public override IRadarModule Build()
        {
            var p = new Phasant(this);
            return p;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    class BOctopusMFactory : Interfaces.IDeviceFactory, Interfaces.ISerialDeviceFactory
    {
        public override string Type => "БМ Осьминог-М";

        [JsonProperty()]
        public string PortName { get; set; }

        public override string DeviceInfo => String.Format("{0} ({1})", Type, PortName);

        public override IRadarModule Build()
        {
            var p = new Phasant(this);
            return p;
        }
    }

    public class Phasant : Interfaces.ISerialDevice, IRadarModule
    {

        public Interfaces.IDeviceFactory DeviceFactory { get; private set; }

        public Phasant(Interfaces.IDeviceFactory Factory)
        {
            DeviceFactory = Factory;
        }

        Olvia.Devices.pheasant.Device _dev = new Olvia.Devices.pheasant.Device();

        public string PortName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Serial => "00000";

        public string Type { get; set; } = "БМ Фазан, БМ Осьминог, БМ Осьминог-M";

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> GetData()
        {
            throw new NotImplementedException();
        }
    }
}
