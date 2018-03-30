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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Catarina.View
{
    /// <summary>
    /// Логика взаимодействия для DeviceSelectView.xaml
    /// </summary>
    public partial class DeviceSelectView : Xceed.Wpf.Toolkit.WizardPage
    {
        public DeviceSelectView()
        {
            InitializeComponent();
        }

        private void lb_DeviceTypeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.CanSelectNextPage = (sender as ListBox).SelectedIndex != -1;
        }
    }
}
