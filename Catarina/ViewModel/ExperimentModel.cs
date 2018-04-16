//using LiveCharts;
//using LiveCharts.Wpf;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Catarina.ViewModel
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TestInfo
    {
        [JsonProperty()]
        public int MasurmentsCount { get; set; }

        [JsonProperty()]
        public string FinishCause { get; set; }

        [JsonProperty()]
        public bool IsTestFinished { get; set; }

        [JsonProperty()]
        public DateTime StartTime { get; set; }

        [JsonProperty()]
        public DateTime? EndTime { get; set; } = null;

        [JsonProperty()]
        public TimeSpan FetchSpan { get; set; }

        [JsonProperty()]
        public string Device { get; set; }

        [JsonProperty()]
        public string DeviceSerial { get; set; }

        [JsonProperty()]
        public string Imitator { get; set; }

        [JsonProperty()]
        public string ImitatorSerial { get; set; }

        [JsonProperty()]
        public string Environment { get; set; }

        [JsonProperty()]
        public int EnvironmentNumber { get; set; }

        [JsonProperty()]
        public List<string> Headers { get; set; }

        public string FolderPath { get; set; }
    }

    

    public class ExperimentModel : ViewModelBase, IDisposable
    {
        Components.CsvFile dataFile;
        Components.CsvFile spectrumsFile;
        string JsonInfoFile = string.Empty;

        void WriteJsonInfoFile()
        {
            var info = new TestInfo()
            {
                Environment = this.Environment.Title,
                Device = this.SelectedDevice.Type,
                FetchSpan = this.FetchSpan,
                StartTime = this.start_time,
                Imitator = this.Environment.Imitator.Type,
                Headers = this.Headers,
                ImitatorSerial = this.ImitatorSerial,
                DeviceSerial = this.DeviceSerial,
                FinishCause = FinishCause,
                MasurmentsCount = MeasCount,
                EndTime = FinishTime,
                IsTestFinished = IsTestFinished,
                EnvironmentNumber = Environment.Number
            };
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(info, Formatting.Indented);
            try { System.IO.File.WriteAllText(JsonInfoFile, json); }
            catch (Exception) { }

        }

        string FinishCause = "";

        DateTime? FinishTime = null;

        bool IsTestFinished = false;

        public int MeasCount { get; set; } = 0;


        public void FinishTest(string Cause)
        {
            FinishTime = DateTime.Now;
            IsTestFinished = true;
            FinishCause = Cause;
            WriteJsonInfoFile();
        }

        public void CreateExperimentFiles()
        {
            string path = Properties.Settings.Default.OutputDirectory;
            path += start_time.ToString("yyyy-MM-dd HH.mm.ss") + @"\";
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
            dataFile = new Components.CsvFile(path + "data.csv");
            spectrumsFile = new Components.CsvFile(path + "spectrums.csv");
            JsonInfoFile = path + "info.json";
            WriteJsonInfoFile();
            FilesCreated = true;
        }

        List<string> Headers = new List<string>();

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

            //ExpirementData.Clear();

            FetchProgress.ProgressChanged += FetchProgress_ProgressChanged;
            DataProgress.ProgressChanged += DataProgress_ProgressChanged;

            FetchData = new ViewModel.RelayCommand(o =>
            {
                CancelTestingSource = new CancellationTokenSource();
                CancelTesting = CancelTestingSource.Token;
                fetchData(FetchProgress, DataProgress);
            }, o => true);

            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Interval = TimeSpan.FromMilliseconds(400);
            lastMeasurment = DateTime.Now;
            updateTimer.Start();

            ExpirementChartModel.Axes.Add(new OxyPlot.Axes.DateTimeAxis() {
                Font = "Consolas",
                IsAxisVisible = true,
                IsPanEnabled = true,
                FontSize = 10,
                Key = "Время",
                IsZoomEnabled = true,
                MajorGridlineThickness = 1,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
            });

            ExpirementChartModel.Axes.Add(new OxyPlot.Axes.LinearAxis() {
                Font = "Consolas",
                IsPanEnabled = false,
                MaximumPadding = 1,
                MinimumPadding = 1,
                Angle = -45,
                FontSize = 10,
                Key = "Значение",
                AbsoluteMaximum = 200,
                AbsoluteMinimum = -100,
                MajorStep = 50,
                IsZoomEnabled = false,
                MajorGridlineThickness = 1,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
            });
                
        }

        DateTime lastMeasurment = DateTime.Now;

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(TimeLeft));
            OnPropertyChanged(nameof(TimeFetchLeft));
            OnPropertyChanged(nameof(DeviceSerial));
            OnPropertyChanged(nameof(ImitatorSerial));
            if (DateTime.Now - lastMeasurment > FetchSpan)
            {
                lastMeasurment = DateTime.Now;
                if(!IsFetching) FetchData.Execute(null);
            }
        }

        bool IsFetching = false;

        System.Windows.Threading.DispatcherTimer updateTimer = new System.Windows.Threading.DispatcherTimer(System.Windows.Threading.DispatcherPriority.Render);

        DateTime start_time;

        public TimeSpan TimeLeft => (start_time + TerminateSpan) - DateTime.Now;

        public TimeSpan TimeFetchLeft => FetchSpan - (DateTime.Now - lastMeasurment) ;

        bool FilesCreated = false;

        private void DataProgress_ProgressChanged(object sender, List<Interfaces.IParameter> e)
        {
            var wrt = DateTime.Now;

            if (!FilesCreated) { CreateExperimentFiles();  }

            int i = 0;
            foreach (Interfaces.DoubleParameter item in e.OfType<Interfaces.DoubleParameter>())
            {
                if (!Headers.Contains(item.Name)) { Headers.Add(item.Name); }
                var ser = ExpirementChartModel.Series.Where(val => val.Title == item.Name).OfType<LineSeries>().DefaultIfEmpty(null).FirstOrDefault();
                if (ser == null)
                {
                    ser = new LineSeries { XAxisKey = "Время",  YAxisKey = "Значение", StrokeThickness = 1, MarkerSize = 3, CanTrackerInterpolatePoints = true, Title = item.Name, Smooth = false  };
                    ExpirementChartModel.Series.Add(ser);
                }
                if (ser != null) { ser.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(wrt), item.Value)); }
                i++;
                
            }

            ExpirementChartModel.InvalidatePlot(true);

           

            dataFile.WriteRow(wrt,
            e.Where(val => val is Interfaces.DoubleParameter)
            .Select(val => (val as Interfaces.DoubleParameter).Value).ToArray<double>());

            foreach (Interfaces.SpectrumsParameter sp in e.OfType<Interfaces.SpectrumsParameter>())
            {
                foreach (var spectrum in sp.Spectrums) { spectrumsFile.WriteRow(wrt, spectrum); }
            }

            MeasCount++;
        }


        public OxyPlot.PlotModel ExpirementChartModel { get; private set; } = new OxyPlot.PlotModel()
        {
            IsLegendVisible = false, 
            PlotAreaBorderThickness = new OxyThickness(0,0,0,0),
            Padding = new OxyThickness(0, 3, 0, 0),
        };

        public string State { get; set; }

        private void FetchProgress_ProgressChanged(object sender, string e)
        {
            State = e;
            OnPropertyChanged(nameof(State));
        }

        Progress<string> FetchProgress = new Progress<string>();

        Progress<List<Interfaces.IParameter>> DataProgress = new Progress<List<Interfaces.IParameter>>();

        int maxtrycount = 5;

        void fetchData(IProgress<string> message_progress, IProgress<List<Interfaces.IParameter>> data)
        {
            Task.Factory.StartNew(() =>
            {
                int trycount = 0;

                IsFetching = true;

                if (imitator == null) { imitator = Environment.Imitator.Build(); }
                if (device == null) { device = SelectedDevice.Build(); }

                trycount = 0;
                bool SucessStep = false;
                if (!imitator.IsConnected)
                {
                    while (!SucessStep && !CancelTesting.IsCancellationRequested && trycount <= maxtrycount)
                    {
                        message_progress?.Report("Попытка подключения к имитатору");
                        try
                        {
                            imitator.Connect();
                            message_progress?.Report("Имитатор подключен");
                            SucessStep = true;
                        }
                        catch (Exception) { message_progress?.Report("Ошибка подключения к имитатору"); System.Threading.Thread.Sleep(500); }
                    
                        trycount++;
                    }
                }

                if (trycount <= maxtrycount) { trycount = 0; }
                SucessStep = false;
                while (!SucessStep && !CancelTesting.IsCancellationRequested && trycount <= maxtrycount)
                {
                    message_progress?.Report("Установка параметров имитации");
                    try
                    {
                        imitator.Speed = 60;
                        imitator.Direction = Direction.Income;
                        imitator.Distance = 30;
                        SucessStep = true;
                    }
                    catch (Exception) { message_progress?.Report("Ошибка установки параметров имитации"); System.Threading.Thread.Sleep(500); }
                    trycount++;
                }

                if (trycount <= maxtrycount) { trycount = 0; }
                SucessStep = false;
                if (!device.IsConnected)
                {
                    while (!SucessStep && !CancelTesting.IsCancellationRequested && trycount <= maxtrycount)
                    {
                        message_progress?.Report("Попытка подключения к устройству");
                        try
                        {
                            device.Connect();
                            message_progress?.Report("Устройство подключено");
                            SucessStep = true;
                        }
                        catch (Exception) { message_progress?.Report("Подключение к устройству не удалось"); System.Threading.Thread.Sleep(500); }
                        trycount++;
                    }
                }

                if (trycount <= maxtrycount) { trycount = 0; }
                SucessStep = false;
                while (!SucessStep && !CancelTesting.IsCancellationRequested && trycount <= maxtrycount)
                {
                    message_progress?.Report("Включение имитации");
                    imitator.Enable();
                    System.Threading.Thread.Sleep(500);

                    OnPropertyChanged(nameof(DeviceSerial));
                    OnPropertyChanged(nameof(ImitatorSerial));

                    message_progress?.Report("Получение данных с устройства");
                    try
                    {
                        var datat = device.GetData(message_progress);
                        message_progress?.Report("Запись данных");
                        data.Report(datat.ToList());
                        SucessStep = true;
                    }
                    catch (Exception) { System.Threading.Thread.Sleep(500); }
                    trycount++;
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

        public TimeSpan TerminateSpan { get; set; } = TimeSpan.FromHours(24);

        public ViewModel.EnvironmentModel Environment { get; set; }

        public Interfaces.IDeviceFactory SelectedDevice { get; set; }

        public Interfaces.IImitator imitator;

        public Interfaces.IDevice device;

        public string ImitatorSerial => imitator.Serial;

        public string DeviceSerial => device.SerialNumber;



        public void Dispose()
        {
            var t = Task.Factory.StartNew(() => { FinishTest("Завершено пользователем"); });
            t.Wait();
            CancelTestingSource.Cancel();
            updateTimer.Stop();
            disconnectAllIfConnected();
            Environment.IsBusy = false;
        }
    }
}
