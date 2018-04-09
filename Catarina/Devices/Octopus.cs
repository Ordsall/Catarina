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
    class OctopusFactory : Interfaces.IDeviceFactory
    {
        public override string Type => "ДТ Осьминог";

        public override string DeviceInfo => String.Format("{0} ({1})", Type, _settings.PortName);

        public SerialSettings _settings = new SerialSettings();

        [JsonProperty()]
        public override ISettings Settings
        {
            get => _settings;
            set
            {
                if (value is SerialSettings) { _settings = value as SerialSettings; }
                else throw new Exception("ISettings must be SerialSettings for this device");
            }
        }

        public override IDevice Build()
        {
            var p = new Octopus(this, _settings);
            return p;
        }
    }

    class Octopus : Interfaces.IDevice
    {
        public Octopus(OctopusFactory Factory, SerialSettings Settings)
        {
            DeviceType = Factory;
            Device = new Olvia.Devices.Octopus.Olvia.Devices.Octopus.Device();
        }

        Olvia.Devices.Octopus.Olvia.Devices.Octopus.Device Device;

        public string SerialNumber
        {
            get
            {
                if (Device.IsConnected) return "123123123";
                else return "Неопределено";
            }
        }

        public OctopusFactory DeviceType { get; private set; }

        public string Type => DeviceType.Type;

        public ISettings Settings { get; set; }

        public void Connect()
        {
            try
            {
                var b = Device.Connect((Settings as SerialSettings).PortName);
                if (!b) { throw new Exception("Невозможно подключится к устройству"); }
            }
            catch (Exception ex) { throw ex; }

        }

        Dictionary<string, object> IDevice.GetData(IProgress<string> progress)
        {

            if (Device.IsConnected)
            {
                var d = new Dictionary<string, object>();
                return d;
            }
            return null;
        }     
    }
}
