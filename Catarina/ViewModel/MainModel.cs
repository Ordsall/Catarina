﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public static void Test()
        {
            Progress<string> _Progress = new Progress<string>();
            _Progress.ProgressChanged += _Progress_ProgressChanged;

            var imf = new Devices.Sapsan3Factory();
            imf.Settings = new Interfaces.SerialSettings() { PortName = "COM7" };
            var im = imf.Build();

            im.Connect();
            im.Speed = 60;
            im.Enable();

            var s = im.Serial;

            var v = new Devices.BOctopustFactory();
            v.Settings = new Interfaces.SerialSettings() { PortName = "COM6" };
            var dev = v.Build();
            dev.Connect();
            var ser = dev.SerialNumber;
            var task = dev.GetData(_Progress);

        }

        private static void _Progress_ProgressChanged(object sender, string e)
        {
            { }
        }

        static Instance()
        {





            Devices.Add(new Devices.OctopusFactory());
            Devices.Add(new Devices.BPhasantFactory());
            Devices.Add(new Devices.BOctopustFactory());
            Devices.Add(new Devices.BOctopusMFactory());
            Imitators.Add(new Devices.Sapsan3Factory());


            Test();

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
