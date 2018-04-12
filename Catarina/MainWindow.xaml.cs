﻿using System;
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

namespace Catarina
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModel.Instance.InstanceModel;
            Olvia.Devices.pheasant.Device dm = new Olvia.Devices.pheasant.Device();
            //TODO Сдлеать настройку параметров тестирования (скорость и т.д.) в настройках камеры и модель страницы с наройками динамическую
        }


        private void bt_Add_Click(object sender, RoutedEventArgs e)
        {
            View.ExperimentAddMaster Master = new View.ExperimentAddMaster();
            Master.DataContext = ViewModel.ExpirementAddMasterModel.Load();
            Master.Owner = this;
            Master.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Master.ShowDialog();
            ViewModel.ExpirementAddMasterModel.Save(Master.DataContext as ViewModel.ExpirementAddMasterModel);
        }

        private void mi_EnvConf_Click(object sender, RoutedEventArgs e)
        {
            View.EnvironmentSetup es = new View.EnvironmentSetup();
            es.Owner = this;
            es.ShowDialog();
        }
    }
}
