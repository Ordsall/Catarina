using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Catarina.ViewModel
{
    public class ExperimentModel : ViewModelBase, IDisposable
    {
        public ExperimentModel(ViewModel.ExpirementAddMasterModel ModelFrom)
        {
            start_time = DateTime.Now;
            State = "Ожидание";
            SelectedDevice = ModelFrom.selectedDeviceFactory;
            Environment = ModelFrom.selecteEnvironmentModel;
            switch (ModelFrom.TerminateCause)
            {
                case TimeExpWatch.ByTime:
                    TerminateSpan = ModelFrom.TerminateDateTime - DateTime.Now;
                    break;
                case TimeExpWatch.ByInterval:
                    TerminateSpan = ModelFrom.TerminateSpan;
                    break;
            }
            FetchSpan = ModelFrom.FetchSpan;
            Environment.IsBusy = true;

            imitator = Environment.Imitator.Build();
            device = SelectedDevice.Build();

            ExpirementData.Clear();
            var h = device.GetHeaders();
            foreach (var header in h)
            {
                LineSeries l = new LineSeries() { Title = header.Key, Values = new ChartValues<double>() };
                ExpirementData.Add(l);
            }

            FetchProgress.ProgressChanged += FetchProgress_ProgressChanged;
            DataProgress.ProgressChanged += DataProgress_ProgressChanged;

            FetchData = new ViewModel.RelayCommand(o =>
            {
                CancelTestingSource = new CancellationTokenSource();
                CancelTesting = CancelTestingSource.Token;
                fetchData(FetchProgress, DataProgress);
            }, o => true);

            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Interval = TimeSpan.FromSeconds(1);
            lastMeasurment = DateTime.Now;
            updateTimer.Start();
        }

        DateTime lastMeasurment = DateTime.Now;

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(TimeLeft));
            OnPropertyChanged(nameof(TimeFetchLeft));
            if (DateTime.Now - lastMeasurment > FetchSpan)
            {
                lastMeasurment = DateTime.Now;
                if(!IsFetching) FetchData.Execute(null);
            }
        }

        bool IsFetching = false;

        System.Windows.Threading.DispatcherTimer updateTimer = new System.Windows.Threading.DispatcherTimer();

        DateTime start_time;

        public TimeSpan TimeLeft => (start_time + TerminateSpan) - DateTime.Now;

        public TimeSpan TimeFetchLeft => FetchSpan - (DateTime.Now - lastMeasurment) ;

        private void DataProgress_ProgressChanged(object sender, Dictionary<int, double> e)
        {
            foreach (var item in e)
            {
                ExpirementData[item.Key].Values.Add(item.Value);
            }
        }

        public SeriesCollection ExpirementData { get; set; } = new SeriesCollection { };

        public string State { get; set; }

        private void FetchProgress_ProgressChanged(object sender, string e)
        {
            State = e;
            OnPropertyChanged(nameof(State));
        }

        Progress<string> FetchProgress = new Progress<string>();

        Progress<Dictionary<int, double>> DataProgress = new Progress<Dictionary<int, double>>();

        void fetchData(IProgress<string> message_progress, IProgress<Dictionary<int, double>> data)
        {
            Task.Factory.StartNew(() =>
            {
                IsFetching = true;

                if (imitator == null) { imitator = Environment.Imitator.Build(); }
                if (device == null) { device = SelectedDevice.Build(); }

                bool SucessStep = false;
                if (!imitator.IsConnected)
                {
                    while (!SucessStep && !CancelTesting.IsCancellationRequested)
                    {
                        message_progress?.Report("Попытка подключения к имитатору");
                        try
                        {
                            imitator.Connect();
                            message_progress?.Report("Имитатор подключен");
                            SucessStep = true;
                        }
                        catch (Exception) { message_progress?.Report("Ошибка подключения к имитатору"); }
                        System.Threading.Thread.Sleep(500);
                    }
                }

                SucessStep = false;
                while (!SucessStep && !CancelTesting.IsCancellationRequested)
                {
                    message_progress?.Report("Установка параметров имитации");
                    try
                    {
                        imitator.Speed = 60;
                        imitator.Direction = Direction.Income;
                        imitator.Distance = 10;
                        SucessStep = true;
                    }
                    catch (Exception) { message_progress?.Report("Ошибка установки параметров имитации"); }
                    System.Threading.Thread.Sleep(500);
                }

                SucessStep = false;
                if (!device.IsConnected)
                {
                    while (!SucessStep && !CancelTesting.IsCancellationRequested)
                    {
                        message_progress?.Report("Попытка подключения к устройству");
                        try
                        {
                            device.Connect();
                            message_progress?.Report("Устройство подключено");
                            SucessStep = true;
                        }
                        catch (Exception) { message_progress?.Report("Подключение к устройству не удалось"); }
                        System.Threading.Thread.Sleep(500);
                    }
                }

                SucessStep = false;
                while (!SucessStep && !CancelTesting.IsCancellationRequested)
                {
                    message_progress?.Report("Включение имитации");
                    imitator.Enable();
                    System.Threading.Thread.Sleep(500);
                    message_progress?.Report("Получение данных с устройства");
                    try
                    {
                        var datat = device.GetData(message_progress);
                        message_progress?.Report("Запись данных");
                        data.Report(datat);
                        SucessStep = true;
                    }
                    catch (Exception) { }
                    System.Threading.Thread.Sleep(500);
                }

                message_progress?.Report("Ожидание");

                try { imitator.Disconnect(); } catch (Exception) { }
                try { device.Disconnect(); } catch (Exception) { }

                IsFetching = false;
            }, CancelTesting);
        }

        void disconnectAllIfConnected()
        {
            Task.Factory.StartNew(() =>
            {
                try { imitator.Disconnect(); } catch (Exception) { }
                try { device.Disconnect(); } catch (Exception) { }
                imitator = null;
                device = null;
            });
        }

        CancellationTokenSource CancelTestingSource = new CancellationTokenSource();
        public CancellationToken CancelTesting;

        public ICommand FetchData { get; set; }

        public TimeSpan FetchSpan { get; set; }
        //public TimeSpan FetchSpan
        //{
        //    get => updateTimer.Interval;
        //    set { updateTimer.Interval = value; OnPropertyChanged(nameof(FetchSpan)); }
        //}


        public TimeSpan TerminateSpan { get; set; } = TimeSpan.FromHours(24);

        public ViewModel.EnvironmentModel Environment { get; set; }

        public Interfaces.IDeviceFactory SelectedDevice { get; set; }

        public Interfaces.IImitator imitator;

        public Interfaces.IDevice device;

        public string ImitatorSerial { get; set; }

        public string DeviceSerial { get; set; }

        public void Dispose()
        {
            CancelTestingSource.Cancel();
            updateTimer.Stop();
            disconnectAllIfConnected();
            Environment.IsBusy = false;
        }
    }
}
