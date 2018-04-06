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
    class OctopusFactory : Interfaces.IDeviceFactory, Interfaces.ISerialDeviceFactory
    {
        public override string Type => "ДТ Осьминог";

        [JsonProperty()]
        public string PortName { get; set; }

        public override string DeviceInfo => String.Format("{0} ({1})", Type, PortName);

        public override IRadarModule Build()
        {
            var p = new Octopus(this);
            return p;
        }
    }

    class Octopus : Interfaces.IRadarModule, ISerialDevice
    {
        public Octopus(OctopusFactory Factory)
        {
            DeviceType = Factory;
            Device = new Olvia.Devices.Octopus.Olvia.Devices.Octopus.Device();
        }

        Olvia.Devices.Octopus.Olvia.Devices.Octopus.Device Device;

        public string Serial
        {
            get
            {
                if (Device.IsConnected) return "123123123";
                else return "Неопределено";
            }
        }

        public OctopusFactory DeviceType { get; private set; }

        public string Type => DeviceType.Type;

        public string PortName => DeviceType.PortName;

        public void Connect()
        {
            try
            {
                var b = Device.Connect(DeviceType.PortName);
                if (!b) { throw new Exception("Невозможно подключится к устройству"); }
            }
            catch (Exception ex) { throw ex; }

        }

        Dictionary<string, object> IRadarModule.GetData()
        {
            if (Device.IsConnected)
            {
                var d = new Dictionary<string, object>();
                
                return d;
            }
            else throw new Exception("Невозможно подключится к устройству");
        }
    }
}
