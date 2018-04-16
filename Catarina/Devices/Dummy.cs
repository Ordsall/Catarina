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

    public static class DummyChain 
    {
        static double _speed = 0;

        static System.Threading.Mutex mu_sp = new System.Threading.Mutex();

        public static double Speed
        {
            get => _speed;
            set
            {
                mu_sp.WaitOne();
                _speed = value;
                mu_sp.ReleaseMutex();
            }
        }
    }


    public class DummyDeviceFactory : Interfaces.IDeviceFactory
    {

        public override string Type => "Dummy-радар";

        public override string DeviceInfo => String.Format("{0} ({1})", Type, _settings.PortName);

        public SerialSettings _settings = new SerialSettings(null);

        public DummyDeviceFactory(SerialSettings settings) { _settings = settings; }

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
            var p = new DummyDevice(this, _settings);
            return p;
        }
    }

    public class DummyDevice : IDevice, IFlowable, ISpectrum
    {

        DummyDeviceFactory factory;

        System.Timers.Timer tm_upd = new System.Timers.Timer();

        public DummyDevice(DummyDeviceFactory factory, SerialSettings Settings)
        {
            this.factory = factory;
            this.Settings = Settings;

            tm_upd.Elapsed += Tm_upd_Elapsed;
            tm_upd.Interval = 100;
        }

        private void Tm_upd_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var plst = new List<IParameter>();
            plst.Add(new DoubleParameter("Амплитуда", rm.Next(-30, 0) + rm.NextDouble()));
            plst.Add(new DoubleParameter("Скорость", rm.Next(0, 160) + rm.NextDouble()));
            plst.Add(new DoubleParameter("Угловая координата", rm.Next(-30, 30) + rm.NextDouble()));
            plst.Add(new DoubleParameter("Расстояние", rm.Next(5, 60) + rm.NextDouble()));
            ParametersChanged?.Invoke(this, new ParametersChangedArgs() { Parameters = plst });
        }


        Random rm = new Random();

        string sn = null;

        public string SerialNumber
        {
            get
            {
                if (sn == null) { sn = rm.Next(1000000, 9999999).ToString(); }
                return sn;
            }
        }
            
        public string Type => factory.Type;

        public ISettings Settings { get; set; }

        public bool IsConnected { get; private set; }

        public double Speed => DummyChain.Speed;
        
        public event EventHandler ParametersChanged;

        public void Connect() => IsConnected = true;

        public void Disconnect() => IsConnected = false;

        public void EnableFlow() => tm_upd.Start();

        public void DisableFlow() => tm_upd.Stop();

        public IEnumerable<IParameter> GetData(IProgress<string> progress = null)
        {
            var plst = new List<IParameter>();
            plst.Add(new DoubleParameter("Амплитуда", rm.Next(-30, 0) + rm.NextDouble()));
            plst.Add(new DoubleParameter("Скорость", rm.Next(0, 160) + rm.NextDouble()));
            plst.Add(new DoubleParameter("Угловая координата", rm.Next(-30, 30) + rm.NextDouble()));
            plst.Add(new DoubleParameter("Расстояние", rm.Next(5, 60) + rm.NextDouble()));

            var s = GetSpectrum();
            plst.Add(new SpectrumsParameter("Спектры", new List<double[]>() { s.ToArray() }));

            plst.Add(new DoubleParameter("Сигнал", rm.Next(-30, 0) + rm.NextDouble()));
            plst.Add(new DoubleParameter("Шум", rm.Next(-100, -40) + rm.NextDouble()));

            return plst;
        }

        public List<string> GetHeaders() => new List<string>() { "Амплитуда", "Скорость", "Угловая координата", "Расстояние", "Спектры", "Сигнал", "Шум" };

        public List<string> GetFlowHeaders() => new List<string>() { "Амплитуда", "Скорость", "Угловая координата", "Расстояние" };

        public double GetNoize() => rm.Next(-100, -40) + rm.NextDouble();

        public double GetSignal() => rm.Next(-30, 0) + rm.NextDouble();

        public IEnumerable<double> GetSpectrum()
        {
            var spectrum = new List<double>();
            var t = Task.Factory.StartNew(() => 
            {
                for (int i = 0; i < 512; i++)
                {
                    spectrum.Add(rm.Next(-100, 0) + rm.NextDouble());
                    System.Threading.Thread.Sleep(2);
                }
            });
            t.Wait();            
            return spectrum;
        }
    }

    public class DummyImitatorFactory : Interfaces.IImitatorFactory
    {
        Interfaces.SerialSettings _settings;

        public string Type => "Dummy-имитатор";

        public string DeviceInfo => String.Format("{0} ({1})", Type, _settings.PortName);

        public DummyImitatorFactory(SerialSettings settings) { _settings = settings; }

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
            var p = new DummyImitator(this, _settings);
            return p;
        }
    }

    public class DummyImitator : Interfaces.IImitator
    {
        Random rm = new Random();

        public DummyImitator(DummyImitatorFactory Factory, SerialSettings Settings)
        {
            dummyImitatorFactory = Factory;
            this.Settings = Settings;
        }

        DummyImitatorFactory dummyImitatorFactory;

        string sn = null;

        public string Serial
        {
            get
            {
                if (sn == null) { sn = rm.Next(1000000, 9999999).ToString(); }
                return sn;
            }
        }

        public string Type => dummyImitatorFactory.Type;

        public ISettings Settings { get; private set; }

        public bool IsConnected { get; private set; }

        public bool IsEnabled { get; private set; }

        public Direction Direction { get; set; }

        public double Distance { get; set; }

        public double Speed
        {
            get => DummyChain.Speed;
            set { DummyChain.Speed = value; }
        }


        public void Connect() => IsConnected = true;

        public void Disable()
        {
            saved_speed = Speed;
            Speed = 0;
            IsEnabled = false;
        }

        public void Disconnect() => IsConnected = false;

        public double saved_speed = 0;

        public void Enable()
        {
            Speed = saved_speed;
            IsEnabled = true;
        }
    }
}
