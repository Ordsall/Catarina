using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catarina.ViewModel
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EnvironmentModel : ViewModelBase
    {
        [JsonProperty()]
        public string Title => String.Format("Камера {0}", Number);

        [JsonProperty()]
        public int Number { get; set; } = 1;

        [JsonProperty()]
        public ObservableCollection<Interfaces.IDeviceFactory> DeviceTypes { get; set; } = new ObservableCollection<Interfaces.IDeviceFactory>();

        [JsonProperty()]
        public Interfaces.IImitatorFactory Imitator { get; set; } = new Devices.Sapsan3Factory();

        public string DevicesIncludedToString
        {
            get
            {
                if (DeviceTypes.Count > 0)
                {
                    string DevNames = "";
                    foreach (var Dev in DeviceTypes) { DevNames += Dev.DeviceInfo + ", "; }
                    if (DevNames.Length > 2) {DevNames = DevNames.Remove(DevNames.Length - 2); }
                    return DevNames;
                }
                else { return null; }

            }
        } 
    }
}
