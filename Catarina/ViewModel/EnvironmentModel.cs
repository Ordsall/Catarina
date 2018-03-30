using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.ViewModel
{
    public class EnvironmentModel : ViewModelBase
    {
        public ObservableCollection<Interfaces.IDeviceBuilder> DeviceTypes { get; set; } = new ObservableCollection<Interfaces.IDeviceBuilder>();

        public Interfaces.IImitatorBuilder Imitator { get; set; } = new Devices.Sapsan3Builder();

        public string Title => String.Format("Камера {0}", Number);

        public int Number { get; set; } = 1;

        public string DevicesIncludedToString
        {
            get
            {
                if (DeviceTypes.Count > 0)
                {
                    string DevNames = "";
                    foreach (var Dev in DeviceTypes) { DevNames += String.Format("{0} ({1}), ", Dev.Type, Dev.SettingsStirng); }
                    if (DevNames.Length > 2) {DevNames = DevNames.Remove(DevNames.Length - 2); }
                    return DevNames;
                }
                else { return null; }

            }
        } 
    }
}
