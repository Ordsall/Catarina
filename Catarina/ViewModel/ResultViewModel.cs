using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Catarina.ViewModel
{
    public class ResultViewModel : ViewModelBase
    {
        public ViewModel.TestInfo TestInformation { get; private set; }

        public OxyPlot.PlotModel ParametersChartModel { get; private set; } = new OxyPlot.PlotModel()
        {
            IsLegendVisible = false,
            PlotAreaBorderThickness = new OxyThickness(0, 0, 0, 0),
            Padding = new OxyThickness(0, 3, 0, 0),
        };

        public OxyPlot.PlotModel SpectrumChartModel { get; private set; } = new OxyPlot.PlotModel()
        {
            IsLegendVisible = false,
            PlotAreaBorderThickness = new OxyThickness(0, 0, 0, 0),
            Padding = new OxyThickness(0, 3, 0, 0),
        };

        public ICommand OpenDataCommand { get; set; }

        public struct ProgressInfo
        {
            public bool IsBusy { get; set; }
            public string Message { get; set; }
            public Dictionary<DateTime, double[]> DataPoints { get; set; }
            public Dictionary<DateTime, double[]> SpectrumPoints { get; set; }
        }

        Progress<ProgressInfo> opening_progress = new Progress<ProgressInfo>();

        public void OpenData(IProgress<ProgressInfo> openingProgress)
        {
            Task.Factory.StartNew(() => 
            {
                if(System.IO.File.Exists(TestInformation.FolderPath + "\\data.csv"))
                {
                    Components.CsvFile data = new Components.CsvFile(TestInformation.FolderPath + "\\data.csv");
                    var datad = data.ReadAll();
                    openingProgress.Report(new ProgressInfo() { DataPoints = datad });
                }

                if (System.IO.File.Exists(TestInformation.FolderPath + "\\spectrums.csv"))
                {
                    Components.CsvFile data = new Components.CsvFile(TestInformation.FolderPath + "\\spectrums.csv");
                    var datas = data.ReadAll();
                    openingProgress.Report(new ProgressInfo() { SpectrumPoints = datas });
                }
            });
        }


        private void Opening_progress_ProgressChanged(object sender, ProgressInfo e)
        {
            if(e.DataPoints != null)
            {
                foreach (var meas in e.DataPoints)
                {
                    int i = 0;
                    foreach (var item in meas.Value)
                    {
                        string name = string.Empty;
                        if (TestInformation.Headers.Count == meas.Value.Length) { name = TestInformation.Headers[i]; }
                        else { name = i.ToString(); }
                        var ser = ParametersChartModel.Series.Where(val => val.Title == name).OfType<LineSeries>().DefaultIfEmpty(null).FirstOrDefault();
                        if (ser == null)
                        {
                            ser = new LineSeries { StrokeThickness = 1, MarkerSize = 3, CanTrackerInterpolatePoints = true, Title = name, Smooth = false };
                            ParametersChartModel.Series.Add(ser);
                        }
                        if (ser != null) { ser.Points.Add(new DataPoint(OxyPlot.Axes.DateTimeAxis.ToDouble(meas.Key), item)); }
                        i++;
                       
                    }

                }
                ParametersChartModel.InvalidatePlot(true);
            }
            if(e.SpectrumPoints != null && e.SpectrumPoints.Count > 0)
            {
                var ser = SpectrumChartModel.Series.Where(val => val.Title == "Спектр").OfType<LineSeries>().DefaultIfEmpty(null).FirstOrDefault();
                int i = 0;
                foreach (var sp in e.SpectrumPoints.FirstOrDefault().Value)
                {
                    ser.Points.Add(new DataPoint(i, sp));
                    i++;
                }
                SpectrumChartModel.InvalidatePlot(true);
            }
        }

        public ResultViewModel(ViewModel.TestInfo Item)
        {
            TestInformation = Item;

            OpenDataCommand = new ViewModel.RelayCommand(o =>
            {
                OpenData(opening_progress);
            }, o => TestInformation != null && TestInformation.FolderPath != null && System.IO.Directory.Exists(TestInformation.FolderPath));

            opening_progress.ProgressChanged += Opening_progress_ProgressChanged;



            ParametersChartModel.Axes.Add(new OxyPlot.Axes.DateTimeAxis()
            {
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

            ParametersChartModel.Axes.Add(new OxyPlot.Axes.LinearAxis()
            {
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

            SpectrumChartModel.Axes.Add(new OxyPlot.Axes.LinearAxis()
            {
                Font = "Consolas",
                IsPanEnabled = false,
                Key = "Отсчет",
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                MaximumPadding = 1,
                MinimumPadding = 1,
                Angle = -45,
                FontSize = 10,
                AbsoluteMaximum = 512,
                AbsoluteMinimum = 0,
                MajorStep = 50,
                IsZoomEnabled = false,
                MajorGridlineThickness = 1,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
            });

            SpectrumChartModel.Axes.Add(new OxyPlot.Axes.LinearAxis()
            {
                Font = "Consolas",
                IsPanEnabled = false,
                Key = "Амплитуда",
                MaximumPadding = 1,
                MinimumPadding = 1,
                Angle = -45,
                FontSize = 10,
                AbsoluteMaximum = 20,
                AbsoluteMinimum = -120,
                MajorStep = 20,
                IsZoomEnabled = false,
                MajorGridlineThickness = 1,
                MajorGridlineStyle = LineStyle.Dot,
                MajorGridlineColor = OxyColors.LightGray,
            });

            var spec = new LineSeries {XAxisKey = "Отсчет", YAxisKey = "Амплитуда", StrokeThickness = 1, MarkerSize = 3, CanTrackerInterpolatePoints = true, Title = "Спектр", Smooth = false };
            SpectrumChartModel.Series.Add(spec);


        }

       
    }
}
