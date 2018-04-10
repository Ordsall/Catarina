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

        Random random = new Random();

        public LiveCharts.SeriesCollection LastHourSeries { get; set; }

        void TryFetchTestData(IProgress<string> progress_reporter)
        {
            Task.Factory.StartNew(() =>
            {
                var immitator = selecteEnvironmentModel.Imitator.Build();
                var device = selectedDeviceFactory.Build();
                bool SucessStep = false;
                while(!SucessStep && !CancelTesting.IsCancellationRequested)
                {
                    progress_reporter?.Report(String.Format("Попытка подключения к имитатору {0}", selecteEnvironmentModel.Imitator.DeviceInfo));
                    try { immitator.Connect();
                        progress_reporter?.Report(String.Format("Имитатор {0} {1} подключен", selecteEnvironmentModel.Imitator.DeviceInfo, immitator.Serial));
                        SucessStep = true; }
                    catch (Exception) { progress_reporter?.Report("Подключение к имитатору не удалось"); }
                    System.Threading.Thread.Sleep(500);
                }
                SucessStep = false;
                while (!SucessStep && !CancelTesting.IsCancellationRequested)
                {
                    progress_reporter?.Report("Установка параметров имитации");
                    try
                    {
                        immitator.Speed = random.Next(10, 100);
                        immitator.Direction = Direction.Income;
                        immitator.Distance = 50;
                        progress_reporter?.Report(String.Format("Параметры:\n \tСкорость: {0} км/ч \n \tРасстояние: {1} м\n \tНаправление: {2}",
                            immitator.Speed, immitator.Distance, immitator.Direction == Direction.Income ? "Встречное" : "Попутное"));
                        SucessStep = true;
                    }
                    catch (Exception) { progress_reporter?.Report("Ошибка установки параметров имитации"); }
                    System.Threading.Thread.Sleep(500);
                }

                SucessStep = false;
                while (!SucessStep && !CancelTesting.IsCancellationRequested)
                {
                    progress_reporter?.Report(String.Format("Попытка подключения к устройству {0}", selectedDeviceFactory.DeviceInfo));
                    try { device.Connect();
                        progress_reporter?.Report(String.Format("Устройство {0} {1} подключено", selectedDeviceFactory.DeviceInfo, device.SerialNumber));
                        SucessStep = true; }
                    catch (Exception) { progress_reporter?.Report("Подключение к устройству не удалось"); }
                    System.Threading.Thread.Sleep(500);
                }

                SucessStep = false;
                while (!SucessStep && !CancelTesting.IsCancellationRequested)
                {
                    progress_reporter?.Report("Включение имитации");
                    immitator.Enable();
                    System.Threading.Thread.Sleep(500);
                    progress_reporter?.Report("Получение данных с устройства при включенной имитации");
                    try
                    {
                        var data = device.GetData(progress);
                        progress_reporter?.Report(String.Format("Измеренная скорость: {0} км/ч", device.Speed));

                        if (IsWithin((int)device.Speed, (int)(immitator.Speed -1), (int)(immitator.Speed+1))) 
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
                while (!SucessStep && !CancelTesting.IsCancellationRequested)
                {
                    progress_reporter?.Report("Выключение имитации");
                    immitator.Disable();
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

                

                if (CancelTesting.IsCancellationRequested) { progress_reporter?.Report("Операция прервана");  }



                progress_reporter?.Report("Автоматический тест пройден");
                if (!CancelTesting.IsCancellationRequested) { TestComplete = true; }

                if (device is Interfaces.IFlowable) { (device as Interfaces.IFlowable).EnableFlow(); }
                immitator.Enable();
                int iter = 0;
                bool immit = true;
                while (!CancelTesting.IsCancellationRequested)
                {
                    LastHourSeries[0].Values.Add(new ObservableValue(device.Speed));
                    if (LastHourSeries[0].Values.Count > 15) { LastHourSeries[0].Values.RemoveAt(0); }
                    System.Threading.Thread.Sleep(1000);
                    iter++;
                    if (iter > 10)
                    {
                        if (immit) { immitator.Disable(); }
                        else { immitator.Enable(); }
                        immit = !immit;
                        iter = 0;
                    }
                }
                if (device is Interfaces.IFlowable) { (device as Interfaces.IFlowable).DisableFlow(); }

                try { immitator.Disconnect(); } catch (Exception) { }
                try { device.Disconnect(); } catch (Exception) { }

            }, CancelTesting
            );
        }

        public static bool IsWithin(int value, int minimum, int maximum)
        {
            return value >= minimum && value <= maximum;
        }

        public ExpirementAddMasterModel()
        {
            LastHourSeries = new LiveCharts.SeriesCollection
            {
                new LineSeries
                {
                    AreaLimit = -10,
                    Values = new ChartValues<ObservableValue> { }
                }
            };

            progress.ProgressChanged += Progress_ProgressChanged;

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
            }, o => true);
        }

        private void Progress_ProgressChanged(object sender, string e)
        {
            CheckLog += e + "\n";
            OnPropertyChanged(nameof(CheckLog));
        }

        ViewModel.EnvironmentModel _selecteEnvironmentModel = null;

        bool _testComplete = false;

        public bool TestComplete
        {
            get => _testComplete;
            set { _testComplete = value; OnPropertyChanged(nameof(TestComplete)); }
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

        public ICommand FetchTestData { get; set; }

        public ICommand CancelFetchTestData { get; set; }

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
