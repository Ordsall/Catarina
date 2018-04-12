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

        public SerialSettings _settings = new SerialSettings(null);

        public BPhasantFactory(SerialSettings settings) { _settings = settings; }

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

        public SerialSettings _settings = new SerialSettings(null);

        public BOctopustFactory(SerialSettings settings) { _settings = settings; }

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

        public SerialSettings _settings = new SerialSettings(null);

        public BOctopusMFactory(SerialSettings settings) { _settings = settings; }

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

    public class Phasant : IDevice, IFlowable, ISpectrum
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
            Speed = Detectors[7].Speed;

            Dictionary<int, double> ptemp = new Dictionary<int, double>();

            ptemp.Add(0, Detectors[7].Amp);
            ptemp.Add(1, Detectors[7].Speed);
            ptemp.Add(2, Detectors[7].Angle);
            ptemp.Add(3, Detectors[7].Distance);
      

            ParametersChanged?.Invoke(this, new ParametersChangedArgs() { Parameters = ptemp });
        }

        Olvia.Devices.pheasant.Device _dev = new Olvia.Devices.pheasant.Device();

        public event EventHandler ParametersChanged;

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

        public bool IsConnected => device.IsConnected;

        public double Speed { get; private set; }

        public void Connect()
        {
            try
            {
                bool r = device.Connect((Settings as SerialSettings).PortName);
                if (!r) { throw new Exception("Невозможно подключится к устройству!"); }
            }
            catch (Exception) { throw; }           
        }

       

        public Dictionary<string, object> GetData(IProgress<string> progress = null) //TODO Унаследовать от dict
        {
            if (device.IsConnected)
            {
                var d = new Dictionary<string, object>();

                progress?.Report("Снятие уровня шума");

                device.GetSpectrum();
                //d.Add("Спектр сигнала", device.GetSpectrum());

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

                while (Detectors == null) { System.Threading.Thread.Sleep(100); }

                progress?.Report("Набор данных");

                int div = 0;

                for (int i = 0; i < 10; i++)
                {
                    if (Detectors[7].Distance > 0) det.Angle += (-Detectors[7].Angle);
                    else det.Angle += Detectors[7].Angle;

                    det.Amp += Detectors[7].Amp;
                    det.Distance += Detectors[7].Distance;
                    det.Speed += Detectors[7].Speed;
                    div++;
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

        public void Disconnect()
        {
            if(device.IsConnected)
            {
                device.DisableFlow();
                device.Disconnect();
            }
        }

        public void EnableFlow() => device.EnableFlow();

        public void DisableFlow() => device.DisableFlow();

        public IEnumerable<double> GetSpectrum()
        {
            var d = device.GetSpectrum();
            double[] tmp = new double[d.GetLength(1)];
            Buffer.BlockCopy(d, 0, tmp, 0, tmp.Length * sizeof(double));
            List<double> list = new List<double>(tmp);
            return list;
        }

        public double GetSignal() {
            return device.Signal[0]; }

        public double GetNoize() {
            return device.Noise[0]; }

        public Dictionary<string, int> GetHeaders()
        {
            return new Dictionary<string, int>() { { "Амплитуда", 0 }, { "Скорость", 1 }, { "Угловая координата", 2 }, { "Расстояние", 3 } };
        }
    }
}
