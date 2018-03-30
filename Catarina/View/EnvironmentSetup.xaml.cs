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
    /// Логика взаимодействия для EnvironmentSetup.xaml
    /// </summary>
    public partial class EnvironmentSetup : Window
    {
        public EnvironmentSetup()
        {
            InitializeComponent();
        }

        private void bt_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void bt_Add_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EnvironmentModel Environment = new ViewModel.EnvironmentModel();
            View.EnvironmentAddMaster EnvAdd = new EnvironmentAddMaster() {
                DataContext = new ViewModel.EnvironmentAddMasterModel(ViewModel.Instance.Devices, Environment, ViewModel.Instance.Imitators)
            };                      
            EnvAdd.Owner = this;
            var Resoult = EnvAdd.ShowDialog();
            if(Resoult != null && Resoult != false && (EnvAdd.DataContext as ViewModel.EnvironmentAddMasterModel).IsMasterFinished)
            {
                ViewModel.Instance.Environments.Add((EnvAdd.DataContext as ViewModel.EnvironmentAddMasterModel).Environment);
            }

            { }
        }

        private void bt_Rem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Instance.Environments.Remove(lb_Envs.SelectedItem as ViewModel.EnvironmentModel);
        }
    }
}
