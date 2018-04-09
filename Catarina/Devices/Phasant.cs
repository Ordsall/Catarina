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
    class BPhasantFactory : Interfaces.IDeviceFactory
    {
        public override string Type => "БМ Фазан";

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
            var p = new Phasant(this, _settings);
            return p;
        }

    }

    [JsonObject(MemberSerialization.OptIn)]
    class BOctopustFactory : Interfaces.IDeviceFactory
    {
        public override string Type => "БМ Осьминог";

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
            var p = new Phasant(this, _settings);
            return p;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    class BOctopusMFactory : Interfaces.IDeviceFactory
    {
        public override string Type => "БМ Осьминог-М";

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
            var p = new Phasant(this, _settings);
            return p;
        }
    }

    public class Phasant : IDevice
    {

        Olvia.Devices.pheasant.Device device;

        public Interfaces.IDeviceFactory DeviceFactory { get; private set; }

        public Phasant(Interfaces.IDeviceFactory Factory, SerialSettings Settings)
        {
            DeviceFactory = Factory;
            this.Settings = Settings;
            device = new Olvia.Devices.pheasant.Device();
            device.DetectorsChanged += Device_DetectorsChanged;
        }

        Olvia.Devices.pheasant.Detector[] Detectors = null;

        private void Device_DetectorsChanged(Olvia.Devices.pheasant.Detector[] Detectors)
        {
            this.Detectors = Detectors;
        }

        Olvia.Devices.pheasant.Device _dev = new Olvia.Devices.pheasant.Device();

        public string SerialNumber
        {
            get
            {
                string ser = "";
                try { ser = device.Settings.SerialNumber; }
                catch (Exception) { ser = null; }
                return ser;
            }
        }

        public string Type => DeviceFactory.Type;

        public ISettings Settings { get; set; }

        public void Connect()
        {
            try
            {
                bool r = device.Connect((Settings as SerialSettings).PortName);
                if (!r) { throw new Exception("Невозможно подключится к устройству!"); }
            }
            catch (Exception) { throw; }           
        }

        public Dictionary<string, object> GetData(IProgress<string> progress = null)
        {
            if (device.IsConnected)
            {
                var d = new Dictionary<string, object>();

                progress?.Report("Снятие уровня шума");

                Double noize = (device.Noise[0] + device.Noise[0] + device.Noise[0] + device.Noise[0]) / 4;

                d.Add("Уровень шума", noize);

                Olvia.Devices.pheasant.Detector det = new Olvia.Devices.pheasant.Detector();

                progress?.Report("Запись настроек");
                bool set_res = false;
                for (int i = 0; i < 5; i++)
                {
                    set_res = device.ReadServiceSettings();
                    if (set_res) break;
                }
                if (!set_res) { throw new Exception("Невозможно получить настройки"); }

                device.Settings.RadarMode[2] = true;
                device.Settings.PhA_A0 = -50;
                device.WriteServiceSettings();

                progress?.Report("Включение потокового режима");
                device.EnableFlow();

                Double? fist_speed = 0;
                while (Detectors == null) { System.Threading.Thread.Sleep(100); }

                progress?.Report("Набор данных");
                fist_speed = Detectors[7].Speed;




                int div = 0;

                for (int i = 0; i < 10; i++)
                {
                    if (fist_speed == Detectors[7].Speed)
                    {
                        if (Detectors[7].Distance > 0) det.Angle += (-Detectors[7].Angle);
                        else det.Angle += Detectors[7].Angle;

                        det.Amp += Detectors[7].Amp;
                        det.Distance += Detectors[7].Distance;
                        det.Speed += Detectors[7].Speed;
                        div++;
                    }
                }

                det.Amp /= div;
                det.Distance /= div;
                det.Angle /= div;
                det.Speed /= div;

                progress?.Report("Выключение потокового режима");
                device.DisableFlow();

                d.Add("Уровень сигнала", det.Amp);
                d.Add("Расстояние", det.Distance);
                d.Add("Угловая координата", det.Angle);
                d.Add("Скорость", det.Speed);


                return d;
            }
            else throw new Exception("Невозможно подключится к устройству");
        }


    }
}
