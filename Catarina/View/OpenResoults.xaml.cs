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
    /// Логика взаимодействия для OpenResoults.xaml
    /// </summary>
    public partial class OpenResoults : Window
    {
        public OpenResoults()
        {
            InitializeComponent();
        }

        private void dg_Files_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            dg_Files.Columns[0].SortDirection = System.ComponentModel.ListSortDirection.Descending;
        }

        private void dg_Files_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            dg_Files.Columns[0].SortDirection = System.ComponentModel.ListSortDirection.Descending;
        }

        private void bt_SelectFolder_Click(object sender, RoutedEventArgs e)
        {
            WPFFolderBrowser.WPFFolderBrowserDialog bfd = new WPFFolderBrowser.WPFFolderBrowserDialog();
            bfd.InitialDirectory = tb_Path.Text;
            if(bfd.ShowDialog(this) == true)
            {
                tb_Path.Text = bfd.FileName;
                (DataContext as ViewModel.OpenResoultsModel).Directory = bfd.FileName; //TODO Убрать связанность модели с вьюхой
            }
            if( (DataContext as ViewModel.OpenResoultsModel).ScanDirectory.CanExecute(null))
            {
                (DataContext as ViewModel.OpenResoultsModel).ScanDirectory.Execute(null);
            }




        }

        private void dg_Files_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(dg_Files.SelectedItem != null)
            {
                ResultView rw = new ResultView();
                rw.Owner = this;
                rw.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                rw.DataContext = new ViewModel.ResultViewModel(dg_Files.SelectedItem as ViewModel.TestInfo); //TODO Убрать связанность модели с вьюхой
                rw.ShowDialog();
            }
        }
    }
}
