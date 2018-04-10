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

namespace Catarina.View.ExperimentAddMasterPages
{
    /// <summary>
    /// Логика взаимодействия для EnvironmentCheckPage.xaml
    /// </summary>
    public partial class EnvironmentCheckPage : Xceed.Wpf.Toolkit.WizardPage
    {
        public EnvironmentCheckPage()
        {
            InitializeComponent();
        }

        private void rtb_Log_TextChanged(object sender, TextChangedEventArgs e)
        {
            rtb_Log.ScrollToEnd();
        }
    }
}
