using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.ViewModel
{
    static public class Instance
    {
        static JsonSerializer serializer = new JsonSerializer();

        static Instance()
        {
            Devices.Add(new Devices.OctopusBuilder());
            Devices.Add(new Devices.BPhasantBuilder());
            Devices.Add(new Devices.BOctopustBuilder());
            Devices.Add(new Devices.BOctopusMBuilder());
            Imitators.Add(new Devices.Sapsan3Builder());
        
            Environments.Add(new EnvironmentModel());

            if(System.IO.File.Exists(@".\Environment.json"))
            {
                string json = System.IO.File.ReadAllText(@".\Environment.json");
                //var s = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<ViewModel.EnvironmentModel>>(json);
            }

            Environments.CollectionChanged += Environments_CollectionChanged;
        }

        private static void Environments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(sender, Formatting.Indented);
            System.IO.File.WriteAllText(@".\Environment.json", json);
        }

        public static ObservableCollection<Interfaces.IDeviceBuilder> Devices { get; set; } = new ObservableCollection<Interfaces.IDeviceBuilder>();

        public static ObservableCollection<Interfaces.IImitatorBuilder> Imitators { get; set; } = new ObservableCollection<Interfaces.IImitatorBuilder>();



        public static ObservableCollection<ViewModel.EnvironmentModel> Environments { get; set; } = new ObservableCollection<EnvironmentModel>();

        public static MainModel InstanceModel { get; set; } = new MainModel();
    }

    

    public class MainModel : ViewModelBase
    {
        public string Title { get; private set; } = String.Format("{0} ({1}) {2}",
            (((AssemblyTitleAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute), false))?.Title ?? "Unknown Title"),
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Name,
            System.Reflection.Assembly.GetExecutingAssembly().GetName().Version
            );

        public ObservableCollection<ExperimentModel> Experiments { get; set; } = new ObservableCollection<ExperimentModel>();


    }
}
