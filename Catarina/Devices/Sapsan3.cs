using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catarina.Interfaces;
using Catarina.ViewModel;
using Newtonsoft.Json;

namespace Catarina.Devices
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Sapsan3Factory : Interfaces.IImitatorFactory
    {
        Interfaces.SerialSettings _settings;

        public string Type => "Сапсан 3";

        public string DeviceInfo => String.Format("{0} ({1})", Type, _settings.PortName);

        public Sapsan3Factory(SerialSettings settings) { _settings = settings; }

        [JsonProperty()]
        public ISettings Settings
        {
            get => _settings;
            set
            {
                if (value is SerialSettings) { _settings = value as SerialSettings; }
                else throw new Exception("ISettings must be SerialSettings for this device");
            }
        }

        [JsonProperty()]
        public double Speed { get; set; }

        [JsonProperty()]
        public double Distance { get; set; }

        [JsonProperty()]
        public Direction Direction { get; set; }

        public IImitator Build()
        {
            var p = new Sapsan3(this, _settings);
            return p;
        }
    }


    public class Sapsan3 : Catarina.Interfaces.IImitator
    {

        Sapsan3Factory sapsan3Factory;

        Olvia.Devices.Sapsan3.Device device;

        public Sapsan3(Sapsan3Factory Factory, SerialSettings Settings)
        {
            sapsan3Factory = Factory;
            this.Settings = Settings;
            device = new Olvia.Devices.Sapsan3.Device();
        }


        public string Serial { get; private set; }

        public string Type => sapsan3Factory.Type;

        public ISettings Settings { get; private set; }

        public bool IsConnected => device.IsConnected;

        public bool IsEnabled => device.Targets[0].Enable;

        Olvia.Devices.Sapsan3.Target tg = new Olvia.Devices.Sapsan3.Target()
        {
            Direction = Olvia.Devices.Sapsan3.Target.Directn.Встречное,
            Distance = 10,
            Enable = false,
            Speed = 60
        };

        public Direction Direction
        {
            get
            {
                if (device.Targets[0].Direction == Olvia.Devices.Sapsan3.Target.Directn.Встречное) return Direction.Income;
                else return Direction.Outcome;
            }
            set
            {
                if (value == Direction.Income) tg.Direction = Olvia.Devices.Sapsan3.Target.Directn.Встречное;
                else tg.Direction = Olvia.Devices.Sapsan3.Target.Directn.Попутное;
                device.SetTarget(0, tg);
            }
        }



        public double Distance
        {
            get => device.Targets[0].Distance;
            set
            {
                tg.Distance = value;
                if(device.IsConnected) { device.SetTarget(0, tg); }
            }
        }

        public double Speed
        {
            get => device.Targets[0].Speed;
            set
            {
                tg.Speed = value;
                if (device.IsConnected) { device.SetTarget(0, tg); }
            }
        }

        public void Connect()
        {
            bool b = device.Connect((Settings as SerialSettings).PortName);
            if (!b) { throw new Exception("Невозможно подключится к имитатору"); }
            Serial = device.Settings.p0_SerialNumber;
        }

        public void Disable()
        {
            tg.Enable = false;
            device.SetTarget(0, tg); 
        }

        public void Enable()
        {
            tg.Enable = true;
            if (device.IsConnected) { device.SetTarget(0, tg); }
        }

        public void Disconnect()
        {
            device.Disconnect();
        }
    }
}
