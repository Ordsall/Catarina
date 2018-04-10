using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Catarina.ViewModel
{

    public enum TimeExpWatch { ByTime, ByInterval }

    [JsonObject(MemberSerialization.OptIn)]
    public class ExpirementAddMasterModel : ViewModelBase
    {

        public static void Save(ExpirementAddMasterModel Model)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(Model, Formatting.Indented);
            System.IO.File.WriteAllText(@".\default_expirement.json", json);
        }

        public static ExpirementAddMasterModel Load()
        {
            if (System.IO.File.Exists(@".\default_expirement.json"))
            {
                string json = System.IO.File.ReadAllText(@".\default_expirement.json");
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ExpirementAddMasterModel>(json);                
            }
            return new ExpirementAddMasterModel();
        }

        public Progress<string> progress = new Progress<string>();

        void TryFetchTestData(IProgress<string> progress_reporter)
        {
            Task.Factory.StartNew(() =>
            {
                var immitator = selecteEnvironmentModel.Imitator.Build();
                progress_reporter?.Report(String.Format("Попытка подключения к имитатору {0}", selecteEnvironmentModel.Imitator.DeviceInfo));
                try { immitator.Connect(); progress_reporter?.Report("Имитатор подключен"); }
                catch (Exception) { progress_reporter?.Report("Подключение не удалось..."); }
                
            }
            );
        }

        public ExpirementAddMasterModel()
        {
            progress.ProgressChanged += Progress_ProgressChanged;

            FetchTestData = new ViewModel.RelayCommand(o =>
            {
                TryFetchTestData(progress);
            }, o => true);
        }

        private void Progress_ProgressChanged(object sender, string e)
        {
            CheckLog += e + "\n";
            OnPropertyChanged(nameof(CheckLog));
        }

        ViewModel.EnvironmentModel _selecteEnvironmentModel = null;

        public ViewModel.EnvironmentModel selecteEnvironmentModel
        {
            get => _selecteEnvironmentModel;
            set { _selecteEnvironmentModel = value; OnPropertyChanged(nameof(selecteEnvironmentModel)); }
        }

        Interfaces.IDeviceFactory _selectedDeviceFactory = null;

        public Interfaces.IDeviceFactory selectedDeviceFactory
        {
            get => _selectedDeviceFactory;
            set { _selectedDeviceFactory = value; OnPropertyChanged(nameof(selectedDeviceFactory)); }
        }

        [JsonProperty()]
        public TimeSpan FetchSpan { get; set; } = TimeSpan.FromMinutes(5);

        [JsonProperty()]
        public TimeSpan TerminateSpan { get; set; } = TimeSpan.FromHours(24);

        public DateTime TerminateDateTime { get; set; } = (DateTime.Now + TimeSpan.FromHours(24));

        [JsonProperty()]
        public TimeExpWatch TerminateCause { get; set; } = TimeExpWatch.ByTime;

        public string CheckLog { get; set; } 

        public ICommand FetchTestData { get; set; }

        public bool ByTimeIsEnabled
        {
            get
            {
                if (TerminateCause == TimeExpWatch.ByTime) { return true; }
                else return false;
            }
            set { if (value) { TerminateCause = TimeExpWatch.ByTime; } OnPropertyChanged(nameof(ByTimeIsEnabled)); OnPropertyChanged(nameof(TerminateCause)); }
        }

        public bool ByIntervalIsEnabled
        {
            get
            {
                if (TerminateCause == TimeExpWatch.ByInterval) { return true; }
                else return false;
            }
            set { if (value) { TerminateCause = TimeExpWatch.ByInterval; } OnPropertyChanged(nameof(ByIntervalIsEnabled)); OnPropertyChanged(nameof(TerminateCause)); }
        }
    }
}
