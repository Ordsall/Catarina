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
    /// <summary>
    /// Pairing dummy imitator with dummy radar device
    /// </summary>
    public static class DummyChain 
    {
        static double _speed = 0;

        /// <summary>
        /// Access mutex for Speed value
        /// </summary>
        static System.Threading.Mutex mu_sp = new System.Threading.Mutex();

        /// <summary>
        /// Speed value for device paring
        /// </summary>
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

    /// <summary>
    /// Device factory for Dummy-devices
    /// </summary>
    public class DummyDeviceFactory : Interfaces.IDeviceFactory
    {
        /// <summary>
        /// Type of the device
        /// </summary>
        public override string Type => "Dummy-радар";

        /// <summary>
        /// Device information string presentation 
        /// </summary>
        public override string DeviceInfo => String.Format("{0} ({1})", Type, _settings.PortName);

        public SerialSettings _settings = new SerialSettings(null);

        /// <summary>
        /// Device factory for Dummy-devices
        /// </summary>
        /// <param name="settings">Settings for serial devices</param>
        public DummyDeviceFactory(SerialSettings settings) { _settings = settings; }

        /// <summary>
        /// Settings of the device
        /// </summary>
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

        /// <summary>
        /// Device builder
        /// </summary>
        /// <returns>Builded IDevice with factory settings</returns>
        public override IDevice Build()
        {
            var p = new DummyDevice(this, _settings);
            return p;
        }
    }

    /// <summary>
    /// Dummuy device class
    /// </summary>
    public class DummyDevice : IDevice, IFlowable, ISpectrum
    {

        DummyDeviceFactory factory;

        System.Timers.Timer tm_upd = new System.Timers.Timer();

        /// <summary>
        /// Dummuy device class
        /// </summary>
        /// <param name="factory">Reference to factory</param>
        /// <param name="Settings">Device settings</param>
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

        /// <summary>
        /// Device serial number
        /// </summary>
        public string SerialNumber
        {
            get
            {
                if (sn == null) { sn = rm.Next(1000000, 9999999).ToString(); }
                return sn;
            }
        }
        
        /// <summary>
        /// Device type
        /// </summary>
        public string Type => factory.Type;

        /// <summary>
        /// Device settings
        /// </summary>
        public ISettings Settings { get; set; }

        /// <summary>
        /// Is device connected
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Device speed recived from Dummy-imitator
        /// </summary>
        public double Speed => DummyChain.Speed;
        
        /// <summary>
        /// Raise when device parameters is changed
        /// </summary>
        public event EventHandler ParametersChanged;

        /// <summary>
        /// Connect to device
        /// </summary>
        public void Connect() => IsConnected = true;

        /// <summary>
        /// Disconnect from device
        /// </summary>
        public void Disconnect() => IsConnected = false;

        /// <summary>
        /// Enable flow mode (device start update parameters automatically, then ParametersChanged raise)
        /// </summary>
        public void EnableFlow() => tm_upd.Start();

        /// <summary>
        /// Disable flow mode
        /// </summary>
        public void DisableFlow() => tm_upd.Stop();

        /// <summary>
        /// Get data from device
        /// </summary>
        /// <param name="progress">Progress reporting while parameters reciving</param>
        /// <returns></returns>
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

        /// <summary>
        /// (Deprecated) Get list of string headers for the parameters
        /// </summary>
        /// <returns>List of string headers</returns>
        [Obsolete("GetHeaders() is deprecated, please use GetData() with parameters names instead.")]
        public List<string> GetHeaders() => new List<string>() { "Амплитуда", "Скорость", "Угловая координата", "Расстояние", "Спектры", "Сигнал", "Шум" };

        /// <summary>
        /// (Deprecated) Get list of string headers for the parameters in flow mode
        /// </summary>
        /// <returns>List of string headers</returns>
        [Obsolete("GetFlowHeaders() is deprecated, please use ParametersChanged event with parameters names instead.")]
        public List<string> GetFlowHeaders() => new List<string>() { "Амплитуда", "Скорость", "Угловая координата", "Расстояние" };

        /// <summary>
        /// Calculte noize value in spectrum data
        /// </summary>
        /// <returns>Noize value</returns>
        public double GetNoize() => rm.Next(-100, -40) + rm.NextDouble();

        /// <summary>
        /// Calculte signal value in spectrum data
        /// </summary>
        /// <returns>Signal value</returns>
        public double GetSignal() => rm.Next(-30, 0) + rm.NextDouble();

        /// <summary>
        /// Get current spectrum vlaues (default 512 points)
        /// </summary>
        /// <returns>Spectrum</returns>
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

    /// <summary>
    /// Dummy imitator factory
    /// </summary>
    public class DummyImitatorFactory : Interfaces.IImitatorFactory
    {
        Interfaces.SerialSettings _settings;

        /// <summary>
        /// Imitator type name
        /// </summary>
        public string Type => "Dummy-имитатор";

        /// <summary>
        /// Device information string presentation 
        /// </summary>
        public string DeviceInfo => String.Format("{0} ({1})", Type, _settings.PortName);

        /// <summary>
        /// Dummy imitator factory
        /// </summary>
        /// <param name="settings">Settings for serial imitator</param>
        public DummyImitatorFactory(SerialSettings settings) { _settings = settings; }

        /// <summary>
        /// Settings for imitator
        /// </summary>
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

        /// <summary>
        /// Imitation speed value
        /// </summary>
        [JsonProperty()]
        public double Speed { get; set; }

        /// <summary>
        /// Imitation distance value
        /// </summary>
        [JsonProperty()]
        public double Distance { get; set; }

        /// <summary>
        /// Imitation direction value
        /// </summary>
        [JsonProperty()]
        public Direction Direction { get; set; }


        /// <summary>
        /// Imitator builder
        /// </summary>
        /// <returns>Builded IDevice with factory settings</returns>
        public IImitator Build()
        {
            var p = new DummyImitator(this, _settings);
            return p;
        }
    }

    /// <summary>
    /// Dummy-imitator class
    /// </summary>
    public class DummyImitator : Interfaces.IImitator
    {
        Random rm = new Random();

        /// <summary>
        /// Dummy-imitator class
        /// </summary>
        /// <param name="Factory">Reference to parent imitator fatory</param>
        /// <param name="Settings">Imitator connection settings</param>
        public DummyImitator(DummyImitatorFactory Factory, SerialSettings Settings)
        {
            dummyImitatorFactory = Factory;
            this.Settings = Settings;
        }

        DummyImitatorFactory dummyImitatorFactory;

        string sn = null;

        /// <summary>
        /// Serial number of connected imitator
        /// </summary>
        public string Serial
        {
            get
            {
                if (sn == null) { sn = rm.Next(1000000, 9999999).ToString(); }
                return sn;
            }
        }

        /// <summary>
        /// Imitator string type
        /// </summary>
        public string Type => dummyImitatorFactory.Type;

        /// <summary>
        /// Imitator connection settings
        /// </summary>
        public ISettings Settings { get; private set; }

        /// <summary>
        /// Is imitator connected
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// Is imitation enabled
        /// </summary>
        public bool IsEnabled { get; private set; }

        /// <summary>
        /// Imitated direction
        /// </summary>
        public Direction Direction { get; set; }

        /// <summary>
        /// Imitated distance
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Imitated speed
        /// </summary>
        public double Speed
        {
            get => DummyChain.Speed;
            set { DummyChain.Speed = value; }
        }

        /// <summary>
        /// Connect device
        /// </summary>
        public void Connect() => IsConnected = true;

        /// <summary>
        /// Disable imitation
        /// </summary>
        public void Disable()
        {
            saved_speed = Speed;
            Speed = 0;
            IsEnabled = false;
        }

        /// <summary>
        /// Disconnect device
        /// </summary>
        public void Disconnect() => IsConnected = false;

        public double saved_speed = 0;

        /// <summary>
        /// Enable imitation
        /// </summary>
        public void Enable()
        {
            Speed = saved_speed;
            IsEnabled = true;
        }
    }
}
