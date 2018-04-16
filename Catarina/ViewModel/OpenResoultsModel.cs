using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Catarina.ViewModel
{

    

    public class OpenResoultsModel : ViewModelBase
    {
        string directory = null;

        public string Directory
        {
            get => directory;
            set { directory = value; OnPropertyChanged(nameof(Directory)); }
        }

        public ObservableCollection<TestInfo> Resoults { get; set; } = new ObservableCollection<TestInfo>();

        public OpenResoultsModel(string DefaultDirectory)
        {
            Directory = (new System.IO.DirectoryInfo(DefaultDirectory)).FullName;
            OnPropertyChanged(nameof(Directory));

            ScanDirectory = new ViewModel.RelayCommand(o =>
            {
                ScanDirectoryRecursive(Directory, progress_scan);
            }, o => System.IO.Directory.Exists(Directory));

            progress_scan.ProgressChanged += Progress_scan_ProgressChanged;
        }

        private void Progress_scan_ProgressChanged(object sender, ProgressInfo e)
        {
            IsBusy = e.IsBusy;
            BusyMessage = e.Message;
            if(e.testInfo != null) { Resoults.Add(e.testInfo); }
        }

        public TestInfo SelectedItem { get; set; }

        public struct ProgressInfo
        {
            public bool IsBusy { get; set; }
            public string Message { get; set; }
            public TestInfo testInfo { get; set; }
        }

        Progress<ProgressInfo> progress_scan = new Progress<ProgressInfo>();

        void ScanDirectoryRecursive(string Directory, IProgress<ProgressInfo> progress)
        {
            Resoults.Clear();
            Task.Factory.StartNew(() =>
            {
                progress?.Report(new ProgressInfo() {IsBusy = true, Message = "Сканировние директорий" });
                var d = new System.IO.DirectoryInfo(Directory);
                var ds = d.GetDirectories();
                foreach (var item in ds)
                {
                    var f = item.GetFiles("info.json").DefaultIfEmpty(null).FirstOrDefault();
                    if(f != null)
                    {
                        string  json = System.IO.File.ReadAllText(f.FullName);
                        TestInfo ti = null;
                        try
                        {
                            ti = Newtonsoft.Json.JsonConvert.DeserializeObject<TestInfo>(json);
                            ti.FolderPath = item.FullName;
                            progress?.Report(new ProgressInfo() { IsBusy = true, Message = item.FullName, testInfo = ti });
                        }
                        catch (Exception ex) { }                       
                    }
                }
                progress?.Report(new ProgressInfo() { IsBusy = false, Message = "Завершено" });
            });
        }

        public ICommand ScanDirectory { get; set; }

        bool isBusy = false;

        string busyMessage = "";

        public string BusyMessage
        {
            get => busyMessage;
            set { busyMessage = value; OnPropertyChanged(nameof(BusyMessage)); }
        }

        public bool IsBusy
        {
            get => isBusy;
            set { isBusy = value; OnPropertyChanged(nameof(IsBusy)); }
        }


    }
}
