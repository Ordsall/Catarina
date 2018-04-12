using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Catarina.ViewModel
{

    public enum TimeExpWatch { ByTime, ByInterval }

    [JsonObject(MemberSerialization.OptIn)]
    public class ExpirementAddMasterModel : ViewModelBase
    {

        public static void Save(ExpirementAddMasterModel Model)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(Model, Formatting.Indented);
            System.IO.File.WriteAllText(@".\default_expirement.json", json);
        }

        public static ExpirementAddMasterModel Load()
        {
            if (System.IO.File.Exists(@".\default_expirement.json"))
            {
                string json = System.IO.File.ReadAllText(@".\default_expirement.json");
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ExpirementAddMasterModel>(json);                
            }
            return new ExpirementAddMasterModel();
        }

        public Progress<string> progress = new Progress<string>();

        CancellationTokenSource CancelTestingSource = new CancellationTokenSource();
        public CancellationToken CancelTesting;

        CancellationTokenSource CancelSamplingSource = new CancellationTokenSource();
        public CancellationToken CancelSampling;

        Random random = new Random();

        System.Windows.Threading.Dispatcher dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;

        public ChartValues<double> Spectrum { get; set; } = new ChartValues<double>();

        int FailAvalibleCount = 5;

        public Interfaces.IDevice device = null;
        public Interfaces.IImitator imitator = null;

        

        void TryFetchTestData(IProgress<string> progress_reporter)
        {
            Task.Factory.StartNew(() =>
            {
                IsTestFinish = false;
                int failcounter = 0;

                if (imitator == null) { imitator = selecteEnvironmentModel.Imitator.Build(); }
                if (device == null) { device = selectedDeviceFactory.Build(); }

                bool SucessStep = false;
                if (!imitator.IsConnected)
                {
                    while (!SucessStep && !CancelTesting.IsCancellationRequested && failcounter < FailAvalibleCount + 5)
                    {
                        failcounter++;
                        progress_reporter?.Report(String.Format("Попытка подключения к имитатору {0}", selecteEnvironmentModel.Imitator.DeviceInfo));
                        try
                        {
                            imitator.Connect();
                            progress_reporter?.Report(String.Format("Имитатор {0} {1} подключен", selecteEnvironmentModel.Imitator.DeviceInfo, imitator.Serial));
                            SucessStep = true;
                        }
                        catch (Exception) { progress_reporter?.Report("Подключение к имитатору не удалось"); }
                        System.Threading.Thread.Sleep(500);
                    }
                }

                SucessStep = false;
                while (!SucessStep && !CancelTesting.IsCancellationRequested && failcounter < FailAvalibleCount + 5)
                {
                    failcounter++;
                    progress_reporter?.Report("Установка параметров имитации");
                    try
                    {
                        imitator.Speed = random.Next(10, 100);
                        imitator.Direction = Direction.Income;
                        imitator.Distance = 10;
                        progress_reporter?.Report(String.Format("Параметры:\n \tСкорость: {0} км/ч \n \tРасстояние: {1} м\n \tНаправление: {2}",
                            imitator.Speed, imitator.Distance, imitator.Direction == Direction.Income ? "Встречное" : "Попутное"));
                        SucessStep = true;
                    }
                    catch (Exception) { progress_reporter?.Report("Ошибка установки параметров имитации"); }
                    System.Threading.Thread.Sleep(500);
                }

                SucessStep = false;
                if (!device.IsConnected)
                {
                    while (!SucessStep && !CancelTesting.IsCancellationRequested && failcounter < FailAvalibleCount + 5)
                    {
                        failcounter++;
                        progress_reporter?.Report(String.Format("Попытка подключения к устройству {0}", selectedDeviceFactory.DeviceInfo));
                        try
                        {
                            device.Connect();
                            progress_reporter?.Report(String.Format("Устройство {0} {1} подключено", selectedDeviceFactory.DeviceInfo, device.SerialNumber));
                            SucessStep = true;
                        }
                        catch (Exception) { progress_reporter?.Report("Подключение к устройству не удалось"); }
                        System.Threading.Thread.Sleep(500);
                    }
                }

                
                SucessStep = false;
                while (!SucessStep && !CancelTesting.IsCancellationRequested && failcounter < FailAvalibleCount + 5)
                {
                    failcounter++;
                    progress_reporter?.Report("Включение имитации");
                    imitator.Enable();
                    System.Threading.Thread.Sleep(500);
                    progress_reporter?.Report("Получение данных с устройства при включенной имитации");
                    try
                    {

                        var data = device.GetData(progress);
                        progress_reporter?.Report(String.Format("Измеренная скорость: {0} км/ч", device.Speed));

                        if (IsWithin((int)device.Speed, (int)(imitator.Speed -1), (int)(imitator.Speed+1))) 
                        {
                            progress_reporter?.Report("Значение скорости в рамках погрешности(±1 км/ч) при включенной имитации");
                            SucessStep = true;
                        }
                        else { progress_reporter?.Report("Значение скорости не в рамках погрешности(±1 км/ч) при включенной имитации"); }
                    }
                    catch (Exception) { }
                    System.Threading.Thread.Sleep(500);
                }

                SucessStep = false;
                while (!SucessStep && !CancelTesting.IsCancellationRequested && failcounter < FailAvalibleCount + 5)
                {
                    failcounter++;
                    progress_reporter?.Report("Выключение имитации");
                    imitator.Disable();
                    System.Threading.Thread.Sleep(500);
                    progress_reporter?.Report("Получение данных с устройства при выключенной имитации");
                    try
                    {
                        var data = device.GetData(progress);
                        progress_reporter?.Report(String.Format("Измеренная скорость: {0} км/ч", device.Speed));

                        if (IsWithin((int)device.Speed, (int)(0 - 1), (int)(0 + 1))) ///TODO Хардкод - исправить на заполнение из конфигов
                        {
                            progress_reporter?.Report("Значение скорости в рамках погрешности(±1 км/ч) при выключенной имитации");
                            SucessStep = true;
                        }
                        else { progress_reporter?.Report("Значение скорости не в рамках погрешности(±1 км/ч) при выключенной имитации"); }
                    }
                    catch (Exception) { }
                    System.Threading.Thread.Sleep(500);
                }

                if (CancelTesting.IsCancellationRequested) { progress_reporter?.Report("Операция прервана"); }
                if (!CancelTesting.IsCancellationRequested && failcounter < FailAvalibleCount + 5) { progress_reporter?.Report("Автоматический тест пройден"); TestComplete = true; }
                if (!CancelTesting.IsCancellationRequested && failcounter >= FailAvalibleCount + 5) { progress_reporter?.Report("Автоматический тест не пройден"); TestComplete = false; }

                IsTestFinish = true;

                try { if (device is Interfaces.IFlowable) { (device as Interfaces.IFlowable).DisableFlow(); } } catch (Exception) { }

            }, CancelTesting
            );
        }

        Progress<IEnumerable<double>> SpectrumProgress = new Progress<IEnumerable<double>>();

        void TryFetchSamplingData(IProgress<IEnumerable<double>> progress)
        {
            Task.Factory.StartNew(() =>
            {
                progress.Report(Enumerable.Empty<double>());
                if (!imitator.IsConnected) { imitator.Connect(); }
                try { imitator.Enable(); } catch (Exception) { }
                try { imitator.Speed = 60; } catch (Exception) { }
                
                try { if (device is Interfaces.IFlowable) { (device as Interfaces.IFlowable).DisableFlow(); } } catch (Exception) { }
                while (!CancelSampling.IsCancellationRequested)
                {
                    try
                    {
                        if (!device.IsConnected) { device.Connect(); }
                        
                        if (device is Interfaces.ISpectrum)
                        {
                            var spectrum = (device as Interfaces.ISpectrum).GetSpectrum();
                            Signal = (device as Interfaces.ISpectrum).GetSignal();
                            Noize = (device as Interfaces.ISpectrum).GetNoize();
                            progress.Report(spectrum);
                        }
                    }
                    catch (Exception) { System.Threading.Thread.Sleep(1000); }                 
                }

                try { imitator.Disable(); } catch (Exception) { }
            }, CancelSampling );
        }


        void DisconnectDevicesIfConnected()
        {
            Task.Factory.StartNew(() =>
            {
                try { imitator.Disconnect(); } catch (Exception) { }
                try { device.Disconnect(); } catch (Exception) { }
                DeviceDisconnectionFinished = true;
            });
        }

        void CloseMasterRequest()
        {
            Task.Factory.StartNew(() =>
            {
                CancelTestingSource.Cancel();
                CancelSamplingSource.Cancel();
                try { imitator.Disable(); } catch (Exception) { }
                try { if (device is Interfaces.IFlowable) { (device as Interfaces.IFlowable).DisableFlow(); } } catch (Exception) { }
                try { imitator.Disconnect(); } catch (Exception) { }
                try { device.Disconnect(); } catch (Exception) { }
                device = null;
                imitator = null;
            });
        }

        

        double signal = 0;

        public double Signal
        {
            get => signal;
            set { signal = value; OnPropertyChanged(nameof(Signal)); OnPropertyChanged(nameof(SignalNoize)); }
        }

        double noize = 0;

        public double Noize
        {
            get => noize;
            set { noize = value; OnPropertyChanged(nameof(Noize)); OnPropertyChanged(nameof(SignalNoize)); }
        }

        public double SignalNoize
        {
            get => signal - noize;
        }


        public static bool IsWithin(int value, int minimum, int maximum)
        {
            return value >= minimum && value <= maximum;
        }

        public ICommand FetchTestData { get; set; }

        public ICommand CancelFetchTestData { get; set; }

        public ICommand FetchSamplingData { get; set; }

        public ICommand CancelSamplingTestData { get; set; }

        public ICommand RefreshFetchTestData { get; set; }

        public ICommand DisconnectDevices { get; set; }

        public ICommand CloseMaster { get; set; }

        public ICommand EnableImitation { get; set; }

        public ICommand DisableImitation { get; set; }

        public ICommand FetchEchogramm { get; set; }

        public ICommand CancelFetchEchogramm { get; set; }

        public ICommand AddExpToTesting { get; set; }

        public ExpirementAddMasterModel()
        {
            progress.ProgressChanged += Progress_ProgressChanged;
            SpectrumProgress.ProgressChanged += SpectrumProgress_ProgressChanged;
            EchographProgress.ProgressChanged += EchographProgress_ProgressChanged;

            FetchTestData = new ViewModel.RelayCommand(o =>
            {
                CheckLog = string.Empty;
                CancelTestingSource = new CancellationTokenSource();
                CancelTesting = CancelTestingSource.Token;
                TryFetchTestData(progress);
            }, o => true);

            CancelFetchTestData = new ViewModel.RelayCommand(o =>
            {
                CancelTestingSource.Cancel();
                CheckLog = string.Empty;
                TestComplete = false;
            }, o => true);

            RefreshFetchTestData = new ViewModel.RelayCommand(o =>
            {
                CheckLog = string.Empty;
                TestComplete = false;
                TryFetchTestData(progress);
            }, o => true);

            FetchSamplingData = new ViewModel.RelayCommand(o =>
            {
                CancelSamplingSource = new CancellationTokenSource();
                CancelSampling = CancelSamplingSource.Token;
                TryFetchSamplingData(SpectrumProgress);
            }, o => true);

            CancelSamplingTestData = new ViewModel.RelayCommand(o =>
            {
                CancelSamplingSource.Cancel();
                Spectrum.Clear();
            }, o => true);

            DisconnectDevices = new ViewModel.RelayCommand(o =>
            {
                DeviceDisconnectionFinished = false;
                DisconnectDevicesIfConnected();
            }, o => true);

            CloseMaster = new ViewModel.RelayCommand(o =>
            {
                CloseMasterRequest();
            }, o => true);

            EnableImitation = new ViewModel.RelayCommand(o =>
            {
                try { imitator.Enable(); } catch (Exception) { }
            }, o => true);

            DisableImitation = new ViewModel.RelayCommand(o =>
            {
                try { imitator.Disable(); } catch (Exception) { }
            }, o => true);

            FetchEchogramm = new ViewModel.RelayCommand(o =>
            {
                if(device != null && device is Interfaces.IFlowable)
                {
                    EchographData.Clear();
                    var h = device.GetHeaders();
                    foreach (var header in h)
                    {
                        LineSeries l = new LineSeries() { Title = header.Key, Values = new ChartValues<double>() };
                        l.Values.AddRange(Enumerable.Repeat<object>(double.NaN, 150));
                        EchographData.Add(l);
                    }
                    Task.Factory.StartNew(()=> { try { imitator.Speed = 60; imitator.Enable(); } catch (Exception) { }; try { (device as Interfaces.IFlowable).EnableFlow();  } catch (Exception) { } });
                    (device as Interfaces.IFlowable).ParametersChanged += ExpirementAddMasterModel_ParametersChanged;
                }
            }, o => true);

            CancelFetchEchogramm = new ViewModel.RelayCommand(o =>
            {
                if (device != null && device is Interfaces.IFlowable)
                {
                    Task.Factory.StartNew(() => { try { imitator.Disable(); } catch (Exception) { }; try { (device as Interfaces.IFlowable).DisableFlow(); } catch (Exception) { } });
                    (device as Interfaces.IFlowable).ParametersChanged -= ExpirementAddMasterModel_ParametersChanged;
                }
            }, o => true);

            AddExpToTesting = new ViewModel.RelayCommand(o =>
            {
                Instance.Expirements.Add(new ExperimentModel(this));
            }, o => true);
        }

        private void EchographProgress_ProgressChanged(object sender, Dictionary<int, double> e)
        {
            foreach (var item in e)
            {
                EchographData[item.Key].Values.Add(item.Value);
                EchographData[item.Key].Values.RemoveAt(0);
            }
        }

        Progress<Dictionary<int, double>> EchographProgress = new Progress<Dictionary<int, double>>();

        public SeriesCollection EchographData { get; set; } = new SeriesCollection { };

        private void ExpirementAddMasterModel_ParametersChanged(object sender, EventArgs e)
        {
            try
            {
                IProgress<Dictionary<int, double>> prog = EchographProgress;
                prog.Report((e as Interfaces.ParametersChangedArgs).Parameters);
            }
            catch (Exception) { }           
        }

        private void SpectrumProgress_ProgressChanged(object sender, IEnumerable<double> e)
        {
            Spectrum.Clear();
            Spectrum.AddRange(e);
        }

        private void Progress_ProgressChanged(object sender, string e)
        {
            CheckLog += e + "\n";
            OnPropertyChanged(nameof(CheckLog));
        }

        ViewModel.EnvironmentModel _selecteEnvironmentModel = null;

        bool _deviceDisconnectionFinished = true;

        public bool DeviceDisconnectionFinished //TODO Сделать статусы для обоих устройств и отслеживать по ним
        {
            get => _deviceDisconnectionFinished;
            set { _deviceDisconnectionFinished = value; OnPropertyChanged(nameof(DeviceDisconnectionFinished)); }
        }

        bool _testComplete = false;

        public bool TestComplete
        {
            get => _testComplete;
            set { _testComplete = value; OnPropertyChanged(nameof(TestComplete)); }
        }

        bool _isTestFinish = false;

        public bool IsTestFinish
        {
            get => _isTestFinish;
            set { _isTestFinish = value; OnPropertyChanged(nameof(IsTestFinish)); }
        }

        public ViewModel.EnvironmentModel selecteEnvironmentModel
        {
            get => _selecteEnvironmentModel;
            set { _selecteEnvironmentModel = value; OnPropertyChanged(nameof(selecteEnvironmentModel)); }
        }

        Interfaces.IDeviceFactory _selectedDeviceFactory = null;

        public Interfaces.IDeviceFactory selectedDeviceFactory
        {
            get => _selectedDeviceFactory;
            set { _selectedDeviceFactory = value; OnPropertyChanged(nameof(selectedDeviceFactory)); }
        }

        [JsonProperty()]
        public TimeSpan FetchSpan { get; set; } = TimeSpan.FromMinutes(5);

        [JsonProperty()]
        public TimeSpan TerminateSpan { get; set; } = TimeSpan.FromHours(24);

        public DateTime TerminateDateTime { get; set; } = (DateTime.Now + TimeSpan.FromHours(24));

        [JsonProperty()]
        public TimeExpWatch TerminateCause { get; set; } = TimeExpWatch.ByTime;

        public string CheckLog { get; set; } 

        public bool ByTimeIsEnabled
        {
            get
            {
                if (TerminateCause == TimeExpWatch.ByTime) { return true; }
                else return false;
            }
            set { if (value) { TerminateCause = TimeExpWatch.ByTime; } OnPropertyChanged(nameof(ByTimeIsEnabled)); OnPropertyChanged(nameof(TerminateCause)); }
        }

        public bool ByIntervalIsEnabled
        {
            get
            {
                if (TerminateCause == TimeExpWatch.ByInterval) { return true; }
                else return false;
            }
            set { if (value) { TerminateCause = TimeExpWatch.ByInterval; } OnPropertyChanged(nameof(ByIntervalIsEnabled)); OnPropertyChanged(nameof(TerminateCause)); }
        }


       

    }

  

}
