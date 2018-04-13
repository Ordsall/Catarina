//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Catarina.Interfaces;
//using Newtonsoft.Json;

//namespace Catarina.Devices
//{
//    [JsonObject(MemberSerialization.OptIn)]
//    class OctopusFactory : Interfaces.IDeviceFactory
//    {
//        public OctopusFactory(SerialSettings settings) { _settings = settings; }

//        public override string Type => "ДТ Осьминог";

//        public override string DeviceInfo => String.Format("{0} ({1})", Type, _settings.PortName);

//        public SerialSettings _settings = new SerialSettings(null);

//        [JsonProperty()]
//        public override ISettings Settings
//        {
//            get => _settings;
//            set
//            {
//                if (value is SerialSettings) { _settings = value as SerialSettings; }
//                else throw new Exception("ISettings must be SerialSettings for this device");
//            }
//        }

//        public override IDevice Build()
//        {
//            var p = new Octopus(this, _settings);
//            return p;
//        }
//    }

//    class Octopus : Interfaces.IDevice
//    {

//        public Dictionary<string, int> GetHeaders()
//        {
//            return new Dictionary<string, int>() { { "Амплитуда", 0 }, { "Скорость", 1 }, { "Угловая координата", 2 }, { "Расстояние", 3 } };
//        }

//        public Octopus(OctopusFactory Factory, SerialSettings Settings)
//        {
//            DeviceType = Factory;
//            OctDevice = new Olvia.Devices.Octopus.Olvia.Devices.Octopus.Device();
//            PhDevice = new Olvia.Devices.pheasant.Device();

//            PhDevice.DetectorsChanged += PhDevice_DetectorsChanged;
//        }

//        Olvia.Devices.pheasant.Detector[] Detectors = null;

//        private void PhDevice_DetectorsChanged(Olvia.Devices.pheasant.Detector[] Detectors)
//        {
//            this.Detectors = Detectors;
//            Speed = Detectors[7].Speed;
//        }

//        Olvia.Devices.Octopus.Olvia.Devices.Octopus.Device OctDevice;
//        Olvia.Devices.pheasant.Device PhDevice;

//        public string SerialNumber
//        {
//            get
//            {
//                try { if (PhDevice.IsConnected) return PhDevice.Settings.SerialNumber; }
//                catch (Exception) { return null; }
//                return null;
//            }
//        }

//        public OctopusFactory DeviceType { get; private set; }

//        public string Type => DeviceType.Type;

//        public ISettings Settings { get; set; }

//        public bool IsConnected => PhDevice.IsConnected;

//        public double Speed { get; private set; }

//        public void Connect()
//        {
//            try
//            {
//                var b = OctDevice.Connect((Settings as SerialSettings).PortName);
//                OctDevice.SwitchToPheasant();
//                OctDevice.Disconnect();
//                var s = PhDevice.Connect((Settings as SerialSettings).PortName);
//                if (!b || !s) { throw new Exception("Невозможно подключится к устройству"); }
//            }
//            catch (Exception ex) { throw ex; }

//        }

//        Dictionary<int, double> IDevice.GetData(IProgress<string> progress)
//        {

//            if (PhDevice.IsConnected)
//            {
//                var d = new Dictionary<int, double>();

//                progress?.Report("Снятие уровня шума");

//                Double noize = (PhDevice.Noise[0] + PhDevice.Noise[0] + PhDevice.Noise[0] + PhDevice.Noise[0]) / 4;

//                //d.Add("Уровень шума", noize);

//                Olvia.Devices.pheasant.Detector det = new Olvia.Devices.pheasant.Detector();

//                progress?.Report("Запись настроек");
//                bool set_res = false;
//                for (int i = 0; i < 5; i++)
//                {
//                    set_res = PhDevice.ReadServiceSettings();
//                    if (set_res) break;
//                }
//                if (!set_res) { throw new Exception("Невозможно получить настройки"); }

//                PhDevice.Settings.RadarMode[2] = true;
//                PhDevice.Settings.PhA_A0 = -50;
//                PhDevice.WriteServiceSettings();

//                progress?.Report("Включение потокового режима");
//                PhDevice.EnableFlow();

//                Double? fist_speed = 0;
//                while (Detectors == null) { System.Threading.Thread.Sleep(100); }

//                progress?.Report("Набор данных");
//                fist_speed = Detectors[7].Speed;




//                int div = 0;

//                for (int i = 0; i < 10; i++)
//                {
//                    if (fist_speed == Detectors[7].Speed)
//                    {
//                        if (Detectors[7].Distance > 0) det.Angle += (-Detectors[7].Angle);
//                        else det.Angle += Detectors[7].Angle;

//                        det.Amp += Detectors[7].Amp;
//                        det.Distance += Detectors[7].Distance;
//                        det.Speed += Detectors[7].Speed;
//                        div++;
//                    }
//                }

//                det.Amp /= div;
//                det.Distance /= div;
//                det.Angle /= div;
//                det.Speed /= div;

//                progress?.Report("Выключение потокового режима");
//                PhDevice.DisableFlow();

//                d.Add(0, det.Amp);
//                d.Add(1, det.Speed);
//                d.Add(2, det.Angle);
//                d.Add(3, det.Distance);




//                return d;
//            }
//            else throw new Exception("Невозможно подключится к устройству");
//        }

//        public void Disconnect()
//        {
//            try
//            {
//                if (PhDevice.IsConnected)
//                {
//                    PhDevice.DisableFlow();
//                    PhDevice.Disconnect();
//                }
//                OctDevice.Connect((Settings as SerialSettings).PortName);
//                if (OctDevice.IsConnected)
//                {
//                    OctDevice.SwitchFromPheasant();
//                    OctDevice.Disconnect();
//                }
//            }
//            catch (Exception) { throw; }           
//        }
//    }
//}
