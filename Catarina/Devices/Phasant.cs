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

            List<IParameter> ptemp = new List<IParameter>();

            var names = GetFlowHeaders();

            ptemp.Add(new DoubleParameter(names[0], Detectors[7].Amp));
            ptemp.Add(new DoubleParameter(names[1], Detectors[7].Speed));
            ptemp.Add(new DoubleParameter(names[2], Detectors[7].Angle));
            ptemp.Add(new DoubleParameter(names[3], Detectors[7].Distance));


            ParametersChanged?.Invoke(this, new ParametersChangedArgs() { Parameters = ptemp });
        }

        Olvia.Devices.pheasant.Device _dev = new Olvia.Devices.pheasant.Device();

        public event EventHandler ParametersChanged;

        public string SerialNumber { get; private set; }

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
                SerialNumber = device.Settings.SerialNumber;
            }
            catch (Exception) { throw; }
        }



        IEnumerable<IParameter> IDevice.GetData(IProgress<string> progress)
        {
            if (device.IsConnected)
            {
                var d = new List<IParameter>();

                progress?.Report("Снятие уровня шума");

                var s = GetSpectrum();

                Double noize = (device.Noise[0] + device.Noise[0] + device.Noise[0] + device.Noise[0]) / 4;

                Double signal = (device.Signal[0] + device.Signal[0] + device.Signal[0] + device.Signal[0]) / 4;

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

                d.Add(new DoubleParameter(GetHeaders()[0], det.Amp));
                d.Add(new DoubleParameter(GetHeaders()[1], det.Speed));
                d.Add(new DoubleParameter(GetHeaders()[2], det.Angle));
                d.Add(new DoubleParameter(GetHeaders()[3], det.Distance));
                d.Add(new SpectrumsParameter(GetHeaders()[4], new List<double[]>() { s.ToArray() }));
                d.Add(new DoubleParameter(GetHeaders()[5], signal));
                d.Add(new DoubleParameter(GetHeaders()[6], noize));

                return d;
            }
            else throw new Exception("Невозможно подключится к устройству");
        }

        public void Disconnect()
        {
            if (device.IsConnected)
            {
                device.DisableFlow();
                device.Disconnect();
            }
        }

        public void EnableFlow() => device.EnableFlow();

        public void DisableFlow() => device.DisableFlow();

        double[] ConvertSpectrum(int Number, double[,] Data)
        {
            var d = device.GetSpectrum();
            double[] tmp = new double[d.GetLength(1)];
            Buffer.BlockCopy(d, Number * 512, tmp, 0, tmp.Length * sizeof(double));
            return tmp;
        }

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

        public List<string> GetHeaders() => new List<string>() { "Амплитуда", "Скорость", "Угловая координата", "Расстояние", "Спектры", "Сигнал", "Шум" };

        public List<string> GetFlowHeaders() => new List<string>() { "Амплитуда", "Скорость", "Угловая координата", "Расстояние" };
    }
}
