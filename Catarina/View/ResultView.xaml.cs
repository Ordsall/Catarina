using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Catarina.View
{
    /// <summary>
    /// Логика взаимодействия для ResultView.xaml
    /// </summary>
    public partial class ResultView : Window
    {
        public ResultView()
        {
            InitializeComponent();
        }

        private void pv_ValuesPlot_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var plot = sender as OxyPlot.Wpf.PlotView;
                var pos = e.GetPosition(plot);
                var series = plot.Model.GetSeriesFromPoint(new OxyPlot.ScreenPoint(pos.X, pos.Y), 10);
                if(series != null)
                {
                    var point = series.GetNearestPoint(new OxyPlot.ScreenPoint(pos.X, pos.Y), true);
                    if(point != null)
                    {
                        var dt = OxyPlot.Axes.DateTimeAxis.ToDateTime(point.DataPoint.X);
                        { }
                    }
                }
                
            }
            
        }
    }
}
