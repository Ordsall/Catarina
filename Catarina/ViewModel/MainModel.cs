using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Catarina.ViewModel
{
    static public class Instance
    {
        static JsonSerializer serializer = new JsonSerializer();

        static Instance()
        {

            //Devices.Add(new Devices.OctopusFactory(new Interfaces.SerialSettings("COM4")));
            Devices.Add(new Devices.BPhasantFactory(new Interfaces.SerialSettings("COM4")));
            Devices.Add(new Devices.BOctopustFactory(new Interfaces.SerialSettings("COM4")));
            Devices.Add(new Devices.BOctopusMFactory(new Interfaces.SerialSettings("COM4")));
            Imitators.Add(new Devices.Sapsan3Factory(new Interfaces.SerialSettings("COM3")));


            if(System.IO.File.Exists(@".\environment.json"))
            {
                string json = System.IO.File.ReadAllText(@".\environment.json");

                var s = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<ViewModel.EnvironmentModel>>(json, new JsonSerializerSettings() {
                    TypeNameHandling = TypeNameHandling.Auto });

                Environments = s;
            }

            Environments.CollectionChanged += Environments_CollectionChanged;
        }

        public static void SaveSettings()
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(Environments, Formatting.Indented, new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto });
            System.IO.File.WriteAllText(@".\environment.json", json);
        }

        private static void Environments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

        public static ObservableCollection<Interfaces.IDeviceFactory> Devices { get; set; } = new ObservableCollection<Interfaces.IDeviceFactory>();

        public static ObservableCollection<Interfaces.IImitatorFactory> Imitators { get; set; } = new ObservableCollection<Interfaces.IImitatorFactory>();



        public static ObservableCollection<ViewModel.EnvironmentModel> Environments { get; set; } = new ObservableCollection<EnvironmentModel>();

        public static ObservableCollection<ViewModel.ExperimentModel> Expirements { get; set; } = new ObservableCollection<ExperimentModel>();

        public static MainModel InstanceModel { get; set; } = new MainModel();
    }



    public class MainModel : ViewModelBase
    {
        public string Title { get; private set; } = String.Format("{0} ({1}) {2}",
            (((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute), false))?.Title ?? "Unknown Title"),
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
            );

        ViewModel.ExperimentModel selectedExperimentModel = null;

        public ViewModel.ExperimentModel SelectedExperimentModel
        {
            get => selectedExperimentModel;
            set { selectedExperimentModel = value; OnPropertyChanged(nameof(SelectedExperimentModel)); }
        }

      

        public ICommand RemoveExperimentModel { get; set; }

        public MainModel()
        {
            RemoveExperimentModel = new ViewModel.RelayCommand(o =>
            {
                SelectedExperimentModel.Dispose();
                Instance.Expirements.Remove(SelectedExperimentModel);
                SelectedExperimentModel = null;
            }, o => SelectedExperimentModel != null);
        }
    }
}
